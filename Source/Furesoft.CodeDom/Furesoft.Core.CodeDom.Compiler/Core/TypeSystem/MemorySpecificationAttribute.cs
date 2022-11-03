using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem;

/// <summary>
/// An attribute that attaches a memory specification to a method.
/// </summary>
public sealed class MemorySpecificationAttribute : IAttribute
{
    /// <summary>
    /// The attribute type of memory specification attributes.
    /// </summary>
    /// <value>An attribute type.</value>
    public static readonly IType AttributeType = new DescribedType(
        new SimpleName("MemorySpecification").Qualify(), null);

    /// <summary>
    /// Creates a memory specification attribute.
    /// </summary>
    /// <param name="specification">A memory specification.</param>
    public MemorySpecificationAttribute(MemorySpecification specification)
    {
        Specification = specification;
    }

    IType IAttribute.AttributeType => AttributeType;

    /// <summary>
    /// Gets the memory specification wrapped by this memory specification
    /// attribute.
    /// </summary>
    /// <value>A memory specification.</value>
    public MemorySpecification Specification { get; private set; }
}