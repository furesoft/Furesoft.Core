using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// An attribute that indicates that a method is implemented by calling
    /// into an external library.
    /// </summary>
    public sealed class ExternAttribute : IAttribute
    {
        /// <summary>
        /// The attribute type of extern attributes.
        /// </summary>
        /// <value>An attribute type.</value>
        public static readonly IType AttributeType = new DescribedType(
            new SimpleName("Extern").Qualify(), null);

        /// <summary>
        /// Creates a new extern attribute.
        /// </summary>
        public ExternAttribute()
            : this(null)
        { }

        /// <summary>
        /// Creates a new extern attribute.
        /// </summary>
        /// <param name="importName">
        /// The name of the imported function.
        /// </param>
        public ExternAttribute(string importName)
        {
            ImportNameOrNull = importName;
        }

        IType IAttribute.AttributeType => AttributeType;

        /// <summary>
        /// Gets the name of the imported function, if any.
        /// </summary>
        /// <value>The imported name.</value>
        public string ImportNameOrNull { get; private set; }
    }
}