using System;

namespace Furesoft.Core.ExpressionEvaluator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}