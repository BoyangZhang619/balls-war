using Raylib_cs;
using BallsWar.Game;
using BallsWar.App.Rendering;
using BallsWar.App.Input;

namespace BallsWar.App;

public enum AppPage { Home, Settings, Game }

public class GameWindow
{
    private readonly GameLoop _gameLoop;
    private AppPage _page = AppPage.Home;
    private HomeRenderer _home = null!;
    private SetupRenderer _setup = null!;
    private LayoutManager _layout = null!;
    private AreaARenderer _arenaRenderer = null!;
    private AreaBRenderer _gridRenderer = null!;
    private UiRenderer _ui = null!;
    private InputHandler _input = null!;
    private int _width = 1600;
    private int _height = 900;

    public GameWindow(GameLoop gameLoop) => _gameLoop = gameLoop;

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(800, 450, "Balls War");
        int monitorW = Raylib.GetMonitorWidth(0);
        _width = monitorW / 2;
        _height = _width * 9 / 16;
        Raylib.SetWindowSize(_width, _height);
        Raylib.SetWindowMinSize(800, 450);
        Raylib.SetTargetFPS(0);
        Raylib.SetExitKey(KeyboardKey.Null);

        FontManager.Load();
        Strings.Current = _gameLoop.Config.LanguageIndex == 1 ? Language.English : Language.Chinese;

        _home = new HomeRenderer();
        _setup = new SetupRenderer(_gameLoop.Config);
        _arenaRenderer = new AreaARenderer();
        _gridRenderer = new AreaBRenderer();
        _ui = new UiRenderer();
        _input = new InputHandler(_gameLoop);

        while (!Raylib.WindowShouldClose())
        {
            int w = Raylib.GetScreenWidth(), h = Raylib.GetScreenHeight();
            if (w != _width) { _width = w; _height = w * 9 / 16; Raylib.SetWindowSize(_width, _height); }

            switch (_page)
            {
                case AppPage.Home:
                    _home.HandleInput();
                    Raylib.BeginDrawing();
                    _home.Render();
                    Raylib.EndDrawing();
                    if (_home.Action == HomeAction.Start) { _page = AppPage.Game; StartGame(); }
                    if (_home.Action == HomeAction.Settings) _page = AppPage.Settings;
                    if (_home.Action == HomeAction.Quit) { Raylib.CloseWindow(); return; }
                    break;

                case AppPage.Settings:
                    _setup.HandleInput();
                    Raylib.BeginDrawing();
                    _setup.Render();
                    Raylib.EndDrawing();
                    if (_setup.BackRequested) { _page = AppPage.Home; _setup = new SetupRenderer(_gameLoop.Config); }
                    if (_setup.StartRequested) { _page = AppPage.Game; ConfigStorage.Save(_gameLoop.Config); StartGame(); _setup = new SetupRenderer(_gameLoop.Config); }
                    break;

                case AppPage.Game:
                    float dt = Math.Min(Raylib.GetFrameTime(), 0.1f);
                    _layout?.Recalculate(_width, _height);
                    _input.Process();
                    _gameLoop.Update(dt);

                    if (_input.BackToSetupRequested)
                    {
                        _gameLoop.State.GoToSetup();
                        _gridRenderer.Unload();
                        _input.BackToSetupRequested = false;
                        _page = AppPage.Home;
                        continue;
                    }

                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(new Color(20, 20, 30, 255));
                    if (_layout != null)
                    {
                        _arenaRenderer.Render(_gameLoop.Arena, _layout.ArenaRect);
                        _gridRenderer.Render(_gameLoop.Grid, _layout.BattleGridRect);
                        var counts = _gameLoop.Grid.PelletManager.PelletCountsByFaction;
                        _ui.RenderTopBar(_gameLoop.State, _gameLoop.Speed, _gameLoop.Factions, counts, _layout.TopBarRect);
                        _ui.RenderBottomBar(_layout.BottomBarRect);
                    }

                    // Pause overlay
                    if (_gameLoop.State.Phase == GamePhase.Paused)
                        RenderPauseOverlay();

                    if (_gameLoop.State.Phase == GamePhase.Finished)
                    {
                        _ui.RenderGameOver(_gameLoop.State, _gameLoop.Factions);
                        if (Raylib.IsKeyPressed(KeyboardKey.R))
                        {
                            _gameLoop.Reset(); _gridRenderer.Unload();
                            _page = AppPage.Home;
                        }
                    }
                    Raylib.EndDrawing();
                    break;
            }
        }

        _gridRenderer?.Unload();
        Raylib.CloseWindow();
    }

    private void StartGame()
    {
        _gameLoop.Initialize();
        _layout = new LayoutManager(_gameLoop.Config.GridWidth, _gameLoop.Config.GridHeight, _width, _height);
        _gridRenderer.Initialize(_gameLoop.Config.GridWidth, _gameLoop.Config.GridHeight);
    }

    private void RenderPauseOverlay()
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 180));
        string title = "PAUSED";
        int fs = 48, tw = Raylib.MeasureText(title, fs);
        Raylib.DrawText(title, sw / 2 - tw / 2, sh / 2 - 100, fs, Color.White);

        string h1 = "[Space] Continue";
        string h2 = "[ESC] Return to Home (no save)";
        int sFs = 22;
        int w1 = Raylib.MeasureText(h1, sFs), w2 = Raylib.MeasureText(h2, sFs);
        Raylib.DrawText(h1, sw / 2 - w1 / 2, sh / 2 - 20, sFs, Color.White);
        Raylib.DrawText(h2, sw / 2 - w2 / 2, sh / 2 + 20, sFs, Color.Gray);
    }
}
