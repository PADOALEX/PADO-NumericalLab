namespace Pado.NumericalLab.Core.Manual;

/// <summary>Hand-written educational algorithms retained from both C# laboratory projects.</summary>
public static class ManualMath
{
    public const double Pi = 3.141592653589793238462643383279502884;
    public const double TwoPi = 2 * Pi;
    public const double HalfPi = Pi / 2;
    private static readonly double Ln2 = LogSeries(2);

    public static double Abs(double x) => x < 0 ? -x : x;
    public static bool IsFinite(double x) => double.IsFinite(x);
    public static void RequireFinite(double x, string name)
    {
        if (!IsFinite(x)) throw new ArgumentOutOfRangeException(name, "A finite number is required.");
    }
    public static void AddKahan(ref double sum, ref double compensation, double term)
    {
        double corrected = term - compensation;
        double next = sum + corrected;
        compensation = (next - sum) - corrected;
        sum = next;
    }

    /// <summary>Angle range deliberately matches the original trig laboratory: |x| ≤ 1,000,000.</summary>
    public static double ReduceAngle(double x)
    {
        RequireFinite(x, nameof(x));
        if (Abs(x) > 1_000_000) throw new ArgumentOutOfRangeException(nameof(x), "Angle exceeds the documented range.");
        long turns = (long)(x / TwoPi);
        x -= turns * TwoPi;
        if (x > Pi) x -= TwoPi;
        if (x < -Pi) x += TwoPi;
        return x;
    }

    public static double Sin(double x)
    {
        x = ReduceAngle(x);
        if (x > HalfPi) x = Pi - x;
        if (x < -HalfPi) x = -Pi - x;
        double term = x, sum = x;
        for (int k = 1; k < 25; k++)
        {
            term *= -(x * x) / ((2.0 * k) * (2.0 * k + 1));
            sum += term;
        }
        return sum;
    }
    public static double Cos(double x)
    {
        x = ReduceAngle(x);
        double sign = 1;
        if (x > HalfPi) { x = Pi - x; sign = -1; }
        if (x < -HalfPi) { x = -Pi - x; sign = -1; }
        double term = 1, sum = 1;
        for (int k = 1; k < 25; k++)
        {
            term *= -(x * x) / ((2.0 * k - 1) * (2.0 * k));
            sum += term;
        }
        return sign * sum;
    }

    /// <summary>Taylor-series exponential; intentionally limited to the original laboratory domain [-2,2].</summary>
    public static double Exp(double x)
    {
        RequireFinite(x, nameof(x));
        if (Abs(x) > 2) throw new ArgumentOutOfRangeException(nameof(x), "Manual Exp supports [-2,2].");
        bool negative = x < 0;
        if (negative) x = -x;
        double term = 1, sum = 1;
        for (int k = 1; k < 60; k++) { term *= x / k; sum += term; }
        return negative ? 1 / sum : sum;
    }

    /// <summary>Argument reduction and the atanh series, copied in method from the original interpolation laboratory.</summary>
    public static double NaturalLog(double x)
    {
        RequireFinite(x, nameof(x));
        if (x <= 0) throw new ArgumentOutOfRangeException(nameof(x), "Logarithm requires x > 0.");
        int exponent = 0;
        while (x >= 2) { x *= 0.5; exponent++; }
        while (x < 1) { x *= 2; exponent--; }
        double result = exponent * Ln2 + LogSeries(x);
        if (!double.IsFinite(result)) throw new ArithmeticException("Logarithm overflow.");
        return result;
    }
    private static double LogSeries(double value)
    {
        double z = (value - 1) / (value + 1), z2 = z * z, power = z;
        double sum = 0, correction = 0;
        for (int k = 0; k < 100; k++)
        {
            double term = power / (2 * k + 1);
            AddKahan(ref sum, ref correction, term);
            if (Abs(term) < 1e-17) return 2 * sum;
            power *= z2;
        }
        throw new ArithmeticException("Log series did not converge.");
    }
}
