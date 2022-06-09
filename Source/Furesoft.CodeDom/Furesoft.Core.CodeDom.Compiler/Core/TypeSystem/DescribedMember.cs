using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// A member that can be constructed incrementally in an imperative fashion.
    /// </summary>
    public class DescribedMember : IMember
    {
        private AttributeMapBuilder attributeBuilder;

        /// <summary>
        /// Creates a described member from a fully qualified name.
        /// </summary>
        /// <param name="fullName">
        /// The described member's fully qualified name.
        /// </param>
        public DescribedMember(QualifiedName fullName)
        {
            this.FullName = fullName;
            this.attributeBuilder = new AttributeMapBuilder();
        }

        /// <inheritdoc/>
        public AttributeMap Attributes => new(attributeBuilder);

        /// <inheritdoc/>
        public QualifiedName FullName { get; private set; }

        /// <inheritdoc/>
        public UnqualifiedName Name => FullName.FullyUnqualifiedName;

        /// <summary>
        /// Adds an attribute to this member's attribute map.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        public void AddAttribute(IAttribute attribute)
        {
            attributeBuilder.Add(attribute);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FullName.ToString();
        }
    }
}