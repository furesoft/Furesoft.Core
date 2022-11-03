using Furesoft.Core.CodeDom.Backends.CLR.Analysis;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Furesoft.Core.CodeDom.Compiler;

namespace Furesoft.Core.CodeDom.Backends.CLR;

/// <summary>
/// A Flame method that wraps an IL method definition.
/// </summary>
public class ClrMethodDefinition : IBodyMethod
{
    private AttributeMap attributeMap;

    private IReadOnlyList<Parameter> formalParams;

    private Lazy<IReadOnlyList<IGenericParameter>> genericParameterCache;

    private Lazy<MethodBody> methodBody;

    private Parameter returnParam;

    private DeferredInitializer signatureInitializer;

    /// <summary>
    /// Creates a wrapper around an IL method definition.
    /// </summary>
    /// <param name="definition">
    /// The definition to wrap in a Flame method.
    /// </param>
    /// <param name="parentType">
    /// The definition's declaring type.
    /// </param>
    public ClrMethodDefinition(
        MethodDefinition definition,
        ClrTypeDefinition parentType)
    {
        this.Definition = definition;
        this.ParentType = parentType;
        this.IsConstructor = Definition.IsConstructor;
        this.IsStatic = Definition.IsStatic;

        this.FullName = NameConversion
            .ParseSimpleName(definition.Name)
            .Qualify(parentType.FullName);

        this.genericParameterCache = parentType.Assembly
            .CreateSynchronizedLazy<IReadOnlyList<IGenericParameter>>(() =>
                definition.GenericParameters
                    .Select(param => new ClrGenericParameter(param, this))
                    .ToArray());

        this.signatureInitializer = parentType.Assembly
            .CreateSynchronizedInitializer(AnalyzeSignature);

        this.methodBody = parentType.Assembly
            .CreateSynchronizedLazy(AnalyzeBody);
    }

    /// <inheritdoc/>
    public AttributeMap Attributes
    {
        get
        {
            signatureInitializer.Initialize();
            return attributeMap;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IMethod> BaseMethods
    {
        get
        {
            ParentType.OverrideInitializer.Initialize();
            return BaseMethodStore;
        }
    }

    /// <inheritdoc/>
    public MethodBody Body
    {
        get
        {
            return methodBody.Value;
        }
    }

    /// <summary>
    /// Gets the IL method definition wrapped by this
    /// Flame method definition.
    /// </summary>
    /// <returns>An IL method definition.</returns>
    public MethodDefinition Definition { get; private set; }

    /// <inheritdoc/>
    public QualifiedName FullName { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<IGenericParameter> GenericParameters =>
        genericParameterCache.Value;

    /// <inheritdoc/>
    public bool IsConstructor { get; private set; }

    /// <inheritdoc/>
    public bool IsDestructor { get; private set; }

    /// <inheritdoc/>
    public bool IsStatic { get; private set; }

    /// <inheritdoc/>
    public UnqualifiedName Name => FullName.FullyUnqualifiedName;

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters
    {
        get
        {
            signatureInitializer.Initialize();
            return formalParams;
        }
    }

    /// <summary>
    /// Gets the type that defines this method.
    /// </summary>
    /// <returns>The type that defines this method.</returns>
    public ClrTypeDefinition ParentType { get; private set; }

    /// <inheritdoc/>
    IType ITypeMember.ParentType => ParentType;

    /// <inheritdoc/>
    public Parameter ReturnParameter
    {
        get
        {
            signatureInitializer.Initialize();
            return returnParam;
        }
    }

    /// <summary>
    /// Gets or sets the backing store for the base method list.
    /// </summary>
    /// <returns>The list of base methods.</returns>
    internal List<IMethod> BaseMethodStore { get; set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents
    /// the current <see cref="T:Flame.Clr.ClrMethodDefinition"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents
    /// the current <see cref="T:Flame.Clr.ClrMethodDefinition"/>.
    /// </returns>
    public override string ToString()
    {
        return Definition.ToString();
    }

    internal static Parameter WrapParameter(
        ParameterDefinition parameter,
        ClrAssembly assembly,
        IGenericMember enclosingMember)
    {
        var attrBuilder = new AttributeMapBuilder();
        // TODO: actually analyze the parameter's attributes.
        return new Parameter(
            TypeHelpers.BoxIfReferenceType(
                assembly.Resolve(
                    parameter.ParameterType,
                    enclosingMember)),
            parameter.Name,
            new AttributeMap(attrBuilder));
    }

    internal static Parameter WrapReturnParameter(
        MethodReturnType returnParameter,
        ClrAssembly assembly,
        IGenericMember enclosingMember)
    {
        var attrBuilder = new AttributeMapBuilder();
        // TODO: actually analyze the parameter's attributes.
        return new Parameter(
            TypeHelpers.BoxIfReferenceType(
                assembly.Resolve(
                    returnParameter.ReturnType,
                    enclosingMember)),
            returnParameter.Name,
            new AttributeMap(attrBuilder));
    }

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

    private MethodBody AnalyzeBody()
    {
        if (Definition.HasBody)
        {
            return ClrMethodBodyAnalyzer.Analyze(
                Definition.Body,
                this);
        }
        else
        {
            return null;
        }
    }

    private void AnalyzeSignature()
    {
        var assembly = ParentType.Assembly;

        // Analyze the return parameter.
        returnParam = WrapReturnParameter(
            Definition.MethodReturnType,
            assembly,
            this);

        // Analyze the parameter list.
        formalParams = Definition.Parameters
            .Select(param => WrapParameter(param, assembly, this))
            .ToArray();

        // Analyze the method definition's attributes.
        var attrBuilder = new AttributeMapBuilder();
        if (Definition.IsAbstract || Definition.DeclaringType.IsInterface)
        {
            attrBuilder.Add(FlagAttribute.Abstract);
            attrBuilder.Add(FlagAttribute.Virtual);
        }
        else if (Definition.IsVirtual)
        {
            attrBuilder.Add(FlagAttribute.Virtual);
        }

        if (Definition.IsPInvokeImpl)
        {
            if (Definition.HasPInvokeInfo)
            {
                attrBuilder.Add(new ExternAttribute(Definition.PInvokeInfo.EntryPoint));
            }
            else
            {
                attrBuilder.Add(new ExternAttribute());
            }
        }
        else if (Definition.IsInternalCall)
        {
            attrBuilder.Add(FlagAttribute.InternalCall);
        }

        // The default 'object' constructor is a nop. Taking that
        // into account can significantly improve constructor inlining
        // results.
        var declaringType = Definition.DeclaringType;
        if (declaringType.Namespace == "System"
            && declaringType.Name == "Object"
            && Definition.IsConstructor
            && Definition.Parameters.Count == 0)
        {
            attrBuilder.Add(new ExceptionSpecificationAttribute(ExceptionSpecification.NoThrow));
            attrBuilder.Add(new MemorySpecificationAttribute(MemorySpecification.Nothing));
        }

        // Analyze access modifier.
        attrBuilder.Add(AccessModifierAttribute.Create(AnalyzeAccessModifier()));
        // TODO: analyze more attributes.
        attributeMap = new AttributeMap(attrBuilder);
    }
}