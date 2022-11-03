using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem;

public class DescribedProperty : DescribedMember, IProperty
{
    private readonly IType _parentType;
    private readonly IType _propertyType;
    private IList<IAccessor> _accessors;
    private IList<Parameter> _indexers;

    public DescribedPropertyMethod Getter
    {
        get => GetMethod(PropertyTypeModifier.Getter);
        set { if (HasGetter) { PropertyMethods.Remove(Getter); } PropertyMethods.Add(value); value.IsGetter = true; }
    }
    public bool HasGetter 
    {
        get => HasMethod(PropertyTypeModifier.Getter);
    }
    public DescribedPropertyMethod Setter
    {
        get => GetMethod(PropertyTypeModifier.Setter);
        set { if (HasSetter) { PropertyMethods.Remove(Setter); } PropertyMethods.Add(value); value.IsSetter = true; }
    }
    public bool HasSetter
    {
        get => HasMethod(PropertyTypeModifier.Setter);
    }
    public DescribedPropertyMethod InitOnlySetter
    {
        get => GetMethod(PropertyTypeModifier.Init);
        set { if (HasInitOnlySetter) { PropertyMethods.Remove(InitOnlySetter); } PropertyMethods.Add(value); value.IsInit = true; }
    }
    public bool HasInitOnlySetter
    {
        get => HasMethod(PropertyTypeModifier.Init);
    }

    public bool HasMethod(PropertyTypeModifier typeMod) => PropertyMethods.Any(_ => _.GetPropertyTypeModifier().Equals(typeMod));
    public DescribedPropertyMethod GetMethod(PropertyTypeModifier typeMod) => PropertyMethods.First(_ => _.GetPropertyTypeModifier().Equals(typeMod));

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
    public List<DescribedPropertyMethod> PropertyMethods { get; } = new();
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