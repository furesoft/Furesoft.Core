using System;
using System.Drawing;

namespace Furesoft.Core.ExpressionEvaluator.FunctionPlotter;

public class Graph
{
    public Func<double, double> Function { get; set; }
    public Color Color;
}