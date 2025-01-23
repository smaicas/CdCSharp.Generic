using CdCSharp.Strings;
using static CdCSharp.Strings.StringExtensions;

namespace CdCSharp.Generic.UnitTests.Strings_Tests;

public class StringExtensions_Transform
{
    [Fact]
    public void Transform_ShouldConvertStringToCamelCaseWithHyphens()
    {
        // Arrange
        string input = "hello-world-example";
        string expected = "helloWorldExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldConvertStringToCamelCaseWithSpaces()
    {
        // Arrange
        string input = "hello world example";
        string expected = "helloWorldExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldConvertStringToCamelCaseWithUnderscores()
    {
        // Arrange
        string input = "hello_world_example";
        string expected = "helloWorldExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldConvertStringToUppercase()
    {
        // Arrange
        string input = "hello world";
        string expected = "HELLO WORLD";

        // Act
        string actual = input.Transform<UppercaseTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldHandleEmptyString()
    {
        // Arrange
        string input = string.Empty;
        string expected = string.Empty;

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldHandleMixedDelimiters()
    {
        // Arrange
        string input = "hello-world_example   example";
        string expected = "helloWorldExampleExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldHandleSingleWord()
    {
        // Arrange
        string input = "example";
        string expected = "example";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldHandleUppercaseWordsCorrectly()
    {
        // Arrange
        string input = "HELLO WORLD EXAMPLE";
        string expected = "helloWorldExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldIgnoreExtraSpacesAndNonAlphanumericCharacters()
    {
        // Arrange
        string input = " hello   world!!  example   ";
        string expected = "helloWorldExample";

        // Act
        string actual = input.Transform<CamelCaseStringTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldReturnInputUnchangedForIdentityTransformer()
    {
        // Arrange
        string input = "no change expected";
        string expected = "no change expected";

        // Act
        string actual = input.Transform<IdentityTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldReverseString()
    {
        // Arrange
        string input = "abcd";
        string expected = "dcba";

        // Act
        string actual = input.Transform<ReverseTransformer>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Transform_ShouldThrowForMissingParameterlessConstructor()
    {
        // Arrange
        string input = "test";

        // Act & Assert
        Assert.Throws<MissingMethodException>(() => input.Transform<InvalidTransformer>());
    }

    // Helper class to simulate a transformer without a parameterless constructor
}

internal class InvalidTransformer : IStringTransformer
{
    private readonly string _parameter;

    public InvalidTransformer(string parameter)
    { _parameter = parameter; _parameter.Replace("a", "b"); }

    public string Transform(string input) => input;
}

internal class ReverseTransformer : IStringTransformer
{
    public string Transform(string input) => new(input.Reverse().ToArray());
}

internal class UppercaseTransformer : IStringTransformer
{
    public string Transform(string input) => input.ToUpper();
}

internal class IdentityTransformer : IStringTransformer
{
    public string Transform(string input) => input;
}