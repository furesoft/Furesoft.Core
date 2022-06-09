using System.Collections.Generic;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// A type that can be constructed incrementally in an imperative fashion.
    /// </summary>
    public class DescribedType : DescribedGenericMember, IType
    {
        /// <summary>
        /// Creates a type from a name and a parent assembly.
        /// </summary>
        /// <param name="fullName">The type's full name.</param>
        /// <param name="assembly">The assembly that defines the type.</param>
        public DescribedType(QualifiedName fullName, IAssembly assembly)
            : base(fullName)
        {
            this.Parent = new TypeParent(assembly);
            Initialize();
        }

        /// <summary>
        /// Creates a type from a name and a parent type.
        /// </summary>
        /// <param name="name">The type's unqualified name.</param>
        /// <param name="parentType">
        /// The type's parent type, i.e., the type that defines it.
        /// </param>
        public DescribedType(UnqualifiedName name, IType parentType)
            : base(name.Qualify(parentType.FullName))
        {
            this.Parent = new TypeParent(parentType);
            Initialize();
        }

        private void Initialize()
        {
            baseTypeList = new List<IType>();
            fieldList = new List<IField>();
            methodList = new List<IMethod>();
            propertyList = new List<IProperty>();
            nestedTypeList = new List<IType>();
        }

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
        public TypeParent Parent { get; private set; }

        private List<IType> baseTypeList;
        private List<IField> fieldList;
        private List<IMethod> methodList;
        private List<IProperty> propertyList;
        private List<IType> nestedTypeList;

        /// <inheritdoc/>
        public IReadOnlyList<IType> BaseTypes => baseTypeList;

        /// <inheritdoc/>
        public IReadOnlyList<IField> Fields => fieldList;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> Methods => methodList;

        /// <inheritdoc/>
        public IReadOnlyList<IProperty> Properties => propertyList;

        /// <inheritdoc/>
        public IReadOnlyList<IType> NestedTypes => nestedTypeList;

        /// <summary>
        /// Makes a particular type a base type of this type.
        /// </summary>
        /// <param name="type">
        /// The type to add to this type's base type list.
        /// </param>
        public void AddBaseType(IType type)
        {
            baseTypeList.Add(type);
        }

        /// <summary>
        /// Adds a field to this type.
        /// </summary>
        /// <param name="field">The field to add.</param>
        public void AddField(IField field)
        {
            CheckParent(field);
            fieldList.Add(field);
        }

        /// <summary>
        /// Adds a method to this type.
        /// </summary>
        /// <param name="method">The method to add.</param>

        public void AddMethod(IMethod method)
        {
            CheckParent(method);
            methodList.Add(method);
        }

        /// <summary>
        /// Adds a property to this type.
        /// </summary>
        /// <param name="property">The property to add.</param>
        public void AddProperty(IProperty property)
        {
            CheckParent(property);
            propertyList.Add(property);
        }

        /// <summary>
        /// Adds a nested type to this type.
        /// </summary>
        /// <param name="nestedType">The nested type to add.</param>
        public void AddNestedType(IType nestedType)
        {
            ContractHelpers.Assert(
                nestedType.Parent.IsType,
                "Cannot add a non-nested type as a nested type.");

            ContractHelpers.Assert(
                object.Equals(this, nestedType.Parent.Type),
                "A nested type can only be added to its defining type.");
            nestedTypeList.Add(nestedType);
        }

        private void CheckParent(ITypeMember member)
        {
            ContractHelpers.Assert(
                object.Equals(this, member.ParentType),
                "A member can only be added to its defining type.");
        }

        public bool Owns(IAttribute attribute) => Attributes.Contains(attribute.AttributeType);

        public void SetAttr(bool value, IAttribute attribute) {
            if (value) AddAttribute(attribute);
            else RemoveAttributes(attribute);
        }

        public AccessModifier GetAccessModifier() => AccessModifierAttribute.GetAccessModifier(this);

        public void RemoveAccessModifier() => RemoveAttributesFromType(AccessModifierAttribute.AttributeType);
    }
}
