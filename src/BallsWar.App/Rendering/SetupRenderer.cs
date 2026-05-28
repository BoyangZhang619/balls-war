using System.Text.RegularExpressions;
using Raylib_cs;
using BallsWar.Game;

namespace BallsWar.App.Rendering;

public partial class SetupRenderer
{
    private readonly GameConfig _config;
    private int _selectedIndex;
    private bool _editing;
    private string _editBuffer = "";
    private readonly string[] _labels;
    private readonly Action[] _increaseActions;
    private readonly Action[] _decreaseActions;
    private readonly Func<string>[] _valueGetters;
    private readonly string[] _validationPatterns;
    private readonly int _itemCount;
    public bool StartRequested { get; private set; }

    public SetupRenderer(GameConfig config)
    {
        _config = config;
        _itemCount = 10;
        _labels = new string[_itemCount];
        _increaseActions = new Action[_itemCount];
        _decreaseActions = new Action[_itemCount];
        _valueGetters = new Func<string>[_itemCount];
        _validationPatterns = new string[_itemCount];

        var gridSizes = new[] { 200, 300, 400, 500, 750, 1000, 1500, 2000 };
        int gsIdx = 3; // default 500
        int i = 0;

        AddRow(i++, "Factions", () => { if (_config.FactionCount < 8) _config.FactionCount++; },
            () => { if (_config.FactionCount > 2) _config.FactionCount--; },
            () => $"{_config.FactionCount}", @"^[2-8]$");

        AddRow(i++, "Balls per Arena", () => { if (_config.BallsPerArena < 12) _config.BallsPerArena++; },
            () => { if (_config.BallsPerArena > 1) _config.BallsPerArena--; },
            () => $"{_config.BallsPerArena}", @"^[1-9]$|^1[0-2]$");

        AddRow(i++, "Value Cap (drop)", () => { _config.BallValueDropThreshold *= 2; },
            () => { if (_config.BallValueDropThreshold > 64) _config.BallValueDropThreshold /= 2; },
            () => FormatThreshold(_config.BallValueDropThreshold), @"^[1-9]\d*$");

        AddRow(i++, "Grid Size",
            () => { gsIdx = (gsIdx + 1) % gridSizes.Length; _config.GridWidth = gridSizes[gsIdx]; _config.GridHeight = gridSizes[gsIdx]; },
            () => { gsIdx = (gsIdx - 1 + gridSizes.Length) % gridSizes.Length; _config.GridWidth = gridSizes[gsIdx]; _config.GridHeight = gridSizes[gsIdx]; },
            () => $"{_config.GridWidth}x{_config.GridHeight}", @"^[2-5]00$");

        AddRow(i++, "Starting Health", () => { if (_config.StartingCampHealth < 200) _config.StartingCampHealth += 10; },
            () => { if (_config.StartingCampHealth > 10) _config.StartingCampHealth -= 10; },
            () => $"{_config.StartingCampHealth}", @"^[1-9]\d*$");

        AddRow(i++, "Shotgun Pellets", () => { if (_config.PelletsPerShotgun < 80) _config.PelletsPerShotgun += 4; },
            () => { if (_config.PelletsPerShotgun > 4) _config.PelletsPerShotgun -= 4; },
            () => $"{_config.PelletsPerShotgun}", @"^[1-9]\d*$");

        AddRow(i++, "Spread Angle", () => { _config.ShotgunSpreadDegrees = Math.Min(90f, _config.ShotgunSpreadDegrees * 2f); },
            () => { _config.ShotgunSpreadDegrees = Math.Max(0.1f, _config.ShotgunSpreadDegrees / 2f); },
            () => $"{_config.ShotgunSpreadDegrees:F1} deg", @"^[0-9]*\.?[0-9]*$");

        AddRow(i++, "Pellet Speed", () => { if (_config.PelletSpeed < 100) _config.PelletSpeed += 5; },
            () => { if (_config.PelletSpeed > 5) _config.PelletSpeed -= 5; },
            () => $"{_config.PelletSpeed} c/s", @"^[1-9]\d*$");

        AddRow(i++, "Bounces", () => { if (_config.PelletBounces < 8) _config.PelletBounces++; },
            () => { if (_config.PelletBounces > 0) _config.PelletBounces--; },
            () => $"{_config.PelletBounces}", @"^[0-8]$");

        AddRow(i++, "Firing Rotation", () => { _config.CampFiringAngularSpeed += 0.2f; },
            () => { if (_config.CampFiringAngularSpeed > 0.1f) _config.CampFiringAngularSpeed -= 0.2f; },
            () => $"{_config.CampFiringAngularSpeed:F1} rad/s", @"^[0-9]+\.?[0-9]*$");
    }

    private void AddRow(int idx, string label, Action inc, Action dec, Func<string> getter, string pattern)
    {
        _labels[idx] = label;
        _increaseActions[idx] = inc;
        _decreaseActions[idx] = dec;
        _valueGetters[idx] = getter;
        _validationPatterns[idx] = pattern;
    }

    public void HandleInput()
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        var mouse = Raylib.GetMousePosition();
        int startY = 150, lineH = 38, centerX = sw / 2;

        // Process text editing
        if (_editing)
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if (key >= 32 && key <= 126 && _editBuffer.Length < 20)
                    _editBuffer += (char)key;
                key = Raylib.GetCharPressed();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && _editBuffer.Length > 0)
                _editBuffer = _editBuffer[..^1];
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter))
            {
                if (Regex.IsMatch(_editBuffer, _validationPatterns[_selectedIndex]))
                    ApplyEditValue(_editBuffer);
                else
                    _editBuffer = StripNonDigits(_editBuffer);
                _editing = false;
                _editBuffer = "";
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                _editing = false;
                _editBuffer = "";
            }
            return;
        }

        // Keyboard nav
        if (Raylib.IsKeyPressed(KeyboardKey.Up)) _selectedIndex = (_selectedIndex - 1 + _itemCount) % _itemCount;
        if (Raylib.IsKeyPressed(KeyboardKey.Down)) _selectedIndex = (_selectedIndex + 1) % _itemCount;
        if (Raylib.IsKeyPressed(KeyboardKey.Right)) _increaseActions[_selectedIndex]();
        if (Raylib.IsKeyPressed(KeyboardKey.Left)) _decreaseActions[_selectedIndex]();
        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) StartRequested = true;
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) System.Environment.Exit(0);
        if (Raylib.IsKeyPressed(KeyboardKey.Space)) StartRequested = true;

        // Check each row for click
        for (int i = 0; i < _itemCount; i++)
        {
            int y = startY + i * lineH;
            var row = new Rectangle(centerX - 280, y, 560, lineH - 2);
            if (!Raylib.CheckCollisionPointRec(mouse, row)) continue;

            _selectedIndex = i;

            // Minus button (left part of row, around 40px wide)
            var minusBtn = new Rectangle(row.X + 220, row.Y + 4, 30, row.Height - 8);
            if (Raylib.CheckCollisionPointRec(mouse, minusBtn) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                _decreaseActions[i]();

            // Plus button (right side of minus)
            var plusBtn = new Rectangle(row.X + 280, row.Y + 4, 30, row.Height - 8);
            if (Raylib.CheckCollisionPointRec(mouse, plusBtn) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                _increaseActions[i]();

            // Value area click → enter edit mode
            var valueArea = new Rectangle(row.X + 310, row.Y + 4, 80, row.Height - 8);
            if (Raylib.CheckCollisionPointRec(mouse, valueArea) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                _editing = true;
                _editBuffer = StripNonDigits(_valueGetters[i]());
            }
        }

        // Buttons
        int btnW = 180, btnH = 44, btnY = startY + _itemCount * lineH + 30;
        var startBtn = new Rectangle(centerX - 200, btnY, btnW, btnH);
        var quitBtn = new Rectangle(centerX + 20, btnY, btnW, btnH);
        if (Raylib.CheckCollisionPointRec(mouse, startBtn) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            StartRequested = true;
        if (Raylib.CheckCollisionPointRec(mouse, quitBtn) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            System.Environment.Exit(0);
    }

    public void Render()
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        var mouse = Raylib.GetMousePosition();
        Raylib.ClearBackground(new Color(12, 12, 22, 255));

        string title = "BALLS WAR";
        int titleFs = 52;
        int titleW = Raylib.MeasureText(title, titleFs);
        Raylib.DrawText(title, sw / 2 - titleW / 2, 30, titleFs, Color.Gold);

        string subtitle = "Click a value to type, or use arrow keys";
        int subFs = 16;
        int subW = Raylib.MeasureText(subtitle, subFs);
        Raylib.DrawText(subtitle, sw / 2 - subW / 2, 90, subFs, Color.Gray);

        int startY = 150, lineH = 38, centerX = sw / 2;

        for (int i = 0; i < _itemCount; i++)
        {
            int y = startY + i * lineH;
            var row = new Rectangle(centerX - 280, y, 560, lineH - 2);
            bool hover = Raylib.CheckCollisionPointRec(mouse, row);
            bool sel = i == _selectedIndex;
            var bg = sel ? new Color(50, 50, 70, 220) : hover ? new Color(35, 35, 50, 200) : new Color(25, 25, 35, 200);
            Raylib.DrawRectangleRec(row, bg);
            if (sel && !_editing) Raylib.DrawRectangleLinesEx(row, 1, Color.Gold);

            // Label
            Raylib.DrawText(_labels[i], centerX - 270, y + 9, 18, sel ? Color.Yellow : Color.White);

            // Minus button
            var minusBtn = new Rectangle(row.X + 220, row.Y + 4, 28, row.Height - 8);
            bool mHover = Raylib.CheckCollisionPointRec(mouse, minusBtn);
            var mColor = mHover ? new Color(200, 60, 60, 255) : new Color(120, 40, 40, 255);
            Raylib.DrawRectangleRec(minusBtn, mColor);
            Raylib.DrawText("-", (int)minusBtn.X + 10, (int)minusBtn.Y + 2, 18, Color.White);

            // Plus button
            var plusBtn = new Rectangle(row.X + 280, row.Y + 4, 28, row.Height - 8);
            bool pHover = Raylib.CheckCollisionPointRec(mouse, plusBtn);
            var pColor = pHover ? new Color(60, 200, 60, 255) : new Color(40, 120, 40, 255);
            Raylib.DrawRectangleRec(plusBtn, pColor);
            Raylib.DrawText("+", (int)plusBtn.X + 8, (int)plusBtn.Y + 2, 18, Color.White);

            // Value display / edit
            var valueArea = new Rectangle(row.X + 315, row.Y + 4, 90, row.Height - 8);
            if (_editing && sel)
            {
                Raylib.DrawRectangleRec(valueArea, new Color(60, 60, 80, 255));
                Raylib.DrawRectangleLinesEx(valueArea, 1, Color.Yellow);
                string disp = _editBuffer + ((int)(Raylib.GetTime() * 2) % 2 == 0 ? "_" : "");
                Raylib.DrawText(disp, (int)valueArea.X + 4, (int)valueArea.Y + 9, 18, Color.Yellow);
            }
            else
            {
                var vArea = new Rectangle(row.X + 315, row.Y + 4, 90, row.Height - 8);
                bool vHover = Raylib.CheckCollisionPointRec(mouse, vArea);
                Raylib.DrawRectangleRec(vArea, vHover ? new Color(45, 45, 60, 255) : new Color(35, 35, 45, 255));
                Raylib.DrawText(_valueGetters[i](), (int)valueArea.X + 4, (int)valueArea.Y + 9, 18, new Color(0, 200, 200, 255));
            }
        }

        // Bottom buttons
        int btnW = 180, btnH = 44, btnY = startY + _itemCount * lineH + 30;
        DrawButton(new Rectangle(centerX - 200, btnY, btnW, btnH), "START", new Color(30, 120, 30, 255), mouse);
        DrawButton(new Rectangle(centerX + 20, btnY, btnW, btnH), "QUIT", new Color(120, 30, 30, 255), mouse);

        // Faction color preview
        int prevX = sw / 2 + 320, prevY = startY;
        Raylib.DrawText("Colors:", prevX, prevY - 20, 14, Color.Gray);
        for (int j = 0; j < _config.FactionCount; j++)
        {
            var c = ColorMap.GetFactionColor(j);
            int cy = prevY + j * 20;
            Raylib.DrawRectangle(prevX, cy, 14, 14, c);
            Raylib.DrawText($"F{j + 1}", prevX + 20, cy - 1, 14, c);
        }

        string hint = "[Enter/Space] Start  [ESC] Quit  [Arrows] Navigate  Click value to edit";
        int hW = Raylib.MeasureText(hint, 16);
        Raylib.DrawText(hint, sw / 2 - hW / 2, sh - 40, 16, Color.DarkGray);
    }

    private static void DrawButton(Rectangle r, string text, Color baseColor, System.Numerics.Vector2 mouse)
    {
        bool hover = Raylib.CheckCollisionPointRec(mouse, r);
        Color c;
        if (hover)
            c = new Color((byte)Math.Min(255, baseColor.R + 40), (byte)Math.Min(255, baseColor.G + 40), (byte)Math.Min(255, baseColor.B + 40), (byte)255);
        else
            c = baseColor;
        Raylib.DrawRectangleRec(r, c);
        Raylib.DrawRectangleLinesEx(r, 2, Color.White);
        int fs = 22, tw = Raylib.MeasureText(text, fs);
        Raylib.DrawText(text, (int)(r.X + r.Width / 2 - tw / 2), (int)(r.Y + 10), fs, Color.White);
    }

    private static string FormatThreshold(long v) => v >= 1_000_000 ? $"{v / 1_000_000}M" : v >= 1_000 ? $"{v / 1_000}k" : v.ToString();

    private void ApplyEditValue(string val)
    {
        int idx = _selectedIndex;
        switch (idx)
        {
            case 0: if (int.TryParse(val, out int fc) && fc >= 2 && fc <= 8) _config.FactionCount = fc; break;
            case 1: if (int.TryParse(val, out int ba) && ba >= 1 && ba <= 12) _config.BallsPerArena = ba; break;
            case 2: if (long.TryParse(val, out long vc) && vc > 0) _config.BallValueDropThreshold = vc; break;
            case 3: if (int.TryParse(val, out int gs) && gs >= 50 && gs <= 500) { _config.GridWidth = gs; _config.GridHeight = gs; } break;
            case 4: if (int.TryParse(val, out int hp) && hp > 0) _config.StartingCampHealth = hp; break;
            case 5: if (int.TryParse(val, out int pp) && pp > 0) _config.PelletsPerShotgun = pp; break;
            case 6: if (float.TryParse(val, out float sa) && sa > 0 && sa <= 90) _config.ShotgunSpreadDegrees = sa; break;
            case 7: if (int.TryParse(val, out int ps) && ps > 0) _config.PelletSpeed = ps; break;
            case 8: if (int.TryParse(val, out int b) && b >= 0 && b <= 8) _config.PelletBounces = b; break;
            case 9: if (float.TryParse(val, out float fr) && fr > 0) _config.CampFiringAngularSpeed = fr; break;
        }
    }

    private static string StripNonDigits(string s)
    {
        var digits = s.Where(c => char.IsDigit(c) || c == '.').ToArray();
        return new string(digits);
    }
}
