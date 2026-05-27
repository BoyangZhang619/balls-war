using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Collision.Shapes;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.AreaA;

public enum BallState { Active, Converting, Respawning }

public class Ball
{
    public int Id { get; }
    public int FactionId { get; }
    public long CurrentValue { get; set; } = 1;
    public BallState State { get; set; } = BallState.Active;
    public Body PhysicsBody { get; }
    public float RespawnTimer { get; set; }
    public float StuckTimer { get; set; }
    public AEVector2 LastCheckPos { get; set; }

    public Ball(int id, int factionId, World world, AEVector2 position,
                 float radius, float density)
    {
        Id = id;
        FactionId = factionId;

        PhysicsBody = new Body();
        PhysicsBody.BodyType = BodyType.Dynamic;
        PhysicsBody.Position = position;
        PhysicsBody.LinearDamping = 0f;
        PhysicsBody.AngularDamping = 0f;
        PhysicsBody.IgnoreGravity = false;
        PhysicsBody.Tag = this;

        var shape = new CircleShape(radius, density);
        var fixture = PhysicsBody.CreateFixture(shape);
        fixture.Restitution = 1.0f;
        fixture.Friction = 0f;

        world.Add(PhysicsBody);
        LastCheckPos = position;
    }

    public void ApplyInitialVelocity(float speed, Random rng)
    {
        // Launch downward with guaranteed horizontal component
        float hMin = 0.3f;
        float hDir, vDir;
        do
        {
            float angle = (float)(rng.NextDouble() * Math.PI * 0.8f - Math.PI * 0.4f - Math.PI / 2);
            hDir = (float)Math.Cos(angle);
            vDir = (float)Math.Sin(angle);
        } while (Math.Abs(hDir) < hMin);
        PhysicsBody.LinearVelocity = new AEVector2(hDir * speed, vDir * speed);
    }

    public void MarkForConversion() => State = BallState.Converting;
    public void Deactivate() => PhysicsBody.Enabled = false;

    public void Respawn(AEVector2 position, float speed, Random rng)
    {
        State = BallState.Active;
        CurrentValue = 1;
        StuckTimer = 0f;
        LastCheckPos = position;
        PhysicsBody.SetTransform(position, 0f);
        PhysicsBody.LinearVelocity = AEVector2.Zero;
        PhysicsBody.Enabled = true;
        ApplyInitialVelocity(speed, rng);
    }

    public static string FormatValue(long v) => v switch
    {
        >= 1_000_000_000 => $"{v / 1_000_000_000}B",
        >= 1_000_000 => $"{v / 1_000_000}M",
        >= 1_000 => $"{v / 1_000}k",
        _ => v.ToString()
    };
}
