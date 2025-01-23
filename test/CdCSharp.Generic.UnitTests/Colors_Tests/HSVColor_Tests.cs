using CdCSharp.Generic.Colors;
using System.Drawing;

namespace CdCSharp.Generic.UnitTests.Colors_Tests;
public class HSVColor_Tests
{
    [Fact]
    public void Constructor_ValidValues_H0_S0_V0()
    {
        HSVColor color = new(0, 0, 0);
        Assert.Equal(0, color.Hue);
        Assert.Equal(0, color.Saturation);
        Assert.Equal(0, color.Value);
    }

    [Fact]
    public void Constructor_ValidValues_H360_S1_V1()
    {
        HSVColor color = new(360, 1, 1);
        Assert.Equal(360, color.Hue);
        Assert.Equal(1, color.Saturation);
        Assert.Equal(1, color.Value);
    }

    [Fact]
    public void Constructor_ValidValues_H180_S0_5_V0_5()
    {
        HSVColor color = new(180, 0.5, 0.5);
        Assert.Equal(180, color.Hue);
        Assert.Equal(0.5, color.Saturation);
        Assert.Equal(0.5, color.Value);
    }

    [Fact]
    public void Constructor_OutOfRangeValues_HMinus90()
    {
        HSVColor color = new(-90, 0, 0);
        Assert.Equal(0, color.Hue);
        Assert.InRange(color.Saturation, 0, 1);
        Assert.InRange(color.Value, 0, 1);
    }

    [Fact]
    public void Constructor_OutOfRangeValues_H450()
    {
        HSVColor color = new(450, 1, 1);
        Assert.Equal(360, color.Hue);
        Assert.InRange(color.Saturation, 0, 1);
        Assert.InRange(color.Value, 0, 1);
    }

    [Fact]
    public void Constructor_OutOfRangeValues_SMinus0_5()
    {
        HSVColor color = new(0, -0.5, 0);
        Assert.Equal(0, color.Hue);
        Assert.InRange(color.Saturation, 0, 1);
        Assert.InRange(color.Value, 0, 1);
    }

    [Fact]
    public void Constructor_OutOfRangeValues_S1_5()
    {
        HSVColor color = new(0, 1.5, 0);
        Assert.Equal(0, color.Hue);
        Assert.InRange(color.Saturation, 0, 1);
        Assert.InRange(color.Value, 0, 1);
    }

    [Fact]
    public void FromColor_BasicColors_Red()
    {
        Color color = Color.FromArgb(255, 255, 0, 0);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(0, hsv.Hue);
        Assert.Equal(1, Math.Round(hsv.Saturation, 3));
        Assert.Equal(1, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void FromColor_BasicColors_Green()
    {
        Color color = Color.FromArgb(255, 0, 255, 0);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(120, hsv.Hue);
        Assert.Equal(1, Math.Round(hsv.Saturation, 3));
        Assert.Equal(1, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void FromColor_BasicColors_Blue()
    {
        Color color = Color.FromArgb(255, 0, 0, 255);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(240, hsv.Hue);
        Assert.Equal(1, Math.Round(hsv.Saturation, 3));
        Assert.Equal(1, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void FromColor_BasicColors_Black()
    {
        Color color = Color.FromArgb(255, 0, 0, 0);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(0, hsv.Hue);
        Assert.Equal(0, Math.Round(hsv.Saturation, 3));
        Assert.Equal(0, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void FromColor_BasicColors_White()
    {
        Color color = Color.FromArgb(255, 255, 255, 255);
        HSVColor hsv = HSVColor.FromColor(color);

        Assert.Equal(0, hsv.Hue);
        Assert.Equal(0, Math.Round(hsv.Saturation, 3));
        Assert.Equal(1, Math.Round(hsv.Value, 3));
    }

    [Fact]
    public void ToColor_BasicHSV_Red()
    {
        HSVColor hsv = new(0, 1, 1);
        Color rgb = hsv.ToColor();

        Assert.Equal(255, rgb.R);
        Assert.Equal(0, rgb.G);
        Assert.Equal(0, rgb.B);
    }

    [Fact]
    public void ToColor_BasicHSV_Green()
    {
        HSVColor hsv = new(120, 1, 1);
        Color rgb = hsv.ToColor();

        Assert.Equal(0, rgb.R);
        Assert.Equal(255, rgb.G);
        Assert.Equal(0, rgb.B);
    }

    [Fact]
    public void ToColor_BasicHSV_Blue()
    {
        HSVColor hsv = new(240, 1, 1);
        Color rgb = hsv.ToColor();

        Assert.Equal(0, rgb.R);
        Assert.Equal(0, rgb.G);
        Assert.Equal(255, rgb.B);
    }

    [Fact]
    public void ToColor_BasicHSV_Black()
    {
        HSVColor hsv = new(0, 0, 0);
        Color rgb = hsv.ToColor();

        Assert.Equal(0, rgb.R);
        Assert.Equal(0, rgb.G);
        Assert.Equal(0, rgb.B);
    }

    [Fact]
    public void ToColor_BasicHSV_White()
    {
        HSVColor hsv = new(0, 0, 1);
        Color rgb = hsv.ToColor();

        Assert.Equal(255, rgb.R);
        Assert.Equal(255, rgb.G);
        Assert.Equal(255, rgb.B);
    }

    [Fact]
    public void ToColor_DifferentAlphaValues_Alpha128()
    {
        HSVColor hsv = new(0, 1, 1);
        Color rgb = hsv.ToColor(128);

        Assert.Equal(128, rgb.A);
    }

    [Fact]
    public void ToColor_DifferentAlphaValues_Alpha0()
    {
        HSVColor hsv = new(0, 1, 1);
        Color rgb = hsv.ToColor(0);

        Assert.Equal(0, rgb.A);
    }

    [Fact]
    public void ToColor_DifferentAlphaValues_Alpha255()
    {
        HSVColor hsv = new(0, 1, 1);
        Color rgb = hsv.ToColor(255);

        Assert.Equal(255, rgb.A);
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

    [Fact]
    public void ColorConversion_Roundtrip_PreservesValues()
    {
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

            Assert.InRange(convertedBack.R, originalColor.R - 1, originalColor.R + 1);
            Assert.InRange(convertedBack.G, originalColor.G - 1, originalColor.G + 1);
            Assert.InRange(convertedBack.B, originalColor.B - 1, originalColor.B + 1);
        }
    }
}