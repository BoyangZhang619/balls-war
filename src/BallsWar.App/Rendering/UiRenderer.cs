using Raylib_cs;
using BallsWar.Game;
using BallsWar.Faction;

namespace BallsWar.App.Rendering;

public class UiRenderer
{
    public void RenderTopBar(GameState state, SimulationSpeed speed,
                              IReadOnlyList<Faction.Faction> factions,
                              int[] pelletCounts, Rectangle rect)
    {
        Raylib.DrawRectangleRec(rect, new Color(15, 15, 20, 255));

        int x = 10;
        string speedText = $"Speed: {speed.Current}x  ";
        Raylib.DrawText(speedText, x, (int)rect.Y + 10, 20, Color.White);
        x += Raylib.MeasureText(speedText, 20);

        string stateText = state.Phase == GamePhase.Paused ? "[PAUSED]  " : "";
        if (stateText.Length > 0)
        {
            Raylib.DrawText(stateText, x, (int)rect.Y + 10, 20, Color.Yellow);
            x += Raylib.MeasureText(stateText, 20);
        }

        for (int i = 0; i < factions.Count; i++)
        {
            var f = factions[i];
            if (f.IsEliminated) continue;
            var color = ColorMap.GetFactionColor(i);
            var camp = f.Camp;
            int pc = i < pelletCounts.Length ? pelletCounts[i] : 0;
            string text = $"{f.Name} HP:{camp.Health}  S:{pc}";
            Raylib.DrawText(text, x, (int)rect.Y + 10, 20, color);
            x += Raylib.MeasureText(text, 20) + 16;
        }
    }

    public void RenderBottomBar(Rectangle rect)
    {
        Raylib.DrawRectangleRec(rect, new Color(15, 15, 20, 255));
        string help = "[Space] Pause  [1-4] Speed  [Q/E] +/-  [ESC] Quit";
        Raylib.DrawText(help, 10, (int)rect.Y + 6, 18, Color.DarkGray);
    }

    public void RenderGameOver(GameState state, IReadOnlyList<Faction.Faction> factions)
    {
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(),
            new Color(0, 0, 0, 200));
        string title = "GAME OVER";
        int fs = 60;
        int tw = Raylib.MeasureText(title, fs);
        Raylib.DrawText(title, Raylib.GetScreenWidth() / 2 - tw / 2,
            Raylib.GetScreenHeight() / 2 - 80, fs, Color.Gold);

        if (state.WinnerFactionId.HasValue)
        {
            var winner = factions[state.WinnerFactionId.Value];
            string winText = $"{winner.Name} Wins!";
            fs = 30;
            tw = Raylib.MeasureText(winText, fs);
            var color = ColorMap.GetFactionColor(winner.Id);
            Raylib.DrawText(winText, Raylib.GetScreenWidth() / 2 - tw / 2,
                Raylib.GetScreenHeight() / 2, fs, color);
        }

        string restart = "[R] to Restart";
        fs = 20;
        tw = Raylib.MeasureText(restart, fs);
        Raylib.DrawText(restart, Raylib.GetScreenWidth() / 2 - tw / 2,
            Raylib.GetScreenHeight() / 2 + 50, fs, Color.White);
    }
}
