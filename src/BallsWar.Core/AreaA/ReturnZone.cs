using nkast.Aether.Physics2D.Dynamics;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.AreaA;

/// Ball touching this zone gets teleported back to the top of the arena.
public class ReturnZone : Zone
{
    public ReturnZone(int id, AEVector2 position, World world, float width, float height,
                       ZoneShape shape)
        : base(id, position, world, width, height, shape) { }

    public override void OnBallEnter(Ball ball) { } // handled inline in PinballArena
}
