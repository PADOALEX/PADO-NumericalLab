namespace Pado.NumericalLab.Core.LinearAlgebra;

/// <summary>Thomas / right-sweep method, including the source CHMLA_2 boundary-value experiment.</summary>
public static class TridiagonalSolver
{
    /// <param name="lower">Subdiagonal, lower[0] is ignored.</param>
    /// <param name="diagonal">Main diagonal.</param>
    /// <param name="upper">Superdiagonal, upper[n-1] is ignored.</param>
    public static double[] Solve(double[] lower, double[] diagonal, double[] upper, double[] rhs)
    {
        ArgumentNullException.ThrowIfNull(lower); ArgumentNullException.ThrowIfNull(diagonal);
        ArgumentNullException.ThrowIfNull(upper); ArgumentNullException.ThrowIfNull(rhs);
        int n = diagonal.Length;
        if (n == 0 || lower.Length != n || upper.Length != n || rhs.Length != n)
            throw new ArgumentException("All diagonals and rhs must have matching nonzero lengths.");
        foreach (double[] array in new[] { lower, diagonal, upper, rhs })
            foreach (double value in array) Matrix.CheckFinite(value);
        var c = new double[n]; var d = new double[n];
        double pivot = diagonal[0];
        if (pivot == 0) throw new InvalidOperationException("Zero pivot in tridiagonal sweep.");
        c[0] = n > 1 ? upper[0] / pivot : 0;
        d[0] = rhs[0] / pivot;
        for (int i = 1; i < n; i++)
        {
            pivot = diagonal[i] - lower[i] * c[i - 1];
            if (pivot == 0 || !double.IsFinite(pivot)) throw new InvalidOperationException("Zero/nonfinite pivot in tridiagonal sweep.");
            c[i] = i < n - 1 ? upper[i] / pivot : 0;
            d[i] = (rhs[i] - lower[i] * d[i - 1]) / pivot;
        }
        var result = new double[n];
        result[^1] = d[^1];
        for (int i = n - 2; i >= 0; i--) result[i] = d[i] - c[i] * result[i + 1];
        foreach (double value in result) Matrix.CheckFinite(value);
        return result;
    }

    /// <summary>Original y(0)=1, y(1)=0.5 experiment; n is the number of intervals.</summary>
    public static double[] SolveOriginalBoundaryProblem(int intervals)
    {
        if (intervals < 2 || intervals > 100_000) throw new ArgumentOutOfRangeException(nameof(intervals));
        double h = 1.0 / intervals;
        int unknowns = intervals - 1;
        var lower = new double[unknowns]; var diag = new double[unknowns];
        var upper = new double[unknowns]; var rhs = new double[unknowns];
        for (int i = 1; i < intervals; i++)
        {
            double x = i * h, factor = 1 + x;
            double l = 1 / (h * h) + factor / (2 * h);
            double u = 1 / (h * h) - factor / (2 * h);
            int index = i - 1;
            diag[index] = 1 + 2 / (h * h);
            lower[index] = i == 1 ? 0 : -l;
            upper[index] = i == intervals - 1 ? 0 : -u;
            rhs[index] = -2 / (factor * factor * factor);
            if (i == 1) rhs[index] += l;
            if (i == intervals - 1) rhs[index] += u * 0.5;
        }
        var interior = Solve(lower, diag, upper, rhs);
        var result = new double[intervals + 1];
        result[0] = 1; result[^1] = 0.5;
        Array.Copy(interior, 0, result, 1, unknowns);
        return result;
    }
}
