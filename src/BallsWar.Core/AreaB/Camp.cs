namespace BallsWar.AreaB;

public class Camp
{
    public int FactionId { get; }
    public int CenterX { get; }
    public int CenterY { get; }
    public int Health { get; private set; }
    public int HitRadius { get; set; }

    /// Current firing direction in radians, rotates clockwise each frame.
    public float FiringAngle { get; set; }

    /// Angular speed of firing direction (rad/s, clockwise = positive).
    public float FiringAngularSpeed { get; set; } = MathF.PI / 180f * 60f; // ~1 degree per frame at 60fps

    /// Accumulated shotgun conversions — each shot uses this.
    public long ShotgunAmmo { get; set; }

    public bool IsDestroyed => _destroyed;
    private bool _destroyed;

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

    /// Returns true only on the first call that destroys the camp.
    public bool TakeDamage(int damage)
    {
        if (_destroyed) return false;
        if (damage <= 0) return false;
        Health -= damage;
        if (Health < 0) Health = 0;
        if (Health <= 0) { _destroyed = true; return true; }
        return false;
    }
}
