namespace BallsWar.Game;

public enum GamePhase { Setup, Running, Paused, Finished }

public class GameState
{
    public GamePhase Phase { get; private set; } = GamePhase.Setup;
    public int ActiveFactionCount { get; private set; }
    public int? WinnerFactionId { get; set; }
    public float ElapsedTime { get; private set; }

    public void Start(int factionCount)
    {
        Phase = GamePhase.Running;
        ActiveFactionCount = factionCount;
        WinnerFactionId = null;
        ElapsedTime = 0f;
    }

    public void Pause() { if (Phase == GamePhase.Running) Phase = GamePhase.Paused; }
    public void Resume() { if (Phase == GamePhase.Paused) Phase = GamePhase.Running; }
    public void TogglePause() { if (Phase == GamePhase.Running) Pause(); else if (Phase == GamePhase.Paused) Resume(); }
    public void GoToSetup() { Phase = GamePhase.Setup; }

    public void OnFactionEliminated(int factionId)
    {
        ActiveFactionCount--;
        if (ActiveFactionCount <= 1 && Phase == GamePhase.Running)
        {
            Phase = GamePhase.Finished;
            WinnerFactionId = factionId;
        }
    }

    public void AdvanceTime(float dt) { ElapsedTime += dt; }
}
