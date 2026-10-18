namespace Pado.NumericalLab.Cli.Tools;

/// <summary>Independent OO geometry demo from C#_projects(1).zip.</summary>
public abstract class Shape
{
    public abstract double Area { get; }
    public abstract double Perimeter { get; }
    protected static double Positive(double value)
    {
        if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Dimensions must be positive and finite.");
        return value;
    }
}
public sealed class Circle : Shape
{
    public double Radius { get; }
    public Circle(double radius) => Radius = Positive(radius);
    public override double Area => Math.PI * Radius * Radius;
    public override double Perimeter => 2 * Math.PI * Radius;
}
public class Rectangle : Shape
{
    public double Width { get; }
    public double Height { get; }
    public Rectangle(double width, double height) { Width = Positive(width); Height = Positive(height); }
    public override double Area => Width * Height;
    public override double Perimeter => 2 * (Width + Height);
}
public sealed class Square : Rectangle
{
    public Square(double side) : base(side, side) { }
}
public static class ShapeParser
{
    public static Shape Parse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) throw new ArgumentException("Empty shape line.");
        double Read(int i) => double.Parse(parts[i], System.Globalization.CultureInfo.InvariantCulture);
        return parts[0].ToLowerInvariant() switch
        {
            "circle" when parts.Length == 2 => new Circle(Read(1)),
            "square" when parts.Length == 2 => new Square(Read(1)),
            "rectangle" when parts.Length == 3 => new Rectangle(Read(1), Read(2)),
            _ => throw new FormatException("Expected circle r, square a, or rectangle w h.")
        };
    }
}
