using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;
using System.Diagnostics;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("geometry")]
    public static class Geometry
    {
        public static GeometryFigure Figure = new();
        public static GeometryPlanes Planes = new();
        public static GeometryPolinomal Polinomal = new();
        public static GeometryTrigonometric Trigonometric = new();

        [Macro(IsInitializer = true)]
        public static Expression InitializerTest(MacroContext context, Expression[] args)
        {
            Debug.WriteLine(42);

            return 0;
        }
    }
}