using Raylib_cs;
using BallsWar.AreaA;
using BallsWar.Events;
using AEVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace BallsWar.App.Rendering;

public class AreaARenderer
{
    public void Render(PinballArena arena, Rectangle rect)
    {
        float aw = arena.Width, ah = arena.Height;
        float sx = rect.Width / aw;
        float sy = rect.Height / ah;

        Raylib.DrawRectangleRec(rect, new Color(15, 20, 40, 255));
        Raylib.DrawRectangleLinesEx(rect, 2f, new Color(60, 70, 100, 255));

        // Dividing platform visual (gray bars at platform Y)
        float platY = ah * 0.55f;
        float holeHalfW = 1.0f;
        float centerX = aw / 2;
        float platThick = 0.3f;

        // Left platform visual
        float leftW = centerX - holeHalfW;
        if (leftW > 0)
        {
            var lr = PhysRectToScreen(new AEVector2(leftW / 2, platY), leftW, platThick, rect, sx, sy);
            Raylib.DrawRectangleRec(lr, new Color(100, 100, 120, 255));
        }
        // Right platform visual
        float rightStart = centerX + holeHalfW;
        float rightW = aw - rightStart;
        if (rightW > 0)
        {
            var rr = PhysRectToScreen(new AEVector2(rightStart + rightW / 2, platY), rightW, platThick, rect, sx, sy);
            Raylib.DrawRectangleRec(rr, new Color(100, 100, 120, 255));
        }

        // Multiplier zones
        foreach (var zone in arena.MultiplierZones)
        {
            var mz = (MultiplierZone)zone;
            var color = ColorMap.GetMultiplierColor(mz.Multiplier);
            var r = PhysRectToScreen(zone.Position, zone.Width, zone.Height, rect, sx, sy);
            Raylib.DrawRectangleRec(r, color);
            Raylib.DrawRectangleLinesEx(r, 1, Color.White);
            string label = $"{mz.Multiplier}x";
            int fs = Math.Max(8, (int)(r.Height * 1.2f));
            int tw = Raylib.MeasureText(label, fs);
            Raylib.DrawText(label, (int)(r.X + r.Width / 2 - tw / 2),
                (int)(r.Y + r.Height / 2 - fs / 2), fs, Color.White);
        }

        // Conversion zones (Shotgun / Shield)
        foreach (var zone in arena.ConversionZones)
        {
            var cz = (ConversionZone)zone;
            var color = ColorMap.GetConversionColor(cz.ConversionType);
            string label = cz.ConversionType == ConversionType.Shotgun ? "SHOTGUN" : "SHIELD";
            var r = PhysRectToScreen(zone.Position, zone.Width, zone.Height, rect, sx, sy);
            Raylib.DrawRectangleRec(r, color);
            Raylib.DrawRectangleLinesEx(r, 2f, Color.White);
            int fs = Math.Max(10, (int)(r.Height * 0.6f));
            int tw = Raylib.MeasureText(label, fs);
            Raylib.DrawText(label, (int)(r.X + r.Width / 2 - tw / 2),
                (int)(r.Y + r.Height / 2 - fs / 2), fs, Color.White);
        }

        // Balls (physics Y-up → screen Y-down), fixed size
        float ballPR = 0.15f * (sx + sy) / 2;
        foreach (var ball in arena.Balls)
        {
            if (ball.State != BallState.Active) continue;
            float px = rect.X + ball.PhysicsBody.Position.X * sx;
            float py = rect.Y + (ah - ball.PhysicsBody.Position.Y) * sy;
            var color = ColorMap.GetFactionColor(ball.FactionId);
            Raylib.DrawCircle((int)px, (int)py, ballPR, color);
            Raylib.DrawCircleLines((int)px, (int)py, ballPR, Color.White);

            string valText = BallsWar.AreaA.Ball.FormatValue(ball.CurrentValue);
            int fontSize = Math.Max(10, (int)(ballPR * 1.5f));
            int tw = Raylib.MeasureText(valText, fontSize);
            Raylib.DrawText(valText, (int)px - tw / 2, (int)py - fontSize / 2, fontSize, Color.White);
        }
    }

    private static Rectangle PhysRectToScreen(AEVector2 physCenter, float w, float h,
                                                Rectangle rect, float sx, float sy)
    {
        float left = rect.X + (physCenter.X - w / 2) * sx;
        float top = rect.Y + rect.Height - (physCenter.Y + h / 2) * sy;
        return new Rectangle(left, top, w * sx, h * sy);
    }
}
