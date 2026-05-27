using BallsWar.App;
using BallsWar.Game;

var config = new GameConfig
{
    GridWidth = 500,
    GridHeight = 500,
    FactionCount = 4,
    BallsPerArena = 4,
    StartingCampHealth = 50
};

var gameLoop = new GameLoop(config);
var window = new GameWindow(gameLoop);
window.Run();
