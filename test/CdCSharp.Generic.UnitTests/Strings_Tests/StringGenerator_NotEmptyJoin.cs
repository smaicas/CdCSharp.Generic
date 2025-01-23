using CdCSharp.Strings;

namespace CdCSharp.Generic.UnitTests.Strings_Tests;

public class StringGenerator_NotEmptyJoin
{
    [Fact]
    public void Should_Handle_Collection_With_All_Valid_Strings()
    {
        // Arrange
        IReadOnlyCollection<string> strings = ["This", "is", "a", "test"];

        // Act
        string result = strings.NotEmptyJoin();

        // Assert
        Assert.Equal("This is a test", result);
    }

    [Fact]
    public void Should_Handle_Collection_With_Single_Non_Empty_String()
    {
        // Arrange
        IReadOnlyCollection<string?> strings = ["", "   ", "Single", null];

        // Act
        string result = strings.NotEmptyJoin();

        // Assert
        Assert.Equal("Single", result);
    }

    [Fact]
    public void Should_Join_Only_Non_Empty_Strings_With_Custom_Separator()
    {
        // Arrange
        IReadOnlyCollection<string?> strings = ["Apple", " ", "Banana", null, "Cherry"];
        string separator = ", ";

        // Act
        string result = strings.NotEmptyJoin(separator);

        // Assert
        Assert.Equal("Apple, Banana, Cherry", result);
    }

    [Fact]
    public void Should_Join_Only_Non_Empty_Strings_With_Default_Separator()
    {
        // Arrange
        IReadOnlyCollection<string?> strings = ["Hello", "", "World", " ", null, "!"];

        // Act
        string result = strings.NotEmptyJoin();

        // Assert
        Assert.Equal("Hello World !", result);
    }

    [Fact]
    public void Should_Return_Empty_String_When_All_Elements_Are_Whitespace_Or_Null()
    {
        // Arrange
        IReadOnlyCollection<string?> strings = [" ", null, "\t", ""];

        // Act
        string result = strings.NotEmptyJoin();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Should_Return_Empty_String_When_Collection_Is_Empty()
    {
        // Arrange
        IReadOnlyCollection<string> strings = [];

        // Act
        string result = strings.NotEmptyJoin();

        // Assert
        Assert.Equal(string.Empty, result);
    }
}