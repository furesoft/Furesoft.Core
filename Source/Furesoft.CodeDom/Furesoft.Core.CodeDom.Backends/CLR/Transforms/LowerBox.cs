using System;
using System.Linq;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.Transforms;
using Furesoft.Core.CodeDom.Compiler;

namespace Furesoft.Core.CodeDom.Backends.CLR.Transforms;

/// <summary>
/// A transform that lowers reference type creating 'box' instructions
/// to 'newobj' instructions if possible and calls to well-known functions
/// if not.
/// </summary>
public sealed class LowerBox : IntraproceduralOptimization
{
    /// <summary>
    /// Creates an instance of the box-lowering transform for CIL.
    /// </summary>
    /// <param name="corlibResolver">
    /// A type resolver for corlib.
    /// </param>
    public LowerBox(TypeResolver corlibResolver)
    {
        var typeTypes = corlibResolver.ResolveTypes(
            new SimpleName("Type").Qualify("System"));

        if (typeTypes.Count != 1)
        {
            throw new ArgumentException(
                "Type resolver defines '" + typeTypes.Count + "' types named 'System.Type'; expected exactly one.");
        }

        GetTypeFromHandleMethod = typeTypes[0].Methods.FirstOrDefault(
            method => method.Name.ToString() == "GetTypeFromHandle"
                && method.IsStatic
                && method.Parameters.Count == 1);

        if (GetTypeFromHandleMethod == null)
        {
            throw new ArgumentException(
                "Type 'System.Type' does not define an appropriate 'GetTypeFromHandle' method.");
        }

        var formatterServicesTypes = corlibResolver.ResolveTypes(
            new SimpleName("FormatterServices")
            .Qualify("Serialization")
            .Qualify("Runtime")
            .Qualify("System"));

        if (formatterServicesTypes.Count != 1)
        {
            throw new ArgumentException(
                "Type resolver defines '" + formatterServicesTypes.Count +
                "' types named 'System.Runtime.Serialization.FormatterServices'; expected exactly one.");
        }

        CreateUninitializedObjectMethod = formatterServicesTypes[0].Methods.FirstOrDefault(
            method => method.Name.ToString() == "GetUninitializedObject"
                && method.IsStatic
                && method.Parameters.Count == 1
                && method.Parameters[0].Type == GetTypeFromHandleMethod.ReturnParameter.Type);

        if (CreateUninitializedObjectMethod == null)
        {
            throw new ArgumentException(
                "Type 'System.Runtime.Serialization.FormatterServices' does not define " +
                "an appropriate 'GetUninitializedObject' method.");
        }
    }

    /// <summary>
    /// Creates an instance of the box-lowering transform for CIL.
    /// </summary>
    /// <param name="corlib">
    /// The corlib assembly.
    /// </param>
    public LowerBox(IAssembly corlib)
        : this(new TypeResolver(corlib))
    { }

    /// <summary>
    /// Creates an instance of the box-lowering transform for CIL.
    /// </summary>
    /// <param name="getTypeFromHandleMethod">
    /// A reference to a <c>GetTypeFromHandle</c> method that takes
    /// a type token and produces a type.
    /// </param>
    /// <param name="createUninitializedObjectMethod">
    /// A reference to a <c>GetUninitializedObject</c> method that
    /// takes a type and produces a default-initialized object of that type.
    /// </param>
    public LowerBox(
        IMethod getTypeFromHandleMethod,
        IMethod createUninitializedObjectMethod)
    {
        this.GetTypeFromHandleMethod = getTypeFromHandleMethod;
        this.CreateUninitializedObjectMethod = createUninitializedObjectMethod;
    }

    /// <summary>
    /// Gets a reference to a <c>GetUninitializedObject</c> method that
    /// takes a type and produces a default-initialized object of that type.
    /// </summary>
    /// <value>A method reference.</value>
    public IMethod CreateUninitializedObjectMethod { get; private set; }

    /// <summary>
    /// Gets a reference to a <c>GetTypeFromHandle</c> method that takes
    /// a type token and produces a type.
    /// </summary>
    /// <value>A method reference.</value>
    public IMethod GetTypeFromHandleMethod { get; private set; }

    /// <inheritdoc/>
    public override FlowGraph Apply(FlowGraph graph)
    {
        var builder = graph.ToBuilder();
        foreach (var instruction in builder.Instructions)
        {
            var proto = instruction.Prototype;
            if (proto is BoxPrototype)
            {
                var boxProto = (BoxPrototype)proto;
                if (boxProto.ElementType.IsReferenceType())
                {
                    LowerReferenceTypeBox(instruction, boxProto.ElementType);
                }
            }
        }
        return builder.ToImmutable();
    }

    private void LowerReferenceTypeBox(
        InstructionBuilder instruction,
        IType elementType)
    {
        var token = instruction.InsertBefore(
            Instruction.CreateConstant(
                new TypeTokenConstant(elementType),
                GetTypeFromHandleMethod.Parameters[0].Type));

        var type = instruction.InsertBefore(
            Instruction.CreateCall(
                GetTypeFromHandleMethod,
                MethodLookup.Static,
                new ValueTag[] { token }));

        var obj = instruction.InsertBefore(
            Instruction.CreateCall(
                CreateUninitializedObjectMethod,
                MethodLookup.Static,
                new ValueTag[] { type }));

        instruction.Instruction = Instruction.CreateReinterpretCast(
            (PointerType)instruction.ResultType,
            obj);
    }
}