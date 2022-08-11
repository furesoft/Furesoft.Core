using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    public class GenericType : DescribedType
    {
        public GenericType(QualifiedName fullName, IAssembly assembly) : base(fullName, assembly)
        {
        }

        public GenericType(UnqualifiedName name, IType parentType) : base(name, parentType)
        {
        }

        public List<IType> GenericArguments { get; set; } = new();
    }
}