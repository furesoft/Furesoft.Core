using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem
{
    public class AttributeArgument
    {
        public AttributeArgument(IType type, object value)
        {
            Type = type;
            Value = value;
        }

        public IType Type { get; set; }
        public object Value { get; set; }
    }
}