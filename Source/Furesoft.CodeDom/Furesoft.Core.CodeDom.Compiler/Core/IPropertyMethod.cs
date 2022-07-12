namespace Furesoft.Core.CodeDom.Compiler.Core
{
    public interface IPropertyMethod : ITypeMember
    {
        MethodBody Body { get; }
    }
}
