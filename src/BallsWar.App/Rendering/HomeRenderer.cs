using Raylib_cs;

namespace BallsWar.App.Rendering;

public enum HomeAction { None, Start, Settings, Quit }

public class HomeRenderer
{
    public HomeAction Action { get; private set; }

    public void HandleInput()
    {
        Action = HomeAction.None;
        var mouse = Raylib.GetMousePosition();
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        int centerX = sw / 2;

        int btnW = 260, btnH = 50, gap = 16;
        int startBtnY = sh / 2 - btnH - gap;
        int settingsBtnY = sh / 2;
        int quitBtnY = sh / 2 + btnH + gap;

        var startBtn = new Rectangle(centerX - btnW / 2, startBtnY, btnW, btnH);
        var settingsBtn = new Rectangle(centerX - btnW / 2, settingsBtnY, btnW, btnH);
        var quitBtn = new Rectangle(centerX - btnW / 2, quitBtnY, btnW, btnH);

        if (CheckClick(startBtn, mouse)) Action = HomeAction.Start;
        if (CheckClick(settingsBtn, mouse)) Action = HomeAction.Settings;
        if (CheckClick(quitBtn, mouse)) Action = HomeAction.Quit;

        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) Action = HomeAction.Start;
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) Action = HomeAction.Quit;
    }

    public void Render()
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        var mouse = Raylib.GetMousePosition();
        int centerX = sw / 2;

        Raylib.ClearBackground(new Color(10, 10, 22, 255));

        string title = Strings.Get("title");
        int tFs = 68;
        int tW = FontManager.MeasureStr(title, tFs);
        FontManager.DrawStr(title, centerX - tW / 2, sh / 4 - 30, tFs, Color.Gold);

        string subtitle = Strings.Get("subtitle");
        int sFs = 18;
        int sW = FontManager.MeasureStr(subtitle, sFs);
        FontManager.DrawStr(subtitle, centerX - sW / 2, sh / 4 + 46, sFs, Color.Gray);

        int btnW = 260, btnH = 50, gap = 16;
        DrawMenuButton(new Rectangle(centerX - btnW / 2, sh / 2 - btnH - gap, btnW, btnH),
            Strings.Get("start_game"), new Color(30, 120, 30, 255), mouse);
        DrawMenuButton(new Rectangle(centerX - btnW / 2, sh / 2, btnW, btnH),
            Strings.Get("settings"), new Color(40, 80, 140, 255), mouse);
        DrawMenuButton(new Rectangle(centerX - btnW / 2, sh / 2 + btnH + gap, btnW, btnH),
            Strings.Get("quit"), new Color(120, 40, 40, 255), mouse);

        string hint = Strings.Current == Language.Chinese ? "[Enter]开始  [ESC]退出" : "[Enter] Start   [ESC] Quit";
        int hW = FontManager.MeasureStr(hint, 16);
        FontManager.DrawStr(hint, centerX - hW / 2, sh - 50, 16, Color.DarkGray);
    }

    private static void DrawMenuButton(Rectangle r, string text, Color baseColor, System.Numerics.Vector2 mouse)
    {
        bool hover = Raylib.CheckCollisionPointRec(mouse, r);
        Color c = hover ? new Color((byte)Math.Min(255, baseColor.R + 50), (byte)Math.Min(255, baseColor.G + 50), (byte)Math.Min(255, baseColor.B + 50), (byte)255) : baseColor;
        Raylib.DrawRectangleRec(r, c);
        Raylib.DrawRectangleLinesEx(r, 2, Color.White);
        int fs = 22, tw = FontManager.MeasureStr(text, fs);
        FontManager.DrawStr(text, (int)(r.X + r.Width / 2 - tw / 2), (int)(r.Y + 12), fs, Color.White);
    }

    private static bool CheckClick(Rectangle r, System.Numerics.Vector2 mouse)
        => Raylib.CheckCollisionPointRec(mouse, r) && Raylib.IsMouseButtonPressed(MouseButton.Left);
}
