using Mono.Cecil;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Backends.CLR;

/// <summary>
/// A CLR field definition.
/// </summary>
public sealed class ClrFieldDefinition : IField
{
    private AttributeMap attributeMap;

    private DeferredInitializer contentsInitializer;

    private IType fieldTypeValue;

    /// <summary>
    /// Creates a Flame field definition that wraps
    /// around an IL field definition.
    /// </summary>
    /// <param name="definition">
    /// The IL field definition to wrap.
    /// </param>
    /// <param name="parentType">
    /// The parent type that defines the field wrapper.
    /// </param>
    public ClrFieldDefinition(
        FieldDefinition definition,
        ClrTypeDefinition parentType)
    {
        this.Definition = definition;
        this.ParentType = parentType;
        this.FullName = new SimpleName(definition.Name)
            .Qualify(parentType.FullName);
        this.IsStatic = definition.IsStatic;
        this.contentsInitializer = parentType.Assembly
            .CreateSynchronizedInitializer(AnalyzeContents);
    }

    /// <inheritdoc/>
    public AttributeMap Attributes
    {
        get
        {
            contentsInitializer.Initialize();
            return attributeMap;
        }
    }

    /// <summary>
    /// Gets the IL field definition wrapped by this
    /// Flame field definition.
    /// </summary>
    /// <returns>An IL field definition.</returns>
    public FieldDefinition Definition { get; private set; }

    /// <inheritdoc/>
    public IType FieldType
    {
        get
        {
            contentsInitializer.Initialize();
            return fieldTypeValue;
        }
    }

    /// <inheritdoc/>
    public QualifiedName FullName { get; private set; }

    /// <inheritdoc/>
    public bool IsStatic { get; private set; }

    /// <inheritdoc/>
    public UnqualifiedName Name => FullName.FullyUnqualifiedName;

    /// <summary>
    /// Gets this field definition's parent type.
    /// </summary>
    /// <returns>The parent type of this field definition.</returns>
    public ClrTypeDefinition ParentType { get; private set; }

    /// <inheritdoc/>
    IType ITypeMember.ParentType => ParentType;

    private AccessModifier AnalyzeAccessModifier()
    {
        if (Definition.IsPublic)
        {
            return AccessModifier.Public;
        }
        else if (Definition.IsPrivate)
        {
            return AccessModifier.Private;
        }
        else if (Definition.IsFamily)
        {
            return AccessModifier.Protected;
        }
        else if (Definition.IsFamilyAndAssembly)
        {
            return AccessModifier.ProtectedAndInternal;
        }
        else if (Definition.IsFamilyOrAssembly)
        {
            return AccessModifier.ProtectedOrInternal;
        }
        else
        {
            return AccessModifier.Internal;
        }
    }

    private void AnalyzeContents()
    {
        fieldTypeValue = TypeHelpers.BoxIfReferenceType(
            ParentType.Assembly.Resolve(Definition.FieldType));

        var attrBuilder = new AttributeMapBuilder();
        // Analyze access modifier.
        attrBuilder.Add(AccessModifierAttribute.Create(AnalyzeAccessModifier()));

        // TODO: analyze other attributes.
        attributeMap = new AttributeMap(attrBuilder);
    }
}