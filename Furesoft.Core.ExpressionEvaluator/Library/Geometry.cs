using Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("geometry")]
    public static class Geometry
    {
        public static GeometryFigure Figure = new();
        public static GeometryPlanes Planes = new();
        public static GeometryPolinomal Polinomal = new();
        public static GeometryTrigonometric Trigonometric = new();
    }
}