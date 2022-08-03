using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Core
{
    /// <summary>
    /// The root interface for members: constructs that
    /// have a name, a full name and a set of attributes.
    /// </summary>
    public interface IMember
    {
        /// <summary>
        /// Gets the member's attributes.
        /// </summary>
        AttributeMap Attributes { get; }

        /// <summary>
        /// Gets the member's full name.
        /// </summary>
        QualifiedName FullName { get; }

        /// <summary>
        /// Gets the member's unqualified name.
        /// </summary>
        UnqualifiedName Name { get; }

        void AddAttribute(IAttribute attribute);
        void RemoveAttributes(IAttribute attribute);
        void RemoveAttributesFromType(IType attributeType)

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