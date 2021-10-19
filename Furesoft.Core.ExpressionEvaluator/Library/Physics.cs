using Furesoft.Core.ExpressionEvaluator.Library.Modules.Physics;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("physics")]
    public static class Physics
    {
        public static PhysicsForces Forces = new();
        public static double g = 9.81;
        public static PhysicsGravityAccelerations GravityAccelerations = new();
        public static PhysicsMechanics Mechanics = new();
    }
}