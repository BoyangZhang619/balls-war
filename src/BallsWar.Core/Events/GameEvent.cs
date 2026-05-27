namespace BallsWar.Events;

public record BallConvertedEvent(int FactionId, ConversionType Type, long BallValue);
public record PelletSpawnedEvent(int FactionId, int Count);
public record CellCapturedEvent(int X, int Y, int? OldOwnerFactionId, int NewOwnerFactionId);
public record CampDamagedEvent(int FactionId, int Damage, int NewHealth, int _unused);
public record CampDestroyedEvent(int FactionId);

public enum ConversionType { Shotgun, Shield, Armor }
