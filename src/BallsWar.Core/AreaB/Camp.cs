namespace BallsWar.AreaB;

public class Camp
{
    public int FactionId { get; }
    public int CenterX { get; }
    public int CenterY { get; }
    public int Health { get; private set; }

    /// Current firing direction in radians, rotates clockwise each frame.
    public float FiringAngle { get; set; }

    /// Angular speed of firing direction (rad/s, clockwise = positive).
    public float FiringAngularSpeed { get; set; } = MathF.PI / 180f * 60f; // ~1 degree per frame at 60fps

    /// Accumulated shotgun conversions — each shot uses this.
    public long ShotgunAmmo { get; set; }

    public bool IsDestroyed => Health <= 0;

    public Camp(int factionId, int cx, int cy, int startingHealth)
    {
        FactionId = factionId;
        CenterX = cx;
        CenterY = cy;
        Health = startingHealth;
        FiringAngle = (float)new Random().NextDouble() * MathF.PI * 2;
    }

    /// Add health from area A conversions.
    public void AddHealth(int amount) => Health += amount;

    /// Returns true if the camp was destroyed by this damage.
    public bool TakeDamage(int damage)
    {
        if (damage <= 0) return IsDestroyed;
        Health -= damage;
        if (Health < 0) Health = 0;
        return IsDestroyed;
    }
}
