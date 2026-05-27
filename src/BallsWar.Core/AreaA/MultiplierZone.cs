using nkast.Aether.Physics2D.Dynamics;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.AreaA;

public class MultiplierZone : Zone
{
    public int Multiplier { get; }

    public MultiplierZone(int id, AEVector2 position, World world, float radius, int multiplier)
        : base(id, position, world, radius, ZoneShape.Circle)
    {
        Multiplier = multiplier;
    }

    public MultiplierZone(int id, AEVector2 position, World world, float width, float height,
                           ZoneShape shape, int multiplier)
        : base(id, position, world, width, height, shape)
    {
        Multiplier = multiplier;
    }

    public override void OnBallEnter(Ball ball)
    {
        ball.CurrentValue *= (long)Multiplier;
    }
}
