namespace BallsWar.AreaB;

public struct GridCell
{
    public int? OwnerFactionId { get; set; }

    public readonly bool IsCaptured => OwnerFactionId.HasValue;
    public readonly bool IsNeutral => !OwnerFactionId.HasValue;

    public void Capture(int factionId) => OwnerFactionId = factionId;
    public void Reset() => OwnerFactionId = null;
}
