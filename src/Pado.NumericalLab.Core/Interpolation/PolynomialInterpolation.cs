namespace Pado.NumericalLab.Core.Interpolation;

/// <summary>Direct Lagrange form and Newton divided differences (both intentionally retained).</summary>
public sealed class PolynomialInterpolation
{
    private readonly double[] _x;
    private readonly double[] _y;
    private readonly double[] _newton;
    public IReadOnlyList<double> Nodes => Array.AsReadOnly(_x);
    public IReadOnlyList<double> Values => Array.AsReadOnly(_y);

    public PolynomialInterpolation(IReadOnlyList<double> nodes, IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(values);
        if (nodes.Count == 0 || nodes.Count != values.Count) throw new ArgumentException("Node/value lengths must match and be nonempty.");
        _x = nodes.ToArray(); _y = values.ToArray();
        for (int i = 0; i < _x.Length; i++)
        {
            if (!double.IsFinite(_x[i]) || !double.IsFinite(_y[i])) throw new ArgumentException("Nodes and values must be finite.");
            for (int j = i + 1; j < _x.Length; j++)
                if (_x[i] == _x[j]) throw new ArgumentException("Duplicate interpolation nodes.");
        }
        _newton = (double[])_y.Clone();
        for (int order = 1; order < _x.Length; order++)
            for (int i = _x.Length - 1; i >= order; i--)
            {
                _newton[i] = (_newton[i] - _newton[i - 1]) / (_x[i] - _x[i - order]);
                EnsureFinite(_newton[i]);
            }
    }

    public double Lagrange(double x)
    {
        RequireX(x);
        for (int i = 0; i < _x.Length; i++) if (x == _x[i]) return _y[i];
        double result = 0;
        for (int i = 0; i < _x.Length; i++)
        {
            double basis = 1;
            for (int j = 0; j < _x.Length; j++)
                if (i != j) basis *= (x - _x[j]) / (_x[i] - _x[j]);
            result += _y[i] * basis;
        }
        EnsureFinite(result);
        return result;
    }

    public double Newton(double x)
    {
        RequireX(x);
        for (int i = 0; i < _x.Length; i++) if (x == _x[i]) return _y[i];
        double result = _newton[^1];
        for (int i = _newton.Length - 2; i >= 0; i--)
            result = result * (x - _x[i]) + _newton[i];
        EnsureFinite(result);
        return result;
    }

    private static void RequireX(double x)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
    }
    private static void EnsureFinite(double v)
    {
        if (!double.IsFinite(v)) throw new ArithmeticException("Interpolation overflow or undefined result.");
    }
}

public static class InterpolationGrid
{
    /// <summary>n = number of subintervals; returns n+1 interpolation nodes.</summary>
    public static double[] EquidistantNodes(double a, double b, int n) => Grid(a, b, checked(n + 1));
    /// <summary>count = number of plot points, independently chosen from node count.</summary>
    public static double[] Grid(double a, double b, int count)
    {
        if (!double.IsFinite(a) || !double.IsFinite(b) || !(a < b)) throw new ArgumentException("Expected finite a < b.");
        if (count < 2 || count > 1_000_000) throw new ArgumentOutOfRangeException(nameof(count));
        double step = (b - a) / (count - 1);
        if (!double.IsFinite(step) || step <= 0) throw new ArgumentException("Unrepresentable interval.");
        var points = new double[count];
        for (int i = 0; i < count; i++) points[i] = i == count - 1 ? b : a + i * step;
        for (int i = 0; i < count; i++) if (!double.IsFinite(points[i])) throw new ArgumentException("Grid overflow.");
        return points;
    }
}
