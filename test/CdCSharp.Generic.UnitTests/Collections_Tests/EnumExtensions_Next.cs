using CdCSharp.Generic.Collections;

namespace CdCSharp.Generic.UnitTests.Collections_Tests;

public class EnumExtensions_Next
{
    private enum TestEnum
    { First, Second, Third }

    [Fact]
    public void Next_ShouldReturnNextEnumValue()
    {
        // Arrange
        TestEnum current = TestEnum.First;
        TestEnum expected = TestEnum.Second;

        // Act
        TestEnum result = current.Next();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Next_ShouldWrapToFirstEnumValue()
    {
        // Arrange
        TestEnum current = TestEnum.Third;
        TestEnum expected = TestEnum.First;

        // Act
        TestEnum result = current.Next();

        // Assert
        Assert.Equal(expected, result);
    }
}