namespace BallsWar.AreaB;

public class ShotgunPellet
{
    public int Id { get; }
    public int FactionId { get; }
    public bool Dead { get; set; }

    // Continuous position (floating point, for smooth movement)
    public float X { get; set; }
    public float Y { get; set; }

    // Travel direction in radians
    public float Angle { get; set; }

    // Speed in cells per second
    public float Speed { get; set; }

    // Remaining reflections
    public int BouncesLeft { get; set; }

    // Accumulated delta for grid-step detection
    public float Accumulator { get; set; }

    // Previous grid cell for detecting cell changes
    public int PrevGridX { get; set; }
    public int PrevGridY { get; set; }

    /// Remaining capture budget: -1 per cell captured, -50 per camp hit.
    public int CaptureBudget { get; set; }

    public ShotgunPellet(int id, int factionId, float startX, float startY,
                          float angle, float speed, int bounces, int captureBudget)
    {
        Id = id;
        FactionId = factionId;
        X = startX;
        Y = startY;
        Angle = angle;
        Speed = speed;
        BouncesLeft = bounces;
        CaptureBudget = captureBudget;
        PrevGridX = (int)startX;
        PrevGridY = (int)startY;
    }

    public void Kill() => Dead = true;
}
