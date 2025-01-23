namespace CdCSharp.Generic.Maths.ExpressionEvaluator;

internal class Function
{
    public Function(string name, int argumentCount, Func<double[], double> implementation)
    {
        Name = name;
        ArgumentCount = argumentCount;
        Implementation = implementation;
    }

    public int ArgumentCount { get; }
    public Func<double[], double> Implementation { get; }
    public string Name { get; }
}