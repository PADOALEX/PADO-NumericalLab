namespace Pado.NumericalLab.Cli.Tools;

/// <summary>Self-contained calculator demo. Not a dependency of the numerical core.</summary>
public static class Calculator
{
    public static double Calculate(double a, string operation, double b)
    {
        if (!double.IsFinite(a) || !double.IsFinite(b)) throw new ArgumentOutOfRangeException(nameof(a));
        double result = operation switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" when b == 0 => throw new DivideByZeroException(),
            "/" => a / b,
            "^" or "pow" => Math.Pow(a, b),
            _ => throw new ArgumentException("Supported operations: +, -, *, /, ^, pow.", nameof(operation))
        };
        if (!double.IsFinite(result)) throw new ArithmeticException("Calculator output is nonfinite.");
        return result;
    }
}
