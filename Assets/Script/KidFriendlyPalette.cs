using UnityEngine;

/// <summary>Shared colour roles; consumers opt in for MainMenu only.</summary>
public static class KidFriendlyPalette
{
    public static readonly Color Sky = Hex("EAF7FF");
    public static readonly Color Surface = Hex("FFFEF7");
    public static readonly Color Slot = Hex("DDF1FF");
    public static readonly Color Ink = Hex("183153");
    public static readonly Color Muted = Hex("405777");
    public static readonly Color Yellow = Hex("FFD84D");
    public static readonly Color Blue = Hex("55C4F5");
    public static readonly Color Mint = Hex("66DDB0");
    public static readonly Color Lavender = Hex("B6A0F5");
    public static readonly Color Coral = Hex("FF927F");
    public static readonly Color Neutral = Hex("D6E1EB");
    public static readonly Color Success = Hex("176447");
    public static readonly Color Warning = Hex("805000");
    public static readonly Color Error = Hex("AC303C");
    private static Color Hex(string value)
    {
        // UI vertex colours are rendered in the project colour space; author the palette in sRGB.
        if (ColorUtility.TryParseHtmlString("#" + value, out var color)) return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
        Debug.LogError("Invalid kid-friendly palette colour: " + value);
        return Color.magenta;
    }
}
