using System.Drawing;
using System.Threading.Tasks;

namespace Furesoft.Core.ExpressionEvaluator.FunctionPlotter;

public class Plotter
{
    public Bitmap Plot { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }
    public double xMin { get; private set; }
    public double xMax { get; private set; }
    public double yMin { get; private set; }
    public double yMax { get; private set; }
    public Color Background { get; set; }
    public Color AxisColor { get; set; }
    public List<Graph> Functions { get; set; }


    public Plotter(double xmin, double xmax, double ymin, double ymax)
    {
        Initialize(xmin, xmax, ymin, ymax);
    }

    public Plotter(int width, int height, double xmin, double xmax, double ymin, double ymax)
    {
        Initialize(xmin, xmax, ymin, ymax);
        Plot = new Bitmap(width, height);
        Height = height;
        Width = width;
    }

    public Plotter(Func<double, double> f, double xmin, double xmax, double ymin, double ymax)
    {
        Initialize(xmin, xmax, ymin, ymax);
        Functions.Add(new Graph { Function = f, Color = Color.Blue });
    }

    void Initialize(double xmin, double xmax, double ymin, double ymax)
    {
        // default initialization
        Plot = new Bitmap(500, 500);
        Height = 500;
        Width = 500;
        xMin = xmin;
        xMax = xmax;
        yMin = ymin;
        yMax = ymax;
        Background = Color.White;
        AxisColor = Color.Black;
        Functions = [];
    }

    void EmptyPlot()
    {
        // using LockBitmap for faster SetPixel and faster GetPixel
        // allows working with class Parallel, making it super fast
        var bmp = new LockBitmap(Plot);
        bmp.LockBits();
        Parallel.For(0, Width, x =>
        {
            Parallel.For(0, Height, y =>
            {
                bmp.SetPixel(x, y, Background);
            });
        });
        bmp.UnlockBits();
    }

    // scales a number n within [min1, max1] to [min2, max2]
    double ScaleNumber(double n, double min1, double max1, double min2, double max2)
    {
        var oldRange = max1 - min1;
        var newRange = max2 - min2;
        var proportion = (n - min1) / oldRange;
        return proportion * newRange;
    }

    double ScaleX(double x) => ScaleNumber(x, xMin, xMax, 0, Width);
    double ScaleY(double y) => Height - ScaleNumber(y, yMin, yMax, 0, Height);

    // generate an interval of [k, n] with step
    IEnumerable<double> Range(double k, double n, double step)
    {
        for (double i = k; i <= n; i += step)
            yield return i;
    }

    void DrawPoints(IEnumerable<double> xs, IEnumerable<double> ys, Color c)
    {
        var xValues = xs.ToArray();
        var yValues = ys.ToArray();
        var bmp = new LockBitmap(Plot);
        bmp.LockBits();

        // for each point within the plot-box, draw the point
        Parallel.For(0, xValues.Length, i =>
        {
            var x = xValues[i];
            var y = yValues[i];
            if (y > 0 && y < Height)
                bmp.SetPixel((int)x, (int)y, c);
        });
        bmp.UnlockBits();
    }

    void DrawVerticalAxis()
    {
        if (xMin > 0)
            return;
        else
        {
            var x0 = ScaleX(0.0);
            var ys = Range(yMin, yMax, 0.001).Select(x => ScaleY(x));
            var xs = ys.Select(x => x0);
            DrawPoints(xs, ys, AxisColor);
        }
    }

    void DrawHorizontalAxis()
    {
        if (yMin > 0)
            return;
        else
        {
            var y0 = ScaleY(0.0);
            var xs = Range(xMin, xMax, 0.001).Select(x => ScaleX(x));
            var ys = xs.Select(x => y0);
            DrawPoints(xs, ys, AxisColor);
        }
    }

    // draws the functions
    public void Draw()
    {
        EmptyPlot();
        // generate points and draw points
        Functions.ForEach(graph =>
        {
            var xs = Range(xMin, xMax, 0.0001);
            var ys = xs.Select(graph.Function);
            xs = xs.Select(x => ScaleX(x));
            ys = ys.Select(y => ScaleY(y));
            // Color.Name == "0" -> means Color isn't intialized
            var graphColor = graph.Color.Name == "0" ? Color.Black : graph.Color;
            DrawPoints(xs, ys, graphColor);
        });
        DrawHorizontalAxis();
        DrawVerticalAxis();
    }
}
