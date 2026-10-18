namespace Pado.NumericalLab.Core.Statistics;

public sealed record StatisticsResult(
    int Count, double Mean, double Median, double[] Modes, double Range,
    double PopulationVariance, double SampleVariance, double SampleStdDev,
    double Skewness, double ExcessKurtosis, double? CoefficientOfVariationPercent);

/// <summary>Descriptive measures ported from MatStat/main.py; explicit n≥2 for sample statistics.</summary>
public static class DescriptiveStatistics
{
    public static StatisticsResult Analyze(IReadOnlyList<double> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        int n = data.Count;
        if (n < 2) throw new ArgumentException("At least two observations are required.");
        var sorted = data.ToArray();
        foreach (double v in sorted) if (!double.IsFinite(v)) throw new ArgumentOutOfRangeException(nameof(data));
        Array.Sort(sorted);
        double mean = Sum(data) / n;
        double mu2 = 0, mu3 = 0, mu4 = 0;
        foreach (double x in data)
        {
            double d = x - mean, s = d * d;
            mu2 += s; mu3 += s * d; mu4 += s * s;
        }
        mu2 /= n; mu3 /= n; mu4 /= n;
        double sample = mu2 * n / (n - 1);
        double skewness = mu2 == 0 ? 0 : mu3 / Math.Pow(mu2, 1.5);
        double excess = mu2 == 0 ? 0 : mu4 / (mu2 * mu2) - 3;
        var frequency = new SortedDictionary<double, int>();
        foreach (double x in sorted) frequency[x] = frequency.TryGetValue(x, out int count) ? count + 1 : 1;
        int maxCount = frequency.Values.Max();
        double[] modes = frequency.Where(item => item.Value == maxCount).Select(item => item.Key).ToArray();
        double median = n % 2 == 0 ? sorted[n / 2 - 1] / 2 + sorted[n / 2] / 2 : sorted[n / 2];
        double standardDeviation = Math.Sqrt(sample);
        var result = new StatisticsResult(n, mean, median, modes, sorted[^1] - sorted[0], mu2, sample,
            standardDeviation, skewness, excess, mean == 0 ? null : standardDeviation / mean * 100);
        foreach (double x in new[] { result.Mean, result.Median, result.Range, result.PopulationVariance,
            result.SampleVariance, result.SampleStdDev, result.Skewness, result.ExcessKurtosis })
            if (!double.IsFinite(x)) throw new ArithmeticException("Statistical overflow.");
        return result;
    }
    private static double Sum(IReadOnlyList<double> data)
    {
        double sum = 0, compensation = 0;
        foreach (double x in data) Pado.NumericalLab.Core.Manual.ManualMath.AddKahan(ref sum, ref compensation, x);
        return sum;
    }

    /// <summary>Sturges-style grouped counts; first k-1 bins are half open, last is closed.</summary>
    public static (double[] Boundaries, int[] Frequencies) GroupedCounts(IReadOnlyList<double> data, int? bins = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Count < 2) throw new ArgumentException("Expected at least two values.");
        foreach (double x in data) if (!double.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(data));
        int k = bins ?? Math.Max(5, (int)(1 + 3.322 * Math.Log10(data.Count)));
        if (k < 1 || k > 10_000) throw new ArgumentOutOfRangeException(nameof(bins));
        double min = data.Min(), max = data.Max();
        double width = (max - min) / k;
        if (width == 0) width = 1;
        if (!double.IsFinite(width)) throw new ArithmeticException("Unrepresentable grouping range.");
        var boundaries = new double[k + 1];
        for (int i = 0; i <= k; i++) boundaries[i] = min + i * width;
        var counts = new int[k];
        foreach (double v in data)
        {
            int index = min == max ? 0 : v == max ? k - 1 : (int)((v - min) / width);
            counts[Math.Clamp(index, 0, k - 1)]++;
        }
        return (boundaries, counts);
    }
}
