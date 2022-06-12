using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// A field that can be constructed incrementally in an imperative fashion.
    /// </summary>
    public class DescribedField : DescribedMember, IField
    {
        /// <summary>
        /// Creates a field from a parent type, a name, a staticness and
        /// a type of value to store.
        /// </summary>
        /// <param name="parentType">The field's parent type.</param>
        /// <param name="name">The field's name.</param>
        /// <param name="isStatic">Tells if the field is static.</param>
        /// <param name="fieldType">The type of value stored in the field.</param>
        public DescribedField(
            IType parentType,
            UnqualifiedName name,
            bool isStatic,
            IType fieldType)
            : base(name.Qualify(parentType.FullName))
        {
            this.ParentType = parentType;
            this.IsStatic = isStatic;
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets or sets the type of value stored in this field.
        /// </summary>
        /// <returns>The type of value stored in this field.</returns>
        public IType FieldType { get; set; }

        public object InitialValue { get; set; }

        /// <inheritdoc/>
        public IType ParentType { get; private set; }
    }
}