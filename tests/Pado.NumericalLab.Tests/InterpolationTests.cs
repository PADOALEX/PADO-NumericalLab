using Pado.NumericalLab.Core.Experiments;
using Pado.NumericalLab.Core.Interpolation;
using Xunit;

namespace Pado.NumericalLab.Tests;

public class InterpolationTests
{
    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    [InlineData(0.5, 0.25)]
    [InlineData(1, 1)]
    public void QuadraticIsExact(double x, double expected)
    {
        var p = new PolynomialInterpolation(new double[] { -1, 0, 1 }, new double[] { 1, 0, 1 });
        Assert.Equal(expected, p.Lagrange(x), 12);
        Assert.Equal(expected, p.Newton(x), 12);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(30)]
    public void BothMethodsInterpolateEveryNode(int n)
    {
        double[] nodes = InterpolationGrid.EquidistantNodes(-1, 1, n);
        double[] values = nodes.Select(ExperimentRunner.Runge).ToArray();
        var p = new PolynomialInterpolation(nodes, values);
        for (int i = 0; i < nodes.Length; i++)
        {
            Assert.Equal(values[i], p.Lagrange(nodes[i]));
            Assert.Equal(values[i], p.Newton(nodes[i]));
        }
    }
    [Fact]
    public void Legacy1001GridIsIndependentFromInterpolationNodes()
    {
        var r = ExperimentRunner.Polynomial("Runge", ExperimentRunner.Runge, 5, 1001);
        Assert.Equal(1001, r.PlotPoints);
        Assert.Equal(1001, r.Lagrange.X.Length);
        Assert.Equal(6, InterpolationGrid.EquidistantNodes(-1, 1, 5).Length);
        Assert.Equal(-1, r.Lagrange.X[0]); Assert.Equal(1, r.Lagrange.X[^1]);
    }
    [Fact]
    public void FunctionsAreComparedOnExactlySameGrid()
    {
        var r = ExperimentRunner.Polynomial("log", ExperimentRunner.Logarithmic, 10);
        Assert.Equal(r.Lagrange.X, r.Newton.X);
        Assert.Equal(r.Lagrange.Exact, r.Newton.Exact);
        Assert.True(r.Lagrange.MaxError >= 0);
        Assert.True(r.Newton.MaxError >= 0);
    }
    [Fact]
    public void DuplicateNodesAreRejected() =>
        Assert.Throws<ArgumentException>(() => new PolynomialInterpolation(new double[] { 0, 0 }, new double[] { 1, 2 }));
    [Fact]
    public void NonfiniteInputsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new PolynomialInterpolation(new double[] { 0, double.NaN }, new double[] { 1, 2 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PolynomialInterpolation(new double[] { 0, 1 }, new double[] { 0, 1 }).Newton(double.PositiveInfinity));
    }
    [Fact]
    public void InvalidGridIsRejected()
    {
        Assert.Throws<ArgumentException>(() => InterpolationGrid.Grid(2, 2, 1001));
        Assert.Throws<ArgumentOutOfRangeException>(() => InterpolationGrid.Grid(0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => InterpolationGrid.Grid(0, 1, 1_000_001));
    }
}
