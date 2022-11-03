using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.Transforms;
using Furesoft.Core.CodeDom.Compiler;

namespace Furesoft.Core.CodeDom.Backends.CLR.Transforms;

/// <summary>
/// An intraprocedural transform that turns CIL idioms for delegates
/// into specialized Flame IR instructions.
/// </summary>
public sealed class CanonicalizeDelegates : IntraproceduralOptimization
{
    /// <summary>
    /// An instance of the delegate canonicalization transform.
    /// </summary>
    public static readonly CanonicalizeDelegates Instance = new();

    private CanonicalizeDelegates()
    { }

    /// <inheritdoc/>
    public override FlowGraph Apply(FlowGraph graph)
    {
        var builder = graph.ToBuilder();
        foreach (var instruction in builder.NamedInstructions)
        {
            var proto = instruction.Prototype;
            if (proto is CallPrototype)
            {
                // CIL delegates are called using a (virtual) call to the
                // delegate's 'Invoke' method. However, in Flame IR we want
                // to make indirect function calls explicit using a specialized
                // opcode.
                var callProto = (CallPrototype)proto;
                var delegateType = callProto.Callee.ParentType;
                IMethod invokeMethod;
                if (TypeHelpers.TryGetDelegateInvokeMethod(delegateType, out invokeMethod)
                    && invokeMethod == callProto.Callee)
                {
                    instruction.Instruction = Instruction.CreateIndirectCall(
                        callProto.ResultType,
                        callProto.Callee.Parameters.EagerSelect(p => p.Type),
                        callProto.GetThisArgument(instruction.Instruction),
                        callProto.GetArgumentList(instruction.Instruction).ToArray());
                }
            }
            else if (proto is NewObjectPrototype && proto.ParameterCount == 2)
            {
                // Delegates are created by the 'newobj' opcode, but we want
                // to use Flame IR's delegate-creation instructions, which better
                // communicate what is going on to the optimizer.

                var newobjProto = (NewObjectPrototype)proto;
                var argList = newobjProto.GetArgumentList(instruction.Instruction);
                var boundObject = argList[0];
                var functionPointer = argList[1];
                if (!builder.ContainsInstruction(functionPointer))
                {
                    continue;
                }

                var functionPointerInstruction = builder.GetInstruction(functionPointer);
                var functionPointerProto = functionPointerInstruction.Prototype as NewDelegatePrototype;
                if (functionPointerProto == null)
                {
                    continue;
                }

                var callee = functionPointerProto.Callee;
                var delegateType = TypeHelpers.UnboxIfPossible(newobjProto.ResultType);
                IMethod invokeMethod;
                if (TypeHelpers.TryGetDelegateInvokeMethod(delegateType, out invokeMethod))
                {
                    if (functionPointerProto.Lookup == MethodLookup.Static)
                    {
                        // We're dealing with a static method lookup. These are actually
                        // slightly tricky because CIL has zany rules that allow for either
                        // a 'this' pointer or a first parameter to be captured by the
                        // delegate itself.
                        bool hasThisParameter = !callee.IsStatic
                            || invokeMethod.Parameters.Count == callee.Parameters.Count - 1;

                        instruction.Instruction = Instruction.CreateNewDelegate(
                            newobjProto.ResultType,
                            callee,
                            hasThisParameter
                                ? ConvertThisArgument(
                                    boundObject,
                                    callee.IsStatic
                                        ? callee.Parameters[0].Type
                                        : TypeHelpers.BoxIfReferenceType(callee.ParentType),
                                    instruction)
                                : null,
                            MethodLookup.Static);
                    }
                    else
                    {
                        // We're dealing with a virtual method lookup. These are actually fairly
                        // easy to handle.
                        var thisArg = functionPointerProto.GetThisArgument(functionPointerInstruction.Instruction);
                        if (boundObject == thisArg)
                        {
                            instruction.Instruction = Instruction.CreateNewDelegate(
                                newobjProto.ResultType,
                                callee,
                                ConvertThisArgument(
                                    thisArg,
                                    TypeHelpers.BoxIfReferenceType(callee.ParentType),
                                    instruction),
                                MethodLookup.Virtual);
                        }
                    }
                }
            }
        }
        return builder.ToImmutable();
    }

    private static ValueTag ConvertThisArgument(
        ValueTag thisArgument,
        IType expectedThisType,
        NamedInstructionBuilder insertionPoint)
    {
        var thisType = insertionPoint.Graph.GetValueType(thisArgument);

        if (thisType != expectedThisType)
        {
            return insertionPoint.InsertBefore(
                Instruction.CreateReinterpretCast(
                    (PointerType)expectedThisType,
                    thisArgument));
        }
        else
        {
            return thisArgument;
        }
    }
}