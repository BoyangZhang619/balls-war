using BallsWar.Events;

namespace BallsWar.AreaB;

public class BattleGrid
{
    public int Width { get; }
    public int Height { get; }
    public GridCell[,] Cells { get; }
    public PelletManager PelletManager { get; }
    public BigBallManager BigBallManager { get; }
    public IReadOnlyDictionary<int, Camp> Camps => _camps;
    private readonly Dictionary<int, Camp> _camps = new();
    private readonly Game.GameConfig _config;

    public event Action<CampDamagedEvent>? CampDamaged;
    public event Action<CampDestroyedEvent>? CampDestroyed;

    private List<(int, int, int?)> _dirtyA = new();
    private List<(int, int, int?)> _dirtyB = new();
    private bool _dirtySwap;

    public BattleGrid(Game.GameConfig config, int factionCount)
    {
        _config = config;
        Width = config.GridWidth;
        Height = config.GridHeight;
        Cells = new GridCell[Width, Height];
        PelletManager = new PelletManager(this, config);
        BigBallManager = new BigBallManager(this);

        InitializeCamps(factionCount);
    }

    private void InitializeCamps(int factionCount)
    {
        for (int i = 0; i < factionCount; i++)
        {
            int margin = System.Math.Min(10, Width / 10);
            float angle = (float)i / factionCount * MathF.PI * 2 - MathF.PI / 2;
            int cx = Width / 2 + (int)(MathF.Cos(angle) * (Width / 2 - margin));
            int cy = Height / 2 + (int)(MathF.Sin(angle) * (Height / 2 - margin));
            cx = System.Math.Clamp(cx, margin, Width - 1 - margin);
            cy = System.Math.Clamp(cy, margin, Height - 1 - margin);

            var camp = new Camp(i, cx, cy, _config.StartingCampHealth);
            int hitR = (int)(Math.Min(Width, Height) * 0.05f) + 1;
            camp.HitRadius = hitR;
            _camps[i] = camp;

            // Initialize camp territory cells
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    int cellX = cx + dx;
                    int cellY = cy + dy;
                    if (IsInBounds(cellX, cellY))
                    {
                        Cells[cellX, cellY].Capture(i);
                        _dirtyA.Add((cellX, cellY, i));
                    }
                }
            }
        }
    }

    public Camp GetCamp(int factionId) => _camps[factionId];

    public bool CaptureCell(int x, int y, int factionId)
    {
        if (!IsInBounds(x, y)) return false;
        ref var cell = ref Cells[x, y];
        int? oldOwner = cell.OwnerFactionId;
        if (oldOwner == factionId) return false;
        cell.Capture(factionId);
        (_dirtySwap ? _dirtyB : _dirtyA).Add((x, y, factionId));
        return true;
    }

    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public void RaiseCampDamaged(int factionId, int damage, int newHealth)
        => CampDamaged?.Invoke(new CampDamagedEvent(factionId, damage, newHealth, 0));

    public void RaiseCampDestroyed(int factionId)
    {
        // Cells stay frozen with their faction color — do NOT reset them.
        // Other factions will capture them normally as they pass.
        CampDestroyed?.Invoke(new CampDestroyedEvent(factionId));
    }

    public void Update(float dt)
    {
        // Shield decay: HP bleeds 0.2%/sec, harder to turtle forever
        foreach (var camp in _camps.Values)
        {
            if (!camp.IsDestroyed && camp.Health > _config.ShieldDecayMinHealth)
            {
                int decay = Math.Max(1, (int)(camp.Health * _config.ShieldDecayPerSec * dt));
                camp.TakeDamage(decay);
                if (camp.IsDestroyed) RaiseCampDestroyed(camp.FactionId);
            }
        }
        PelletManager.Update(dt);
        BigBallManager.Update(dt);
    }

    public List<(int X, int Y, int? OwnerId)> GetDirtyCellsAndClear()
    {
        var current = _dirtySwap ? _dirtyB : _dirtyA;
        var next = _dirtySwap ? _dirtyA : _dirtyB;
        _dirtySwap = !_dirtySwap;
        next.Clear();
        return current;
    }
}
