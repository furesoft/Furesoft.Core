using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem
{
    public class DescribedAttribute : IAttribute
    {
        private readonly IType _attributeType;

        public DescribedAttribute(IType attributeType)
        {
            _attributeType = attributeType;
        }

        public IType AttributeType => _attributeType;
        public List<AttributeArgument> ConstructorArguments { get; set; } = new();
    }
}