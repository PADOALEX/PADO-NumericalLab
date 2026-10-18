using Pado.NumericalLab.Core.Manual;

namespace Pado.NumericalLab.Core.Trigonometry;

/// <summary>Odd-node discrete Fourier interpolant: N=2n+1, nodes t_j=2πj/N.</summary>
public sealed class TrigonometricInterpolator
{
    public const int MaxDegree = 100;
    private readonly double[] _nodes;
    private readonly double[] _values;
    private readonly double[] _a;
    private readonly double[] _b;
    public int Degree { get; }
    public int NodeCount => _nodes.Length;
    public double GetNode(int index) => _nodes[index];
    public double GetValue(int index) => _values[index];
    public double GetA(int k) => _a[k];
    public double GetB(int k) => _b[k];

    public TrigonometricInterpolator(int degree, Func<double, double> function)
    {
        if (degree < 1 || degree > MaxDegree) throw new ArgumentOutOfRangeException(nameof(degree));
        ArgumentNullException.ThrowIfNull(function);
        Degree = degree;
        int n = 2 * degree + 1;
        _nodes = new double[n]; _values = new double[n];
        _a = new double[degree + 1]; _b = new double[degree + 1];
        for (int j = 0; j < n; j++)
        {
            _nodes[j] = ManualMath.TwoPi * j / n;
            _values[j] = function(_nodes[j]);
            if (!double.IsFinite(_values[j])) throw new ArgumentException("Function returned NaN/Infinity.", nameof(function));
        }
        for (int k = 0; k <= degree; k++)
        {
            double a = 0, b = 0, ac = 0, bc = 0;
            for (int j = 0; j < n; j++)
            {
                double angle = k * _nodes[j];
                ManualMath.AddKahan(ref a, ref ac, _values[j] * ManualMath.Cos(angle));
                if (k > 0) ManualMath.AddKahan(ref b, ref bc, _values[j] * ManualMath.Sin(angle));
            }
            _a[k] = 2 * a / n;
            _b[k] = k == 0 ? 0 : 2 * b / n;
            if (!double.IsFinite(_a[k]) || !double.IsFinite(_b[k])) throw new ArithmeticException("Fourier coefficient overflow.");
        }
    }

    public double Evaluate(double t)
    {
        t = ManualMath.ReduceAngle(t);
        double sum = _a[0] / 2, correction = 0;
        for (int k = 1; k <= Degree; k++)
            ManualMath.AddKahan(ref sum, ref correction,
                _a[k] * ManualMath.Cos(k * t) + _b[k] * ManualMath.Sin(k * t));
        if (!double.IsFinite(sum)) throw new ArithmeticException("Trigonometric interpolation overflow.");
        return sum;
    }
}

public static class TrigonometricTestFunctions
{
    public static double ExpSinCos(double t) => ManualMath.Exp(ManualMath.Sin(t) + ManualMath.Cos(t));
    public static double Cos15(double t) => 3 * ManualMath.Cos(15 * t);
}
