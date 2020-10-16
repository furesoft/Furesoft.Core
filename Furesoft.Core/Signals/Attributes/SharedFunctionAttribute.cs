using System;

namespace Furesoft.Core.Signals.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SharedFunctionAttribute : Attribute
    {
        public int ID { get; }

        public SharedFunctionAttribute(int id)
        {
            ID = id;
        }
    }
}