using CdCSharp.Generic.Colors;
using System.Drawing;

namespace CdCSharp.Generic.UnitTests.Colors_Tests;
public class ColorExtensions_Tests
{
    [Fact]
    public void GetHex_ReturnsCorrectFormat()
    {
        Color color = Color.FromArgb(255, 255, 0, 0);
        Assert.Equal("#FF0000FF", color.GetHex());
    }

    [Theory]
    [InlineData(255, 0, 0, "FF")]
    [InlineData(128, 0, 0, "80")]
    [InlineData(0, 0, 0, "00")]
    public void GetHexA_ReturnsCorrectHexValue(int alpha, int r, int g, string expected)
    {
        Color color = Color.FromArgb(alpha, r, g, 0);
        Assert.Equal(expected, color.GetHexA());
    }

    [Theory]
    [InlineData(0, 0, 255, "FF")]
    [InlineData(0, 0, 128, "80")]
    [InlineData(0, 0, 0, "00")]
    public void GetHexB_ReturnsCorrectHexValue(int r, int g, int b, string expected)
    {
        Color color = Color.FromArgb(255, r, g, b);
        Assert.Equal(expected, color.GetHexB());
    }

    [Theory]
    [InlineData(0, 255, 0, "FF")]
    [InlineData(0, 128, 0, "80")]
    [InlineData(0, 0, 0, "00")]
    public void GetHexG_ReturnsCorrectHexValue(int r, int g, int b, string expected)
    {
        Color color = Color.FromArgb(255, r, g, b);
        Assert.Equal(expected, color.GetHexG());
    }

    [Theory]
    [InlineData(255, 0, 0, "FF")]
    [InlineData(128, 0, 0, "80")]
    [InlineData(0, 0, 0, "00")]
    public void GetHexR_ReturnsCorrectHexValue(int r, int g, int b, string expected)
    {
        Color color = Color.FromArgb(255, r, g, b);
        Assert.Equal(expected, color.GetHexR());
    }

    [Theory]
    [InlineData(255, 255, 0, 0, null, true, "rgba(255,0,0,1.00)")]
    [InlineData(128, 255, 0, 0, null, true, "rgba(255,0,0,0.50)")]
    [InlineData(255, 255, 0, 0, 0.5, true, "rgba(255,0,0,0.5)")]
    [InlineData(255, 255, 0, 0, 128, false, "rgba(255,0,0,128)")]
    [InlineData(0, 0, 0, 0, null, true, "rgba(0,0,0,0.00)")]
    public void ToStringRgba_ReturnsCorrectFormat(
        int alpha, int r, int g, int b,
        decimal? alphaValue, bool? alphaPercent,
        string expected)
    {
        Color color = Color.FromArgb(alpha, r, g, b);
        Assert.Equal(expected, color.ToStringRgba(alphaValue, alphaPercent));
    }

    [Theory]
    [InlineData(255, 128, 64, "#FF804000FF")]
    [InlineData(0, 0, 0, "#00000000")]
    [InlineData(255, 255, 255, "#FFFFFFFF")]
    public void GetHex_DifferentColors_ReturnsCorrectHexString(int r, int g, int b, string expected)
    {
        Color color = Color.FromArgb(255, r, g, b);
        Assert.Equal(expected, color.GetHex());
    }
}
