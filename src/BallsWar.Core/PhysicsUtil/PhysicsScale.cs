using System.Numerics;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.PhysicsUtil;

public static class PhysicsScale
{
    public const float PixelsPerMeter = 50f;

    public static float ToPixels(float meters, float scale = PixelsPerMeter) => meters * scale;
    public static float ToMeters(float pixels, float scale = PixelsPerMeter) => pixels / scale;

    public static Vector2 ToSysVector(AEVector2 v) => new(v.X, v.Y);
    public static AEVector2 ToAeVector(Vector2 v) => new(v.X, v.Y);
}
