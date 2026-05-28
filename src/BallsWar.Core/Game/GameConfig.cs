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
    public int MultiplierZoneCount { get; set; } = 5;
    public int ConversionZoneCount { get; set; } = 4;
    // Powers of 2, placed 5 per side: [8,4,2,2,2] | [2,2,2,4,8]
    public int[] MultiplierValues { get; set; } = { 8, 4, 2, 2, 2 };
    public float AirflowStrength { get; set; } = 45f;

    // Bottom bar ratios: Shotgun, BigBall, Shield. Each bar width = ratio / sum * arenaWidth
    public int[] BottomBarRatios { get; set; } = { 3, 2, 3 };

    // Area B
    public int GridWidth { get; set; } = 500;
    public int GridHeight { get; set; } = 500;
    public int StartingCampHealth { get; set; } = 50;
    public int PelletDamage { get; set; } = 1;
    public float ShieldDecayPerSec { get; set; } = 0.002f; // 0.2% HP lost per second
    public int ShieldDecayMinHealth { get; set; } = 256;      // no decay when HP <= this
    // Shotgun: auto-fires 60 rounds/sec, each round = max(1, ammo/100) pellets
    public int PelletsPerShotgun { get; set; } = 1;
    public float ShotgunSpreadDegrees { get; set; } = 1.0f;
    public float PelletSpeed { get; set; } = 25f;
    public int PelletBounces { get; set; } = 3;
    public float CampFiringAngularSpeed { get; set; } = MathF.PI / 180f * 60f;
    public int LanguageIndex { get; set; } = 1; // 0=Chinese, 1=English
}
