using CdCSharp.Generic.Colors;
using System.Drawing;

namespace CdCSharp.Generic.UnitTests.Colors_Tests;
public class HSVColor_Tests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(360, 1, 1)]
    [InlineData(180, 0.5, 0.5)]
    public void Constructor_ValidValues_CreatesInstance(int h, double s, double v)
    {
        HSVColor color = new(h, s, v);
        Assert.Equal(h, color.Hue);
        Assert.Equal(s, color.Saturation);
        Assert.Equal(v, color.Value);
    }

    [Theory]
    [InlineData(-90, 0, 0, 0)]
    [InlineData(450, 1, 1, 360)]
    [InlineData(0, -0.5, 0, 0)]
    [InlineData(0, 1.5, 0, 1)]
    public void Constructor_OutOfRangeValues_ClampedToValidRange(int h, double s, double v, int expectedH)
    {
        HSVColor color = new(h, s, v);
        Assert.Equal(expectedH, color.Hue);
        Assert.InRange(color.Saturation, 0, 1);
        Assert.InRange(color.Value, 0, 1);
    }

    [Theory]
    [InlineData(255, 0, 0, 0, 1, 1)]      // Red
    [InlineData(0, 255, 0, 120, 1, 1)]    // Green
    [InlineData(0, 0, 255, 240, 1, 1)]    // Blue
    [InlineData(0, 0, 0, 0, 0, 0)]        // Black
    [InlineData(255, 255, 255, 0, 0, 1)]  // White
    public void FromColor_BasicColors_CorrectHSV(byte r, byte g, byte b, int expectedH, double expectedS, double expectedV)
    {
        Color color = Color.FromArgb(255, r, g, b);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(expectedH, hsv.Hue);
        Assert.Equal(expectedS, Math.Round(hsv.Saturation, 3));
        Assert.Equal(expectedV, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void FromColor_GrayscaleColors_ZeroSaturation()
    {
        for (int i = 0; i <= 255; i += 51)
        {
            Color color = Color.FromArgb(255, i, i, i);
            HSVColor hsv = HSVColor.FromColor(color);
            Assert.Equal(0, Math.Round(hsv.Saturation, 3));
            Assert.Equal(i / 255.0, Math.Round(hsv.Value, 3));
        }
    }

    [Theory]
    [InlineData(0, 1, 1, 255, 0, 0)]      // Red
    [InlineData(120, 1, 1, 0, 255, 0)]    // Green
    [InlineData(240, 1, 1, 0, 0, 255)]    // Blue
    [InlineData(0, 0, 0, 0, 0, 0)]        // Black
    [InlineData(0, 0, 1, 255, 255, 255)]  // White
    public void ToColor_BasicHSV_CorrectRGB(int h, double s, double v, int expectedR, int expectedG, int expectedB)
    {
        HSVColor hsv = new(h, s, v);
        Color rgb = hsv.ToColor();

        Assert.Equal(expectedR, rgb.R);
        Assert.Equal(expectedG, rgb.G);
        Assert.Equal(expectedB, rgb.B);
    }

    [Theory]
    [InlineData(128)]
    [InlineData(0)]
    [InlineData(255)]
    public void ToColor_DifferentAlphaValues_SetsCorrectAlpha(int alpha)
    {
        HSVColor hsv = new(0, 1, 1);
        Color rgb = hsv.ToColor(alpha);
        Assert.Equal(alpha, rgb.A);
    }

    [Fact]
    public void ColorConversion_Roundtrip_PreservesValues()
    {
        // Test multiple colors through the conversion cycle
        Color[] testColors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Black,
            Color.FromArgb(255, 128, 128, 128),
            Color.FromArgb(255, 255, 128, 0)
        };

        foreach (Color originalColor in testColors)
        {
            HSVColor hsv = HSVColor.FromColor(originalColor);
            Color convertedBack = hsv.ToColor();

            // Allow small differences due to rounding
            Assert.InRange(convertedBack.R, originalColor.R - 1, originalColor.R + 1);
            Assert.InRange(convertedBack.G, originalColor.G - 1, originalColor.G + 1);
            Assert.InRange(convertedBack.B, originalColor.B - 1, originalColor.B + 1);
        }
    }
}
