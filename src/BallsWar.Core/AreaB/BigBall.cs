namespace BallsWar.AreaB;

public class BigBall
{
    public int Id { get; }
    public int FactionId { get; }
    public bool Dead { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Angle { get; set; }
    public float Speed { get; set; } = 3f;
    public long Value { get; set; }
    public float Accumulator { get; set; }
    public int PrevGridX { get; set; }
    public int PrevGridY { get; set; }

    /// Visual radius (log-scale), computed from value.
    public float VisualRadius => Math.Max(1.5f, 2f + MathF.Log(Value + 1) * 0.6f);

    public BigBall(int id, int factionId, float x, float y, float angle, float speed, long value)
    {
        Id = id;
        FactionId = factionId;
        X = x;
        Y = y;
        Angle = angle;
        Speed = speed;
        Value = value;
        PrevGridX = (int)x;
        PrevGridY = (int)y;
    }

    public void Kill() => Dead = true;
}
