namespace Furesoft.Core.ExpressionEvaluator
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MacroAttribute : Attribute
    {
        public bool IsInitializer { get; set; }
    }
}