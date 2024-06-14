using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using System.Collections.Generic;
using System.Linq;
using static Furesoft.Core.CodeDom.Compiler.Instructions.ArithmeticIntrinsics;

namespace Furesoft.Core.CodeDom.Compiler.Transforms;

/// <summary>
/// An optimization that reassociates operators to simplify computations.
/// </summary>
public sealed class ReassociateOperators : IntraproceduralOptimization
{
    /// <summary>
    /// An instance of the operator reassociation pass.
    /// </summary>
    public static readonly ReassociateOperators Instance
        = new();

    // A set of integer arithmetic operators `op` for which
    // `(a op b) op c == a op (b op c)` holds.
    private static readonly HashSet<string> assocIntArith =
        [
Operators.Add,
Operators.Multiply,
Operators.And,
Operators.Or,
Operators.Xor        ];

    // A set of `(op1, op2)` pairs for which `(a op1 b) op1 c == a op1 (b op2 c)` holds,
    // where both `op1` and `op2` are integer arithmetic operators.
    private static readonly Dictionary<string, string> intArithLeftToRight =
        new()
    {
        { Operators.Subtract, Operators.Add},
        { Operators.Divide, Operators.Multiply}
    };

    private ReassociateOperators()
    { }

    /// <inheritdoc/>
    public override FlowGraph Apply(FlowGraph graph)
    {
        var builder = graph.ToBuilder();

        foreach (var block in builder.BasicBlocks)
        {
            var instruction = block.NamedInstructions.FirstOrDefault();
            while (instruction != null)
            {
                ReassociateNonAssociative(instruction);
                instruction = instruction.NextInstructionOrNull;
            }
        }

        foreach (var block in builder.BasicBlocks)
        {
            var instruction = block.NamedInstructions.LastOrDefault();
            while (instruction != null)
            {
                ReassociateReduction(instruction);
                instruction = instruction.PreviousInstructionOrNull;
            }
        }

        return builder.ToImmutable();
    }

    private static NamedInstructionBuilder ConstantFold(
        ValueTag first,
        ValueTag second,
        InstructionPrototype prototype,
        NamedInstructionBuilder insertionPoint)
    {
        var graph = insertionPoint.Graph;
        if (IsConstant(first, insertionPoint.Graph.ToImmutable(), out Constant firstConstant)
            && IsConstant(second, insertionPoint.Graph.ToImmutable(), out Constant secondConstant))
        {
            var newConstant = ConstantPropagation.EvaluateDefault(
                prototype,
                new[] { firstConstant, secondConstant });
            if (newConstant != null)
            {
                return insertionPoint.InsertBefore(
                    Instruction.CreateConstant(newConstant, prototype.ResultType));
            }
        }
        return null;
    }

    private static bool IsAssociative(IntrinsicPrototype prototype)
    {
        if (ArithmeticIntrinsics.TryParseArithmeticIntrinsicName(prototype.Name, out string op, out bool isChecked)
            && !isChecked
            && assocIntArith.Contains(op))
        {
            return prototype.ParameterTypes.All(x => x.IsIntegerType());
        }
        else
        {
            return false;
        }
    }

    private static bool IsConstant(ValueTag tag, FlowGraph graph, out Constant constant)
    {
        if (graph.TryGetInstruction(tag, out NamedInstruction instruction)
            && instruction.Prototype is ConstantPrototype)
        {
            constant = ((ConstantPrototype)instruction.Prototype).Value; ;
            return true;
        }
        else
        {
            constant = null;
            return false;
        }
    }

    private static bool IsLeftToRightReassociable(
        IntrinsicPrototype prototype,
        out IntrinsicPrototype rightPrototype)
    {
        if (ArithmeticIntrinsics.TryParseArithmeticIntrinsicName(prototype.Name, out string op, out bool isChecked)
            && !isChecked
            && intArithLeftToRight.TryGetValue(op, out string rightOp)
            && prototype.ParameterTypes.All(x => x.IsIntegerType()))
        {
            rightPrototype = ArithmeticIntrinsics.CreatePrototype(
                rightOp,
                isChecked,
                prototype.ResultType,
                prototype.ParameterTypes);
            return true;
        }
        else
        {
            rightPrototype = null;
            return false;
        }
    }

    private static void MaterializeReduction(
        IReadOnlyList<ValueTag> args,
        InstructionPrototype prototype,
        NamedInstructionBuilder result)
    {
        if (args.Count == 1)
        {
            result.Instruction = Instruction.CreateCopy(result.ResultType, args[0]);
            return;
        }

        var accumulator = args[0];
        for (int i = 1; i < args.Count - 1; i++)
        {
            accumulator = result.InsertBefore(
                prototype.Instantiate(new[] { accumulator, args[i] }));
        }
        result.Instruction = prototype.Instantiate(new[] { accumulator, args[args.Count - 1] });
    }

    private static void ReassociateNonAssociative(NamedInstructionBuilder instruction)
    {
        // Look for instructions that compute an expression that looks
        // like so: `(a op1 b) op1 c` and replace it with `a op1 (b op2 c)`,
        // where `op2` is some (associative) operator such that the transformation
        // is semantics-preserving, e.g., op1 == (-) and op2 == (+) for integers.
        //
        // Here's why this transformation is useful: consider the following
        // expression: `(((x - 1) - 1) - 1) - 1`. It is clear that this can be
        // optimized to `x - 4`, but it's not super easy to see how: the reduction-
        // based reassociation doesn't apply to nonassociative operators. However,
        // by rewriting the expression as `x - (1 + 1 + 1 + 1)`, we can apply
        // reduction-based reassocation to the RHS.

        var proto = instruction.Prototype as IntrinsicPrototype;
        if (proto == null || !IsLeftToRightReassociable(proto, out IntrinsicPrototype rightPrototype))
        {
            return;
        }

        if (instruction.Graph.TryGetInstruction(instruction.Instruction.Arguments[0], out NamedInstructionBuilder left)
            && left.Prototype == proto)
        {
            var uses = instruction.Graph.GetAnalysisResult<ValueUses>();
            if (uses.GetUseCount(left) == 1)
            {
                var right = instruction.InsertBefore(
                    rightPrototype.Instantiate(
                        [
                            left.Instruction.Arguments[1],
                            instruction.Instruction.Arguments[1]
                        ]));
                instruction.Instruction = proto.Instantiate([left.Instruction.Arguments[0], right]);
                instruction.Graph.RemoveInstruction(left);
            }
        }
    }

    private static void ReassociateReduction(NamedInstructionBuilder instruction)
    {
        var proto = instruction.Prototype as IntrinsicPrototype;
        if (proto == null)
        {
            return;
        }

        if (IsAssociative(proto))
        {
            var uses = instruction.Graph.GetAnalysisResult<ValueUses>();
            var reductionArgs = new List<ValueTag>();
            var reductionOps = new HashSet<ValueTag>();
            ToReductionList(
                instruction.Instruction.Arguments,
                proto,
                reductionArgs,
                reductionOps,
                uses,
                instruction.Graph.ToImmutable());

            if (TrySimplifyReduction(ref reductionArgs, proto, instruction))
            {
                MaterializeReduction(reductionArgs, proto, instruction);
                instruction.Graph.RemoveInstructionDefinitions(reductionOps);
            }
        }
    }

    private static void ToReductionList(
        NamedInstruction instruction,
        InstructionPrototype prototype,
        List<ValueTag> reductionArgs,
        HashSet<ValueTag> reductionOps,
        ValueUses uses)
    {
        if (uses.GetUseCount(instruction) == 1
            && instruction.Prototype == prototype)
        {
            ToReductionList(
                instruction.Instruction.Arguments,
                prototype,
                reductionArgs,
                reductionOps,
                uses,
                instruction.Block.Graph);
            reductionOps.Add(instruction);
        }
        else
        {
            reductionArgs.Add(instruction);
        }
    }

    private static void ToReductionList(
        IEnumerable<ValueTag> arguments,
        InstructionPrototype prototype,
        List<ValueTag> reductionArgs,
        HashSet<ValueTag> reductionOps,
        ValueUses uses,
        FlowGraph graph)
    {
        foreach (var arg in arguments)
        {
            if (graph.TryGetInstruction(arg, out NamedInstruction insn))
            {
                ToReductionList(
                    insn,
                    prototype,
                    reductionArgs,
                    reductionOps,
                    uses);
            }
            else
            {
                reductionArgs.Add(arg);
            }
        }
    }

    private static bool TrySimplifyReduction(
        ref List<ValueTag> args,
        InstructionPrototype prototype,
        NamedInstructionBuilder insertionPoint)
    {
        bool changed = false;
        var newArgs = new List<ValueTag>();
        foreach (var operand in args)
        {
            if (newArgs.Count > 0)
            {
                var prevOperand = newArgs[newArgs.Count - 1];
                var folded = ConstantFold(prevOperand, operand, prototype, insertionPoint);
                if (folded == null)
                {
                    newArgs.Add(operand);
                }
                else
                {
                    newArgs[newArgs.Count - 1] = folded;
                    changed = true;
                }
            }
            else
            {
                newArgs.Add(operand);
            }
        }
        args = newArgs;
        return changed;
    }
}