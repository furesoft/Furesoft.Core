namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry
{
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