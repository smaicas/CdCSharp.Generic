using CdCSharp.Generic.Collections;

namespace CdCSharp.Generic.UnitTests.Collections_Tests;

public class EnumExtensions_Prev
{
    private enum TestEnum
    { First, Second, Third }

    [Fact]
    public void Prev_ShouldReturnPreviousEnumValue()
    {
        // Arrange
        TestEnum current = TestEnum.Second;
        TestEnum expected = TestEnum.First;

        // Act
        TestEnum result = current.Prev();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Prev_ShouldWrapToLastEnumValue()
    {
        // Arrange
        TestEnum current = TestEnum.First;
        TestEnum expected = TestEnum.Third;

        // Act
        TestEnum result = current.Prev();

        // Assert
        Assert.Equal(expected, result);
    }
}