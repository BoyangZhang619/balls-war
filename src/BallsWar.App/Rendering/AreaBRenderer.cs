using Raylib_cs;
using BallsWar.AreaB;

namespace BallsWar.App.Rendering;

public class AreaBRenderer
{
    private Texture2D _gridTexture;
    private Color[] _buffer = [];
    private int _gridW;
    private int _gridH;
    private bool _initialized;

    public unsafe void Initialize(int width, int height)
    {
        _gridW = width;
        _gridH = height;
        _buffer = new Color[width * height];
        for (int i = 0; i < _buffer.Length; i++)
            _buffer[i] = new Color(40, 40, 40, 255);

        var image = Raylib.GenImageColor(width, height, Color.Black);
        fixed (Color* src = _buffer)
        {
            Color* dst = (Color*)image.Data;
            for (int i = 0; i < _buffer.Length; i++)
                dst[i] = src[i];
        }
        _gridTexture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        _initialized = true;
    }

    public void Render(BattleGrid grid, Rectangle screenRect)
    {
        if (!_initialized) Initialize(grid.Width, grid.Height);

        var dirty = grid.GetDirtyCellsAndClear();
        foreach (var (x, y, owner) in dirty)
            _buffer[y * _gridW + x] = ColorMap.GetCellColor(owner);

        Raylib.UpdateTexture(_gridTexture, _buffer);

        Raylib.DrawTexturePro(_gridTexture,
            new Rectangle(0, 0, _gridW, _gridH),
            screenRect,
            System.Numerics.Vector2.Zero, 0f, Color.White);

        float cs = screenRect.Width / _gridW;

        // Camp 11x11 overlay (semi-transparent territory)
        foreach (var camp in grid.Camps.Values)
        {
            if (camp.IsDestroyed) continue;
            var fc = ColorMap.GetFactionColor(camp.FactionId);
            fc.A = 60;
            float ox = screenRect.X + (camp.CenterX - 5) * cs;
            float oy = screenRect.Y + (camp.CenterY - 5) * cs;
            float os = 11 * cs;
            Raylib.DrawRectangle((int)ox, (int)oy, (int)os, (int)os, fc);
        }

        // Camps
        foreach (var camp in grid.Camps.Values)
        {
            if (camp.IsDestroyed) continue;
            float cx = screenRect.X + camp.CenterX * cs + cs / 2;
            float cy = screenRect.Y + camp.CenterY * cs + cs / 2;
            float r = 3 * cs;

            // Firing direction indicator
            float fx = cx + MathF.Cos(camp.FiringAngle) * r * 2.5f;
            float fy = cy + MathF.Sin(camp.FiringAngle) * r * 2.5f;
            Raylib.DrawLineEx(new System.Numerics.Vector2(cx, cy),
                new System.Numerics.Vector2(fx, fy), 2f, Color.White);

            string healthText = $"HP:{camp.Health}";
            int fs = Math.Max(10, (int)(cs * 3));
            Raylib.DrawText(healthText, (int)cx - fs, (int)cy - fs / 2, fs, Color.White);
        }

        // Pellets
        foreach (var p in grid.PelletManager.ActivePellets)
        {
            if (p.Dead) continue;
            float px = screenRect.X + p.X * cs + cs / 2;
            float py = screenRect.Y + p.Y * cs + cs / 2;
            var color = ColorMap.GetFactionColor(p.FactionId);
            Raylib.DrawCircle((int)px, (int)py, Math.Max(1, cs * 0.4f), color);
        }
    }

    public void Unload()
    {
        if (_initialized) { Raylib.UnloadTexture(_gridTexture); _initialized = false; }
    }
}
