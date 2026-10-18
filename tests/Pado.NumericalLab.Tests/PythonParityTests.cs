using System.Text.Json;
using Pado.NumericalLab.Core.LinearAlgebra;
using Pado.NumericalLab.Core.Statistics;
using Xunit;

namespace Pado.NumericalLab.Tests;

/// <summary>Golden outputs produced by executing original Python functions on fixed inputs (docs/reference/python-golden.json).</summary>
public class PythonParityTests
{
    private static JsonDocument Load() => JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "python-golden.json")));
    private static double[] Vector(JsonElement x) => x.EnumerateArray().Select(v => v.GetDouble()).ToArray();
    private static double[,] MatrixArray(JsonElement x)
    {
        var rows = x.EnumerateArray().ToArray();
        int n = rows[0].GetArrayLength();
        var matrix = new double[rows.Length, n];
        for (int i = 0; i < rows.Length; i++)
            for (int j = 0; j < n; j++) matrix[i, j] = rows[i][j].GetDouble();
        return matrix;
    }
    private static void Near(double actual, double expected, double absoluteTolerance = 1e-10) =>
        Assert.InRange(Math.Abs(actual - expected), 0, absoluteTolerance);
    private static void NearVector(IReadOnlyList<double> actual, IReadOnlyList<double> expected, double tol = 1e-10)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++) Near(actual[i], expected[i], tol);
    }
    private static void NearMatrix(Matrix actual, double[,] expected)
    {
        Assert.Equal(expected.GetLength(0), actual.Rows); Assert.Equal(expected.GetLength(1), actual.Columns);
        for (int i = 0; i < actual.Rows; i++) for (int j = 0; j < actual.Columns; j++) Near(actual[i, j], expected[i, j]);
    }
    [Fact]
    public void VectorAddAndHadamardMatchPython()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("vectors");
        var a = Vector(c.GetProperty("left")); var b = Vector(c.GetProperty("right"));
        NearVector(Vectors.Add(a, b), Vector(c.GetProperty("sum")));
        NearVector(Vectors.MultiplyComponents(a, b), Vector(c.GetProperty("hadamard")));
    }
    [Fact]
    public void MatrixOperationsMatchPython()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("matrices");
        var a = new Matrix(MatrixArray(c.GetProperty("left")));
        var b = new Matrix(MatrixArray(c.GetProperty("right")));
        NearMatrix(a.Add(b), MatrixArray(c.GetProperty("sum")));
        NearMatrix(a.Multiply(b), MatrixArray(c.GetProperty("product")));
        NearMatrix(b.Inverse(), MatrixArray(c.GetProperty("inverse_right")));
        NearMatrix(a.Divide(b), MatrixArray(c.GetProperty("division")));
    }
    [Fact]
    public void DescriptiveStatisticsMatchPython()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("statistics");
        var s = DescriptiveStatistics.Analyze(Vector(c.GetProperty("samples")));
        Near(s.Mean, c.GetProperty("mean").GetDouble());
        Near(s.Median, c.GetProperty("median").GetDouble());
        Near(s.Range, c.GetProperty("rho").GetDouble());
        Near(s.PopulationVariance, c.GetProperty("d_var").GetDouble());
        Near(s.SampleVariance, c.GetProperty("s2").GetDouble());
        Near(s.SampleStdDev, c.GetProperty("s").GetDouble());
        Near(s.Skewness, c.GetProperty("gamma1").GetDouble());
        Near(s.ExcessKurtosis, c.GetProperty("gamma2").GetDouble());
        NearVector(s.Modes, Vector(c.GetProperty("mode")));
    }
    [Fact]
    public void HouseholderSolutionMatchesPython()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("householder");
        var output = HouseholderSolver.Solve(new Matrix(MatrixArray(c.GetProperty("a"))), Vector(c.GetProperty("b")));
        NearVector(output, Vector(c.GetProperty("solution")));
    }
    [Fact]
    public void SweepMatchesOriginalPythonNumbers()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("sweep");
        NearVector(TridiagonalSolver.SolveOriginalBoundaryProblem(c.GetProperty("intervals").GetInt32()), Vector(c.GetProperty("numerical")), 1e-10);
    }
    [Fact]
    public void PowerMethodEigenvalueMatchesPythonButConvergenceIsFixed()
    {
        using var d = Load(); var c = d.RootElement.GetProperty("power");
        var result = PowerIteration.Dominant(new Matrix(MatrixArray(c.GetProperty("a"))), Vector(c.GetProperty("v")));
        Near(result.Eigenvalue, c.GetProperty("eigenvalue").GetDouble(), 1e-8);
        Assert.True(result.Iterations > c.GetProperty("iterations").GetInt32());
        Assert.InRange(result.ResidualInfinityNorm, 0, 1e-9);
    }
}
