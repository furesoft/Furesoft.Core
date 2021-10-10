using System;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("geometry")]
    public static class Geometry
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

        [FunctionName("areaSquare")]
        public static double AreaSquare(double width, double height)
        {
            return width * height;
        }

        [FunctionName("areaTriangle")]
        public static double AreaTriangle(double width, double height)
        {
            return width * height / 2;
        }

        [FunctionName("circumference")]
        public static double Circumference(double radius)
        {
            return 2 * radius * Math.PI;
        }
    }
}