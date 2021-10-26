namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Geometry;

[Module("geometry.figure")]
public class GeometryFigure
{
    [FunctionName("surfaceBall")]
    public static double SurfaceBall(double radius)
    {
        return 4 * Math.PI * Math.Pow(radius, 2);
    }

    [FunctionName("volumeBall")]
    public static double VolumeBall(double radius)
    {
        return 4 / 3 * Math.PI * Math.Pow(radius, 3);
    }

    [FunctionName("volumePyramide")]
    public static double VolumePyramide(double ground, double height)
    {
        return ground * height / 3;
    }

    [FunctionName("volumeQuader")]
    public static double VolumeQuader(double width, double height, double depth)
    {
        return width * height * depth;
    }
}
