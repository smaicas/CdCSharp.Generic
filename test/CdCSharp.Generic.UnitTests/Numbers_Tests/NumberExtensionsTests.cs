using CdCSharp.Generic.Numbers;

namespace CdCSharp.Generic.UnitTests.Numbers_Tests;

/// <summary>
/// Contains unit tests for the NumberExtensions class.
/// </summary>
public class NumberExtensionsTests
{
    #region Double EnsureRange Tests

    [Theory]
    [InlineData(-10.5, 100.0, 0.0)]
    [InlineData(50.0, 100.0, 50.0)]
    [InlineData(150.0, 100.0, 100.0)]
    [InlineData(0.0, 100.0, 0.0)]
    [InlineData(100.0, 100.0, 100.0)]
    public void EnsureRange_Should_ReturnCorrectValue_When_InputIsWithinOrOutsideDefaultRange(double input, double max, double expected)
    {
        // Act
        double result = input.EnsureRange(max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-10.5, 0.0, 100.0, 0.0)]
    [InlineData(50.0, 0.0, 100.0, 50.0)]
    [InlineData(150.0, 0.0, 100.0, 100.0)]
    [InlineData(0.0, 0.0, 100.0, 0.0)]
    [InlineData(100.0, 0.0, 100.0, 100.0)]
    [InlineData(50.0, 60.0, 80.0, 60.0)]
    [InlineData(70.0, 60.0, 80.0, 70.0)]
    [InlineData(90.0, 60.0, 80.0, 80.0)]
    public void EnsureRange_Should_ReturnCorrectValue_When_InputIsWithinOrOutsideSpecifiedRange(double input, double min, double max, double expected)
    {
        // Act
        double result = input.EnsureRange(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion Double EnsureRange Tests

    #region Byte EnsureRange Tests

    [Theory]
    [InlineData((byte)10, (byte)100, (byte)10)]
    [InlineData((byte)0, (byte)100, (byte)0)]
    [InlineData((byte)150, (byte)100, (byte)100)]
    [InlineData((byte)100, (byte)100, (byte)100)]
    public void EnsureRange_Should_ReturnCorrectValue_When_ByteInputIsWithinOrExceedsDefaultRange(byte input, byte max, byte expected)
    {
        // Act
        byte result = input.EnsureRange(max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData((byte)10, (byte)20, (byte)100, (byte)20)]
    [InlineData((byte)50, (byte)20, (byte)100, (byte)50)]
    [InlineData((byte)150, (byte)20, (byte)100, (byte)100)]
    [InlineData((byte)20, (byte)20, (byte)100, (byte)20)]
    [InlineData((byte)100, (byte)20, (byte)100, (byte)100)]
    public void EnsureRange_Should_ReturnCorrectValue_When_ByteInputIsWithinOrExceedsSpecifiedRange(byte input, byte min, byte max, byte expected)
    {
        // Act
        byte result = input.EnsureRange(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion Byte EnsureRange Tests

    #region Int EnsureRange Tests

    [Theory]
    [InlineData(-50, 100, 0)]
    [InlineData(50, 100, 50)]
    [InlineData(150, 100, 100)]
    [InlineData(0, 100, 0)]
    [InlineData(100, 100, 100)]
    public void EnsureRange_Should_ReturnCorrectValue_When_IntInputIsWithinOrExceedsDefaultRange(int input, int max, int expected)
    {
        // Act
        int result = input.EnsureRange(max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-50, 0, 100, 0)]
    [InlineData(50, 0, 100, 50)]
    [InlineData(150, 0, 100, 100)]
    [InlineData(0, 0, 100, 0)]
    [InlineData(100, 0, 100, 100)]
    [InlineData(50, 60, 80, 60)]
    [InlineData(70, 60, 80, 70)]
    [InlineData(90, 60, 80, 80)]
    public void EnsureRange_Should_ReturnCorrectValue_When_IntInputIsWithinOrExceedsSpecifiedRange(int input, int min, int max, int expected)
    {
        // Act
        int result = input.EnsureRange(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion Int EnsureRange Tests

    #region EnsureRangeToByte Tests

    [Theory]
    [InlineData(-10, (byte)0)]
    [InlineData(0, (byte)0)]
    [InlineData(100, (byte)100)]
    [InlineData(255, (byte)255)]
    [InlineData(300, (byte)255)]
    public void EnsureRangeToByte_Should_ReturnCorrectByteValue_When_IntInputIsWithinOrExceedsByteRange(int input, byte expected)
    {
        // Act
        byte result = input.EnsureRangeToByte();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion EnsureRangeToByte Tests
}