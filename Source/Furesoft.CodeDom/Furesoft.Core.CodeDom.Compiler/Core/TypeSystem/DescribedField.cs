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

        #region Helpers
        public bool IsReferenceType
        {
            get { return Owns(FlagAttribute.ReferenceType); }
            set { SetAttr(value, FlagAttribute.ReferenceType); }
        }
        public bool IsInterfaceType
        {
            get { return Owns(FlagAttribute.InterfaceType); }
            set { SetAttr(value, FlagAttribute.InterfaceType); }
        }
        public bool IsSpecialType
        {
            get { return Owns(FlagAttribute.SpecialType); }
            set { SetAttr(value, FlagAttribute.SpecialType); }
        }
        public bool IsInternalCall
        {
            get { return Owns(FlagAttribute.InternalCall); }
            set { SetAttr(value, FlagAttribute.InternalCall); }
        }
        public bool IsAbstract
        {
            get { return Owns(FlagAttribute.Abstract); }
            set { SetAttr(value, FlagAttribute.Abstract); }
        }
        public bool IsStatic
        {
            get { return Owns(FlagAttribute.Static); }
            set { SetAttr(value, FlagAttribute.Static); }
        }
        public bool IsOverride
        {
            get { return Owns(FlagAttribute.Override); }
            set { SetAttr(value, FlagAttribute.Override); }
        }
        public bool IsVirtual
        {
            get { return Owns(FlagAttribute.Virtual); }
            set { SetAttr(value, FlagAttribute.Virtual); }
        }
        public bool IsPublic
        {
            get { return GetAccessModifier().HasFlag(AccessModifier.Public); }
            set { if (value) { RemoveAccessModifier(); AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public)); } else { RemoveAccessModifier(); } }
        }
        public bool IsProtected
        {
            get { return GetAccessModifier().HasFlag(AccessModifier.Protected); }
            set { if (value) { RemoveAccessModifier(); AddAttribute(AccessModifierAttribute.Create(AccessModifier.Protected)); } else { RemoveAccessModifier(); } }
        }
        public bool IsPrivate
        {
            get { return GetAccessModifier().HasFlag(AccessModifier.Private); }
            set { if (value) { RemoveAccessModifier(); AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private)); } else { RemoveAccessModifier(); } }
        }
        #endregion

        /// <inheritdoc/>
        public IType ParentType { get; private set; }

        public bool Owns(IAttribute attribute) => Attributes.Contains(attribute.AttributeType);

        public void SetAttr(bool value, IAttribute attribute)
        {
            if (value) AddAttribute(attribute);
            else RemoveAttributes(attribute);
        }

        public AccessModifier GetAccessModifier() => AccessModifierAttribute.GetAccessModifier(this);

        public void RemoveAccessModifier() => RemoveAttributesFromType(AccessModifierAttribute.AttributeType);
    }
}