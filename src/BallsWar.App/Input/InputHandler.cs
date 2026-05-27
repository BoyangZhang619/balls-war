using Raylib_cs;
using BallsWar.Game;

namespace BallsWar.App.Input;

public class InputHandler
{
    private readonly GameLoop _gameLoop;
    public bool BackToSetupRequested { get; set; }

    public InputHandler(GameLoop gameLoop) => _gameLoop = gameLoop;

    public void Process()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            _gameLoop.TogglePause();

        if (Raylib.IsKeyPressed(KeyboardKey.One)) _gameLoop.Speed.SetSpeed(0.25f);
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) _gameLoop.Speed.SetSpeed(0.5f);
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) _gameLoop.Speed.SetSpeed(1f);
        if (Raylib.IsKeyPressed(KeyboardKey.Four)) _gameLoop.Speed.SetSpeed(2f);
        if (Raylib.IsKeyPressed(KeyboardKey.Five)) _gameLoop.Speed.SetSpeed(4f);
        if (Raylib.IsKeyPressed(KeyboardKey.Six)) _gameLoop.Speed.SetSpeed(8f);

        if (Raylib.IsKeyPressed(KeyboardKey.Q)) _gameLoop.DecreaseSpeed();
        if (Raylib.IsKeyPressed(KeyboardKey.E)) _gameLoop.IncreaseSpeed();

        if (Raylib.IsKeyPressed(KeyboardKey.R) &&
            _gameLoop.State.Phase == GamePhase.Finished)
        {
            _gameLoop.Reset();
            BackToSetupRequested = true;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            if (_gameLoop.State.Phase == GamePhase.Finished)
                BackToSetupRequested = true;
            else
            {
                _gameLoop.Reset();
                BackToSetupRequested = true;
            }
        }
    }
}
