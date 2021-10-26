namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;

[Module("geometry.trigonometric")]
public class GeometryTrigonometric
{
    [FunctionName("period")]
    public static double Period(double b)
    {
        return 2 * Math.PI / b;
    }
}
