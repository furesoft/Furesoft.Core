using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// An attribute that attaches an exception specification to a method.
    /// </summary>
    public sealed class ExceptionSpecificationAttribute : IAttribute
    {
        /// <summary>
        /// The attribute type of exception specification attributes.
        /// </summary>
        /// <value>An attribute type.</value>
        public static readonly IType AttributeType = new DescribedType(
            new SimpleName("ExceptionSpecification").Qualify(), null);

        /// <summary>
        /// Creates an exception specification attribute.
        /// </summary>
        /// <param name="specification">An exception specification.</param>
        public ExceptionSpecificationAttribute(ExceptionSpecification specification)
        {
            Specification = specification;
        }

        IType IAttribute.AttributeType => AttributeType;

        /// <summary>
        /// Gets the exception specification wrapped by this exception specification
        /// attribute.
        /// </summary>
        /// <value>An exception specification.</value>
        public ExceptionSpecification Specification { get; private set; }
    }
}