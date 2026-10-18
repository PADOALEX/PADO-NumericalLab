using Pado.NumericalLab.Core.Manual;
using Xunit;

namespace Pado.NumericalLab.Tests;

public class ManualMathTests
{
    [Theory]
    [InlineData(-2)]
    [InlineData(-1)]
    [InlineData(-0.2)]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(2)]
    public void ExponentialMatchesStandardReferenceOnDocumentedDomain(double x) =>
        Assert.InRange(Math.Abs(ManualMath.Exp(x) - Math.Exp(x)), 0, 1e-12);
    [Theory]
    [InlineData(0)]
    [InlineData(0.17)]
    [InlineData(1.05)]
    [InlineData(-2.8)]
    [InlineData(500)]
    public void TrigonometryAgreesWithStandardReference(double x)
    {
        Assert.InRange(Math.Abs(ManualMath.Sin(x) - Math.Sin(x)), 0, 1e-12);
        Assert.InRange(Math.Abs(ManualMath.Cos(x) - Math.Cos(x)), 0, 1e-12);
        double norm = ManualMath.Sin(x) * ManualMath.Sin(x) + ManualMath.Cos(x) * ManualMath.Cos(x);
        Assert.InRange(Math.Abs(norm - 1), 0, 1e-12);
    }
    [Theory]
    [InlineData(0.000001)]
    [InlineData(0.2)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(1e20)]
    public void LogarithmMatchesStandardReference(double x) =>
        Assert.InRange(Math.Abs(ManualMath.NaturalLog(x) - Math.Log(x)), 0, 1e-12);
    [Fact]
    public void TrigonometricPeriodicity()
    {
        for (int i = -200; i <= 200; i++)
        {
            double x = i / 37.0;
            Assert.InRange(Math.Abs(ManualMath.Sin(x) - ManualMath.Sin(x + 8 * ManualMath.TwoPi)), 0, 1e-12);
        }
    }
    [Fact]
    public void InvalidInputsAreExplicitlyRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.Sin(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.Cos(double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.Sin(1_000_001));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.Exp(2.01));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.NaturalLog(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManualMath.NaturalLog(-1));
    }
}
