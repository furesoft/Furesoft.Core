namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry
{
    [Module("geometry.area")]
    public class GeometryArea
    {
        [FunctionName("areaRectangle")]
        public static double AreaRectangle(double width, double height)
        {
            return width * height;
        }

        [FunctionName("areaSquare")]
        public static double AreaSquare(double width)
        {
            return width * width;
        }

        [FunctionName("areaTrapezoide")]
        public static double AreaTrapezoide(double a, double c, double height)
        {
            return 0.5 * (a + c) * height;
        }

        [FunctionName("areaTriangle")]
        public static double AreaTriangle(double width, double height)
        {
            return width * height / 2;
        }
    }
}
