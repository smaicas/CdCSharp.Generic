using CdCSharp.Generic.Maths.ExpressionEvaluator;

namespace CdCSharp.Generic.UnitTests.Maths_Tests;

public class EvaluatorTests
{
    private readonly Evaluator _evaluator;

    public EvaluatorTests() => _evaluator = new Evaluator();

    #region Basic Arithmetic Operations

    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("5 * 6", 30)]
    [InlineData("20 / 4", 5)]
    [InlineData("8 % 3", 2)]
    public void BasicArithmeticOperations_ShouldEvaluateCorrectly(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Basic Arithmetic Operations

    #region Operator Precedence and Associativity

    [Theory]
    [InlineData("2 + 3 * 4", 14)]
    [InlineData("2 + 3 * 4 - 5", 9)]
    [InlineData("2 + 3 * 4 ^ 2", 50)]
    [InlineData("2 ^ 3 ^ 2", 512)] // 2^(3^2) = 2^9 = 512
    public void OperatorPrecedence_ShouldRespectPrecedenceAndAssociativity(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Operator Precedence and Associativity

    #region Parentheses and Grouping

    [Theory]
    [InlineData("(2 + 3) * 4", 20)]
    [InlineData("((2 + 3) * (4 + 5))", 45)]
    [InlineData("(2 + (3 * 4))", 14)]
    [InlineData("((2 + 3) * 4) + 5", 25)]
    public void Parentheses_ShouldHandleGroupingCorrectly(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Parentheses and Grouping

    #region Unary Operators

    [Theory]
    [InlineData("-5 + 3", -2)]
    [InlineData("-(5 + 3)", -8)]
    [InlineData("-5 * -3", 15)]
    [InlineData("5!", 120)]
    [InlineData("3! + 2!", 8)]
    public void UnaryOperators_ShouldEvaluateCorrectly(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Unary Operators

    #region Predefined Mathematical Functions

    [Theory]
    [InlineData("sin(pi / 2)", 1)]
    [InlineData("cos(0)", 1)]
    [InlineData("tan(pi / 4)", 1)]
    [InlineData("sqrt(16)", 4)]
    [InlineData("log(e)", 1)]
    [InlineData("abs(-5)", 5)]
    [InlineData("pow(2, 3)", 8)]
    [InlineData("max(10, 20)", 20)]
    [InlineData("min(10, 20)", 10)]
    public void PredefinedFunctions_ShouldEvaluateCorrectly(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Predefined Mathematical Functions

    #region Functions with Multiple Arguments

    [Theory]
    [InlineData("pow(2, 3)", 8)]
    [InlineData("pow(5, 2)", 25)]
    [InlineData("max(1, 2)", 2)]
    [InlineData("min(1, 2)", 1)]
    [InlineData("pow(2, pow(3, 2))", 512)] // pow(2, 9) = 512
    [InlineData("pow(pow(2, 3), 2)", 64)] // pow(8, 2) = 64
    public void MultiArgumentFunctions_ShouldEvaluateCorrectly(string expression, double expected)
    {
        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result, precision: 10);
    }

    #endregion Functions with Multiple Arguments

    #region Variables

    [Fact]
    public void UndefinedVariables_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("x + 5"));
        Assert.Equal("Unknown token: x", exception.Message);
    }

    [Fact]
    public void Variables_CanBeOverwritten()
    {
        // Arrange
        _evaluator.SetVariable("x", 10);
        _evaluator.SetVariable("x", 20);

        // Act
        double result = _evaluator.Evaluate("x + 5");

        // Assert
        Assert.Equal(25, result, precision: 10);
    }

    [Fact]
    public void Variables_ShouldEvaluateCorrectly()
    {
        // Arrange
        _evaluator.SetVariable("x", 10);
        _evaluator.SetVariable("y", 5);

        // Act
        double result = _evaluator.Evaluate("x + y * 2");

        // Assert
        Assert.Equal(20, result, precision: 10);
    }

    #endregion Variables

    #region User-Defined Functions

    [Fact]
    public void RemovingUserDefinedFunction_ShouldThrowExceptionWhenUsed()
    {
        // Arrange
        _evaluator.DefineFunction("square", 1, args => Math.Pow(args[0], 2));
        _evaluator.RemoveFunction("square");

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("square(3)"));
        Assert.Equal("Unknown token: square", exception.Message);
    }

    [Fact]
    public void UserDefinedFunctions_CanBeNested()
    {
        // Arrange
        _evaluator.DefineFunction("square", 1, args => Math.Pow(args[0], 2));
        _evaluator.DefineFunction("cube", 1, args => Math.Pow(args[0], 3));

        // Act
        double result = _evaluator.Evaluate("cube(square(2))"); // cube(4) = 64

        // Assert
        Assert.Equal(64, result, precision: 10);
    }

    [Fact]
    public void UserDefinedFunctions_ShouldEvaluateCorrectly()
    {
        // Arrange
        _evaluator.DefineFunction("cube", 1, args => Math.Pow(args[0], 3));

        // Act
        double result = _evaluator.Evaluate("cube(3)");

        // Assert
        Assert.Equal(27, result, precision: 10);
    }

    [Fact]
    public void UserDefinedFunctions_WithIncorrectArgumentCount_ShouldThrowException()
    {
        // Arrange
        _evaluator.DefineFunction("add", 2, args => args[0] + args[1]);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("add(5)"));
        Assert.Equal("Function 'add' requires 2 arguments.", exception.Message);
    }

    [Fact]
    public void UserDefinedFunctions_WithMultipleArguments_ShouldEvaluateCorrectly()
    {
        // Arrange
        _evaluator.DefineFunction("add", 2, args => args[0] + args[1]);

        // Act
        double result = _evaluator.Evaluate("add(10, 20)");

        // Assert
        Assert.Equal(30, result, precision: 10);
    }

    #endregion User-Defined Functions

    #region Error Handling and Exceptions

    [Fact]
    public void DivisionByZero_ShouldThrowDivideByZeroException()
    {
        // Act & Assert
        DivideByZeroException exception = Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("10 / 0"));
        Assert.Equal("Attempted to divide by zero.", exception.Message);
    }

    [Fact]
    public void FactorialOfNegativeNumber_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("(-5)!"));
        Assert.Equal("Factorial is not defined for negative numbers.", exception.Message);
    }

    [Fact]
    public void InvalidCharacters_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("2 + 3a"));
        Assert.Equal("Unknown token: a", exception.Message);
    }

    [Fact]
    public void InvalidExpression_FunctionIncorrectArgumentCount_ShouldThrowException()
    {
        // Arrange
        string expression = "sqrt(16, 9)";

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(expression));
        Assert.Equal("The entered expression is invalid.", exception.Message);
    }

    [Fact]
    public void InvalidExpression_IncompleteExpression_ShouldThrowException()
    {
        // Arrange
        string expression = "5 +";

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(expression));
        Assert.Equal("Operator '+' requires 2 argument(s).", exception.Message);
    }

    [Fact]
    public void InvalidExpression_TwoOperators_ShouldThrowException()
    {
        // Arrange
        string expression = "2 + * 3";

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(expression));
        Assert.Equal("Operator '*' cannot follow operator '+'.", exception.Message);
    }

    [Fact]
    public void InvalidExpression_UnbalancedParentheses_ShouldThrowException()
    {
        // Arrange
        string expression = "((2 + 3)";

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(expression));
        Assert.Equal("Unbalanced parentheses in the expression.", exception.Message);
    }

    [Fact]
    public void InvalidExpression_UnknownFunction_ShouldThrowException()
    {
        // Arrange
        string expression = "unknownFunc(5)";

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(expression));
        Assert.Equal("Unknown token: unknownFunc", exception.Message);
    }

    [Fact]
    public void MultipleDecimalPoints_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("3..5 + 2"));
        Assert.Equal("Invalid number with multiple decimal points.", exception.Message);
    }

    #endregion Error Handling and Exceptions

    #region Edge and Boundary Cases

    [Fact]
    public void EmptyExpression_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(""));
        Assert.Equal("The entered expression is invalid.", exception.Message);
    }

    [Fact]
    public void ExpressionWithConsecutiveOperators_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("2 ++ 3"));
        Assert.Equal("Operator '+' cannot follow operator '+'.", exception.Message);
    }

    [Fact]
    public void ExpressionWithMultipleUnaryOperators_ShouldEvaluateCorrectly()
    {
        // Arrange
        string expression = "--5"; // Equivalent to 5

        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(5, result, precision: 10);
    }

    [Fact]
    public void ExpressionWithOnlyOperators_ShouldThrowException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate("+-*/"));
        Assert.Equal("Operator '/' cannot follow operator '*'.", exception.Message);
    }

    [Fact]
    public void ExpressionWithSpaces_ShouldEvaluateCorrectly()
    {
        // Arrange
        string expression = " 2   +   3 *    4 ";

        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(14, result, precision: 10);
    }

    [Fact]
    public void LargeNumbers_ShouldEvaluateCorrectly()
    {
        // Arrange
        string expression = "1e308 + 1e308"; // This will result in Infinity

        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(double.PositiveInfinity, result);
    }

    [Fact]
    public void NestedFunctions_ShouldEvaluateCorrectly()
    {
        // Arrange
        _evaluator.DefineFunction("cube", 1, args => Math.Pow(args[0], 3));
        _evaluator.DefineFunction("double", 1, args => args[0] * 2);

        string expression = "double(cube(2))"; // double(8) = 16

        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(16, result, precision: 10);
    }

    [Fact]
    public void VeryDeeplyNestedExpressions_ShouldHandleRecursion()
    {
        // Arrange Create an expression with 100 levels of parentheses
        string expression = "";
        for (int i = 0; i < 100; i++)
            expression += "(";
        expression += "1";
        for (int i = 0; i < 100; i++)
            expression += ")";

        // Act
        double result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(1, result, precision: 10);
    }

    #endregion Edge and Boundary Cases
}