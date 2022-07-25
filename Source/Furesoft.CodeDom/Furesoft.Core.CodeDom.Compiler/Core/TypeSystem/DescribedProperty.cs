using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem
{
    public class DescribedProperty : DescribedMember, IProperty
    {
        private readonly IType _parentType;
        private readonly IType _propertyType;
        private IList<IAccessor> _accessors;
        private IList<Parameter> _indexers;
        private IList<DescribedPropertyMethod> _propertyMethods;

        public DescribedPropertyMethod Getter
        {
            get => _propertyMethods.First(_ => _.IsGetter);
            set { if (HasGetter) { _propertyMethods.Remove(Getter); } _propertyMethods.Add(value); value.IsGetter = true; }
        }
        public bool HasGetter 
        {
            get => _propertyMethods.Any(_ => _.IsGetter);
        }
        public DescribedPropertyMethod Setter
        {
            get => _propertyMethods.First(_ => _.IsSetter);
            set { if (HasSetter) { _propertyMethods.Remove(Setter); } _propertyMethods.Add(value); value.IsSetter = true; }
        }
        public bool HasSetter
        {
            get => _propertyMethods.Any(_ => _.IsSetter);
        }

        public DescribedProperty(UnqualifiedName name, IType propertyType, IType parentType)
            : base(name.Qualify(parentType.FullName))
        {
            _propertyType = propertyType;
            _parentType = parentType;
            _indexers = new List<Parameter>();
            _accessors = new List<IAccessor>();
        }

        public IReadOnlyList<IAccessor> Accessors => (IReadOnlyList<IAccessor>)_accessors;
        public IReadOnlyList<Parameter> IndexerParameters => (IReadOnlyList<Parameter>)_indexers;
        public IReadOnlyList<DescribedPropertyMethod> PropertyMethods => (IReadOnlyList<DescribedPropertyMethod>)_propertyMethods;
        public IType ParentType => _parentType;
        public IType PropertyType => _propertyType;

        public void AddAccessor(IAccessor accessor)
        {
            _accessors.Add(accessor);
        }

        public void AddIndexer(Parameter parameter)
        {
            _indexers.Add(parameter);
        }
    }

    public class DescribedPropertyMethod : DescribedMember, IPropertyMethod
    {
        private readonly IType _parentType;
        public MethodBody Body { get; set; }

        public DescribedPropertyMethod(UnqualifiedName name, IType parentType) : base(name.Qualify(parentType.FullName.Qualifier))
        {
            _parentType = parentType;
        }

        public IType ParentType => _parentType;

        public bool IsGetter
        {
            get { return GetPropertyTypeModifier().Equals(PropertyTypeModifier.Getter); }
            set { RemovePropertyTypeModifier(); if (value) AddAttribute(PropertyTypeModifierAttribute.Create(PropertyTypeModifier.Getter)); }
        }
        public bool IsSetter
        {
            get { return GetPropertyTypeModifier().Equals(PropertyTypeModifier.Setter); }
            set { RemovePropertyTypeModifier(); if (value) AddAttribute(PropertyTypeModifierAttribute.Create(PropertyTypeModifier.Setter)); }
        }
        public bool IsInit
        {
            get { return GetPropertyTypeModifier().Equals(PropertyTypeModifier.Init); }
            set { RemovePropertyTypeModifier(); if (value) AddAttribute(PropertyTypeModifierAttribute.Create(PropertyTypeModifier.Init)); }
        }

        public PropertyTypeModifier GetPropertyTypeModifier() => PropertyTypeModifierAttribute.GetPropertyTypeModifier(this);
        public void RemovePropertyTypeModifier() => RemoveAttributesFromType(PropertyTypeModifierAttribute.AttributeType);
    }
}