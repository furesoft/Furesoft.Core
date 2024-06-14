using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

/// <summary>
/// A type that can be constructed incrementally in an imperative fashion.
/// </summary>
public class DescribedType : DescribedGenericMember, IType
{
    private List<IType> baseTypeList;

    private List<IField> fieldList;

    private List<IMethod> methodList;

    private List<IProperty> propertyList;

    private List<IType> nestedTypeList;

    /// <summary>
    /// Creates a type from a name and a parent assembly.
    /// </summary>
    /// <param name="fullName">The type's full name.</param>
    /// <param name="assembly">The assembly that defines the type.</param>
    public DescribedType(QualifiedName fullName, IAssembly assembly)
        : base(fullName)
    {
        Parent = new TypeParent(assembly);
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
        Parent = new TypeParent(parentType);
        Initialize();
    }

    /// <inheritdoc/>
    public TypeParent Parent { get; private set; }

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

    public void AddMethod(IMethod method)
    {
        CheckParent(method);
        methodList.Add(method);
    }

    /// <summary>
    /// Adds a method to this type.
    /// </summary>
    /// <param name="method">The method to add.</param>
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

    private void Initialize()
    {
        baseTypeList = [];
        fieldList = [];
        methodList = [];
        propertyList = [];
        nestedTypeList = [];
    }

    private void CheckParent(ITypeMember member)
    {
        ContractHelpers.Assert(
            object.Equals(this, member.ParentType),
            "A member can only be added to its defining type.");
    }

    public bool IsSealed
    {
        get { return Owns(FlagAttribute.Sealed); }
        set { SetAttr(value, FlagAttribute.Sealed); }
    }
}