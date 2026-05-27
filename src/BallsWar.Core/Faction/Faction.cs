using BallsWar.AreaB;

namespace BallsWar.Faction;

public class Faction
{
    public int Id { get; }
    public string Name { get; }
    public bool IsEliminated => Camp?.IsDestroyed ?? false;

    public Camp? Camp { get; set; }
    public int ConversionsCount { get; set; }
    public int DamageDealt { get; set; }

    public Faction(int id, string name, Camp? camp = null)
    {
        Id = id;
        Name = name;
        Camp = camp;
    }
}
