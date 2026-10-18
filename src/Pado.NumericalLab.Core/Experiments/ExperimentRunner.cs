using Pado.NumericalLab.Core.Interpolation;
using Pado.NumericalLab.Core.Manual;
using Pado.NumericalLab.Core.Trigonometry;

namespace Pado.NumericalLab.Core.Experiments;

public sealed record Curve(string Name, double[] X, double[] Exact, double[] Approximated, double MaxError, double MaxNodeError = 0);
public sealed record PolynomialRun(string Function, int N, int PlotPoints, Curve Lagrange, Curve Newton);
public sealed record TrigonometricRun(string Function, int Degree, int NodeCount, int PlotPoints, Curve Curve, double EndpointError);

public static class ExperimentRunner
{
    public static double Runge(double x) => 1 / (1 + 25 * x * x);
    public static double Logarithmic(double x) => ManualMath.NaturalLog(x + 2);

    public static PolynomialRun Polynomial(string name, Func<double, double> function, int n, int plotPoints = 1001)
    {
        ArgumentNullException.ThrowIfNull(function);
        var nodes = InterpolationGrid.EquidistantNodes(-1, 1, n);
        var values = nodes.Select(function).ToArray();
        var interpolation = new PolynomialInterpolation(nodes, values);
        var grid = InterpolationGrid.Grid(-1, 1, plotPoints);
        var exact = grid.Select(function).ToArray();
        var lagrange = grid.Select(interpolation.Lagrange).ToArray();
        var newton = grid.Select(interpolation.Newton).ToArray();
        return new PolynomialRun(name, n, plotPoints,
            new Curve("Lagrange", grid, exact, lagrange, MaxError(exact, lagrange)),
            new Curve("Newton", grid, exact, newton, MaxError(exact, newton)));
    }
    public static TrigonometricRun Trigonometric(string name, Func<double, double> function, int n, int plotPoints = 1001)
    {
        ArgumentNullException.ThrowIfNull(function);
        var interpolation = new TrigonometricInterpolator(n, function);
        var grid = InterpolationGrid.Grid(0, ManualMath.TwoPi, plotPoints);
        var exact = grid.Select(function).ToArray();
        var interpolated = grid.Select(interpolation.Evaluate).ToArray();
        double nodeError = 0;
        for (int j = 0; j < interpolation.NodeCount; j++)
            nodeError = Math.Max(nodeError, Math.Abs(interpolation.GetValue(j) - interpolation.Evaluate(interpolation.GetNode(j))));
        return new TrigonometricRun(name, n, interpolation.NodeCount, plotPoints,
            new Curve("Trigonometric", grid, exact, interpolated, MaxError(exact, interpolated), nodeError),
            Math.Abs(exact[^1] - interpolated[^1]));
    }
    private static double MaxError(double[] exact, double[] estimate)
    {
        double worst = 0;
        for (int i = 0; i < exact.Length; i++)
        {
            double value = Math.Abs(exact[i] - estimate[i]);
            if (!double.IsFinite(value)) throw new ArithmeticException("Nonfinite interpolation error.");
            worst = Math.Max(worst, value);
        }
        return worst;
    }
}
