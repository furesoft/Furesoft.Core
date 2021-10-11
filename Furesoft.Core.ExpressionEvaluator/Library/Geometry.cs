using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;
using System;
using System.Diagnostics;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("geometry")]
    public static class Geometry
    {
        public static GeometryArea Area = new GeometryArea();
        public static GeometryVolume Volume = new GeometryVolume();

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
    }
}