using Raylib_cs;
using BallsWar.Events;

namespace BallsWar.App.Rendering;

public static class ColorMap
{
    private static readonly Color[] _palette =
    {
        new(220, 50, 50, 255),    // Red
        new(50, 120, 220, 255),   // Blue
        new(50, 200, 50, 255),    // Green
        new(220, 200, 50, 255),   // Gold
        new(200, 50, 200, 255),   // Magenta
        new(50, 200, 200, 255),   // Cyan
        new(220, 120, 50, 255),   // Orange
        new(180, 180, 180, 255),  // Gray
    };

    public static Color NeutralCell { get; } = new(40, 40, 40, 255);
    public static Color Background { get; } = new(20, 20, 20, 255);

    public static Color GetFactionColor(int factionId) => _palette[factionId % _palette.Length];

    public static Color GetCellColor(int? ownerFactionId)
        => ownerFactionId.HasValue ? GetFactionColor(ownerFactionId.Value) : NeutralCell;

    public static Color GetMultiplierColor(int multiplier) => multiplier switch
    {
        2 => new(150, 150, 220, 255),
        3 => new(100, 200, 100, 255),
        5 => new(220, 150, 50, 255),
        10 => new(255, 100, 100, 255),
        _ => Color.White,
    };

    public static Color GetConversionColor(ConversionType type) => type switch
    {
        ConversionType.Shotgun => new(255, 150, 50, 255),
        ConversionType.Shield => new(50, 200, 220, 255),
        ConversionType.Armor => new(200, 100, 255, 255),
        ConversionType.BigBall => new(255, 220, 50, 255),
        _ => Color.White,
    };
}
