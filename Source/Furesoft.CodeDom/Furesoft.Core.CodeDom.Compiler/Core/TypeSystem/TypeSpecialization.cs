using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

/// <summary>
/// A base type for generic type specializations.
/// </summary>
public abstract class TypeSpecialization : IType
{
    private Lazy<IReadOnlyList<IType>> baseTypeCache;

    private Lazy<IReadOnlyList<IField>> fieldsCache;

    private Lazy<IReadOnlyList<IType>> nestedTypesCache;

    private Lazy<TypeMappingVisitor> instantiatingVisitorCache;

    /// <summary>
    /// Creates an uninitialized generic type from a declaration.
    /// </summary>
    /// <param name="declaration">A declaration.</param>
    internal TypeSpecialization(IType declaration)
    {
        Declaration = declaration;
    }

    /// <summary>
    /// Gets the generic type declaration this type instantiates.
    /// </summary>
    /// <returns>The generic type declaration.</returns>
    public IType Declaration { get; private set; }

    /// <inheritdoc/>
    public abstract TypeParent Parent { get; }

    /// <inheritdoc/>
    public abstract UnqualifiedName Name { get; }

    /// <inheritdoc/>
    public abstract QualifiedName FullName { get; }

    /// <inheritdoc/>
    public abstract IReadOnlyList<IGenericParameter> GenericParameters { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IType> BaseTypes => baseTypeCache.Value;

    /// <inheritdoc/>
    public IReadOnlyList<IField> Fields
    {
        get
        {
            return fieldsCache.Value;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IMethod> Methods { get; set; }

    /// <inheritdoc/>
    public IReadOnlyList<IProperty> Properties
    {
        get
        {
            var declProperties = Declaration.Properties;
            var properties = new IProperty[declProperties.Count];
            for (int i = 0; i < properties.Length; i++)
            {
                properties[i] = IndirectPropertySpecialization.Create(declProperties[i], this);
            }
            return properties;
        }
    }

    /// <inheritdoc/>
    public AttributeMap Attributes => Declaration.Attributes;

    /// <inheritdoc/>
    public IReadOnlyList<IType> NestedTypes => nestedTypesCache.Value;

    /// <summary>
    /// Gets a visitor that substitutes generic arguments for parameters.
    /// </summary>
    internal TypeMappingVisitor InstantiatingVisitor => instantiatingVisitorCache.Value;

    /// <inheritdoc/>
    public override string ToString()
    {
        return FullName.ToString();
    }

    /// <summary>
    /// Initializes a generic type.
    /// </summary>
    protected void Initialize()
    {
        instantiatingVisitorCache = new Lazy<TypeMappingVisitor>(CreateInstantiatingVisitor);
        nestedTypesCache = new Lazy<IReadOnlyList<IType>>(CreateNestedTypes);
        baseTypeCache = new Lazy<IReadOnlyList<IType>>(CreateBaseTypes);
        fieldsCache = new Lazy<IReadOnlyList<IField>>(CreateFields);
    }

    private IReadOnlyList<IType> CreateBaseTypes()
    {
        return instantiatingVisitorCache.Value.VisitAll(Declaration.BaseTypes);
    }

    private IReadOnlyList<IField> CreateFields()
    {
        var declFields = Declaration.Fields;
        var fields = new IField[declFields.Count];
        for (int i = 0; i < fields.Length; i++)
        {
            fields[i] = IndirectFieldSpecialization.Create(declFields[i], this);
        }
        return fields;
    }

    private IReadOnlyList<IType> CreateNestedTypes()
    {
        var nestedTypeDecls = Declaration.NestedTypes;
        var results = new IType[nestedTypeDecls.Count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = IndirectTypeSpecialization.Create(nestedTypeDecls[i], this);
        }
        return results;
    }

    private TypeMappingVisitor CreateInstantiatingVisitor()
    {
        return new TypeMappingVisitor(this.GetRecursiveGenericArgumentMapping());
    }
}

/// <summary>
/// A generic type that is instantiated with a list of type arguments.
/// </summary>
public sealed class DirectTypeSpecialization : TypeSpecialization
{
    // This cache interns all generic types: if two GenericType instances
    // (in the wild, not in this private set-up logic) have equal declaration
    // types and type arguments, then they are *referentially* equal.
    private static InterningCache<DirectTypeSpecialization> GenericTypeCache
        = new(
            new StructuralDirectTypeSpecializationComparer(),
            InitializeInstance);

    private UnqualifiedName simpleName;

    private QualifiedName qualName;

    private DirectTypeSpecialization(
                                IType declaration,
        IReadOnlyList<IType> genericArguments)
        : base(declaration)
    {
        GenericArguments = genericArguments;
        Methods = declaration.Methods;
    }

    /// <summary>
    /// Gets this generic type's list of generic arguments.
    /// </summary>
    /// <returns>The generic arguments.</returns>
    public IReadOnlyList<IType> GenericArguments { get; private set; }

    /// <inheritdoc/>
    public override TypeParent Parent => Declaration.Parent;

    /// <inheritdoc/>
    public override UnqualifiedName Name => simpleName;

    /// <inheritdoc/>
    public override QualifiedName FullName => qualName;

    /// <inheritdoc/>
    public override IReadOnlyList<IGenericParameter> GenericParameters =>
        EmptyArray<IGenericParameter>.Value;

    /// <summary>
    /// Creates a generic specialization of a particular generic
    /// type declaration.
    /// </summary>
    /// <param name="declaration">
    /// The generic type declaration that is specialized into
    /// a concrete type.
    /// </param>
    /// <param name="genericArguments">
    /// The type arguments with which the generic type is
    /// specialized.
    /// </param>
    /// <returns>A generic specialization.</returns>
    internal static DirectTypeSpecialization Create(
        IType declaration,
        IReadOnlyList<IType> genericArguments)
    {
        return GenericTypeCache.Intern(
            new DirectTypeSpecialization(declaration, genericArguments));
    }

    private static DirectTypeSpecialization InitializeInstance(DirectTypeSpecialization instance)
    {
        var declaration = instance.Declaration;
        var genericArguments = instance.GenericArguments;
        var simpleTypeArgNames = new QualifiedName[genericArguments.Count];
        var qualTypeArgNames = new QualifiedName[simpleTypeArgNames.Length];
        for (int i = 0; i < qualTypeArgNames.Length; i++)
        {
            simpleTypeArgNames[i] = genericArguments[i].Name.Qualify();
            qualTypeArgNames[i] = genericArguments[i].FullName;
        }

        instance.simpleName = new GenericName(declaration.Name, simpleTypeArgNames);
        instance.qualName = new GenericName(declaration.FullName, qualTypeArgNames).Qualify();

        instance.Initialize();

        return instance;
    }
}

/// <summary>
/// A type that is defined in an instantiated generic type.
/// </summary>
public sealed class IndirectTypeSpecialization : TypeSpecialization
{
    private static InterningCache<IndirectTypeSpecialization> instanceCache =
                new(
                    new StructuralIndirectTypeSpecializationComparer(),
                    InitializeInstance);

    private Lazy<IReadOnlyList<IGenericParameter>> genericParameterCache;

    private QualifiedName qualName;

    /// <summary>
    /// Creates an uninitialized generic instance type.
    /// </summary>
    /// <param name="declaration">The type's declaration.</param>
    /// <param name="parentType">The type's parent type.</param>
    private IndirectTypeSpecialization(
        IType declaration,
        TypeSpecialization parentType)
        : base(declaration)
    {
        ParentType = parentType;
    }

    /// <summary>
    /// Gets the parent type of this generic instance type.
    /// </summary>
    /// <returns>The parent type.</returns>
    public TypeSpecialization ParentType { get; private set; }

    /// <inheritdoc/>
    public override TypeParent Parent => new(ParentType);

    /// <inheritdoc/>
    public override UnqualifiedName Name => qualName.FullyUnqualifiedName;

    /// <inheritdoc/>
    public override QualifiedName FullName => qualName;

    /// <inheritdoc/>
    public override IReadOnlyList<IGenericParameter> GenericParameters => genericParameterCache.Value;

    /// <summary>
    /// Creates a generic instance type from a generic declaration
    /// and a parent type that is itself an (indirect) generic type.
    /// </summary>
    /// <param name="declaration">
    /// The generic declaration to specialize.
    /// </param>
    /// <param name="parentType">
    /// A specialization of the generic declaration's parent type.
    /// </param>
    /// <returns>A specialization of the generic declaration.</returns>
    internal static IndirectTypeSpecialization Create(
        IType declaration,
        TypeSpecialization parentType)
    {
        return instanceCache.Intern(
            new IndirectTypeSpecialization(declaration, parentType));
    }

    /// <summary>
    /// Initializes an uninitialized indirect type specialization.
    /// </summary>
    /// <param name="instance">The instance to initialize.</param>
    /// <returns>The instance.</returns>
    private static IndirectTypeSpecialization InitializeInstance(IndirectTypeSpecialization instance)
    {
        instance.qualName = instance.Declaration.Name.Qualify(
            instance.ParentType.FullName);
        instance.genericParameterCache = new Lazy<IReadOnlyList<IGenericParameter>>(
            instance.CreateGenericParameters);

        instance.Initialize();

        return instance;
    }

    private IReadOnlyList<IGenericParameter> CreateGenericParameters()
    {
        return IndirectGenericParameterSpecialization.CreateAll(Declaration, this);
    }
}

internal sealed class StructuralDirectTypeSpecializationComparer : IEqualityComparer<DirectTypeSpecialization>
{
    public bool Equals(DirectTypeSpecialization x, DirectTypeSpecialization y)
    {
        return object.Equals(x.Declaration, y.Declaration)
            && Enumerable.SequenceEqual<IType>(
                x.GenericArguments, y.GenericArguments);
    }

    public int GetHashCode(DirectTypeSpecialization obj)
    {
        int result = ((object)obj.Declaration).GetHashCode();
        int genericArgCount = obj.GenericArguments.Count;
        for (int i = 0; i < genericArgCount; i++)
        {
            result = (result << 2) ^ ((object)obj.GenericArguments[i]).GetHashCode();
        }
        return result;
    }
}

internal sealed class StructuralIndirectTypeSpecializationComparer : IEqualityComparer<IndirectTypeSpecialization>
{
    public bool Equals(IndirectTypeSpecialization x, IndirectTypeSpecialization y)
    {
        return object.Equals(x.Declaration, y.Declaration)
            && object.Equals(x.ParentType, y.ParentType);
    }

    public int GetHashCode(IndirectTypeSpecialization obj)
    {
        return (((object)obj.ParentType).GetHashCode() << 4) ^ ((object)obj.Declaration).GetHashCode();
    }
}