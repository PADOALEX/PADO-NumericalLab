using System.Globalization;
using Pado.NumericalLab.Cli.Reporting;
using Pado.NumericalLab.Cli.Tools;
using Pado.NumericalLab.Core.Experiments;
using Pado.NumericalLab.Core.LinearAlgebra;
using Pado.NumericalLab.Core.Statistics;
using Pado.NumericalLab.Core.Trigonometry;

namespace Pado.NumericalLab.Cli;

public static class Program
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h") { Help(); return 0; }
            switch (args[0])
            {
                case "interpolate": RunPolynomial(args); break;
                case "trig": RunTrigonometric(args); break;
                case "stats": RunStats(); break;
                case "algebra": RunAlgebra(); break;
                case "calc": RunCalc(args); break;
                case "shapes": RunShapes(args); break;
                default: throw new ArgumentException("Unknown command. Use help.");
            }
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or ArithmeticException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
    private static void Help()
    {
        Console.WriteLine("PADO NumericalLab (.NET 10)\n" +
            "  interpolate [--n 5,10,15] [--grid 1001] [--out results/polynomial]\n" +
            "  trig [--n 1,7,15] [--grid 1001] [--out results/trig]\n" +
            "  stats                       Statistical demonstration\n" +
            "  algebra                     Matrix, QR, power method and right sweep\n" +
            "  calc <a> <+|-|*|/|^|pow> <b>  Standalone calculator\n" +
            "  shapes [path-to-shapes.txt] Standalone geometry demonstration\n" +
            "Node count for polynomial n is n+1, while --grid is the independent number of plot points.");
    }
    private static string Option(string[] args, string name, string fallback)
    {
        int index = Array.IndexOf(args, name);
        if (index < 0) return fallback;
        if (index + 1 >= args.Length) throw new ArgumentException("Missing value for " + name);
        return args[index + 1];
    }
    private static int[] Degrees(string text, int max)
    {
        int[] result = text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x, Invariant)).ToArray();
        if (result.Length == 0 || result.Length > 30 || result.Any(x => x < 1 || x > max)) throw new ArgumentOutOfRangeException(nameof(text));
        return result.Distinct().ToArray();
    }
    private static int PlotCount(string[] args)
    {
        int count = int.Parse(Option(args, "--grid", "1001"), Invariant);
        if (count < 2 || count > 100_001) throw new ArgumentOutOfRangeException(nameof(count));
        return count;
    }
    private static string Folder(string[] args, string fallback)
    {
        string directory = Path.GetFullPath(Option(args, "--out", fallback));
        Directory.CreateDirectory(directory);
        return directory;
    }
    private static void RunPolynomial(string[] args)
    {
        var curves = new List<(string Name, Pado.NumericalLab.Core.Experiments.Curve Curve)>();
        int grid = PlotCount(args);
        foreach (int n in Degrees(Option(args, "--n", "5,10,15"), 30))
            foreach (var (name, function) in new (string, Func<double, double>)[] { ("Runge", ExperimentRunner.Runge), ("Log", ExperimentRunner.Logarithmic) })
            {
                var run = ExperimentRunner.Polynomial(name, function, n, grid);
                curves.Add(($"{name} n={n} Lagrange", run.Lagrange));
                curves.Add(($"{name} n={n} Newton", run.Newton));
                Console.WriteLine($"{name,-6} n={n,2} nodes={n + 1,2} grid={grid} L={run.Lagrange.MaxError:E4} N={run.Newton.MaxError:E4}");
            }
        string folder = Folder(args, "results/polynomial");
        ReportExporter.WriteHtml(folder, "Polynomial interpolation · Lagrange vs Newton", curves);
        Console.WriteLine("HTML: " + Path.Combine(folder, "report.html"));
    }
    private static void RunTrigonometric(string[] args)
    {
        var curves = new List<(string Name, Pado.NumericalLab.Core.Experiments.Curve Curve)>();
        int grid = PlotCount(args);
        foreach (int n in Degrees(Option(args, "--n", "1,7,15"), TrigonometricInterpolator.MaxDegree))
            foreach (var (name, function) in new (string, Func<double, double>)[] {
                ("exp(sin+cos)", TrigonometricTestFunctions.ExpSinCos),
                ("3cos(15t)", TrigonometricTestFunctions.Cos15) })
            {
                var run = ExperimentRunner.Trigonometric(name, function, n, grid);
                curves.Add(($"{name} n={n}", run.Curve));
                Console.WriteLine($"{name,-14} n={n,2} N={run.NodeCount,3} grid={grid} max={run.Curve.MaxError:E4} nodes={run.Curve.MaxNodeError:E4} end={run.EndpointError:E4}");
            }
        string folder = Folder(args, "results/trig");
        ReportExporter.WriteHtml(folder, "Trigonometric interpolation · manual Fourier coefficients", curves);
        Console.WriteLine("HTML: " + Path.Combine(folder, "report.html"));
    }
    private static void RunStats()
    {
        var input = new double[] { 1, 2, 2, 3, 4, 4, 4, 5 };
        var stats = DescriptiveStatistics.Analyze(input);
        var groups = DescriptiveStatistics.GroupedCounts(input);
        Console.WriteLine($"Mean={stats.Mean:G17}, Median={stats.Median:G17}, Modes={string.Join(",", stats.Modes.Select(x => x.ToString("G17", Invariant)))}, " +
            $"PopulationVar={stats.PopulationVariance:G17}, SampleVar={stats.SampleVariance:G17}");
        Console.WriteLine("Grouped frequencies: " + string.Join(",", groups.Frequencies));
    }
    private static void RunAlgebra()
    {
        var a = new Matrix(new double[,] { { 4, 1 }, { 2, 3 } });
        double[] rhs = { 9, 8 };
        Console.WriteLine("Householder solution: " + string.Join(", ", HouseholderSolver.Solve(a, rhs).Select(x => x.ToString("G17", Invariant))));
        var eigen = PowerIteration.Dominant(new Matrix(new double[,] { { 3, 0 }, { 0, 1 } }), new double[] { 1, 1 });
        Console.WriteLine($"Power eigenvalue={eigen.Eigenvalue:G17}, iterations={eigen.Iterations}, residual={eigen.ResidualInfinityNorm:E3}");
        var sweep = TridiagonalSolver.SolveOriginalBoundaryProblem(10);
        Console.WriteLine($"Right-sweep y(0.5)={sweep[5]:G17} (exact={1 / 1.5:G17})");
    }
    private static void RunCalc(string[] args)
    {
        if (args.Length != 4) throw new ArgumentException("Usage: calc <a> <op> <b>");
        Console.WriteLine(Calculator.Calculate(double.Parse(args[1], Invariant), args[2], double.Parse(args[3], Invariant)).ToString("G17", Invariant));
    }
    private static void RunShapes(string[] args)
    {
        if (args.Length > 2) throw new ArgumentException("Usage: shapes [file]");
        var lines = args.Length == 2 ? File.ReadAllLines(args[1]) : new[] { "circle 2", "rectangle 3 4", "square 5" };
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
            Shape shape = ShapeParser.Parse(line);
            if (!double.IsFinite(shape.Area) || !double.IsFinite(shape.Perimeter)) throw new ArithmeticException("Geometry overflow.");
            Console.WriteLine($"{shape.GetType().Name,-12} area={shape.Area:G17} perimeter={shape.Perimeter:G17}");
        }
    }
}
