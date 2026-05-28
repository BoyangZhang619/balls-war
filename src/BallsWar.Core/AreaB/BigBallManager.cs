namespace BallsWar.AreaB;

public class BigBallManager
{
    private readonly BattleGrid _grid;
    private readonly List<BigBall> _balls = new();
    private int _nextId;

    public IReadOnlyList<BigBall> ActiveBalls => _balls;

    public BigBallManager(BattleGrid grid) => _grid = grid;

    public void Spawn(int factionId, long value, int campX, int campY, Random rng)
    {
        float angle = (float)(rng.NextDouble() * Math.PI * 2);
        _balls.Add(new BigBall(_nextId++, factionId, campX, campY, angle, 3f, value));
    }

    public BigBall? GetBallAt(int x, int y, int ownerFactionId)
    {
        foreach (var b in _balls)
        {
            if (b.Dead || b.FactionId != ownerFactionId) continue;
            float dx = b.X - x, dy = b.Y - y;
            if (dx * dx + dy * dy <= b.VisualRadius * b.VisualRadius)
                return b;
        }
        return null;
    }

    /// Enemy pellet hits this big ball — deal damage.
    public bool DamageBall(BigBall ball, int damage)
    {
        ball.Value -= damage;
        if (ball.Value <= 0) ball.Kill();
        return ball.Dead;
    }

    public void Update(float dt)
    {
        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            var b = _balls[i];
            if (b.Dead) { _balls.RemoveAt(i); continue; }

            var camp = _grid.GetCamp(b.FactionId);
            if (camp.IsDestroyed || b.Value <= 0) { b.Kill(); continue; }

            // Move
            float stepDist = b.Speed * dt;
            b.Accumulator += stepDist;
            bool moved = false;

            while (b.Accumulator >= 1f && !b.Dead && b.Value > 0)
            {
                b.Accumulator -= 1f;
                b.PrevGridX = (int)b.X; b.PrevGridY = (int)b.Y;
                b.X += MathF.Cos(b.Angle);
                b.Y += MathF.Sin(b.Angle);
                moved = true;

                int gx = (int)MathF.Round(b.X);
                int gy = (int)MathF.Round(b.Y);

                if (!_grid.IsInBounds(gx, gy))
                {
                    Reflect(b, gx, gy);
                    gx = (int)MathF.Round(b.X); gy = (int)MathF.Round(b.Y);
                    if (!_grid.IsInBounds(gx, gy)) { b.Kill(); break; }
                }

                if (gx != b.PrevGridX || gy != b.PrevGridY)
                    ProcessCell(b, gx, gy);
            }

            // Wide-area capture: all cells within visual radius
            if (moved && !b.Dead)
                CaptureWideArea(b);

            // BigBall vs BigBall collision
            for (int j = i - 1; j >= 0; j--)
            {
                var other = _balls[j];
                if (other.Dead || other.FactionId == b.FactionId) continue;
                float dx = b.X - other.X, dy = b.Y - other.Y;
                float dist = MathF.Sqrt(dx * dx + dy * dy);
                float minDist = b.VisualRadius + other.VisualRadius;
                if (dist < minDist && dist > 0.01f)
                {
                    float overlap = minDist - dist;
                    float nx = dx / dist, ny = dy / dist;
                    b.X += nx * overlap * 0.5f; b.Y += ny * overlap * 0.5f;
                    other.X -= nx * overlap * 0.5f; other.Y -= ny * overlap * 0.5f;
                    float tmp = b.Angle; b.Angle = other.Angle; other.Angle = tmp;
                }
            }
        }
    }

    private void CaptureWideArea(BigBall b)
    {
        float r = b.VisualRadius;
        int minX = Math.Max(0, (int)(b.X - r));
        int maxX = Math.Min(_grid.Width - 1, (int)(b.X + r));
        int minY = Math.Max(0, (int)(b.Y - r));
        int maxY = Math.Min(_grid.Height - 1, (int)(b.Y + r));

        for (int cx = minX; cx <= maxX; cx++)
        {
            for (int cy = minY; cy <= maxY; cy++)
            {
                float dx = cx - b.X, dy = cy - b.Y;
                if (dx * dx + dy * dy > r * r) continue;

                var cell = _grid.Cells[cx, cy];
                if (cell.OwnerFactionId.HasValue && cell.OwnerFactionId != b.FactionId)
                {
                    var enemyCamp = _grid.GetCamp(cell.OwnerFactionId.Value);
                    if (!enemyCamp.IsDestroyed && IsCampCenter(enemyCamp, cx, cy))
                    {
                        long dmg = b.Value < 100 ? b.Value : Math.Max(100, b.Value / 2);
                        b.Value -= dmg;
                        enemyCamp.TakeDamage((int)Math.Min(int.MaxValue, dmg));
                        _grid.RaiseCampDamaged(enemyCamp.FactionId, (int)dmg, enemyCamp.Health);
                        if (enemyCamp.IsDestroyed) _grid.RaiseCampDestroyed(enemyCamp.FactionId);
                        if (b.Value <= 0) { b.Kill(); return; }
                    }
                }

                // Only consume value when actually changing ownership
                if (cell.OwnerFactionId != b.FactionId)
                {
                    _grid.CaptureCell(cx, cy, b.FactionId);
                    b.Value--;
                    if (b.Value <= 0) { b.Kill(); return; }
                }
            }
        }
    }

    private void Reflect(BigBall b, int outX, int outY)
    {
        if (outX < 0 || outX >= _grid.Width)
        { b.Angle = MathF.PI - b.Angle; b.X = Math.Clamp(b.X, 0.5f, _grid.Width - 1.5f); }
        if (outY < 0 || outY >= _grid.Height)
        { b.Angle = -b.Angle; b.Y = Math.Clamp(b.Y, 0.5f, _grid.Height - 1.5f); }
        while (b.Angle < 0) b.Angle += MathF.PI * 2;
        while (b.Angle >= MathF.PI * 2) b.Angle -= MathF.PI * 2;
    }

    private void ProcessCell(BigBall b, int gx, int gy)
    {
        var cell = _grid.Cells[gx, gy];
        if (cell.OwnerFactionId.HasValue && cell.OwnerFactionId != b.FactionId)
        {
            var enemyCamp = _grid.GetCamp(cell.OwnerFactionId.Value);
            if (!enemyCamp.IsDestroyed && IsCampCenter(enemyCamp, gx, gy))
            {
                long dmg = b.Value < 100 ? b.Value : Math.Max(100, b.Value / 2);
                b.Value -= dmg;
                _grid.CaptureCell(gx, gy, b.FactionId);
                bool destroyed = enemyCamp.TakeDamage((int)Math.Min(int.MaxValue, dmg));
                if (destroyed) _grid.RaiseCampDestroyed(enemyCamp.FactionId);
                else _grid.RaiseCampDamaged(enemyCamp.FactionId, (int)dmg, enemyCamp.Health);
                if (b.Value <= 0) b.Kill();
                return;
            }
        }

        if (!cell.OwnerFactionId.HasValue || cell.OwnerFactionId != b.FactionId)
        {
            _grid.CaptureCell(gx, gy, b.FactionId);
            b.Value--;
            if (b.Value <= 0) b.Kill();
        }
    }

    private static bool IsCampCenter(Camp camp, int x, int y)
        => Math.Abs(x - camp.CenterX) <= camp.HitRadius && Math.Abs(y - camp.CenterY) <= camp.HitRadius;

    public void Clear() => _balls.Clear();
}
