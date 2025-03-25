
using SkiaSharp;

public class CaptchaOptions
{
    public int Width { get; set; } = 200;
    public int Height { get; set; } = 80;
    public int CodeLength { get; set; } = 4;
    public int FontSize { get; set; } = 40;
    public string FontFamily { get; set; } = "Arial";
    public int ExpirationSeconds { get; set; } = 60;

    public List<SKColor> TextColors { get; set; } = new()
        {
            SKColors.Black,
            SKColors.DarkBlue,
            SKColors.DarkGreen,
            SKColors.DarkRed
        };

    public SKColor BackgroundColor { get; set; } = SKColors.White;
    public int NoiseCount { get; set; } = 50;
    public int InterferenceLineCount { get; set; } = 3;

    public string Characters { get; set; } = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
    public string ExcludedCharacters { get; set; } = "01IOlo";
}