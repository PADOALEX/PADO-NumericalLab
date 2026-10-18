namespace Pado.NumericalLab.Core.LinearAlgebra;

public sealed record EigenResult(double Eigenvalue, double[] Eigenvector, int Iterations, double ResidualInfinityNorm);

/// <summary>Power method from CHMLA_3, with residual convergence rather than eigenvalue-only early exit.</summary>
public static class PowerIteration
{
    public static EigenResult Dominant(Matrix matrix, double[] initial, double tolerance = 1e-10, int maxIterations = 10_000)
    {
        ArgumentNullException.ThrowIfNull(matrix); ArgumentNullException.ThrowIfNull(initial);
        if (matrix.Rows != matrix.Columns || initial.Length != matrix.Rows) throw new ArgumentException("Expected square matrix and matching vector.");
        if (tolerance <= 0 || !double.IsFinite(tolerance) || maxIterations < 1) throw new ArgumentOutOfRangeException(nameof(tolerance));
        var v = (double[])initial.Clone();
        foreach (double x in v) Matrix.CheckFinite(x);
        double scale = v.Max(x => Math.Abs(x));
        if (scale == 0) throw new ArgumentException("Initial eigenvector cannot be zero.");
        for (int i = 0; i < v.Length; i++) v[i] /= scale;
        for (int iteration = 1; iteration <= maxIterations; iteration++)
        {
            var y = matrix.Multiply(v);
            int index = 0;
            for (int i = 1; i < y.Length; i++) if (Math.Abs(y[i]) > Math.Abs(y[index])) index = i;
            double dominant = y[index];
            if (dominant == 0) throw new InvalidOperationException("Cannot normalize a zero vector.");
            for (int i = 0; i < v.Length; i++) v[i] = y[i] / dominant;
            var av = matrix.Multiply(v);
            double lambda = Vectors.Dot(v, av) / Vectors.Dot(v, v);
            double residual = 0;
            for (int i = 0; i < v.Length; i++) residual = Math.Max(residual, Math.Abs(av[i] - lambda * v[i]));
            if (residual <= tolerance * Math.Max(1, Math.Abs(lambda)))
                return new EigenResult(lambda, v, iteration, residual);
        }
        throw new InvalidOperationException("Power method did not converge (e.g., no unique dominant eigenvalue).");
    }
}
