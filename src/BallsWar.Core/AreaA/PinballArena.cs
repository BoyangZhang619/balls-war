using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;
using BallsWar.Events;

namespace BallsWar.AreaA;

/// Shared pinball arena. No gravity — balls maintain constant speed.
/// Layout (8m × 12m):
///   Top: spawn area
///   Middle: dividing platform with multiplier strips + center hole
///   Bottom: SHOTGUN (left) | RETURN (center) | SHIELD (right)
public class PinballArena
{
    public World World { get; }
    public IReadOnlyList<Ball> Balls => _balls;
    public IReadOnlyList<Zone> MultiplierZones => _multiplierZones;
    public IReadOnlyList<Zone> ConversionZones => _conversionZones;
    public IReadOnlyList<(AEVector2 Pos, float Radius)> Pegs => _pegs;
    public IReadOnlyList<(AEVector2 Pos, float Radius)> LowerObstacles => _lowerObs;
    private readonly List<Ball> _balls = new();
    private readonly List<Zone> _multiplierZones = new();
    private readonly List<Zone> _conversionZones = new();
    private readonly List<Ball> _deadBalls = new();
    private readonly List<Ball> _multiplierReturnQueue = new();
    private readonly List<Ball> _overflowQueue = new();
    private readonly List<(AEVector2, float)> _pegs = new();
    private readonly List<(AEVector2, float)> _lowerObs = new();
    private readonly Game.GameConfig _config;
    private readonly Random _rng;
    private int _nextBallId;
    private int _nextZoneId;

    public event Action<BallConvertedEvent>? BallConverted;

    public float Width => _config.ArenaWidthMeters;
    public float Height => _config.ArenaHeightMeters;

    public PinballArena(Game.GameConfig config, Random rng)
    {
        _config = config;
        _rng = rng;
        World = new World(new AEVector2(0f, -10f));

        CreateWalls();
        CreateDividingPlatform();
        CreateMultiplierBars();
        CreateLowerObstacles();
        CreateBottomBars();
        CreateBalls();
    }

    // ── Walls ────────────────────────────────────────────────

    private void CreateWalls()
    {
        float w = Width, h = Height, t = 0.3f;
        CreateStaticRect(new AEVector2(w / 2, -t / 2), w / 2 + t, t / 2);
        CreateStaticRect(new AEVector2(w / 2, h + t / 2), w / 2 + t, t / 2);
        CreateStaticRect(new AEVector2(-t / 2, h / 2), t / 2, h / 2 + t);
        CreateStaticRect(new AEVector2(w + t / 2, h / 2), t / 2, h / 2 + t);
    }

    private Body CreateStaticRect(AEVector2 pos, float halfW, float halfH)
    {
        var body = new Body { BodyType = BodyType.Static, Position = pos };
        var verts = PolygonTools.CreateRectangle(halfW, halfH);
        var fixture = body.CreateFixture(new PolygonShape(verts, 1f));
        fixture.Restitution = 1.0f; // perfect bounce off walls
        World.Add(body);
        return body;
    }

    // ── Dividing Platform (with center hole) ──────────────────

    private void CreateDividingPlatform()
    {
        float w = Width, h = Height;
        float platformY = h * 0.55f;
        float platformThick = 0.3f;
        float holeHalfW = 1.0f;
        float center = w / 2;

        float leftW = center - holeHalfW;
        if (leftW > 0)
            CreateStaticRect(new AEVector2(leftW / 2, platformY), leftW / 2, platformThick / 2);

        float rightStart = center + holeHalfW;
        float rightW = w - rightStart;
        if (rightW > 0)
            CreateStaticRect(new AEVector2(rightStart + rightW / 2, platformY), rightW / 2, platformThick / 2);
    }

    // ── Multiplier Bars (on dividing platform, left + right) ──

    private void CreateMultiplierBars()
    {
        float w = Width, h = Height;
        float platformY = h * 0.55f + 0.18f;
        float holeHalfW = 1.0f;
        float center = w / 2;
        float leftEdge = 0.3f;
        float leftPlatformEnd = center - holeHalfW;
        float halfWidth = leftPlatformEnd - leftEdge;

        int[] mults = _config.MultiplierValues; // [8,4,2,2,2]
        float barH = 0.15f;
        float gap = 0.06f;
        int n = mults.Length;
        // Compute exact segment width to fill from leftEdge to leftPlatformEnd
        float segW = (halfWidth - gap * (n - 1)) / n;
        if (segW < 0.15f) segW = 0.15f;

        float curX = leftEdge;

        for (int i = 0; i < n; i++)
        {
            float midX = curX + segW / 2;

            var zoneL = new MultiplierZone(_nextZoneId++, new AEVector2(midX, platformY),
                World, segW, barH, ZoneShape.Rectangle, mults[i]);
            _multiplierZones.Add(zoneL);
            SetupZoneContact(zoneL);

            float rightMidX = w - midX;
            var zoneR = new MultiplierZone(_nextZoneId++, new AEVector2(rightMidX, platformY),
                World, segW, barH, ZoneShape.Rectangle, mults[i]);
            _multiplierZones.Add(zoneR);
            SetupZoneContact(zoneR);

            // Tall thin separator bar between multiplier strips
            if (i < n - 1)
            {
                float pegX = curX + segW + gap / 2;
                float pegY = platformY + 0.35f;
                float pegHW = 0.06f, pegHH = 0.45f;
                CreatePeg(new AEVector2(pegX, pegY), pegHW, pegHH);
                CreatePeg(new AEVector2(w - pegX, pegY), pegHW, pegHH);
            }

            curX += segW + gap;
        }
    }

    private void CreatePeg(AEVector2 pos, float hw, float hh)
    {
        var body = new Body { BodyType = BodyType.Static, Position = pos };
        var verts = PolygonTools.CreateRectangle(hw, hh);
        var fixture = body.CreateFixture(new PolygonShape(verts, 1f));
        fixture.Restitution = 1.0f;
        fixture.Friction = 0f;
        World.Add(body);
        _pegs.Add((pos, hw)); // store half-width as "radius" for rendering
    }

    // ── Lower Obstacles: spread balls evenly across bottom zones ──

    private void CreateLowerObstacles()
    {
        float w = Width, h = Height;
        float platY = h * 0.55f;
        float pegR = 0.18f;
        float[] pegXs = { w * 0.20f, w * 0.35f, w * 0.50f, w * 0.65f, w * 0.80f };
        for (int row = 0; row < 3; row++)
        {
            float py = platY - 1.0f - row * 1.2f;
            float rowOffset = (row == 1) ? 0.45f : 0f; // middle row centered between top/bottom
            for (int i = 0; i < pegXs.Length; i++)
            {
                float px = pegXs[i] + rowOffset;
                if (px > w - 0.5f) px -= w * 0.6f;
                var body = new Body { BodyType = BodyType.Static, Position = new AEVector2(px, py) };
                var shape = new CircleShape(pegR, 1f);
                var fixture = body.CreateFixture(shape);
                fixture.Restitution = 1.0f;
                fixture.Friction = 0f;
                World.Add(body);
                _lowerObs.Add((new AEVector2(px, py), pegR));
            }
        }
    }

    // ── Bottom Bars: Shotgun | BigBall | Shield, full width, no gap ──

    private void CreateBottomBars()
    {
        float w = Width;
        float barH = 0.5f;
        float barY = barH / 2f;
        float unit = w / 8f; // ratio 3:2:3
        float sw = unit * 3, bw = unit * 2, hw = unit * 3;

        var sz = new ConversionZone(_nextZoneId++, new AEVector2(sw / 2, barY), World, sw, barH,
            ZoneShape.Rectangle, ConversionType.Shotgun);
        _conversionZones.Add(sz);
        SetupZoneContact(sz);

        var bz = new ConversionZone(_nextZoneId++, new AEVector2(sw + bw / 2, barY), World, bw, barH,
            ZoneShape.Rectangle, ConversionType.BigBall);
        _conversionZones.Add(bz);
        SetupZoneContact(bz);

        var hz = new ConversionZone(_nextZoneId++, new AEVector2(w - hw / 2, barY), World, hw, barH,
            ZoneShape.Rectangle, ConversionType.Shield);
        _conversionZones.Add(hz);
        SetupZoneContact(hz);
    }

    // ── Balls ─────────────────────────────────────────────────

    private void CreateBalls()
    {
        float r = _config.BallRadiusMeters;
        float d = _config.BallDensity;
        float speed = _config.BallInitialSpeed;

        int totalBalls = _config.FactionCount * _config.BallsPerArena;
        for (int i = 0; i < totalBalls; i++)
        {
            int factionId = i % _config.FactionCount;
            var pos = GetSpawnPos();
            var ball = new Ball(_nextBallId++, factionId, World, pos, r, d);
            ball.ApplyInitialVelocity(speed, _rng);
            _balls.Add(ball);
        }
    }

    private AEVector2 GetSpawnPos()
    {
        float w = Width, h = Height;
        float sx = w / 2 + (float)(_rng.NextDouble() * 2.5f - 1.25f);
        float sy = h - 1.2f;
        sx = Math.Clamp(sx, 1.2f, w - 1.2f);
        return new AEVector2(sx, sy);
    }

    // ── Zone Contacts ─────────────────────────────────────────

    private void SetupZoneContact(Zone zone)
    {
        zone.SensorFixture.OnCollision += (self, other, contact) =>
        {
            var ball = other.Body.Tag as Ball;
            if (ball == null || ball.State != BallState.Active) return true;
            zone.OnBallEnter(ball);
            if (ball.State == BallState.Converting && zone is ConversionZone cz)
                NotifyBallConverted(ball, cz.ConversionType);
            else if (zone is MultiplierZone)
            {
                // Queue ball to return to spawn after multiply
                if (!_multiplierReturnQueue.Contains(ball))
                    _multiplierReturnQueue.Add(ball);
            }
            return true;
        };
    }

    // ── Step ──────────────────────────────────────────────────

    public void Step(float dt)
    {
        World.Step(dt);

        // Clamp ball speeds + airflow + value-drop
        float maxSpeed = _config.BallInitialSpeed * 2f;
        float holeX = Width / 2f;
        float holeY = Height * 0.55f;
        float holeRadius = 1.5f; // airflow zone radius

        foreach (var ball in _balls)
        {
            if (ball.State != BallState.Active) continue;

            var vel = ball.PhysicsBody.LinearVelocity;
            float spd = vel.Length();
            if (spd > maxSpeed)
                ball.PhysicsBody.LinearVelocity = vel / spd * maxSpeed;

            var pos = ball.PhysicsBody.Position;

            // Airflow at hole: high-value balls get pushed upward
            float dx = pos.X - holeX;
            float dy = pos.Y - holeY;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist < holeRadius)
            {
                // Strong airflow: small balls pushed up, big balls pass through
                float strength = 1f - Math.Min(1f, ball.CurrentValue / (float)_config.BallValueDropThreshold);
                float force = _config.AirflowStrength * strength * (1f - dist / holeRadius);
                ball.PhysicsBody.LinearVelocity += new AEVector2(0, force * dt);
            }

            // Value overflow: only trigger when ball is ABOVE the platform
            if (ball.CurrentValue > _config.BallValueDropThreshold
                && pos.Y > holeY + 0.3f)
            {
                _multiplierReturnQueue.Remove(ball);
                if (!_overflowQueue.Contains(ball))
                    _overflowQueue.Add(ball);
            }
        }

        // Process multiplier return queue — teleport balls back to spawn
        foreach (var ball in _multiplierReturnQueue)
        {
            if (ball.State == BallState.Active)
            {
                ball.PhysicsBody.SetTransform(GetSpawnPos(), 0f);
                ball.PhysicsBody.LinearVelocity = AEVector2.Zero;
                ball.ApplyInitialVelocity(_config.BallInitialSpeed, _rng);
                ball.StuckTimer = 0f;
                ball.LastCheckPos = ball.PhysicsBody.Position;
            }
        }
        _multiplierReturnQueue.Clear();

        // Process overflow queue — drop balls through center hole
        foreach (var ball in _overflowQueue)
        {
            if (ball.State == BallState.Active)
            {
                float cx = Width / 2f;
                ball.CurrentValue = _config.BallValueDropThreshold;
                ball.PhysicsBody.SetTransform(new AEVector2(cx, holeY - 1.2f), 0f);
                ball.PhysicsBody.LinearVelocity = new AEVector2(0, -_config.BallInitialSpeed * 4f);
                ball.StuckTimer = 0f;
                ball.LastCheckPos = ball.PhysicsBody.Position;
            }
        }
        _overflowQueue.Clear();

        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            var ball = _balls[i];
            if (ball.State == BallState.Converting)
            {
                ball.Deactivate();
                _deadBalls.Add(ball);
                ball.RespawnTimer = _config.RespawnDelaySeconds;
            }
        }

        // Stuck / out-of-bounds detection
        float stuckDistThresh = 0.3f;
        float stuckTimeThresh = 3f;

        foreach (var ball in _balls)
        {
            if (ball.State != BallState.Active) continue;

            var pos = ball.PhysicsBody.Position;
            float moved = (pos - ball.LastCheckPos).Length();
            if (moved < stuckDistThresh)
            {
                ball.StuckTimer += dt;
                if (ball.StuckTimer > stuckTimeThresh)
                {
                    RespawnBall(ball);
                    continue;
                }
            }
            else
            {
                ball.StuckTimer = 0f;
                ball.LastCheckPos = pos;
            }

            // Out of bounds check
            if (pos.X < -3f || pos.X > Width + 3f || pos.Y < -3f || pos.Y > Height + 3f)
                RespawnBall(ball);
        }

        HandleRespawns(dt);
    }

    private void RespawnBall(Ball ball)
    {
        ball.Deactivate();
        ball.RespawnTimer = _config.RespawnDelaySeconds * 0.3f;
        _deadBalls.Add(ball);
    }

    private void HandleRespawns(float dt)
    {
        float speed = _config.BallInitialSpeed;
        for (int i = _deadBalls.Count - 1; i >= 0; i--)
        {
            var ball = _deadBalls[i];
            ball.RespawnTimer -= dt;
            if (ball.RespawnTimer <= 0f)
            {
                ball.Respawn(GetSpawnPos(), speed, _rng);
                _deadBalls.RemoveAt(i);
            }
        }
    }

    internal void NotifyBallConverted(Ball ball, ConversionType type)
    {
        BallConverted?.Invoke(new BallConvertedEvent(ball.FactionId, type, ball.CurrentValue));
    }
}
