using nkast.Aether.Physics2D.Dynamics;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;
using BallsWar.Events;

namespace BallsWar.AreaA;

public class ConversionZone : Zone
{
    public ConversionType ConversionType { get; }

    public ConversionZone(int id, AEVector2 position, World world, float radius,
                           ConversionType conversionType)
        : base(id, position, world, radius, ZoneShape.Circle)
    {
        ConversionType = conversionType;
    }

    public ConversionZone(int id, AEVector2 position, World world, float width, float height,
                           ZoneShape shape, ConversionType conversionType)
        : base(id, position, world, width, height, shape)
    {
        ConversionType = conversionType;
    }

    public override void OnBallEnter(Ball ball)
    {
        ball.MarkForConversion();
    }
}
