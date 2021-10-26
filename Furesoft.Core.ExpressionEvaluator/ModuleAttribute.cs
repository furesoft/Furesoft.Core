namespace Furesoft.Core.ExpressionEvaluator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string[] Dependencies { get; set; }
    }
}