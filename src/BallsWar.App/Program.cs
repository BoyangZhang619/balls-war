using BallsWar.App;
using BallsWar.Game;

var config = ConfigStorage.Load();
ConfigStorage.Save(config); // ensure file exists on first run

var gameLoop = new GameLoop(config);
var window = new GameWindow(gameLoop);
window.Run();
