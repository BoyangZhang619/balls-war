namespace BallsWar.AreaB;

public class ShotgunPellet
{
    public int Id { get; }
    public int FactionId { get; }
    public bool Dead { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Angle { get; set; }
    public float Speed { get; set; }
    public int BouncesLeft { get; set; }
    public float Accumulator { get; set; }
    public int PrevGridX { get; set; }
    public int PrevGridY { get; set; }

    public ShotgunPellet(int id, int factionId, float startX, float startY,
                          float angle, float speed, int bounces)
    {
        Id = id; FactionId = factionId;
        X = startX; Y = startY;
        Angle = angle; Speed = speed;
        BouncesLeft = bounces;
        PrevGridX = (int)startX; PrevGridY = (int)startY;
    }

    public void Kill() => Dead = true;
}
