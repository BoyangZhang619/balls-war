namespace BallsWar.Game;

public class GameConfig
{
    public int FactionCount { get; set; } = 4;
    public float PhysicsTimestep { get; set; } = 1f / 60f;

    // Area A
    public float ArenaWidthMeters { get; set; } = 8f;
    public float ArenaHeightMeters { get; set; } = 12f;
    public int BallsPerArena { get; set; } = 4;
    public float BallRadiusMeters { get; set; } = 0.15f;
    public float BallDensity { get; set; } = 1f;
    public float BallRestitution { get; set; } = 1.0f;
    public float BallInitialSpeed { get; set; } = 2.5f;
    public long BallValueDropThreshold { get; set; } = 4096;
    public float RespawnDelaySeconds { get; set; } = 4f;
    public int MultiplierZoneCount { get; set; } = 6;
    public int ConversionZoneCount { get; set; } = 4;
    // Powers of 2 only
    public int[] MultiplierValues { get; set; } = { 2, 2, 4, 4, 8, 16 };

    // Area B
    public int GridWidth { get; set; } = 500;
    public int GridHeight { get; set; } = 500;
    public int StartingCampHealth { get; set; } = 50;
    public int PelletDamage { get; set; } = 1;
    public int ShieldPerConversion { get; set; } = 5;
    public int ArmorPerConversion { get; set; } = 10;
    // Shotgun: auto-fires 60 rounds/sec, each round = max(1, ammo/100) pellets
    public int PelletsPerShotgun { get; set; } = 1;
    public float ShotgunSpreadDegrees { get; set; } = 1f;
    public float PelletSpeed { get; set; } = 25f;
    public int PelletBounces { get; set; } = 3;
    public float CampFiringAngularSpeed { get; set; } = MathF.PI / 180f * 60f;
}
