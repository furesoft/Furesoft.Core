namespace Furesoft.Core.ExpressionEvaluator.Library.Modules.Physics
{
    [Module("physics.forces")]
    public class PhysicsForces
    {
        [FunctionName("Fg")]
        [Macro]
        public static Expression GravityForce(MacroContext mc, Expression mass)
        {
            return mass * new UnresolvedRef("G");
        }
    }
}