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

        [Macro(IsInitializer = true)]
        public static Expression MacroTest(MacroContext context, Expression[] args)
        {
            Debug.WriteLine(42);

            return 0;
        }
    }
}