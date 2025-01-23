using System.Globalization;

namespace CdCSharp.Generic.Maths.ExpressionEvaluator;

public class Evaluator
{
    #region Operator and Function Definitions

    private static readonly Dictionary<string, Function> Functions = new()
        {
            { "sin", new Function("sin", 1, args => Math.Sin(args[0])) },
            { "cos", new Function("cos", 1, args => Math.Cos(args[0])) },
            { "tan", new Function("tan", 1, args => Math.Tan(args[0])) },
            { "sqrt", new Function("sqrt", 1, args => Math.Sqrt(args[0])) },
            { "log", new Function("log", 1, args => Math.Log(args[0])) },
            { "abs", new Function("abs", 1, args => Math.Abs(args[0])) },
            { "pow", new Function("pow", 2, args => Math.Pow(args[0], args[1])) },
            { "max", new Function("max", 2, args => Math.Max(args[0], args[1])) },
            { "min", new Function("min", 2, args => Math.Min(args[0], args[1])) },
            // Add more functions as needed
        };

    private static readonly Dictionary<string, Operator> Operators = new()
        {
            // Binary Operators
            { "+", new Operator("+", 2, Associativity.Left, 2, (a, b) => a + b) },
            { "-", new Operator("-", 2, Associativity.Left, 2, (a, b) => a - b) },
            { "*", new Operator("*", 3, Associativity.Left, 2, (a, b) => a * b) },
            { "/", new Operator("/", 3, Associativity.Left, 2, (a, b) => {
                if (b == 0)
                    throw new DivideByZeroException("Attempted to divide by zero.");
                return a / b;
            }) },
            { "^", new Operator("^", 4, Associativity.Right, 2, Math.Pow) },
            { "%", new Operator("%", 3, Associativity.Left, 2, (a, b) => {
                if (b == 0)
                    throw new DivideByZeroException("Attempted to modulo by zero.");
                return a % b;
            }) },

            // Unary Operators
            { "u-", new Operator("u-", 5, Associativity.Right, 1, (a, b) => -a) }, // Unary Minus
            { "!", new Operator("!", 6, Associativity.Left, 1, (a, b) => Factorial(a)) } // Factorial
        };

    #endregion Operator and Function Definitions

    #region Member Variables

    // Caching expressions already evaluated
    private readonly Dictionary<string, List<string>> ExpressionCache = [];

    private readonly Dictionary<string, Function> UserDefinedFunctions = [];
    private readonly Dictionary<string, double> Variables = [];

    #endregion Member Variables

    #region Constructor

    public Evaluator()
    {
        // Define predefined constants
        SetVariable("pi", Math.PI);
        SetVariable("e", Math.E);
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Defines a user-defined function.
    /// </summary>
    /// <param name="name">
    /// Function name.
    /// </param>
    /// <param name="argumentCount">
    /// Number of arguments.
    /// </param>
    /// <param name="implementation">
    /// Function implementation.
    /// </param>
    public void DefineFunction(string name, int argumentCount, Func<double[], double> implementation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The function name cannot be empty.", nameof(name));

        if (argumentCount < 0)
            throw new ArgumentException("The number of arguments cannot be negative.", nameof(argumentCount));

        ArgumentNullException.ThrowIfNull(implementation);

        name = name.ToLower();

        if (Functions.ContainsKey(name) || UserDefinedFunctions.ContainsKey(name))
            throw new ArgumentException($"Function '{name}' is already defined.");

        UserDefinedFunctions[name] = new Function(name, argumentCount, implementation);
    }

    /// <summary>
    /// Evaluates a mathematical expression and returns the result.
    /// </summary>
    /// <param name="expression">
    /// The expression to evaluate.
    /// </param>
    /// <returns>
    /// The result of the expression.
    /// </returns>
    public double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("The entered expression is invalid.");

        if (ExpressionCache.TryGetValue(expression, out List<string>? rpn))
            return EvaluateRPN(rpn);

        List<string> tokens = Tokenize(expression);
        List<string> rpnList = ConvertToRPN(tokens);
        ExpressionCache[expression] = rpnList; // Cache the expression
        return EvaluateRPN(rpnList);
    }

    /// <summary>
    /// Gets the value of a defined variable.
    /// </summary>
    /// <param name="name">
    /// Variable name.
    /// </param>
    /// <returns>
    /// Variable value.
    /// </returns>
    public double GetVariable(string name)
    {
        if (Variables.TryGetValue(name, out double value))
            return value;
        throw new ArgumentException($"Variable '{name}' is not defined.");
    }

    /// <summary>
    /// Removes a user-defined function.
    /// </summary>
    /// <param name="name">
    /// Function name.
    /// </param>
    public void RemoveFunction(string name)
    {
        name = name.ToLower();

        if (!UserDefinedFunctions.Remove(name))
            throw new ArgumentException($"Function '{name}' is not defined as a user function.");
    }

    /// <summary>
    /// Defines a variable for use in expressions.
    /// </summary>
    /// <param name="name">
    /// Variable name.
    /// </param>
    /// <param name="value">
    /// Variable value.
    /// </param>
    public void SetVariable(string name, double value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The variable name cannot be empty.", nameof(name));

        Variables[name] = value;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Calculates the factorial of a number.
    /// </summary>
    private static double Factorial(double a)
    {
        if (a < 0)
            throw new ArgumentException("Factorial is not defined for negative numbers.");

        if (a is 0 or 1)
            return 1;

        double result = 1;
        for (int i = 2; i <= (int)a; i++)
        {
            result *= i;
            if (double.IsInfinity(result))
                throw new OverflowException("Factorial result exceeds the range of double.");
        }
        return result;
    }

    /// <summary>
    /// Tokenizes the input expression into a list of tokens.
    /// </summary>
    private static List<string> Tokenize(string expression)
    {
        List<string> tokens = [];
        int i = 0;
        bool expectUnary = true; // At the start, expect a unary operator

        while (i < expression.Length)
        {
            char c = expression[i];

            // Ignore white spaces
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // Number (includes decimals and scientific notation)
            if (char.IsDigit(c) || c == '.')
            {
                string number = "";
                bool hasDecimal = false;
                bool hasExponent = false;
                int startPos = i;

                while (i < expression.Length &&
                       (char.IsDigit(expression[i]) || expression[i] == '.' ||
                       expression[i] == 'e' || expression[i] == 'E' ||
                       hasExponent && (expression[i] == '+' || expression[i] == '-')))
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal)
                            throw new ArgumentException("Invalid number with multiple decimal points.");
                        if (hasExponent)
                            throw new ArgumentException("Invalid number format: decimal point in exponent.");
                        hasDecimal = true;
                    }
                    else if (expression[i] is 'e' or 'E')
                    {
                        if (hasExponent)
                            throw new ArgumentException("Invalid number with multiple exponents.");
                        hasExponent = true;
                        number += expression[i];
                        i++;
                        // After 'e' or 'E', there can be a '+' or '-'
                        if (i < expression.Length && (expression[i] == '+' || expression[i] == '-'))
                        {
                            number += expression[i];
                            i++;
                        }
                        continue;
                    }
                    else if (hasExponent && (expression[i] == '+' || expression[i] == '-'))
                        // '+' or '-' already handled above
                        break;
                    number += expression[i];
                    i++;
                }

                // Validate the number format using double.TryParse
                if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    throw new ArgumentException($"Invalid number format at position {startPos + 1}.");

                tokens.Add(number);
                expectUnary = false;
                continue;
            }

            // Variable or Function
            if (char.IsLetter(c))
            {
                string name = "";
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                {
                    name += expression[i];
                    i++;
                }
                tokens.Add(name);
                expectUnary = false;
                continue;
            }

            // Operator or Parenthesis
            if (Operators.ContainsKey(c.ToString()) || c == '(' || c == ')' || c == ',')
            {
                // Handle unary operators
                if ((c == '-' || c == '+') && expectUnary)
                {
                    string unarySymbol = c == '-' ? "u-" : "u+";
                    if (Operators.ContainsKey(unarySymbol))
                    {
                        tokens.Add(unarySymbol);
                        i++;
                        continue;
                    }
                }

                // Add operator or parenthesis
                tokens.Add(c.ToString());
                i++;

                // Determine if the next token can be a unary operator
                if (c == '(' || Operators.ContainsKey(c.ToString()) || c == ',')
                    expectUnary = true;
                else
                    expectUnary = false;
                continue;
            }

            // Invalid character
            throw new ArgumentException($"Invalid character in expression: '{c}' at position {i + 1}.");
        }

        return tokens;
    }

    /// <summary>
    /// Converts a list of tokens from infix to Reverse Polish Notation (RPN) using the Shunting
    /// Yard algorithm.
    /// </summary>
    private List<string> ConvertToRPN(List<string> tokens)
    {
        List<string> outputQueue = [];
        Stack<string> operatorStack = new();

        string? previousToken = null;

        foreach (string token in tokens)
        {
            // Number or Variable
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _) || Variables.ContainsKey(token))
                outputQueue.Add(token);
            // Function
            else if (Functions.ContainsKey(token.ToLower()) || UserDefinedFunctions.ContainsKey(token.ToLower()))
                operatorStack.Push(token.ToLower());
            // Argument Separator
            else if (token == ",")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    outputQueue.Add(operatorStack.Pop());
                if (operatorStack.Count == 0 || operatorStack.Peek() != "(")
                    throw new ArgumentException("Argument separator out of place or missing parenthesis.");
            }
            // Operator
            else if (Operators.TryGetValue(token, out Operator? op))
            {
                // Additional validation: Detect consecutive binary operators
                if (previousToken != null && Operators.TryGetValue(previousToken, out Operator? prevOp))
                    if (prevOp.Arity == 2 && op.Arity == 2)
                        throw new ArgumentException($"Operator '{op.Symbol}' cannot follow operator '{prevOp.Symbol}'.");

                while (operatorStack.Count > 0 && Operators.ContainsKey(operatorStack.Peek()))
                {
                    Operator o2 = Operators[operatorStack.Peek()];
                    if (op.Associativity == Associativity.Left && op.Precedence <= o2.Precedence ||
                        op.Associativity == Associativity.Right && op.Precedence < o2.Precedence)
                        outputQueue.Add(operatorStack.Pop());
                    else
                        break;
                }
                operatorStack.Push(token);
            }
            // Left Parenthesis
            else if (token == "(")
                operatorStack.Push(token);
            // Right Parenthesis
            else if (token == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    outputQueue.Add(operatorStack.Pop());
                if (operatorStack.Count == 0)
                    throw new ArgumentException("Unbalanced parentheses.");
                operatorStack.Pop(); // Remove '('

                // If the token at the top of the stack is a function, pop it to the output
                if (operatorStack.Count > 0 && (Functions.ContainsKey(operatorStack.Peek()) || UserDefinedFunctions.ContainsKey(operatorStack.Peek())))
                    outputQueue.Add(operatorStack.Pop());
            }
            else
                throw new ArgumentException($"Unknown token: {token}");

            previousToken = token;
        }

        while (operatorStack.Count > 0)
        {
            string op = operatorStack.Pop();
            if (op is "(" or ")")
                throw new ArgumentException("Unbalanced parentheses in the expression.");
            outputQueue.Add(op);
        }

        return outputQueue;
    }

    /// <summary>
    /// Evaluates an expression in Reverse Polish Notation (RPN).
    /// </summary>
    private double EvaluateRPN(List<string> rpn)
    {
        Stack<double> stack = new();

        foreach (string token in rpn)
            // Number
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                stack.Push(number);
            // Variable
            else if (Variables.TryGetValue(token, out double value))
                stack.Push(value);
            // Function
            else if (Functions.ContainsKey(token) || UserDefinedFunctions.ContainsKey(token))
            {
                Function func = Functions.TryGetValue(token, out Function? funcValue) ? funcValue : UserDefinedFunctions[token];
                if (stack.Count < func.ArgumentCount)
                    throw new ArgumentException($"Function '{func.Name}' requires {func.ArgumentCount} arguments.");
                double[] args = new double[func.ArgumentCount];
                for (int i = func.ArgumentCount - 1; i >= 0; i--)
                    args[i] = stack.Pop();
                double result = func.Implementation(args);
                stack.Push(result);
            }
            // Operator
            else if (Operators.TryGetValue(token, out Operator? op))
            {
                if (stack.Count < op.Arity)
                    throw new ArgumentException($"Operator '{op.Symbol}' requires {op.Arity} argument(s).");

                double a = stack.Pop();
                double b = op.Arity == 2 ? stack.Pop() : 0; // 'b' is unused for unary operators

                try
                {
                    double result = op.Operation(op.Arity == 2 ? b : a, a);
                    stack.Push(result);
                }
                catch (DivideByZeroException)
                {
                    throw;
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error evaluating operator '{op.Symbol}': {ex.Message}");
                }
            }
            else
                throw new ArgumentException($"Unknown token during evaluation: {token}");

        if (stack.Count != 1)
            throw new ArgumentException("The entered expression is invalid.");

        return stack.Pop();
    }

    #endregion Private Methods
}