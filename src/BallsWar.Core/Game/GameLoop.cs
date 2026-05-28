using BallsWar.AreaA;
using BallsWar.AreaB;
using BallsWar.Events;
using BallsWar.Faction;

namespace BallsWar.Game;

public class GameLoop
{
    public GameConfig Config { get; }
    public GameState State { get; } = new();
    public SimulationSpeed Speed { get; } = new();
    public IReadOnlyList<Faction.Faction> Factions => _factions;
    public BattleGrid Grid { get; private set; } = null!;
    public PinballArena Arena { get; private set; } = null!;
    public EventBus Events { get; } = new();

    private readonly List<Faction.Faction> _factions = new();
    private readonly Random _rng = new();

    public GameLoop(GameConfig? config = null)
    {
        Config = config ?? new GameConfig();
    }

    public void Initialize()
    {
        // Unsubscribe old events to prevent memory leaks
        Cleanup();

        int count = Config.FactionCount;
        Grid = new BattleGrid(Config, count);

        Arena = new PinballArena(Config, new Random(_rng.Next()));
        Arena.BallConverted += OnBallConverted;

        for (int i = 0; i < count; i++)
        {
            var camp = Grid.GetCamp(i);
            camp.FiringAngularSpeed = Config.CampFiringAngularSpeed;
            var faction = new Faction.Faction(i, $"F{i + 1}", camp);
            _factions.Add(faction);
        }

        Grid.CampDamaged += OnCampDamaged;
        Grid.CampDestroyed += OnCampDestroyed;

        State.Start(count);
    }

    private void Cleanup()
    {
        if (Arena != null) Arena.BallConverted -= OnBallConverted;
        if (Grid != null)
        {
            Grid.CampDamaged -= OnCampDamaged;
            Grid.CampDestroyed -= OnCampDestroyed;
            Grid.PelletManager.Clear();
            Grid.BigBallManager.Clear();
        }
        _factions.Clear();
        Arena = null!;
        Grid = null!;
    }

    public void Update(float frameDeltaTime)
    {
        if (State.Phase != GamePhase.Running) return;
        if (Arena == null || Grid == null) return;

        int steps = Speed.StepsPerFrame(frameDeltaTime, Config.PhysicsTimestep);

        for (int s = 0; s < steps; s++)
        {
            // Step the shared arena
            Arena.Step(Config.PhysicsTimestep);

            // Update Area B
            Grid.Update(Config.PhysicsTimestep);

            State.AdvanceTime(Config.PhysicsTimestep);
            if (CheckGameOver()) break;
        }
    }

    private void OnBallConverted(BallConvertedEvent e)
    {
        var faction = _factions[e.FactionId];
        faction.ConversionsCount++;

        var camp = Grid.GetCamp(e.FactionId);

        switch (e.Type)
        {
            case ConversionType.Shotgun:
                // Accumulate ammo — PelletManager auto-fires 60 rounds/sec
                camp.ShotgunAmmo += e.BallValue;
                break;

            case ConversionType.Shield:
                camp.AddHealth((int)e.BallValue);
                break;

            case ConversionType.BigBall:
                Grid.BigBallManager.Spawn(e.FactionId, e.BallValue,
                    camp.CenterX, camp.CenterY, new Random());
                break;
        }
    }

    private void OnCampDamaged(CampDamagedEvent e) { }
    private void OnCampDestroyed(CampDestroyedEvent e) => State.OnFactionEliminated(e.FactionId);
    private bool CheckGameOver() => State.Phase == GamePhase.Finished;

    public void TogglePause() => State.TogglePause();
    public void IncreaseSpeed() => Speed.Increase();
    public void DecreaseSpeed() => Speed.Decrease();

    public void Reset()
    {
        Cleanup();
        Speed.SetSpeed(1f);
    }
}
