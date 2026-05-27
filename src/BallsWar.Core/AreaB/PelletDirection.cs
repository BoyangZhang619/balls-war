namespace BallsWar.AreaB;

public static class PelletDirection
{
    public static readonly (int dx, int dy)[] Vectors =
    {
        (0, -1), (1, -1), (1, 0), (1, 1),
        (0, 1), (-1, 1), (-1, 0), (-1, -1)
    };

    /// Distributes N pellets evenly across 8 directions.
    /// Returns an array of direction indices (0-7) of length pelletCount.
    public static int[] Distribute(int pelletCount)
    {
        var dirs = new int[pelletCount];
        for (int i = 0; i < pelletCount; i++)
            dirs[i] = i % Vectors.Length;
        return dirs;
    }
}
