using Pado.NumericalLab.Core.Experiments;
using Pado.NumericalLab.Core.Manual;
using Pado.NumericalLab.Core.Trigonometry;
using Xunit;

namespace Pado.NumericalLab.Tests;

public class TrigonometricTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(100)]
    public void HistoricalSelfTestNodeAndEndpointChecks(int degree)
    {
        var a = ExperimentRunner.Trigonometric("f1", TrigonometricTestFunctions.ExpSinCos, degree, 1001);
        var b = ExperimentRunner.Trigonometric("f2", TrigonometricTestFunctions.Cos15, degree, 1001);
        Assert.InRange(a.Curve.MaxNodeError, 0, 1e-9);
        Assert.InRange(b.Curve.MaxNodeError, 0, 1e-9);
        Assert.InRange(a.EndpointError, 0, 1e-9);
        Assert.InRange(b.EndpointError, 0, 1e-9);
        Assert.Equal(2 * degree + 1, b.NodeCount);
        if (degree >= 15) Assert.InRange(b.Curve.MaxError, 0, 1e-9);
    }
    [Fact]
    public void KnownFourierCoefficientsAreRecovered()
    {
        var p = new TrigonometricInterpolator(4, x => 2 + 1.5 * ManualMath.Cos(3 * x) - .75 * ManualMath.Sin(2 * x));
        Assert.InRange(Math.Abs(p.GetA(0) - 4), 0, 1e-12);
        Assert.InRange(Math.Abs(p.GetA(3) - 1.5), 0, 1e-12);
        Assert.InRange(Math.Abs(p.GetB(2) + .75), 0, 1e-12);
        for (int i = 0; i < 1001; i++)
        {
            double x = ManualMath.TwoPi * i / 1000;
            Assert.InRange(Math.Abs(p.Evaluate(x) - (2 + 1.5 * ManualMath.Cos(3 * x) - .75 * ManualMath.Sin(2 * x))), 0, 1e-12);
        }
    }
    [Fact]
    public void AliasForDegreeSevenMatchesOriginalLaboratory()
    {
        var p = new TrigonometricInterpolator(7, TrigonometricTestFunctions.Cos15);
        for (int i = 0; i < 1001; i++)
            Assert.InRange(Math.Abs(p.Evaluate(ManualMath.TwoPi * i / 1000) - 3), 0, 1e-11);
        Assert.True(p.GetNode(14) < ManualMath.TwoPi);
    }
    [Fact]
    public void PeriodicAtBothEnds()
    {
        var p = new TrigonometricInterpolator(5, TrigonometricTestFunctions.ExpSinCos);
        Assert.InRange(Math.Abs(p.Evaluate(0) - p.Evaluate(ManualMath.TwoPi)), 0, 1e-12);
    }
    [Fact]
    public void RejectsNonfiniteFunctionOrInvalidDegree()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TrigonometricInterpolator(0, TrigonometricTestFunctions.Cos15));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TrigonometricInterpolator(101, TrigonometricTestFunctions.Cos15));
        Assert.Throws<ArgumentException>(() => new TrigonometricInterpolator(1, _ => double.NaN));
    }
}
