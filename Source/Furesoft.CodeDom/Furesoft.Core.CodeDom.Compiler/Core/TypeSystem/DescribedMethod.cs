using System.Collections.Generic;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// A method that can be constructed incrementally in an imperative fashion.
    /// </summary>
    public class DescribedMethod : DescribedGenericMember, IMethod
    {
        /// <summary>
        /// Creates a method from a parent type, a name, a staticness
        /// and a return type.
        /// </summary>
        /// <param name="parentType">The method's parent type.</param>
        /// <param name="name">The method's name.</param>
        /// <param name="isStatic">
        /// Tells if the method should be a static method
        /// or an instance method.
        /// </param>
        /// <param name="returnType">The type of value returned by the method.</param>
        public DescribedMethod(
            IType parentType,
            UnqualifiedName name,
            bool isStatic,
            IType returnType)
            : base(name.Qualify(parentType.FullName))
        {
            this.ParentType = parentType;
            this.IsStatic = isStatic;
            this.ReturnParameter = new Parameter(returnType);
            this.paramList = new List<Parameter>();
            this.baseMethodList = new List<IMethod>();
        }

        /// <inheritdoc/>
        public IType ParentType { get; private set; }

        /// <inheritdoc/>
        public bool IsConstructor { get; set; }

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
        public Parameter ReturnParameter { get; set; }

        private List<Parameter> paramList;
        private List<IMethod> baseMethodList;

        /// <inheritdoc/>
        public IReadOnlyList<Parameter> Parameters => paramList;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> BaseMethods => baseMethodList;

        /// <summary>
        /// Adds a parameter to the back of this method's parameter list.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
        public void AddParameter(Parameter parameter)
        {
            paramList.Add(parameter);
        }

        /// <summary>
        /// Adds a method to this method's base methods.
        /// </summary>
        /// <param name="baseMethod">The base method.</param>
        public void AddBaseMethod(IMethod baseMethod)
        {
            baseMethodList.Add(baseMethod);
        }

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
