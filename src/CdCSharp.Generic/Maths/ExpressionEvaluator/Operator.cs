namespace CdCSharp.Generic.Maths.ExpressionEvaluator;

internal class Operator
{
    public Operator(string symbol, int precedence, Associativity associativity, int arity, Func<double, double, double> operation)
    {
        Symbol = symbol;
        Precedence = precedence;
        Associativity = associativity;
        Arity = arity;
        Operation = operation;
    }

    public int Arity { get; }
    public Associativity Associativity { get; }
    public Func<double, double, double> Operation { get; }
    public int Precedence { get; }
    public string Symbol { get; }
}