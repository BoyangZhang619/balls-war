namespace BallsWar.AreaB;

public class PelletManager
{
    private readonly BattleGrid _grid;
    private readonly Game.GameConfig _config;
    private readonly List<ShotgunPellet> _pellets = new();
    private int _nextId;
    private float _fireTimer;

    public IReadOnlyList<ShotgunPellet> ActivePellets => _pellets;
    public int Count => _pellets.Count;
    public int[] PelletCountsByFaction { get; private set; } = [];

    public PelletManager(BattleGrid grid, Game.GameConfig config)
    {
        _grid = grid;
        _config = config;
    }

    public void Update(float dt)
    {
        _fireTimer += dt;
        float fireInterval = 1f / 60f;
        int fc = _grid.Camps.Count;
        if (PelletCountsByFaction.Length != fc)
            PelletCountsByFaction = new int[fc];

        // Auto-fire 60 rounds/sec
        while (_fireTimer >= fireInterval)
        {
            _fireTimer -= fireInterval;
            foreach (var camp in _grid.Camps.Values)
            {
                if (camp.IsDestroyed || camp.ShotgunAmmo <= 0) continue;
                camp.FiringAngle += camp.FiringAngularSpeed * fireInterval;

                int pelletCount = (int)Math.Max(1, camp.ShotgunAmmo * _config.PelletsPerShotgun / 100);
                camp.ShotgunAmmo -= pelletCount;
                if (camp.ShotgunAmmo < 0) camp.ShotgunAmmo = 0;

                float spreadRad = MathF.PI / 180f * _config.ShotgunSpreadDegrees / 2f;
                for (int i = 0; i < pelletCount; i++)
                {
                    float offset = pelletCount == 1 ? 0
                        : (i - (pelletCount - 1) / 2f) / ((pelletCount - 1) / 2f) * spreadRad;
                    _pellets.Add(new ShotgunPellet(_nextId++, camp.FactionId,
                        camp.CenterX, camp.CenterY,
                        camp.FiringAngle + offset,
                        _config.PelletSpeed, _config.PelletBounces));
                }
            }
        }

        // Advance angles for camps with no ammo
        foreach (var camp in _grid.Camps.Values)
        {
            if (camp.IsDestroyed || camp.ShotgunAmmo > 0) continue;
            camp.FiringAngle += camp.FiringAngularSpeed * dt;
        }

        // Update pellets
        Array.Clear(PelletCountsByFaction);
        for (int i = _pellets.Count - 1; i >= 0; i--)
        {
            var p = _pellets[i];
            if (p.Dead) { _pellets.RemoveAt(i); continue; }

            var camp = _grid.GetCamp(p.FactionId);
            if (camp.IsDestroyed) { p.Kill(); continue; }

            PelletCountsByFaction[p.FactionId]++;

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

                gx = (int)MathF.Round(p.X); gy = (int)MathF.Round(p.Y);
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
        var cell = _grid.Cells[gx, gy];

        // Check for enemy big ball protecting this cell
        if (cell.OwnerFactionId.HasValue && cell.OwnerFactionId != p.FactionId)
        {
            var bigBall = _grid.BigBallManager.GetBallAt(gx, gy, cell.OwnerFactionId.Value);
            if (bigBall != null)
            {
                _grid.BigBallManager.DamageBall(bigBall, 1);
                p.Kill();
                return;
            }
        }

        // Enemy camp hit → capture, damage, die
        if (cell.OwnerFactionId.HasValue && cell.OwnerFactionId != p.FactionId)
        {
            var enemyCamp = _grid.GetCamp(cell.OwnerFactionId.Value);
            if (!enemyCamp.IsDestroyed && IsCampCenter(enemyCamp, gx, gy))
            {
                bool destroyed = enemyCamp.TakeDamage(_config.PelletDamage);
                _grid.CaptureCell(gx, gy, p.FactionId);
                if (destroyed) _grid.RaiseCampDestroyed(enemyCamp.FactionId);
                else _grid.RaiseCampDamaged(enemyCamp.FactionId, _config.PelletDamage, enemyCamp.Health);
                p.Kill();
                return;
            }
        }

        // Neutral or enemy cell → capture + die
        if (!cell.OwnerFactionId.HasValue || cell.OwnerFactionId != p.FactionId)
        {
            _grid.CaptureCell(gx, gy, p.FactionId);
            p.Kill();
            return;
        }

        // Friendly cell → pass through, continue flying
    }

    private static bool IsCampCenter(Camp camp, int x, int y)
        => Math.Abs(x - camp.CenterX) <= camp.HitRadius && Math.Abs(y - camp.CenterY) <= camp.HitRadius;

    public void Clear() { _pellets.Clear(); PelletCountsByFaction = []; }
}
