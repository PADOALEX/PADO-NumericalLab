namespace Pado.NumericalLab.Core.LinearAlgebra;

/// <summary>Square linear system solver based on the original CMLA Householder reflections.</summary>
public static class HouseholderSolver
{
    public static double[] Solve(Matrix matrix, IReadOnlyList<double> rhs, double tolerance = 1e-12)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        ArgumentNullException.ThrowIfNull(rhs);
        if (matrix.Rows != matrix.Columns || rhs.Count != matrix.Rows) throw new ArgumentException("Expected a square matrix and matching right side.");
        if (!double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));
        var a = matrix.ToArray();
        var b = rhs.ToArray();
        foreach (double value in b) Matrix.CheckFinite(value);
        int n = b.Length;
        for (int col = 0; col < n - 1; col++)
        {
            double normSquared = 0;
            for (int i = col; i < n; i++) normSquared += a[i, col] * a[i, col];
            double norm = Math.Sqrt(normSquared);
            if (!double.IsFinite(norm) || norm <= tolerance) throw new InvalidOperationException("Rank-deficient linear system.");
            double sign = a[col, col] >= 0 ? 1 : -1;
            var v = new double[n - col];
            for (int i = col; i < n; i++) v[i - col] = a[i, col];
            v[0] += sign * norm;
            double vv = Vectors.Dot(v, v);
            if (vv <= tolerance * tolerance) throw new InvalidOperationException("Degenerate reflection.");
            for (int j = col; j < n; j++)
            {
                double dot = 0;
                for (int i = col; i < n; i++) dot += v[i - col] * a[i, j];
                double factor = 2 * dot / vv;
                for (int i = col; i < n; i++) a[i, j] -= factor * v[i - col];
            }
            double dotB = 0;
            for (int i = col; i < n; i++) dotB += v[i - col] * b[i];
            for (int i = col; i < n; i++) b[i] -= 2 * dotB / vv * v[i - col];
        }
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            if (Math.Abs(a[i, i]) <= tolerance) throw new InvalidOperationException("Singular linear system.");
            double value = b[i];
            for (int j = i + 1; j < n; j++) value -= a[i, j] * x[j];
            x[i] = value / a[i, i];
            Matrix.CheckFinite(x[i]);
        }
        return x;
    }
}
