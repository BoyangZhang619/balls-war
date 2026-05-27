using Raylib_cs;

namespace BallsWar.App.Rendering;

public class LayoutManager
{
    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }
    public Rectangle TopBarRect { get; private set; }
    public Rectangle BottomBarRect { get; private set; }
    public Rectangle ArenaRect { get; private set; }
    public Rectangle BattleGridRect { get; private set; }
    public float CellPixelSize { get; private set; }

    private readonly int _gridW;
    private readonly int _gridH;

    public LayoutManager(int gridW, int gridH, int width, int height)
    {
        _gridW = gridW;
        _gridH = gridH;
        Recalculate(width, height);
    }

    public void Recalculate(int width, int height)
    {
        ScreenWidth = width;
        ScreenHeight = height;
        int topH = 40;
        int bottomH = 30;
        int margin = 8;

        TopBarRect = new Rectangle(0, 0, width, topH);
        BottomBarRect = new Rectangle(0, height - bottomH, width, bottomH);

        // Left half: Area A (pinball arena)
        float splitX = width / 2;
        float arenaH = height - topH - bottomH - margin * 2;
        ArenaRect = new Rectangle(margin, topH + margin, splitX - margin * 2, arenaH);

        // Right half: Area B (battle grid)
        float gridX = splitX + margin;
        float gridY = topH + margin;
        float gridMaxW = width - gridX - margin;
        float gridMaxH = height - topH - bottomH - margin * 2;
        CellPixelSize = Math.Min(gridMaxW / _gridW, gridMaxH / _gridH);
        float gridPixelW = CellPixelSize * _gridW;
        float gridPixelH = CellPixelSize * _gridH;
        BattleGridRect = new Rectangle(gridX + (gridMaxW - gridPixelW) / 2,
                                        gridY + (gridMaxH - gridPixelH) / 2,
                                        gridPixelW, gridPixelH);
    }
}
