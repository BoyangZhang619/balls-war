using Raylib_cs;
using BallsWar.Game;
using BallsWar.App.Rendering;
using BallsWar.App.Input;

namespace BallsWar.App;

public class GameWindow
{
    private readonly GameLoop _gameLoop;
    private SetupRenderer _setup = null!;
    private LayoutManager _layout = null!;
    private AreaARenderer _arenaRenderer = null!;
    private AreaBRenderer _gridRenderer = null!;
    private UiRenderer _ui = null!;
    private InputHandler _input = null!;
    private int _width = 1600;
    private int _height = 900;

    public GameWindow(GameLoop gameLoop)
    {
        _gameLoop = gameLoop;
    }

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(_width, _height, "Balls War");
        Raylib.ToggleFullscreen();
        Raylib.SetTargetFPS(0);
        Raylib.SetExitKey(KeyboardKey.Null);

        _setup = new SetupRenderer(_gameLoop.Config);
        _arenaRenderer = new AreaARenderer();
        _gridRenderer = new AreaBRenderer();
        _ui = new UiRenderer();
        _input = new InputHandler(_gameLoop);

        while (!Raylib.WindowShouldClose())
        {
            int w = Raylib.GetScreenWidth();
            int h = Raylib.GetScreenHeight();
            if (w != _width || h != _height) { _width = w; _height = h; }

            if (_gameLoop.State.Phase == GamePhase.Setup)
            {
                _setup.HandleInput();

                if (_setup.StartRequested)
                {
                    _gameLoop.Initialize();
                    _layout = new LayoutManager(
                        _gameLoop.Config.GridWidth, _gameLoop.Config.GridHeight,
                        _width, _height);
                    _gridRenderer.Initialize(_gameLoop.Config.GridWidth, _gameLoop.Config.GridHeight);
                    // Reset StartRequested for next time
                    _setup = new SetupRenderer(_gameLoop.Config);
                }

                Raylib.BeginDrawing();
                _setup.Render();
                Raylib.EndDrawing();
            }
            else
            {
                float dt = Raylib.GetFrameTime();

                if (_layout != null) _layout.Recalculate(_width, _height);

                _input.Process();
                _gameLoop.Update(dt);

                if (_input.BackToSetupRequested)
                {
                    _gameLoop.State.GoToSetup();
                    _gridRenderer.Unload();
                    _setup = new SetupRenderer(_gameLoop.Config);
                    _input.BackToSetupRequested = false;
                    continue;
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(20, 20, 30, 255));

                if (_layout != null)
                {
                    _arenaRenderer.Render(_gameLoop.Arena, _layout.ArenaRect);
                    _gridRenderer.Render(_gameLoop.Grid, _layout.BattleGridRect);
                    var counts = _gameLoop.Grid.PelletManager.PelletCountsByFaction;
                    _ui.RenderTopBar(_gameLoop.State, _gameLoop.Speed,
                        _gameLoop.Factions, counts, _layout.TopBarRect);
                    _ui.RenderBottomBar(_layout.BottomBarRect);
                }

                if (_gameLoop.State.Phase == GamePhase.Finished)
                    _ui.RenderGameOver(_gameLoop.State, _gameLoop.Factions);

                Raylib.EndDrawing();
            }
        }

        _gridRenderer?.Unload();
        Raylib.CloseWindow();
    }
}
