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

        /// <summary>
        /// Removes an attribute from this member's attribute map.
        /// </summary>
        public void RemoveAttributes(IAttribute attribute)
        {
            RemoveAttributesFromType(attribute.AttributeType);
        }

        /// <summary>
        /// Removes all attributes with the given type from this member's attribute map.
        /// </summary>
        public void RemoveAttributesFromType(IType attributeType)
        {
            attributeBuilder.RemoveAll(attributeType);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FullName.ToString();
        }

        #region Lixou's Helpers
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
        public bool IsExtern
        {
            get { return Owns(FlagAttribute.Extern); }
            set { SetAttr(value, FlagAttribute.Extern); }
        }
        public bool IsVirtual
        {
            get { return Owns(FlagAttribute.Virtual); }
            set { SetAttr(value, FlagAttribute.Virtual); }
        }
        public bool IsPublic
        {
            get { return GetAccessModifier().Equals(AccessModifier.Public); }
            set { RemoveAccessModifier(); if (value) AddAttribute(AccessModifierAttribute.Create(AccessModifier.Public)); }
        }
        public bool IsInternal
        {
            get { return GetAccessModifier().Equals(AccessModifier.Internal); }
            set { RemoveAccessModifier(); if (value) AddAttribute(AccessModifierAttribute.Create(AccessModifier.Internal)); }
        }
        public bool IsProtected
        {
            get { return GetAccessModifier().Equals(AccessModifier.Protected); }
            set { RemoveAccessModifier(); if (value) AddAttribute(AccessModifierAttribute.Create(AccessModifier.Protected)); }
        }
        public bool IsPrivate
        {
            get { return GetAccessModifier().Equals(AccessModifier.Private); }
            set { RemoveAccessModifier(); if (value) AddAttribute(AccessModifierAttribute.Create(AccessModifier.Private)); }
        }
        public AccessModifier GetAccessModifier() => AccessModifierAttribute.GetAccessModifier(this);
        public void RemoveAccessModifier() => RemoveAttributesFromType(AccessModifierAttribute.AttributeType);
        public bool Owns(IAttribute attribute) => Attributes.Contains(attribute.AttributeType);
        public void SetAttr(bool value, IAttribute attribute)
        {
            if (value) AddAttribute(attribute);
            else RemoveAttributes(attribute);
        }
        #endregion
    }
}