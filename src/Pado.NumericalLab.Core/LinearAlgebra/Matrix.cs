namespace Pado.NumericalLab.Core.LinearAlgebra;

/// <summary>Rectangular dense matrix, copied on construction, with pivoted Gauss-Jordan inverse.</summary>
public sealed class Matrix
{
    private readonly double[,] _data;
    public int Rows => _data.GetLength(0);
    public int Columns => _data.GetLength(1);
    public double this[int row, int column] => _data[row, column];

    public Matrix(double[,] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.GetLength(0) == 0 || data.GetLength(1) == 0) throw new ArgumentException("Matrix must be nonempty.");
        _data = (double[,])data.Clone();
        foreach (double x in _data) CheckFinite(x);
    }
    public double[,] ToArray() => (double[,])_data.Clone();
    public static Matrix Identity(int n)
    {
        if (n < 1) throw new ArgumentOutOfRangeException(nameof(n));
        var data = new double[n, n];
        for (int i = 0; i < n; i++) data[i, i] = 1;
        return new Matrix(data);
    }
    public Matrix Add(Matrix other) => Elementwise(other, (x, y) => x + y);
    public Matrix Subtract(Matrix other) => Elementwise(other, (x, y) => x - y);
    private Matrix Elementwise(Matrix other, Func<double, double, double> op)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Rows != other.Rows || Columns != other.Columns) throw new ArgumentException("Matrix shapes must match.");
        var output = new double[Rows, Columns];
        for (int i = 0; i < Rows; i++) for (int j = 0; j < Columns; j++) output[i, j] = op(this[i, j], other[i, j]);
        return new Matrix(output);
    }
    public Matrix Multiply(Matrix other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Columns != other.Rows) throw new ArgumentException("Incompatible matrix dimensions.");
        var output = new double[Rows, other.Columns];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < other.Columns; j++)
            {
                double sum = 0, correction = 0;
                for (int k = 0; k < Columns; k++)
                    Pado.NumericalLab.Core.Manual.ManualMath.AddKahan(ref sum, ref correction, this[i, k] * other[k, j]);
                output[i, j] = sum;
            }
        return new Matrix(output);
    }
    public double[] Multiply(IReadOnlyList<double> vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        if (Columns != vector.Count) throw new ArgumentException("Incompatible vector dimensions.");
        foreach (double x in vector) CheckFinite(x);
        var result = new double[Rows];
        for (int i = 0; i < Rows; i++)
        {
            double sum = 0, correction = 0;
            for (int k = 0; k < Columns; k++)
                Pado.NumericalLab.Core.Manual.ManualMath.AddKahan(ref sum, ref correction, this[i, k] * vector[k]);
            result[i] = sum;
            CheckFinite(sum);
        }
        return result;
    }

    /// <summary>A / B means A * inverse(B), as in the original Python matrix laboratory.</summary>
    public Matrix Divide(Matrix other) => Multiply(other.Inverse());

    public Matrix Inverse(double pivotTolerance = 1e-14)
    {
        if (Rows != Columns) throw new ArgumentException("Only square matrices can be inverted.");
        if (!double.IsFinite(pivotTolerance) || pivotTolerance <= 0) throw new ArgumentOutOfRangeException(nameof(pivotTolerance));
        int n = Rows;
        var a = ToArray();
        var inverse = Identity(n).ToArray();
        double norm = 0;
        for (int i = 0; i < n; i++)
        {
            double rowSum = 0;
            for (int j = 0; j < n; j++) rowSum += Math.Abs(a[i, j]);
            norm = Math.Max(norm, rowSum);
        }
        if (!double.IsFinite(norm)) throw new ArithmeticException("Norm overflow.");
        for (int col = 0; col < n; col++)
        {
            int pivot = col;
            for (int r = col + 1; r < n; r++)
                if (Math.Abs(a[r, col]) > Math.Abs(a[pivot, col])) pivot = r;
            if (Math.Abs(a[pivot, col]) <= pivotTolerance * norm)
                throw new InvalidOperationException("Singular or numerically singular matrix.");
            if (pivot != col)
                for (int j = 0; j < n; j++)
                {
                    (a[pivot, j], a[col, j]) = (a[col, j], a[pivot, j]);
                    (inverse[pivot, j], inverse[col, j]) = (inverse[col, j], inverse[pivot, j]);
                }
            double divisor = a[col, col];
            for (int j = 0; j < n; j++) { a[col, j] /= divisor; inverse[col, j] /= divisor; }
            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = a[row, col];
                for (int j = 0; j < n; j++) { a[row, j] -= factor * a[col, j]; inverse[row, j] -= factor * inverse[col, j]; }
            }
        }
        return new Matrix(inverse);
    }
    internal static void CheckFinite(double x)
    {
        if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x), "Nonfinite matrix arithmetic.");
    }
}
