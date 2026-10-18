using Pado.NumericalLab.Cli.Reporting;
using Pado.NumericalLab.Cli.Tools;
using Pado.NumericalLab.Core.Experiments;
using Pado.NumericalLab.Core.LinearAlgebra;
using Pado.NumericalLab.Core.Statistics;
using Xunit;

namespace Pado.NumericalLab.Tests;

public class BoundaryAndReportingTests
{
    [Fact]
    public void PivotedInverseHandlesZeroOnDiagonal()
    {
        var a = new Matrix(new double[,] { { 0, 1 }, { 1, 0 } });
        var inverse = a.Inverse();
        Assert.Equal(1, inverse[0, 1]); Assert.Equal(1, inverse[1, 0]);
    }
    [Fact]
    public void SingularMatrixAndBadDimensionsAreRejected()
    {
        Assert.Throws<InvalidOperationException>(() => new Matrix(new double[,] { { 1, 2 }, { 2, 4 } }).Inverse());
        Assert.Throws<ArgumentException>(() => new Matrix(new double[,] { { 1, 2 } }).Multiply(new Matrix(new double[,] { { 2, 3 } })));
        Assert.Throws<ArgumentException>(() => Vectors.Add(new[] { 1.0 }, new[] { 1.0, 2.0 }));
        Assert.Throws<DivideByZeroException>(() => Vectors.DivideComponents(new[] { 1.0 }, new[] { 0.0 }));
        Assert.Throws<InvalidOperationException>(() => HouseholderSolver.Solve(new Matrix(new double[,] { { 1, 2 }, { 2, 4 } }), new double[] { 3, 6 }));
    }
    [Fact]
    public void InvalidOrConstantStatisticsAreHandledExplicitly()
    {
        Assert.Throws<ArgumentException>(() => DescriptiveStatistics.Analyze(new[] { 1.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DescriptiveStatistics.Analyze(new[] { 0.0, double.NaN }));
        var constant = DescriptiveStatistics.Analyze(new[] { 3.0, 3.0, 3.0 });
        Assert.Equal(0, constant.PopulationVariance);
        Assert.Equal(0, constant.Skewness);
        Assert.Equal(0, constant.ExcessKurtosis);
        var zeroMean = DescriptiveStatistics.Analyze(new[] { -1.0, 1.0 });
        Assert.Null(zeroMean.CoefficientOfVariationPercent);
    }
    [Fact]
    public void GroupingCountsEveryObservationIncludingMaxBoundary()
    {
        var result = DescriptiveStatistics.GroupedCounts(new double[] { 0, 0.5, 1.0, 2 }, 2);
        Assert.Equal(4, result.Frequencies.Sum());
        Assert.Equal(2, result.Frequencies[0]);
        Assert.Equal(2, result.Frequencies[1]);
    }
    [Fact]
    public void CalculatorAndShapesRemainIndependentDemonstrations()
    {
        Assert.Equal(8, Calculator.Calculate(2, "^", 3));
        Assert.Throws<DivideByZeroException>(() => Calculator.Calculate(2, "/", 0));
        Assert.InRange(Math.Abs(ShapeParser.Parse("circle 2").Area - 4 * Math.PI), 0, 1e-12);
        Assert.Equal(12, ShapeParser.Parse("rectangle 3 4").Area);
        Assert.Throws<ArgumentOutOfRangeException>(() => ShapeParser.Parse("square NaN"));
    }
    [Fact]
    public void HtmlSvgAndCsvAreGeneratedWithIndependent1001PointGrid()
    {
        string dir = Path.Combine(Path.GetTempPath(), "pado-numerical-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var r = ExperimentRunner.Polynomial("Runge", ExperimentRunner.Runge, 5, 1001);
            ReportExporter.WriteHtml(dir, "Plot & <analysis>", new[] { ("runge", r.Lagrange) });
            string csv = File.ReadAllText(Path.Combine(dir, "runge.csv"));
            string svg = File.ReadAllText(Path.Combine(dir, "runge.svg"));
            string html = File.ReadAllText(Path.Combine(dir, "report.html"));
            Assert.Equal(1002, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
            Assert.Contains("<svg", svg); Assert.Contains("polyline", svg);
            Assert.Contains("&lt;analysis&gt;", html); Assert.Contains("runge.svg", html);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
    [Fact]
    public void SweepMatchesBoundaryConditionsAndExactSolutionOrder()
    {
        double[] y10 = TridiagonalSolver.SolveOriginalBoundaryProblem(10);
        double[] y100 = TridiagonalSolver.SolveOriginalBoundaryProblem(100);
        Assert.Equal(1, y10[0]); Assert.Equal(0.5, y10[^1]);
        double max10 = y10.Select((v, i) => Math.Abs(v - 1 / (1 + i / 10.0))).Max();
        double max100 = y100.Select((v, i) => Math.Abs(v - 1 / (1 + i / 100.0))).Max();
        Assert.True(max100 < max10);
    }
}
