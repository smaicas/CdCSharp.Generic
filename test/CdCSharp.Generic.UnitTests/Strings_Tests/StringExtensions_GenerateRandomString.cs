using CdCSharp.Strings;

namespace CdCSharp.Generic.UnitTests.Strings_Tests;

public class StringExtensions_GenerateRandomString
{
    [Fact]
    public void Should_Contain_Only_Alphanumeric_Characters()
    {
        // Arrange
        const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // Act
        string result = StringGenerator.GenerateRandomString(50);

        // Assert
        Assert.All(result, c => Assert.Contains(c, allowedChars));
    }

    [Fact]
    public void Should_Default_To_Length_Of_24_When_No_Length_Is_Provided()
    {
        // Act
        string result = StringGenerator.GenerateRandomString();

        // Assert
        Assert.Equal(24, result.Length);
    }

    [Fact]
    public void Should_Generate_String_With_Specified_Length()
    {
        // Arrange
        int length = 16;

        // Act
        string result = StringGenerator.GenerateRandomString(length);

        // Assert
        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void Should_Return_Different_Strings_For_Sequential_Calls()
    {
        // Act
        string result1 = StringGenerator.GenerateRandomString();
        string result2 = StringGenerator.GenerateRandomString();

        // Assert
        Assert.NotEqual(result1, result2); // Random values should not be equal in most cases
    }

    [Fact]
    public void Should_Return_Empty_String_When_Length_Is_Zero()
    {
        // Arrange
        int length = 0;

        // Act
        string result = StringGenerator.GenerateRandomString(length);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_Length_Is_Negative()
    {
        // Arrange
        int length = -1;

        // Act & Assert
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => StringGenerator.GenerateRandomString(length));
        Assert.Equal("Length must be non-negative. (Parameter 'length')", ex.Message);
    }
}