using CdCSharp.Generic.Collections;

namespace CdCSharp.Generic.UnitTests.Collections_Tests;

public class AsyncEnumerableExtensions_ToListAsync
{
    [Fact]
    public async Task ToListAsync_ShouldReturnEmptyList_WhenSourceIsEmpty()
    {
        // Arrange
        IAsyncEnumerable<int> source = GetEmptyAsyncEnumerable<int>();

        // Act
        List<int> result = await source.ToListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ToListAsync_ShouldReturnAllElements_WhenSourceHasElements()
    {
        // Arrange
        IAsyncEnumerable<int> source = GetAsyncEnumerable(new[] { 1, 2, 3, 4, 5 });

        // Act
        List<int> result = await source.ToListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public async Task ToListAsync_ShouldThrowArgumentNullException_WhenSourceIsNull()
    {
        // Arrange
        IAsyncEnumerable<int>? source = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => source!.ToListAsync());
    }

    [Fact]
    public async Task ToListAsync_ShouldSupportLargeDataSets()
    {
        // Arrange
        const int largeDataSetSize = 1000;
        IAsyncEnumerable<int> source = GetAsyncEnumerable(Enumerable.Range(1, largeDataSetSize));

        // Act
        List<int> result = await source.ToListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largeDataSetSize, result.Count);
        Assert.Equal(1, result.First());
        Assert.Equal(largeDataSetSize, result.Last());
    }

    [Fact]
    public async Task ToListAsync_ShouldPreserveOrderOfElements()
    {
        // Arrange
        IAsyncEnumerable<int> source = GetAsyncEnumerable(new[] { 10, 20, 30, 40, 50 });

        // Act
        List<int> result = await source.ToListAsync();

        // Assert
        Assert.Equal(new[] { 10, 20, 30, 40, 50 }, result);
    }

    // Helper methods for generating IAsyncEnumerable<T>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    private static async IAsyncEnumerable<T> GetEmptyAsyncEnumerable<T>()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        yield break;
    }

    private static async IAsyncEnumerable<T> GetAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (T? item in items)
        {
            await Task.Delay(1); // Simulate async operation
            yield return item;
        }
    }
}