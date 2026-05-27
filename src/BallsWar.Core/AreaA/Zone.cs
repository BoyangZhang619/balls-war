using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.AreaA;

public enum ZoneShape { Circle, Rectangle, Diamond }

public abstract class Zone
{
    public int Id { get; }
    public AEVector2 Position { get; }
    public Body PhysicsBody { get; }
    public Fixture SensorFixture { get; }
    public ZoneShape Shape { get; }
    public float Radius { get; }
    public float Width { get; }
    public float Height { get; }

    protected Zone(int id, AEVector2 position, World world, float radius,
                    ZoneShape shape = ZoneShape.Circle)
    {
        Id = id;
        Position = position;
        Shape = shape;
        Radius = radius;
        Width = radius;
        Height = radius;

        PhysicsBody = new Body();
        PhysicsBody.BodyType = BodyType.Static;
        PhysicsBody.Position = position;
        PhysicsBody.Tag = this;

        var circleShape = new CircleShape(radius, 0f);
        SensorFixture = PhysicsBody.CreateFixture(circleShape);
        SensorFixture.IsSensor = true;
        world.Add(PhysicsBody);
    }

    protected Zone(int id, AEVector2 position, World world, float width, float height,
                    ZoneShape shape)
    {
        Id = id;
        Position = position;
        Shape = shape;
        Width = width;
        Height = height;
        Radius = 0f;

        PhysicsBody = new Body();
        PhysicsBody.BodyType = BodyType.Static;
        PhysicsBody.Position = position;
        PhysicsBody.Tag = this;

        var vertices = PolygonTools.CreateRectangle(width / 2, height / 2);
        var polyShape = new PolygonShape(vertices, 0f);
        SensorFixture = PhysicsBody.CreateFixture(polyShape);
        SensorFixture.IsSensor = true;
        world.Add(PhysicsBody);
    }

    public abstract void OnBallEnter(Ball ball);
}
