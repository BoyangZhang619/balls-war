namespace BallsWar.AreaB;

public class PelletManager
{
    private readonly BattleGrid _grid;
    private readonly Game.GameConfig _config;
    private readonly List<ShotgunPellet> _pellets = new();
    private int _nextId;
    private readonly Dictionary<int, HashSet<int>> _visitedCells = new();
    private float _fireTimer;

    private const int CampHitCost = 50;

    public IReadOnlyList<ShotgunPellet> ActivePellets => _pellets;

    public PelletManager(BattleGrid grid, Game.GameConfig config)
    {
        _grid = grid;
        _config = config;
    }

    public void SpawnPellets(int factionId, int count, int campX, int campY,
                              float firingAngle, int totalBudget)
    {
        if (count <= 0) return;

        int budgetPerPellet = Math.Max(10, totalBudget / count);
        float spreadRad = MathF.PI / 180f * _config.ShotgunSpreadDegrees / 2f;
        float speed = _config.PelletSpeed;
        int bounces = _config.PelletBounces;

        for (int i = 0; i < count; i++)
        {
            float offset;
            if (count == 1) offset = 0;
            else offset = (i - (count - 1) / 2f) / ((count - 1) / 2f) * spreadRad;
            float angle = firingAngle + offset;

            var pellet = new ShotgunPellet(_nextId++, factionId, campX, campY,
                angle, speed, bounces, budgetPerPellet);
            _pellets.Add(pellet);
            _visitedCells[pellet.Id] = new HashSet<int>();
        }
    }

    public void Update(float dt)
    {
        _fireTimer += dt;
        float fireInterval = 1f / 60f;

        while (_fireTimer >= fireInterval)
        {
            _fireTimer -= fireInterval;
            foreach (var camp in _grid.Camps.Values)
            {
                if (camp.IsDestroyed || camp.ShotgunAmmo <= 0) continue;
                camp.FiringAngle += camp.FiringAngularSpeed * fireInterval;

                // totalBudget = current ammo (1 ammo = 1 potential bullet)
                long totalBudget = camp.ShotgunAmmo;
                int pelletCount = (int)Math.Max(1, totalBudget * _config.PelletsPerShotgun / 100);
                SpawnPellets(camp.FactionId, pelletCount, camp.CenterX, camp.CenterY,
                    camp.FiringAngle, (int)Math.Min(int.MaxValue, totalBudget));
                // Consume fired bullets from ammo pool
                camp.ShotgunAmmo = Math.Max(0, camp.ShotgunAmmo - pelletCount);
            }
        }

        // Advance angles for camps with no ammo
        foreach (var camp in _grid.Camps.Values)
        {
            if (camp.IsDestroyed || camp.ShotgunAmmo > 0) continue;
            camp.FiringAngle += camp.FiringAngularSpeed * dt;
        }

        // Update each pellet
        for (int i = _pellets.Count - 1; i >= 0; i--)
        {
            var p = _pellets[i];
            if (p.Dead) { _visitedCells.Remove(p.Id); _pellets.RemoveAt(i); continue; }

            if (p.CaptureBudget <= 0) { p.Kill(); continue; }

            var camp = _grid.GetCamp(p.FactionId);
            if (camp.IsDestroyed) { p.Kill(); continue; }

            float stepDist = p.Speed * dt;
            p.Accumulator += stepDist;

            while (p.Accumulator >= 1f && !p.Dead)
            {
                p.Accumulator -= 1f;
                p.PrevGridX = (int)p.X; p.PrevGridY = (int)p.Y;
                p.X += MathF.Cos(p.Angle);
                p.Y += MathF.Sin(p.Angle);

                int gx = (int)MathF.Round(p.X);
                int gy = (int)MathF.Round(p.Y);

                if (!_grid.IsInBounds(gx, gy))
                {
                    if (p.BouncesLeft > 0) Reflect(p, gx, gy);
                    else { p.Kill(); break; }
                }

                gx = (int)MathF.Round(p.X);
                gy = (int)MathF.Round(p.Y);
                if (!_grid.IsInBounds(gx, gy)) { p.Kill(); break; }

                if (gx != p.PrevGridX || gy != p.PrevGridY)
                    ProcessCell(p, gx, gy);
            }
        }
    }

    private void Reflect(ShotgunPellet p, int outX, int outY)
    {
        p.BouncesLeft--;
        if (outX < 0 || outX >= _config.GridWidth)
        { p.Angle = MathF.PI - p.Angle; p.X = Math.Clamp(p.X, 0.5f, _config.GridWidth - 1.5f); }
        if (outY < 0 || outY >= _config.GridHeight)
        { p.Angle = -p.Angle; p.Y = Math.Clamp(p.Y, 0.5f, _config.GridHeight - 1.5f); }
        while (p.Angle < 0) p.Angle += MathF.PI * 2;
        while (p.Angle >= MathF.PI * 2) p.Angle -= MathF.PI * 2;
    }

    private void ProcessCell(ShotgunPellet p, int gx, int gy)
    {
        int cellHash = gy * _config.GridWidth + gx;
        if (!_visitedCells[p.Id].Add(cellHash)) return; // already visited

        var cell = _grid.Cells[gx, gy];

        // Check enemy camp center hit
        if (cell.OwnerFactionId.HasValue && cell.OwnerFactionId != p.FactionId)
        {
            var enemyCamp = _grid.GetCamp(cell.OwnerFactionId.Value);
            if (!enemyCamp.IsDestroyed && IsCampCenter(enemyCamp, gx, gy))
            {
                bool destroyed = enemyCamp.TakeDamage(_config.PelletDamage);
                _grid.CaptureCell(gx, gy, p.FactionId);
                p.CaptureBudget -= CampHitCost;
                if (destroyed) _grid.RaiseCampDestroyed(enemyCamp.FactionId);
                else _grid.RaiseCampDamaged(enemyCamp.FactionId, _config.PelletDamage, enemyCamp.Health);

                if (p.CaptureBudget <= 0) p.Kill();
                return;
            }
        }

        // Capture neutral or enemy cell
        if (!cell.OwnerFactionId.HasValue || cell.OwnerFactionId != p.FactionId)
        {
            _grid.CaptureCell(gx, gy, p.FactionId);
            p.CaptureBudget--;
            if (p.CaptureBudget <= 0) p.Kill();
        }
    }

    private static bool IsCampCenter(Camp camp, int x, int y)
        => Math.Abs(x - camp.CenterX) <= 5 && Math.Abs(y - camp.CenterY) <= 5;

    public void Clear()
    {
        _pellets.Clear();
        _visitedCells.Clear();
    }
}
