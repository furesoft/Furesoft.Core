using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.CodeDom.Backends.CLR;

/// <summary>
/// A Flame type that wraps an IL type definition.
/// </summary>
public sealed class ClrTypeDefinition : IType
{
    private static readonly IReadOnlyDictionary<string, IntegerSpec> integerSpecMap =
                new Dictionary<string, IntegerSpec>()
            {
        { nameof(SByte), IntegerSpec.Int8 },
        { nameof(Int16), IntegerSpec.Int16 },
        { nameof(Int32), IntegerSpec.Int32 },
        { nameof(Int64), IntegerSpec.Int64 },
        { nameof(Boolean), IntegerSpec.UInt1 },
        { nameof(Byte), IntegerSpec.UInt8 },
        { nameof(UInt16), IntegerSpec.UInt16 },
        { nameof(UInt32), IntegerSpec.UInt32 },
        { nameof(UInt64), IntegerSpec.UInt64 },
        { nameof(Char), IntegerSpec.Int16 }
            };

    private AttributeMap attributeMap;

    private IReadOnlyList<IType> baseTypeList;

    private DeferredInitializer contentsInitializer;

    private IReadOnlyList<ClrFieldDefinition> fieldDefList;

    private Lazy<IReadOnlyList<ClrGenericParameter>> genericParamCache;

    private IReadOnlyList<ClrMethodDefinition> methodDefList;

    private Lazy<IReadOnlyList<ClrTypeDefinition>> nestedTypeCache;

    private IReadOnlyList<ClrPropertyDefinition> propertyDefList;

    private HashSet<IMethod> virtualMethodSet;

    /// <summary>
    /// Creates a Flame type that wraps a particular type
    /// definition.
    /// </summary>
    /// <param name="definition">The definition to wrap.</param>
    /// <param name="assembly">
    /// The assembly that directly defines this type.
    /// </param>
    public ClrTypeDefinition(
        TypeDefinition definition,
        ClrAssembly assembly)
        : this(
            definition,
            assembly,
            new TypeParent(assembly),
            NameConversion
                .ParseSimpleName(definition.Name)
                .Qualify(
                    NameConversion.ParseNamespace(
                        definition.Namespace)))
    { }

    /// <summary>
    /// Creates a Flame type that wraps a particular nested type
    /// definition.
    /// </summary>
    /// <param name="definition">The definition to wrap.</param>
    /// <param name="parentType">
    /// The type that directly defines this type.
    /// </param>
    public ClrTypeDefinition(
        TypeDefinition definition,
        ClrTypeDefinition parentType)
        : this(
            definition,
            parentType.Assembly,
            new TypeParent(parentType),
            NameConversion
                .ParseSimpleName(definition.Name)
                .Qualify(parentType.FullName))
    { }

    private ClrTypeDefinition(
        TypeDefinition definition,
        ClrAssembly assembly,
        TypeParent parent,
        QualifiedName fullName)
    {
        this.Definition = definition;
        this.Assembly = assembly;
        this.Parent = parent;
        this.contentsInitializer = Assembly
            .CreateSynchronizedInitializer(AnalyzeContents);
        this.OverrideInitializer = Assembly
            .CreateSynchronizedInitializer(AnalyzeOverrides);

        this.FullName = fullName;
        this.nestedTypeCache = Assembly
            .CreateSynchronizedLazy<IReadOnlyList<ClrTypeDefinition>>(() =>
        {
            return definition.NestedTypes
                .Select(t => new ClrTypeDefinition(t, this))
                .ToArray();
        });
        this.genericParamCache = Assembly
            .CreateSynchronizedLazy<IReadOnlyList<ClrGenericParameter>>(() =>
        {
            return definition.GenericParameters
                .Skip(
                    parent.IsType
                        ? parent.TypeOrNull.GenericParameters.Count
                        : 0)
                .Select(param => new ClrGenericParameter(param, this))
                .ToArray();
        });
    }

    /// <summary>
    /// Gets the assembly that directly or indirectly defines
    /// this type.
    /// </summary>
    /// <returns>The assembly.</returns>
    public ClrAssembly Assembly { get; private set; }

    /// <inheritdoc/>
    public AttributeMap Attributes
    {
        get
        {
            contentsInitializer.Initialize();
            return attributeMap;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IType> BaseTypes
    {
        get
        {
            contentsInitializer.Initialize();
            return baseTypeList;
        }
    }

    /// <summary>
    /// Gets the type definition this type is based on.
    /// </summary>
    /// <returns>The type definition.</returns>
    public TypeDefinition Definition { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<IField> Fields
    {
        get
        {
            contentsInitializer.Initialize();
            return fieldDefList;
        }
    }

    /// <inheritdoc/>
    public QualifiedName FullName { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<IGenericParameter> GenericParameters => genericParamCache.Value;

    /// <inheritdoc/>
    public IReadOnlyList<IMethod> Methods
    {
        get
        {
            contentsInitializer.Initialize();
            return methodDefList;
        }
    }

    /// <inheritdoc/>
    public UnqualifiedName Name => FullName.FullyUnqualifiedName;

    /// <inheritdoc/>
    public IReadOnlyList<IType> NestedTypes => nestedTypeCache.Value;

    /// <inheritdoc/>
    public TypeParent Parent { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<IProperty> Properties
    {
        get
        {
            contentsInitializer.Initialize();
            return propertyDefList;
        }
    }

    /// <summary>
    /// Gets the deferred initializer for analyzing method
    /// overrides.
    /// </summary>
    /// <returns>The override analysis initializer.</returns>
    internal DeferredInitializer OverrideInitializer { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return FullName.ToString();
    }

    private AccessModifier AnalyzeAccessModifier()
    {
        if (Definition.IsPublic || Definition.IsNestedPublic)
        {
            return AccessModifier.Public;
        }
        else if (Definition.IsNestedPrivate)
        {
            return AccessModifier.Private;
        }
        else if (Definition.IsNestedFamily)
        {
            return AccessModifier.Protected;
        }
        else if (Definition.IsNestedFamilyAndAssembly)
        {
            return AccessModifier.ProtectedAndInternal;
        }
        else if (Definition.IsNestedFamilyOrAssembly)
        {
            return AccessModifier.ProtectedOrInternal;
        }
        else
        {
            return AccessModifier.Internal;
        }
    }

    private void AnalyzeAttributes()
    {
        var attrBuilder = new AttributeMapBuilder();

        // Handle low-hanging fruit first.
        if (!Definition.IsValueType)
        {
            attrBuilder.Add(FlagAttribute.ReferenceType);
        }
        if (Definition.IsAbstract)
        {
            attrBuilder.Add(FlagAttribute.Abstract);
        }
        if (Definition.IsInterface)
        {
            attrBuilder.Add(FlagAttribute.Abstract);
            attrBuilder.Add(FlagAttribute.InterfaceType);
        }
        if (!Definition.IsSealed)
        {
            attrBuilder.Add(FlagAttribute.Virtual);
        }

        // If we're dealing with an integer type, then we want to
        // assign that type an integer spec.
        IntegerSpec iSpec;
        if (Definition.Namespace == "System")
        {
            if (integerSpecMap.TryGetValue(Definition.Name, out iSpec))
            {
                attrBuilder.Add(IntegerSpecAttribute.Create(iSpec));
                attrBuilder.Add(FlagAttribute.SpecialType);
            }
            else if (Definition.Name == "Single" || Definition.Name == "Double")
            {
                attrBuilder.Add(FlagAttribute.SpecialType);
            }
        }

        // If we are presented an enum type, then we need to look
        // up its 'value__' field, which specifies its enum type.
        if (Definition.IsEnum)
        {
            // The 'value__' field has some very particular properties:
            // it has both the "runtime special name" and "special name"
            // attributes.
            var valueField = Definition.Fields.FirstOrDefault(
                field =>
                    field.Name == "value__"
                    && field.IsRuntimeSpecialName
                    && field.IsSpecialName);

            // Make sure that we didn't encounter a "fake" enum.
            if (valueField != null)
            {
                // Resolve the enum's element type. This should always be an
                // integer type.
                var enumElementType = Assembly.Resolve(valueField.FieldType);
                var enumIntSpec = enumElementType.GetIntegerSpecOrNull();

                if (enumIntSpec != null)
                {
                    // Mark the enum type itself as an integer type because it
                    // acts like an integer type in essentially every way.
                    attrBuilder.Add(IntegerSpecAttribute.Create(enumIntSpec));
                    attrBuilder.Add(FlagAttribute.SpecialType);
                }
            }
        }

        // Analyze access modifier.
        attrBuilder.Add(AccessModifierAttribute.Create(AnalyzeAccessModifier()));

        // TODO: support more attributes.
        attributeMap = new AttributeMap(attrBuilder);
    }

    private void AnalyzeContents()
    {
        // Analyze attributes.
        AnalyzeAttributes();

        // Analyze base types and interface implementations.
        baseTypeList = (Definition.BaseType == null
            ? new TypeReference[] { }
            : new[] { Definition.BaseType })
            .Concat(Definition.Interfaces.Select(impl => impl.InterfaceType))
            .Select(Assembly.Resolve)
            .ToArray();

        // Analyze fields.
        fieldDefList = Definition.Fields
            .Select(field => new ClrFieldDefinition(field, this))
            .ToArray();

        // Analyze methods. Exclude methods that have semantics (property
        // and event accessors). Those will be analyzed elsewhere.
        methodDefList = Definition.Methods
            .Where(method => method.SemanticsAttributes == MethodSemanticsAttributes.None)
            .Select(method => new ClrMethodDefinition(method, this))
            .ToArray();

        // Analyze properties.
        propertyDefList = Definition.Properties
            .Select(property => new ClrPropertyDefinition(property, this))
            .ToArray();
    }

    private void AnalyzeOverrides()
    {
        // A method's base methods consist of its implicit
        // and explicit overrides. (Flame doesn't distinguish
        // between these two.)
        //
        //   * Explicit overrides are extracted directly from
        //     the method definition.
        //
        //   * Implicit overrides are derived by inspecting
        //     the base types of the type declaring the method:
        //     a method declared/defined in one of the base types
        //     is an implicit override candidate if it is not
        //     already overridden either by a method in another
        //     base type or (explicitly) in the declaring type.
        //
        // The ugly bit is that *all* virtual methods in
        // a type participate in override resolution: explicit
        // overrides can be resolved individually, but implicit
        // overrides always depend on other methods.
        //
        // We know that the (type) inheritance graph is a DAG, so
        // we can safely derive overrides by walking the inheritance
        // graph. Specifically, we'll construct a set of virtual
        // methods for each type:
        //
        //   * Initialize the virtual method set as the union of
        //     the virtual method sets of the base types.
        //
        //   * Remove all explicit overrides from the set and add
        //     them to the overriding methods.
        //
        //   * Remove all implicit overrides from the set and add
        //     them to the overriding methods.
        //

        virtualMethodSet = [];

        contentsInitializer.Initialize();
        foreach (var baseType in baseTypeList)
        {
            if (baseType is ClrTypeDefinition)
            {
                // Optimization for ClrTypeDefinition instances.
                var clrBaseType = (ClrTypeDefinition)baseType;
                clrBaseType.OverrideInitializer.Initialize();
                virtualMethodSet.UnionWith(clrBaseType.virtualMethodSet);
            }
            else
            {
                // General case.
                virtualMethodSet.UnionWith(baseType.GetVirtualMethodSet());
            }
        }

        var allMethodDefs = methodDefList
            .Concat(propertyDefList.SelectMany(prop => prop.Accessors))
            .ToArray();

        // Special case: interfaces. These guys never override anything,
        // so we can skip the regular override resolution process and skip
        // right to the part where we add all methods to the virtual
        // method set.
        if (Definition.IsInterface)
        {
            virtualMethodSet.UnionWith(allMethodDefs);
            foreach (var method in allMethodDefs)
            {
                method.BaseMethodStore = [];
            }
            return;
        }

        // Handle explicit overrides.
        foreach (var method in allMethodDefs)
        {
            method.BaseMethodStore = [];
            foreach (var overrideRef in method.Definition.Overrides)
            {
                var overrideMethod = Assembly.Resolve(overrideRef);
                method.BaseMethodStore.Add(overrideMethod);
                virtualMethodSet.Remove(overrideMethod);
            }
        }

        // Populate a mapping of method signatures to lists of methods
        // so we can efficiently match methods to overrides.
        var virtualMethodSignatures = new Dictionary<ClrMethodSignature, List<IMethod>>();
        foreach (var virtualMethod in virtualMethodSet)
        {
            var signature = ClrMethodSignature.Create(virtualMethod);
            List<IMethod> virtualMethodList;
            if (!virtualMethodSignatures.TryGetValue(
                signature,
                out virtualMethodList))
            {
                virtualMethodList = [];
                virtualMethodSignatures[signature] = virtualMethodList;
            }
            virtualMethodList.Add(virtualMethod);
        }

        // Handle implicit overrides.
        foreach (var method in allMethodDefs)
        {
            if (method.Definition.IsVirtual)
            {
                var signature = ClrMethodSignature.Create(method);
                List<IMethod> virtualMethodList;
                if (virtualMethodSignatures.TryGetValue(
                    signature, out virtualMethodList))
                {
                    foreach (var overrideMethod in virtualMethodList)
                    {
                        if (virtualMethodSet.Contains(overrideMethod))
                        {
                            method.BaseMethodStore.Add(overrideMethod);
                            virtualMethodSet.Remove(overrideMethod);
                        }
                    }
                }
            }
        }

        // Add virtual methods to virtual method set.
        foreach (var method in allMethodDefs)
        {
            if (method.Definition.IsVirtual && !method.Definition.IsFinal)
            {
                virtualMethodSet.Add(method);
            }
        }
    }
}