using System;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("geometry")]
    public static class Geometry
    {
        [FunctionName("circumference")]
        public static double Circumference(double radius)
        {
            return 2 * radius * Math.PI;
        }
    }
}