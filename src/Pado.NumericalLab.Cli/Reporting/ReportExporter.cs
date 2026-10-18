using System.Globalization;
using System.Net;
using System.Text;
using Pado.NumericalLab.Core.Experiments;

namespace Pado.NumericalLab.Cli.Reporting;

/// <summary>Dependency-free CSV, SVG and HTML output: zero plotting references in Core.</summary>
public static class ReportExporter
{
    private static string Number(double v) => v.ToString("G17", CultureInfo.InvariantCulture);
    public static void WriteCsv(string path, Curve curve)
    {
        var sb = new StringBuilder("x,exact,approximation,absolute_error\n");
        for (int i = 0; i < curve.X.Length; i++)
            sb.Append(Number(curve.X[i])).Append(',').Append(Number(curve.Exact[i])).Append(',')
              .Append(Number(curve.Approximated[i])).Append(',')
              .Append(Number(Math.Abs(curve.Exact[i] - curve.Approximated[i]))).Append('\n');
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }
    public static void WriteSvg(string path, string title, Curve curve)
    {
        const int width = 1000, height = 560, left = 92, right = 28, top = 78, bottom = 80;
        double x0 = curve.X.Min(), x1 = curve.X.Max();
        double y0 = Math.Min(curve.Exact.Min(), curve.Approximated.Min());
        double y1 = Math.Max(curve.Exact.Max(), curve.Approximated.Max());
        if (y0 == y1) { y0 -= 1; y1 += 1; }
        double margin = (y1 - y0) * 0.06;
        y0 -= margin; y1 += margin;
        double X(double v) => left + (v - x0) / (x1 - x0) * (width - left - right);
        double Y(double v) => height - bottom - (v - y0) / (y1 - y0) * (height - top - bottom);
        string F(double v) => v.ToString("F3", CultureInfo.InvariantCulture);
        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" role=\"img\" aria-label=\"{WebUtility.HtmlEncode(title)}\">\n");
        sb.Append("<rect width=\"100%\" height=\"100%\" fill=\"#0b1221\"/>");
        sb.Append($"<text x=\"{left}\" y=\"40\" font-size=\"24\" fill=\"#e9f0ff\" font-family=\"Arial\">{WebUtility.HtmlEncode(title)}</text>");
        for (int tick = 0; tick <= 5; tick++)
        {
            double y = top + (height - top - bottom) * tick / 5.0;
            sb.Append($"<line x1=\"{left}\" y1=\"{F(y)}\" x2=\"{width - right}\" y2=\"{F(y)}\" stroke=\"#263851\"/>");
            double label = y1 - (y1 - y0) * tick / 5.0;
            sb.Append($"<text x=\"{left - 8}\" y=\"{F(y + 4)}\" text-anchor=\"end\" font-size=\"12\" fill=\"#aebed6\">{Number(label)}</text>");
        }
        foreach (var (data, color) in new[] { (curve.Exact, "#50d5bb"), (curve.Approximated, "#f3a855") })
        {
            sb.Append($"<polyline fill=\"none\" stroke=\"{color}\" stroke-width=\"2.2\" points=\"");
            for (int i = 0; i < curve.X.Length; i++) sb.Append(F(X(curve.X[i]))).Append(',').Append(F(Y(data[i]))).Append(' ');
            sb.Append("\"/>");
        }
        sb.Append($"<text x=\"{left}\" y=\"{height - 22}\" fill=\"#50d5bb\" font-family=\"Arial\">━━ Reference</text>");
        sb.Append($"<text x=\"{left + 180}\" y=\"{height - 22}\" fill=\"#f3a855\" font-family=\"Arial\">━━ Approximation</text>");
        sb.Append("</svg>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }
    public static void WriteHtml(string folder, string title, IReadOnlyList<(string Name, Curve Curve)> curves)
    {
        string Esc(string s) => WebUtility.HtmlEncode(s);
        var sb = new StringBuilder("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width\"><title>");
        sb.Append(Esc(title)).Append("</title><style>body{font:16px system-ui;background:#0b1221;color:#edf3fe;max-width:1050px;margin:0 auto;padding:24px}h1{font-size:2rem}.subtitle{color:#b1c0d7}table{border-collapse:collapse;width:100%;margin:24px 0}td,th{border-bottom:1px solid #304056;padding:12px;text-align:left}th{color:#a0d5ec}img{max-width:100%;border:1px solid #304056;border-radius:12px}section{padding:15px 0}a{color:#50d5bb}code{background:#1e2d43;padding:4px 8px;border-radius:5px}</style>");
        sb.Append("<main><h1>").Append(Esc(title)).Append("</h1><p class=\"subtitle\">PADO NumericalLab · .NET 10 · source computations, independent plotting resolution</p>");
        sb.Append("<table><thead><tr><th>Experiment</th><th>Plot points</th><th>Max absolute error</th><th>Node error</th><th>Data</th></tr></thead><tbody>");
        foreach (var (name, curve) in curves)
        {
            var stem = SafeStem(name);
            sb.Append("<tr><td>").Append(Esc(name)).Append("</td><td>").Append(curve.X.Length)
                .Append("</td><td>").Append(Number(curve.MaxError)).Append("</td><td>").Append(Number(curve.MaxNodeError))
                .Append("</td><td><a href=\"").Append(stem).Append(".csv\">CSV</a></td></tr>");
        }
        sb.Append("</tbody></table>");
        foreach (var (name, curve) in curves)
        {
            var stem = SafeStem(name);
            WriteCsv(Path.Combine(folder, stem + ".csv"), curve);
            WriteSvg(Path.Combine(folder, stem + ".svg"), name, curve);
            sb.Append("<section><h2>").Append(Esc(name)).Append("</h2><img src=\"").Append(stem).Append(".svg\" alt=\"Plot: ").Append(Esc(name)).Append("\"></section>");
        }
        sb.Append("<p>Absolute error = |f(x) − approximation(x)|. This is a grid-based estimate, not a rigorous global bound.</p></main></html>");
        File.WriteAllText(Path.Combine(folder, "report.html"), sb.ToString(), new UTF8Encoding(false));
    }
    private static string SafeStem(string name)
    {
        string output = new string(name.ToLowerInvariant().Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        return output.Length == 0 ? "curve" : output;
    }
}
