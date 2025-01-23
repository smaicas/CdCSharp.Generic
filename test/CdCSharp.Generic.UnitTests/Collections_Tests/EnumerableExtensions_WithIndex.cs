using CdCSharp.Generic.Collections;

namespace CdCSharp.Generic.UnitTests.Collections_Tests;

public class EnumerableExtensions_WithIndex
{
    [Fact]
    public void WithIndex_ShouldHandleEmptySource()
    {
        // Arrange
        IEnumerable<string> source = Enumerable.Empty<string>();

        // Act
        List<(string value, int index)> actual = source.WithIndex().ToList();

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void WithIndex_ShouldHandleNullSource()
    {
        // Arrange
        IEnumerable<string>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source!.WithIndex().ToList());
    }

    [Fact]
    public void WithIndex_ShouldHandleNumericSource()
    {
        // Arrange
        int[] source = new[] { 10, 20, 30 };
        (int, int)[] expected = new[]
        {
            (10, 0),
            (20, 1),
            (30, 2)
        };

        // Act
        List<(int value, int index)> actual = source.WithIndex().ToList();

        // Assert
        Assert.Equal(expected.Length, actual.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Item1, actual[i].value);
            Assert.Equal(expected[i].Item2, actual[i].index);
        }
    }

    [Fact]
    public void WithIndex_ShouldHandleSingleElementSource()
    {
        // Arrange
        string[] source = new[] { "single" };
        List<(string value, int index)> expected = [("single", 0)];

        // Act
        List<(string value, int index)> actual = source.WithIndex().ToList();

        // Assert
        Assert.Single(actual);
        Assert.Equal(expected[0].value, actual[0].value);
        Assert.Equal(expected[0].index, actual[0].index);
    }

    [Fact]
    public void WithIndex_ShouldReturnElementsWithIndices()
    {
        // Arrange
        string[] source = new[] { "a", "b", "c" };
        (string, int)[] expected = new[]
        {
            ("a", 0),
            ("b", 1),
            ("c", 2)
        };

        // Act
        List<(string value, int index)> actual = source.WithIndex().ToList();

        // Assert
        Assert.Equal(expected.Length, actual.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Item1, actual[i].value);
            Assert.Equal(expected[i].Item2, actual[i].index);
        }
    }
}