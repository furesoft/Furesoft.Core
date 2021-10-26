namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Physics;

[Module("physics.mechanics")]
public class PhysicsMechanics
{
    [FunctionName("acceleration")]
    public static double Acceleration(double velocity, double time)
    {
        return velocity / time;
    }

    [FunctionName("velocity")]
    public static double Velocity(double distance, double time)
    {
        return distance / time;
    }
}
