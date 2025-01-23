using CdCSharp.Strings;

namespace CdCSharp.Generic.UnitTests.Strings_Tests;

public class StringExtensions_ExtractRegex
{
    [Fact]
    public void ExtractRegex_ShouldHandleMixedParenthesesCorrectly()
    {
        // Arrange
        string input = "a(b)c123";
        string expected = @"^(\w)((\w))(\w\d\d\d)$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExtractRegex_ShouldHandleSpecialCharactersOutsideParentheses()
    {
        // Arrange
        string input = "a#b@123";
        string expected = @"^(\w)(#)(\w)(@)(\d\d\d)$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExtractRegex_ShouldReturnEmptyPatternForEmptyString()
    {
        // Arrange
        string input = string.Empty;
        string expected = @"^$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExtractRegex_ShouldTransformAlphabeticCharactersToWordRegex()
    {
        // Arrange
        string input = "abc";
        string expected = @"^(\w\w\w)$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExtractRegex_ShouldTransformNumericCharactersToDigitRegex()
    {
        // Arrange
        string input = "123";
        string expected = @"^(\d\d\d)$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExtractRegex_ShouldWrapWordsAndDigitsInParentheses()
    {
        // Arrange
        string input = "abc123";
        string expected = @"^(\w\w\w\d\d\d)$";

        // Act
        string actual = input.ExtractRegex();

        // Assert
        Assert.Equal(expected, actual);
    }
}