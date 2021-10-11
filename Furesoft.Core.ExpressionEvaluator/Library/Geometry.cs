using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using System;
using System.Diagnostics;

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

        [FunctionName("circumference")]
        public static double Circumference(double radius)
        {
            return 2 * radius * Math.PI;
        }

        [Macro(IsInitializer = true)]
        public static Expression InitializerTest(MacroContext context, Expression[] args)
        {
            Debug.WriteLine(42);

            return 0;
        }

        [FunctionName("volumePyramide")]
        public static double VolumePyramide(double ground, double height)
        {
            return 1 / 3 * ground * height;
        }
    }

    [Module("geometry.volume")]
    public class GeometryVolume
    {
        [FunctionName("volumePyramide")]
        public static double VolumePyramide(double ground, double height)
        {
            return ground * height / 3;
        }
    }
}