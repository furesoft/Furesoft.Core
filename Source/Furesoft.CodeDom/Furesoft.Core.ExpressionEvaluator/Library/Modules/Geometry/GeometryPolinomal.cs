namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;

[Module("geometry.polinomal")]
public class GeometryPolinomal
{
    [FunctionName("linearGradiant")]
    public static double LinearGradient(double x1, double y1, double x2, double y2)
    {
        return y1 - y2 / x1 - x2;
    }
}
