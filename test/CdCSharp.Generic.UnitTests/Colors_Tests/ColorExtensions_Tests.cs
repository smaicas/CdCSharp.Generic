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

    [Fact]
    public void GetHexA_ReturnsCorrectHexValue_Alpha255_Red0_Green0()
    {
        Color color = Color.FromArgb(255, 0, 0, 0);
        Assert.Equal("FF", color.GetHexA());
    }

    [Fact]
    public void GetHexA_ReturnsCorrectHexValue_Alpha128_Red0_Green0()
    {
        Color color = Color.FromArgb(128, 0, 0, 0);
        Assert.Equal("80", color.GetHexA());
    }

    [Fact]
    public void GetHexA_ReturnsCorrectHexValue_Alpha0_Red0_Green0()
    {
        Color color = Color.FromArgb(0, 0, 0, 0);
        Assert.Equal("00", color.GetHexA());
    }

    [Fact]
    public void GetHexB_ReturnsCorrectHexValue_Blue255()
    {
        Color color = Color.FromArgb(255, 0, 0, 255);
        Assert.Equal("FF", color.GetHexB());
    }

    [Fact]
    public void GetHexB_ReturnsCorrectHexValue_Blue128()
    {
        Color color = Color.FromArgb(255, 0, 0, 128);
        Assert.Equal("80", color.GetHexB());
    }

    [Fact]
    public void GetHexB_ReturnsCorrectHexValue_Blue0()
    {
        Color color = Color.FromArgb(255, 0, 0, 0);
        Assert.Equal("00", color.GetHexB());
    }

    [Fact]
    public void GetHexG_ReturnsCorrectHexValue_Green255()
    {
        Color color = Color.FromArgb(255, 0, 255, 0);
        Assert.Equal("FF", color.GetHexG());
    }

    [Fact]
    public void GetHexG_ReturnsCorrectHexValue_Green128()
    {
        Color color = Color.FromArgb(255, 0, 128, 0);
        Assert.Equal("80", color.GetHexG());
    }

    [Fact]
    public void GetHexG_ReturnsCorrectHexValue_Green0()
    {
        Color color = Color.FromArgb(255, 0, 0, 0);
        Assert.Equal("00", color.GetHexG());
    }

    [Fact]
    public void GetHexR_ReturnsCorrectHexValue_Red255()
    {
        Color color = Color.FromArgb(255, 255, 0, 0);
        Assert.Equal("FF", color.GetHexR());
    }

    [Fact]
    public void GetHexR_ReturnsCorrectHexValue_Red128()
    {
        Color color = Color.FromArgb(255, 128, 0, 0);
        Assert.Equal("80", color.GetHexR());
    }

    [Fact]
    public void GetHexR_ReturnsCorrectHexValue_Red0()
    {
        Color color = Color.FromArgb(255, 0, 0, 0);
        Assert.Equal("00", color.GetHexR());
    }

    [Fact]
    public void ToStringRgba_ReturnsCorrectFormat_Alpha255_AlphaPercent_True()
    {
        Color color = Color.FromArgb(0, 255, 0, 0);
        Assert.Equal("rgba(255,0,0,0.50)", color.ToStringRgba(128, true));
    }

    [Fact]
    public void ToStringRgba_ReturnsCorrectFormat_Alpha255_AlphaPercent_False()
    {
        Color color = Color.FromArgb(255, 255, 0, 0);
        Assert.Equal("rgba(255,0,0,128)", color.ToStringRgba(128, false));
    }

    [Fact]
    public void GetHex_DifferentColors_Red128_Green64_Blue255()
    {
        Color color = Color.FromArgb(255, 128, 64, 0);
        Assert.Equal("#804000FF", color.GetHex());
    }

    [Fact]
    public void GetHex_DifferentColors_Black()
    {
        Color color = Color.FromArgb(0, 0, 0, 0);
        Assert.Equal("#00000000", color.GetHex());
    }

    [Fact]
    public void GetHex_DifferentColors_White()
    {
        Color color = Color.FromArgb(255, 255, 255, 255);
        Assert.Equal("#FFFFFFFF", color.GetHex());
    }
}
