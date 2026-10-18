namespace Pado.NumericalLab.Core.LinearAlgebra;

/// <summary>Operations based on Python_Projects/Vector_class/vector.py, without file I/O.</summary>
public static class Vectors
{
    public static double[] Add(IReadOnlyList<double> a, IReadOnlyList<double> b) => Binary(a, b, (x, y) => x + y);
    public static double[] Subtract(IReadOnlyList<double> a, IReadOnlyList<double> b) => Binary(a, b, (x, y) => x - y);
    public static double[] MultiplyComponents(IReadOnlyList<double> a, IReadOnlyList<double> b) => Binary(a, b, (x, y) => x * y);
    public static double[] DivideComponents(IReadOnlyList<double> a, IReadOnlyList<double> b) => Binary(a, b, (x, y) => y == 0 ? throw new DivideByZeroException() : x / y);
    public static double[] Scale(IReadOnlyList<double> a, double factor)
    {
        ArgumentNullException.ThrowIfNull(a);
        Require(factor);
        if (a.Count == 0) throw new ArgumentException("Vector must be nonempty.");
        return a.Select(x => { Require(x); double result = x * factor; Require(result); return result; }).ToArray();
    }
    public static double[] Divide(IReadOnlyList<double> a, double scalar)
    {
        Require(scalar);
        if (scalar == 0) throw new DivideByZeroException();
        return Scale(a, 1 / scalar);
    }
    public static double Dot(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        Check(a, b);
        double result = 0, correction = 0;
        for (int i = 0; i < a.Count; i++)
        {
            Pado.NumericalLab.Core.Manual.ManualMath.AddKahan(ref result, ref correction, a[i] * b[i]);
            Require(result);
        }
        return result;
    }
    private static double[] Binary(IReadOnlyList<double> a, IReadOnlyList<double> b, Func<double, double, double> op)
    {
        Check(a, b);
        var result = new double[a.Count];
        for (int i = 0; i < a.Count; i++) { result[i] = op(a[i], b[i]); Require(result[i]); }
        return result;
    }
    private static void Check(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        ArgumentNullException.ThrowIfNull(a); ArgumentNullException.ThrowIfNull(b);
        if (a.Count == 0 || a.Count != b.Count) throw new ArgumentException("Vectors must have the same nonzero dimension.");
        foreach (double x in a) Require(x);
        foreach (double x in b) Require(x);
    }
    private static void Require(double x)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x), "Vector values must be finite.");
    }
}
