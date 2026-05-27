namespace BallsWar.Game;

public class SimulationSpeed
{
    public float Current { get; private set; } = 1f;
    private static readonly float[] Allowed = { 0.25f, 0.5f, 1f, 2f, 4f, 8f };
    private int _index = 2; // index of 1f

    public int StepsPerFrame(float frameDeltaTime, float physicsTimestep)
    {
        int steps = (int)(Current / physicsTimestep * frameDeltaTime);
        return System.Math.Max(1, System.Math.Min(steps, 30));
    }

    public void SetSpeed(float speed)
    {
        for (int i = 0; i < Allowed.Length; i++)
        {
            if (System.Math.Abs(Allowed[i] - speed) < 0.01f)
            {
                _index = i;
                Current = Allowed[i];
                return;
            }
        }
    }

    public void Increase()
    {
        if (_index < Allowed.Length - 1) Current = Allowed[++_index];
    }

    public void Decrease()
    {
        if (_index > 0) Current = Allowed[--_index];
    }
}
