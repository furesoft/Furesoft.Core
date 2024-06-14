using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpCode = Mono.Cecil.Cil.OpCode;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace Furesoft.Core.CodeDom.Backends.CLR.Analysis;

/// <summary>
/// A data structure that analyzes CIL instructions
/// and translates them to Flame IR.
/// </summary>
public sealed class ClrMethodBodyAnalyzer
{
    private static readonly IReadOnlyDictionary<OpCode, string> checkedSignedBinaryOperators =
                new Dictionary<OpCode, string>()
            {
        { OpCodes.Add_Ovf, ArithmeticIntrinsics.Operators.Add},
        { OpCodes.Sub_Ovf, ArithmeticIntrinsics.Operators.Subtract},
        { OpCodes.Mul_Ovf, ArithmeticIntrinsics.Operators.Multiply}
            };

    private static readonly IReadOnlyDictionary<OpCode, string> checkedUnsignedBinaryOperators =
                new Dictionary<OpCode, string>()
            {
        { OpCodes.Add_Ovf_Un, ArithmeticIntrinsics.Operators.Add},
        { OpCodes.Sub_Ovf_Un, ArithmeticIntrinsics.Operators.Subtract},
        { OpCodes.Mul_Ovf_Un, ArithmeticIntrinsics.Operators.Multiply}
            };

    private static readonly IReadOnlyDictionary<OpCode, string> signedBinaryOperators =
                new Dictionary<OpCode, string>()
            {
        { OpCodes.Add, ArithmeticIntrinsics.Operators.Add},
        { OpCodes.Sub, ArithmeticIntrinsics.Operators.Subtract},
        { OpCodes.Mul, ArithmeticIntrinsics.Operators.Multiply},
        { OpCodes.Div, ArithmeticIntrinsics.Operators.Divide},
        { OpCodes.Rem, ArithmeticIntrinsics.Operators.Remainder},
        { OpCodes.Cgt, ArithmeticIntrinsics.Operators.IsGreaterThan},
        { OpCodes.Ceq, ArithmeticIntrinsics.Operators.IsEqualTo},
        { OpCodes.Clt, ArithmeticIntrinsics.Operators.IsLessThan},
        { OpCodes.And, ArithmeticIntrinsics.Operators.And},
        { OpCodes.Or,  ArithmeticIntrinsics.Operators.Or},
        { OpCodes.Xor, ArithmeticIntrinsics.Operators.Xor},
        { OpCodes.Shl, ArithmeticIntrinsics.Operators.LeftShift},
        { OpCodes.Shr, ArithmeticIntrinsics.Operators.RightShift}
            };

    private static readonly IReadOnlyDictionary<OpCode, string> unsignedBinaryOperators =
                new Dictionary<OpCode, string>()
            {
        { OpCodes.Div_Un, ArithmeticIntrinsics.Operators.Divide},
        { OpCodes.Rem_Un, ArithmeticIntrinsics.Operators.Remainder},
        { OpCodes.Cgt_Un, ArithmeticIntrinsics.Operators.IsGreaterThan},
        { OpCodes.Clt_Un, ArithmeticIntrinsics.Operators.IsLessThan},
        { OpCodes.Shr_Un, ArithmeticIntrinsics.Operators.RightShift}
            };

    /// <summary>
    /// A mapping of checked conversion opcodes to an (unchecked equivalent
    /// opcode, source type is signed int) pair.
    /// </summary>
    private static Dictionary<OpCode, Tuple<OpCode, bool>> checkedConversions =
        new()
    {
        { OpCodes.Conv_Ovf_I, Tuple.Create(OpCodes.Conv_I, true) },
        { OpCodes.Conv_Ovf_I_Un, Tuple.Create(OpCodes.Conv_I, false) },
        { OpCodes.Conv_Ovf_U, Tuple.Create(OpCodes.Conv_U, true) },
        { OpCodes.Conv_Ovf_U_Un, Tuple.Create(OpCodes.Conv_U, false) },

        { OpCodes.Conv_Ovf_I1, Tuple.Create(OpCodes.Conv_I1, true) },
        { OpCodes.Conv_Ovf_I1_Un, Tuple.Create(OpCodes.Conv_I1, false) },
        { OpCodes.Conv_Ovf_I2, Tuple.Create(OpCodes.Conv_I2, true) },
        { OpCodes.Conv_Ovf_I2_Un, Tuple.Create(OpCodes.Conv_I2, false) },
        { OpCodes.Conv_Ovf_I4, Tuple.Create(OpCodes.Conv_I4, true) },
        { OpCodes.Conv_Ovf_I4_Un, Tuple.Create(OpCodes.Conv_I4, false) },
        { OpCodes.Conv_Ovf_I8, Tuple.Create(OpCodes.Conv_I8, true) },
        { OpCodes.Conv_Ovf_I8_Un, Tuple.Create(OpCodes.Conv_I8, false) },

        { OpCodes.Conv_Ovf_U1, Tuple.Create(OpCodes.Conv_U1, true) },
        { OpCodes.Conv_Ovf_U1_Un, Tuple.Create(OpCodes.Conv_U1, false) },
        { OpCodes.Conv_Ovf_U2, Tuple.Create(OpCodes.Conv_U2, true) },
        { OpCodes.Conv_Ovf_U2_Un, Tuple.Create(OpCodes.Conv_U2, false) },
        { OpCodes.Conv_Ovf_U4, Tuple.Create(OpCodes.Conv_U4, true) },
        { OpCodes.Conv_Ovf_U4_Un, Tuple.Create(OpCodes.Conv_U4, false) },
        { OpCodes.Conv_Ovf_U8, Tuple.Create(OpCodes.Conv_U8, true) },
        { OpCodes.Conv_Ovf_U8_Un, Tuple.Create(OpCodes.Conv_U8, false) },
    };

    // A mapping of conv.* opcodes to target types.
    private readonly IReadOnlyDictionary<OpCode, IType> convTypes;

    private HashSet<BasicBlockBuilder> analyzedBlocks;

    private Dictionary<Mono.Cecil.Cil.Instruction, BasicBlockBuilder> branchTargets;

    private EndFinallyFlow endfinallyFlow;

    private Dictionary<Mono.Cecil.Cil.Instruction, IReadOnlyList<CilExceptionHandler>> exceptionHandlerClauses;

    private Dictionary<Mono.Cecil.Cil.Instruction, IReadOnlyList<CilExceptionHandler>> exceptionHandlers;

    private ValueTag flowTokenVariable;

    private HashSet<ValueTag> freeTemporaries;

    // The flow graph being constructed by this method body
    // analyzer.
    private FlowGraphBuilder graph;

    private Dictionary<BasicBlockTag, int> leaveTokens;

    private List<NamedInstructionBuilder> localStackSlots;

    private List<NamedInstructionBuilder> parameterStackSlots;

    /// <summary>
    /// Creates a method body analyzer.
    /// </summary>
    /// <param name="returnParameter">
    /// The 'return' parameter of the method body.
    /// </param>
    /// <param name="thisParameter">
    /// The 'this' parameter of the method body.
    /// </param>
    /// <param name="parameters">
    /// The parameter list of the method body.
    /// </param>
    /// <param name="assembly">
    /// A reference to the assembly that defines the
    /// method body.
    /// </param>
    private ClrMethodBodyAnalyzer(
        Parameter returnParameter,
        Parameter thisParameter,
        IReadOnlyList<Parameter> parameters,
        ClrAssembly assembly)
    {
        this.ReturnParameter = returnParameter;
        this.ThisParameter = thisParameter;
        this.Parameters = parameters;
        this.Assembly = assembly;
        this.graph = new FlowGraphBuilder();

        // TODO: support type environments other than CorlibTypeEnvironment?
        PrototypeExceptionSpecs prototypeExceptionSpecs;
        var typeEnv = assembly.Resolver.TypeEnvironment;
        while (typeEnv is MutableTypeEnvironment)
        {
            typeEnv = ((MutableTypeEnvironment)typeEnv).InnerEnvironment;
        }
        if (typeEnv is CorlibTypeEnvironment)
        {
            prototypeExceptionSpecs = CilPrototypeExceptionSpecs.Create(
                ((CorlibTypeEnvironment)typeEnv).CorlibTypeResolver);
        }
        else
        {
            prototypeExceptionSpecs = RuleBasedPrototypeExceptionSpecs.Default;
        }

        this.graph.AddAnalysis(
            new ConstantAnalysis<PrototypeExceptionSpecs>(prototypeExceptionSpecs));
        this.graph.AddAnalysis(new EffectfulInstructionAnalysis());
        this.graph.AddAnalysis(NullabilityAnalysis.Instance);

        this.convTypes = new Dictionary<OpCode, IType>()
        {
            { OpCodes.Conv_I1, TypeEnvironment.Int8 },
            { OpCodes.Conv_I2, TypeEnvironment.Int16 },
            { OpCodes.Conv_I4, TypeEnvironment.Int32 },
            { OpCodes.Conv_I8, TypeEnvironment.Int64 },
            { OpCodes.Conv_U1, TypeEnvironment.UInt8 },
            { OpCodes.Conv_U2, TypeEnvironment.UInt16 },
            { OpCodes.Conv_U4, TypeEnvironment.UInt32 },
            { OpCodes.Conv_U8, TypeEnvironment.UInt64 },
            { OpCodes.Conv_R4, TypeEnvironment.Float32 },
            { OpCodes.Conv_R8, TypeEnvironment.Float64 },
            { OpCodes.Conv_I, TypeEnvironment.NaturalInt },
            { OpCodes.Conv_U, TypeEnvironment.NaturalUInt },
        };

        this.leaveTokens = [];
    }

    /// <summary>
    /// Gets a reference to the assembly that defines the
    /// method body.
    /// </summary>
    /// <returns>An assembly reference.</returns>
    public ClrAssembly Assembly { get; private set; }

    /// <summary>
    /// Gets the parameter list of the method body.
    /// </summary>
    /// <returns>The parameter list.</returns>
    public IReadOnlyList<Parameter> Parameters { get; private set; }

    /// <summary>
    /// Gets the 'return' parameter of the method body.
    /// </summary>
    /// <returns>The 'return' parameter.</returns>
    public Parameter ReturnParameter { get; private set; }

    /// <summary>
    /// Gets the 'this' parameter of the method body.
    /// </summary>
    /// <returns>The 'this' parameter.</returns>
    public Parameter ThisParameter { get; private set; }

    /// <summary>
    /// Gets the type environment used by this analyzer.
    /// </summary>
    public TypeEnvironment TypeEnvironment => Assembly.Resolver.TypeEnvironment;

    /// <summary>
    /// Analyzes a particular method body.
    /// </summary>
    /// <param name="cilMethodBody">
    /// The CIL method body to analyze.
    /// </param>
    /// <param name="method">
    /// The method that defines the method body.
    /// </param>
    /// <returns>
    /// A Flame IR method body.
    /// </returns>
    public static MethodBody Analyze(
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        ClrMethodDefinition method)
    {
        return Analyze(
            cilMethodBody,
            method.ReturnParameter,
            cilMethodBody.ThisParameter == null
                ? default(Parameter)
                : ClrMethodDefinition.WrapParameter(
                    cilMethodBody.ThisParameter,
                    method.ParentType.Assembly,
                    method),
            method.Parameters,
            method.ParentType.Assembly);
    }

    /// <summary>
    /// Analyzes a particular method body.
    /// </summary>
    /// <param name="cilMethodBody">
    /// The CIL method body to analyze.
    /// </param>
    /// <param name="returnParameter">
    /// The 'return' parameter of the method body.
    /// </param>
    /// <param name="thisParameter">
    /// The 'this' parameter of the method body.
    /// </param>
    /// <param name="parameters">
    /// The parameter list of the method body.
    /// </param>
    /// <param name="assembly">
    /// A reference to the assembly that defines the
    /// method body.
    /// </param>
    /// <returns>
    /// A Flame IR method body.
    /// </returns>
    public static MethodBody Analyze(
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        Parameter returnParameter,
        Parameter thisParameter,
        IReadOnlyList<Parameter> parameters,
        ClrAssembly assembly)
    {
        var analyzer = new ClrMethodBodyAnalyzer(
            returnParameter,
            thisParameter,
            parameters,
            assembly);

        // Analyze branch targets so we'll know which instructions
        // belong to which basic blocks.
        analyzer.AnalyzeBranchTargets(cilMethodBody);

        if (cilMethodBody.Instructions.Count > 0)
        {
            // Create an entry point that sets up stack slots
            // for the method body's parameters and locals.
            analyzer.CreateEntryPoint(cilMethodBody);

            // Create a mapping of Cecil exception handlers to
            // our exception handlers.
            var ehMapping = analyzer.CreateExceptionHandlers(cilMethodBody);

            // Analyze the flow graph by starting at the
            // entry point block.
            analyzer.AnalyzeBlock(
                cilMethodBody.Instructions[0],
                EmptyArray<IType>.Value,
                cilMethodBody);

            // Also analyze all exception handlers.
            foreach (var handler in cilMethodBody.ExceptionHandlers)
            {
                analyzer.AnalyzeExceptionHandler(handler, ehMapping[handler], cilMethodBody);
            }
        }

        // Finally, rewrite endfinally flow as switch flow (no pun intended).
        foreach (var block in analyzer.graph.BasicBlocks)
        {
            if (block.Flow is EndFinallyFlow)
            {
                block.Flow = ((EndFinallyFlow)block.Flow).ToSwitchFlow(
                    Instruction.CreateLoad(
                        analyzer.TypeEnvironment.Int32,
                        analyzer.flowTokenVariable));
            }
        }

        return new MethodBody(
            analyzer.ReturnParameter,
            analyzer.ThisParameter,
            analyzer.Parameters,
            analyzer.graph.ToImmutable());
    }

    private static IType GetAllocaElementType(Instruction alloca)
    {
        return ((PointerType)alloca.ResultType).ElementType;
    }

    /// <summary>
    /// Pops a method's formal parameters from the stack. This does
    /// not include the 'this' parameter.
    /// </summary>
    /// <param name="method">The method whose parameters are to be popped.</param>
    /// <param name="context">The CIL analysis context.</param>
    /// <returns>A list of arguments.</returns>
    private static IReadOnlyList<ValueTag> PopArguments(
        IMethod method,
        CilAnalysisContext context)
    {
        var args = new List<ValueTag>();

        // Pop arguments from the stack.
        for (int i = method.Parameters.Count - 1; i >= 0; i--)
        {
            args.Add(context.Pop(method.Parameters[i].Type));
        }

        args.Reverse();
        return args;
    }

    /// <summary>
    /// Takes a value, spills it to a temporary and
    /// returns the instruction that creates a pointer
    /// to the temporary.
    /// </summary>
    /// <param name="value">The value to spill.</param>
    /// <param name="context">
    /// The CIL analysis context that makes the request.
    /// </param>
    /// <returns>A pointer to the temporary.</returns>
    private static ValueTag SpillToTemporary(
        ValueTag value,
        CilAnalysisContext context)
    {
        var graph = context.Block.Graph;
        var type = graph.GetValueType(value);
        var alloca = graph.EntryPoint
            .InsertInstruction(
                0,
                Instruction.CreateAlloca(type));
        context.Emit(Instruction.CreateStore(type, alloca, value));
        return alloca;
    }

    private static void StoreValue(
                ValueTag pointer,
                ValueTag value,
                CilAnalysisContext context)
    {
        context.Emit(
            Instruction.CreateStore(
                context.GetValueType(value),
                pointer,
                value));
    }

    /// <summary>
    /// Tries to either recover an address that can be loaded
    /// to produce a given value or spills the value to a
    /// temporary and returns the temporary address.
    ///
    /// This method assumes that all reads to the returned
    /// address are appended to the given block; reading the
    /// address elsewhere may result in undefined behavior.
    /// </summary>
    /// <param name="value">
    /// The value to turn into a read-only address.
    /// </param>
    /// <param name="context">
    /// The CIL analysis context that makes the request.
    /// </param>
    /// <returns>
    /// A read-only address that points to a copy of <paramref name="value"/>.
    /// </returns>
    private static ValueTag ToReadOnlyAddress(
        ValueTag value,
        CilAnalysisContext context)
    {
        // We have two strategies to recover a read-only address:
        //
        //     1. If the value is produced by a load defined in this basic
        //        block and there has been no intervening effectful
        //        instruction, then we will set the read-only address
        //        to the load's argument.
        //
        //     2. Otherwise, we will copy the object into a temporary.
        //

        var graph = context.Block.Graph;
        if (graph.ContainsInstruction(value)
            && graph.GetValueParent(value).Tag == context.Block.Tag)
        {
            var baseInsn = graph.GetInstruction(value);
            if (baseInsn.Prototype is LoadPrototype)
            {
                var effectfulness = graph.GetAnalysisResult<EffectfulInstructions>();
                if (context.Block.NamedInstructions
                    .SkipWhile(insn => insn.Tag != value)
                    .All(insn =>
                        insn.Prototype is LoadPrototype
                        || !effectfulness.Instructions.Contains(insn)))
                {
                    return baseInsn.Instruction.Arguments[0];
                }
            }
        }
        return SpillToTemporary(value, context);
    }

    private BasicBlockTag AnalyzeBlock(
                Mono.Cecil.Cil.Instruction firstInstruction,
                IReadOnlyList<IType> argumentTypes,
                Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        // Mark the block as analyzed so we don't analyze it
        // ever again. Be sure to check that the types of
        // the arguments the block receives are the same if
        // the block is already analyzed.
        var block = branchTargets[firstInstruction];
        if (!analyzedBlocks.Add(block))
        {
            var parameterTypes = block.Parameters
                .Select(param => param.Type);
            bool sameParameters = parameterTypes
                .SequenceEqual(argumentTypes);
            if (sameParameters)
            {
                return block.Tag;
            }
            else
            {
                throw new InvalidProgramException(
                    $"Different paths to instruction '{firstInstruction.ToString()}' have " +
                    "incompatible stack contents. Stack contents on first path: [" +
                    $"{string.Join(", ", parameterTypes.Select(x => x.FullName))}]. Stack contents " +
                    $"on second path: [{string.Join(", ", argumentTypes.Select(x => x.FullName))}].");
            }
        }

        // Set up block parameters.
        block.Parameters = argumentTypes
            .Select((type, index) =>
                new BlockParameter(type, block.Tag.Name + "_stackarg_" + index))
            .ToImmutableList();

        var currentInstruction = firstInstruction;
        var context = new CilAnalysisContext(
            block,
            this,
            exceptionHandlers[firstInstruction],
            exceptionHandlerClauses[firstInstruction]);

        while (true)
        {
            // Analyze the current instruction.
            var nextInsn = currentInstruction.Next;
            AnalyzeInstruction(
                currentInstruction,
                ref nextInsn,
                cilMethodBody,
                context);

            if (nextInsn == null || branchTargets.ContainsKey(nextInsn))
            {
                // Current instruction is the last instruction of the block.
                // Handle fallthrough.
                if (!context.IsTerminated && branchTargets.ContainsKey(nextInsn))
                {
                    var args = context.EvaluationStack.Reverse().ToArray();
                    context.Terminate(
                        new JumpFlow(
                            CreateBranchTo(
                                nextInsn,
                                args,
                                cilMethodBody,
                                context)));
                }
                return block.Tag;
            }
            else
            {
                // Current instruction is not the last instruction of the
                // block. Proceed to the next instruction.
                currentInstruction = nextInsn;
            }
        }
    }

    /// <summary>
    /// Flags all branch targets in a CIL method body.
    /// </summary>
    /// <param name="cilMethodBody">The method body to analyze.</param>
    private void AnalyzeBranchTargets(
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        branchTargets = [];
        analyzedBlocks = [];

        // Analyze regular control flow.
        if (cilMethodBody.Instructions.Count > 0)
        {
            FlagBranchTarget(cilMethodBody.Instructions[0]);
            foreach (var instruction in cilMethodBody.Instructions)
            {
                AnalyzeBranchTargets(instruction);
            }
        }

        // Analyze exception control flow.
        foreach (var handler in cilMethodBody.ExceptionHandlers)
        {
            FlagBranchTarget(handler.TryStart);
            FlagBranchTarget(handler.TryEnd);
            FlagBranchTarget(handler.FilterStart);
            FlagBranchTarget(handler.HandlerStart);
            FlagBranchTarget(handler.HandlerEnd);
        }
    }

    /// <summary>
    /// Flags all instructions to which a particular instruction may branch.
    /// </summary>
    /// <param name="cilInstruction">The instruction to analyze.</param>
    private void AnalyzeBranchTargets(
        Mono.Cecil.Cil.Instruction cilInstruction)
    {
        if (cilInstruction.Operand is Mono.Cecil.Cil.Instruction)
        {
            FlagBranchTarget((Mono.Cecil.Cil.Instruction)cilInstruction.Operand);
            FlagBranchTarget(cilInstruction.Next);
        }
        else if (cilInstruction.Operand is Mono.Cecil.Cil.Instruction[])
        {
            foreach (var target in (Mono.Cecil.Cil.Instruction[])cilInstruction.Operand)
            {
                FlagBranchTarget(target);
            }
            FlagBranchTarget(cilInstruction.Next);
        }
        else if (cilInstruction.OpCode == OpCodes.Ret
            || cilInstruction.OpCode == OpCodes.Throw
            || cilInstruction.OpCode == OpCodes.Rethrow)
        {
            // Terminate the block defining the 'ret', 'throw' or 'rethrow'
            // by flagging the next block as a branch target.
            FlagBranchTarget(cilInstruction.Next);
        }
    }

    /// <summary>
    /// Analyzes a 'catch' exception handler's implementation.
    /// </summary>
    /// <param name="handler">The exception handler to analyze.</param>
    /// <param name="landingPadTag">
    /// The basic block tag of the landing pad to populate for the handler.
    /// </param>
    /// <param name="cilMethodBody">A CIL method body.</param>
    private void AnalyzeCatchHandler(
        Mono.Cecil.Cil.ExceptionHandler handler,
        BasicBlockTag landingPadTag,
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        // Determine the type of exception the handler is prepared
        // to handle.
        var catchType = Assembly.Resolve(handler.CatchType).MakePointerType(PointerKind.Box);

        // Analyze the handler's implementation.
        var handlerImpl = AnalyzeBlock(
            handler.HandlerStart,
            new[] { catchType },
            cilMethodBody);

        // Grab the landing pad. We're going to populate it in such
        // a way that it redirects control flow to the handler block
        // if and only if the thrown exception's type matches the catch
        // type. Otherwise, it'll transfer control to the next handler.
        var landingPad = graph.GetBasicBlock(landingPadTag);

        // Emit an intrinsic to extract the exception
        // from the landing pad's parameter.
        var exceptionValue = landingPad.AppendInstruction(
            Instruction.CreateGetCapturedExceptionIntrinsic(
                TypeEnvironment.Object.MakePointerType(PointerKind.Box),
                landingPad.Parameters[0].Type,
                landingPad.Parameters[0].Tag));

        // Test if the exception is an instance of the catch type.
        var typedException = landingPad.AppendInstruction(
            Instruction.CreateDynamicCast(catchType, exceptionValue));

        // Set the landing pad's flow.
        landingPad.Flow = SwitchFlow.CreateNullCheck(
            Instruction.CreateCopy(catchType, typedException),
            // Redirect control flow to the next exception handler
            // landing pad if the exception's type does not match
            // the caught type.
            new Branch(
                exceptionHandlers[handler.HandlerStart][0].LandingPad,
                new[] { landingPad.Parameters[0].Tag }),
            // Otherwise, direct control flow to the catch handler.
            new Branch(handlerImpl, new[] { typedException.Tag }));
    }

    /// <summary>
    /// Analyzes an exception handler's implementation.
    /// </summary>
    /// <param name="handler">The exception handler to analyze.</param>
    /// <param name="analyzedHandler">
    /// The analyzed version of the exception handler's structure.
    /// </param>
    /// <param name="cilMethodBody">A CIL method body.</param>
    private void AnalyzeExceptionHandler(
        Mono.Cecil.Cil.ExceptionHandler handler,
        CilExceptionHandler analyzedHandler,
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        switch (handler.HandlerType)
        {
            case Mono.Cecil.Cil.ExceptionHandlerType.Catch:
                AnalyzeCatchHandler(handler, analyzedHandler.LandingPad, cilMethodBody);
                return;

            case Mono.Cecil.Cil.ExceptionHandlerType.Finally:
                AnalyzeFinallyHandler(handler, (CilFinallyHandler)analyzedHandler, cilMethodBody);
                return;

            default:
                throw new NotImplementedException(
                    $"Unimplemented exception handler type '{handler.HandlerType}' " +
                    $"at '{handler.HandlerStart}'.");
        }
    }

    /// <summary>
    /// Analyzes a 'finally' exception handler's implementation.
    /// </summary>
    /// <param name="handler">The exception handler to analyze.</param>
    /// <param name="analyzedHandler">
    /// The structure of the analyzed 'finally' handler.
    /// </param>
    /// <param name="cilMethodBody">A CIL method body.</param>
    private void AnalyzeFinallyHandler(
        Mono.Cecil.Cil.ExceptionHandler handler,
        CilFinallyHandler analyzedHandler,
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        // Analyze the handler's implementation. Set the pending
        // endfinally flow.
        var oldEndfinally = endfinallyFlow;
        endfinallyFlow = analyzedHandler.Flow;
        var handlerImpl = AnalyzeBlock(
            handler.HandlerStart,
            EmptyArray<IType>.Value,
            cilMethodBody);
        endfinallyFlow = oldEndfinally;

        // Grab the landing pad.
        var landingPad = graph.GetBasicBlock(analyzedHandler.LandingPad);

        // Create a variable to put the exception in.
        // We need to use a variable because the landing
        // pad does not necessarily dominate the finally
        // block.
        var exceptionVar = graph.EntryPoint
            .AppendInstruction(
                Instruction.CreateAlloca(landingPad.Parameters[0].Type),
                "stored_exception_var");

        // Store the exception in a variable.
        landingPad.AppendInstruction(
            Instruction.CreateStore(
                landingPad.Parameters[0].Type,
                exceptionVar,
                landingPad.Parameters[0].Tag));

        // Set the flow token to zero.
        landingPad.AppendInstruction(
            Instruction.CreateStore(
                TypeEnvironment.Int32,
                flowTokenVariable,
                landingPad.AppendInstruction(
                    Instruction.CreateConstant(
                        new IntegerConstant(0),
                        TypeEnvironment.Int32))));

        // Create a thunk that loads the exception from the
        // variable and branches to the next landing pad.
        var thunk = graph.AddBasicBlock("finally_next_handler_thunk");
        thunk.Flow = new JumpFlow(
            new Branch(
                exceptionHandlers[handler.HandlerStart][0].LandingPad,
                new[]
                {
                    thunk.AppendInstruction(
                        Instruction.CreateLoad(
                            landingPad.Parameters[0].Type,
                            exceptionVar))
                        .Tag
                }));

        // Make the endfinally flow branch to the thunk by default.
        analyzedHandler.Flow.DefaultBranch = new Branch(thunk);

        // Set the landing pad's flow.
        landingPad.Flow = new JumpFlow(handlerImpl);

        // Set the leave pad's flow.
        graph.GetBasicBlock(analyzedHandler.LeavePad).Flow = new JumpFlow(handlerImpl);
    }

    private void AnalyzeInstruction(
        Mono.Cecil.Cil.Instruction instruction,
        ref Mono.Cecil.Cil.Instruction nextInstruction,
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        CilAnalysisContext context)
    {
        string opName;
        IEnumerable<Mono.Cecil.Cil.Instruction> simplifiedSeq;
        if (signedBinaryOperators.TryGetValue(instruction.OpCode, out opName))
        {
            EmitSignedArithmeticBinary(opName, false, context);
        }
        else if (unsignedBinaryOperators.TryGetValue(instruction.OpCode, out opName))
        {
            EmitUnsignedArithmeticBinary(opName, false, context);
        }
        else if (checkedSignedBinaryOperators.TryGetValue(instruction.OpCode, out opName))
        {
            EmitSignedArithmeticBinary(opName, true, context);
        }
        else if (checkedUnsignedBinaryOperators.TryGetValue(instruction.OpCode, out opName))
        {
            EmitUnsignedArithmeticBinary(opName, true, context);
        }
        else if (instruction.OpCode == OpCodes.Not)
        {
            var operand = context.Pop();
            var operandType = context.GetValueType(operand);
            context.Push(
                ArithmeticIntrinsics.CreatePrototype(
ArithmeticIntrinsics.Operators.Not,
                    false,
                    operandType,
                    operandType)
                .Instantiate(operand));
        }
        else if (instruction.OpCode == OpCodes.Neg)
        {
            var operand = context.Pop();
            var operandType = context.GetValueType(operand);
            context.Push(
                ArithmeticIntrinsics.CreatePrototype(
ArithmeticIntrinsics.Operators.Subtract,
                    false,
                    operandType,
                    operandType,
                    operandType)
                .Instantiate(
                    context.Emit(Instruction.CreateDefaultConstant(operandType)),
                    operand));
        }
        else if (instruction.OpCode == OpCodes.Ldc_I4)
        {
            context.Push(
                Instruction.CreateConstant(
                    new IntegerConstant((int)instruction.Operand),
                    Assembly.Resolver.TypeEnvironment.Int32));
        }
        else if (instruction.OpCode == OpCodes.Ldc_I8)
        {
            context.Push(
                Instruction.CreateConstant(
                    new IntegerConstant((long)instruction.Operand),
                    Assembly.Resolver.TypeEnvironment.Int64));
        }
        else if (instruction.OpCode == OpCodes.Ldc_R4)
        {
            context.Push(
                Instruction.CreateConstant(
                    new Float32Constant((float)instruction.Operand),
                    Assembly.Resolver.TypeEnvironment.Float32));
        }
        else if (instruction.OpCode == OpCodes.Ldc_R8)
        {
            context.Push(
                Instruction.CreateConstant(
                    new Float64Constant((double)instruction.Operand),
                    Assembly.Resolver.TypeEnvironment.Float64));
        }
        else if (instruction.OpCode == OpCodes.Ldnull)
        {
            context.Push(
                Instruction.CreateConstant(
                    NullConstant.Instance,
                    TypeEnvironment.Object.MakePointerType(PointerKind.Box)));
        }
        else if (instruction.OpCode == OpCodes.Ldstr)
        {
            context.Push(
                Instruction.CreateConstant(
                    new StringConstant((string)instruction.Operand),
                    TypeHelpers.BoxIfReferenceType(Assembly.Resolver.TypeEnvironment.String)));
        }
        else if (instruction.OpCode == OpCodes.Ldtoken)
        {
            if (instruction.Operand is Mono.Cecil.TypeReference)
            {
                context.Push(
                    Instruction.CreateConstant(
                        new TypeTokenConstant(
                            Assembly.Resolve(
                                (Mono.Cecil.TypeReference)instruction.Operand)),
                        TypeEnvironment.TypeToken));
            }
            else if (instruction.Operand is Mono.Cecil.FieldReference)
            {
                context.Push(
                    Instruction.CreateConstant(
                        new FieldTokenConstant(
                            Assembly.Resolve(
                                (Mono.Cecil.FieldReference)instruction.Operand)),
                        TypeEnvironment.FieldToken));
            }
            else if (instruction.Operand is Mono.Cecil.MethodReference)
            {
                context.Push(
                    Instruction.CreateConstant(
                        new MethodTokenConstant(
                            Assembly.Resolve(
                                (Mono.Cecil.MethodReference)instruction.Operand)),
                        TypeEnvironment.MethodToken));
            }
            else
            {
                throw new NotImplementedException(
                    $"Instruction '{instruction}' should have a type, field or method reference operand, but doesn't.");
            }
        }
        else if (instruction.OpCode == OpCodes.Box)
        {
            var valType = Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand);
            var val = context.Pop(valType);
            context.Push(
                Instruction.CreateBox(valType, val));
        }
        else if (instruction.OpCode == OpCodes.Unbox_Any
            || instruction.OpCode == OpCodes.Castclass)
        {
            var val = context.Pop();
            var targetType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            var valType = context.GetValueType(val);
            context.Push(
                Instruction.CreateUnboxAnyIntrinsic(targetType, valType, val));
        }
        else if (instruction.OpCode == OpCodes.Ldarga)
        {
            context.Push(GetParameterSlot((Mono.Cecil.ParameterReference)instruction.Operand, cilMethodBody));
        }
        else if (instruction.OpCode == OpCodes.Ldarg)
        {
            var alloca = GetParameterSlot((Mono.Cecil.ParameterReference)instruction.Operand, cilMethodBody);
            LoadValue(alloca.Tag, context);
        }
        else if (instruction.OpCode == OpCodes.Starg)
        {
            var alloca = GetParameterSlot((Mono.Cecil.ParameterReference)instruction.Operand, cilMethodBody);
            StoreValue(
                alloca.Tag,
                context.Pop(GetAllocaElementType(alloca.Instruction)),
                context);
        }
        else if (instruction.OpCode == OpCodes.Ldloca)
        {
            context.Push(
                localStackSlots[((Mono.Cecil.Cil.VariableReference)instruction.Operand).Index].Tag);
        }
        else if (instruction.OpCode == OpCodes.Ldloc)
        {
            var alloca = localStackSlots[((Mono.Cecil.Cil.VariableReference)instruction.Operand).Index];
            LoadValue(alloca.Tag, context);
        }
        else if (instruction.OpCode == OpCodes.Stloc)
        {
            var alloca = localStackSlots[((Mono.Cecil.Cil.VariableReference)instruction.Operand).Index];
            StoreValue(
                alloca.Tag,
                context.Pop(GetAllocaElementType(alloca.Instruction)),
                context);
        }
        else if (instruction.OpCode == OpCodes.Ldfld)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            var basePointer = context.Pop();
            var basePointerType = context.GetValueType(basePointer) as PointerType;
            if (basePointerType == null)
            {
                // 'ldfld' instructions may also load a field from a value type
                // directly. If that is the case, we will find or create a read-only
                // address for the base pointer.
                basePointer = ToReadOnlyAddress(basePointer, context);
                basePointerType = context.GetValueType(basePointer) as PointerType;
            }

            if (basePointerType.ElementType != field.ParentType)
            {
                // Reinterpret the base pointer if necessary.
                basePointer = context.Emit(
                    Instruction.CreateReinterpretCast(
                        field.ParentType.MakePointerType(basePointerType.Kind),
                        basePointer));
            }
            context.Push(
                Instruction.CreateLoad(
                    field.FieldType,
                    context.Emit(
                        Instruction.CreateGetFieldPointer(field, basePointer))));
        }
        else if (instruction.OpCode == OpCodes.Ldflda)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            var basePointer = context.Pop();
            var basePointerType = context.GetValueType(basePointer) as PointerType;
            if (basePointerType == null)
            {
                throw new InvalidProgramException(
                    "'ldflda' instruction expects a base pointer that points to an " +
                    $"element of type '{field.ParentType}'. Instead, a base pointer of " +
                    $"type '{context.GetValueType(basePointer)}' was provided.");
            }

            if (basePointerType.ElementType != field.ParentType)
            {
                // Reinterpret the base pointer if necessary.
                basePointer = context.Emit(
                    Instruction.CreateReinterpretCast(
                        field.ParentType.MakePointerType(basePointerType.Kind),
                        basePointer));
            }
            context.Push(
                Instruction.CreateGetFieldPointer(field, basePointer));
        }
        else if (instruction.OpCode == OpCodes.Stfld)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            var value = context.Pop(field.FieldType);
            var basePointer = context.Pop();
            var basePointerType = context.GetValueType(basePointer) as PointerType;
            if (basePointerType == null)
            {
                throw new InvalidProgramException(
                    "'stfld' instruction expects a base pointer that points to an " +
                    $"element of type '{field.ParentType}'. Instead, a base pointer of " +
                    $"type '{context.GetValueType(basePointer)}' was provided.");
            }

            if (basePointerType.ElementType != field.ParentType)
            {
                // Reinterpret the base pointer if necessary.
                basePointer = context.Emit(
                    Instruction.CreateReinterpretCast(
                        field.ParentType.MakePointerType(basePointerType.Kind),
                        basePointer));
            }
            context.Emit(
                Instruction.CreateStore(
                    field.FieldType,
                    context.Emit(
                        Instruction.CreateGetFieldPointer(field, basePointer)),
                    value));
        }
        else if (instruction.OpCode == OpCodes.Ldsfld)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            context.Push(
                Instruction.CreateLoad(
                    field.FieldType,
                    context.Emit(
                        Instruction.CreateGetStaticFieldPointer(field))));
        }
        else if (instruction.OpCode == OpCodes.Ldsflda)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            context.Push(
                Instruction.CreateGetStaticFieldPointer(field));
        }
        else if (instruction.OpCode == OpCodes.Stsfld)
        {
            var field = Assembly.Resolve((Mono.Cecil.FieldReference)instruction.Operand);
            var value = context.Pop(field.FieldType);
            context.Emit(
                Instruction.CreateStore(
                    field.FieldType,
                    context.Emit(
                        Instruction.CreateGetStaticFieldPointer(field)),
                    value));
        }
        else if (instruction.OpCode == OpCodes.Ldobj)
        {
            var elementType = Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand);
            var pointer = PopPointerToType(elementType, context);
            context.Push(
                Instruction.CreateLoad(
                    elementType,
                    pointer));
        }
        else if (instruction.OpCode == OpCodes.Ldind_Ref)
        {
            var pointer = context.Pop();
            var pointerType = graph.GetValueType(pointer) as PointerType;
            if (pointerType == null)
            {
                throw new InvalidProgramException(
                    "`ldind.ref` instructions can only load pointer values; " +
                    $"argument of type '{graph.GetValueType(pointer)}' isn't one.");
            }
            context.Push(
                Instruction.CreateLoad(
                    pointerType.ElementType,
                    pointer));
        }
        else if (instruction.OpCode == OpCodes.Stobj)
        {
            var elementType = Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand);
            var value = context.Pop(elementType);
            var pointer = PopPointerToType(elementType, context);
            context.Emit(
                Instruction.CreateStore(
                    elementType,
                    pointer,
                    value));
        }
        else if (instruction.OpCode == OpCodes.Volatile)
        {
            // Register the 'volatile' prefix with the context.
            context.RequestVolatile();
        }
        else if (instruction.OpCode == OpCodes.Unaligned)
        {
            context.RequestAlignment(new Alignment((byte)instruction.Operand));
        }
        else if (instruction.OpCode == OpCodes.Ldelema)
        {
            var elementType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            var indexVal = context.Pop();
            var arrayVal = context.Pop();
            var arrayValType = context.GetValueType(arrayVal);
            context.Push(
                Instruction.CreateGetElementPointerIntrinsic(
                    elementType,
                    arrayValType,
                    new[] { context.GetValueType(indexVal) },
                    arrayVal,
                    new[] { indexVal }));
        }
        else if (instruction.OpCode == OpCodes.Ldelem_Any)
        {
            var elementType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            var indexVal = context.Pop();
            var arrayVal = context.Pop();
            var arrayValType = context.GetValueType(arrayVal);
            context.Push(
                Instruction.CreateLoadElementIntrinsic(
                    elementType,
                    arrayValType,
                    new[] { context.GetValueType(indexVal) },
                    arrayVal,
                    new[] { indexVal }));
        }
        else if (instruction.OpCode == OpCodes.Stelem_Any)
        {
            var elementType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            var elemVal = context.Pop(elementType);
            var indexVal = context.Pop();
            var arrayVal = context.Pop();
            var arrayValType = context.GetValueType(arrayVal);
            context.Emit(
                Instruction.CreateStoreElementIntrinsic(
                    elementType,
                    arrayValType,
                    new[] { context.GetValueType(indexVal) },
                    elemVal,
                    arrayVal,
                    new[] { indexVal }));
        }
        else if (instruction.OpCode == OpCodes.Ldelem_Ref)
        {
            var indexVal = context.Pop();
            var arrayVal = context.Pop();
            var arrayValType = context.GetValueType(arrayVal);
            IType elementType;
            if (!ClrArrayType.TryGetArrayElementType(
                TypeHelpers.UnboxIfPossible(arrayValType),
                out elementType))
            {
                throw new InvalidOperationException(
                    "'ldelem.ref' opcodes can only load array elements but the argument " +
                    $"of type '{arrayValType.FullName}' is not one.");
            }
            context.Push(
                Instruction.CreateLoadElementIntrinsic(
                    elementType,
                    arrayValType,
                    new[] { context.GetValueType(indexVal) },
                    arrayVal,
                    new[] { indexVal }));
        }
        else if (instruction.OpCode == OpCodes.Ldlen)
        {
            var arrayVal = context.Pop();
            context.Push(
                Instruction.CreateGetLengthIntrinsic(
                    TypeEnvironment.NaturalUInt,
                    context.GetValueType(arrayVal),
                    arrayVal));
        }
        else if (instruction.OpCode == OpCodes.Newarr)
        {
            var elementType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            IType arrayType;
            if (!TypeEnvironment.TryMakeArrayType(elementType, 1, out arrayType))
            {
                throw new NotSupportedException(
                    "Cannot analyze a 'newarr' opcode because the type " +
                    "environment does not support array types.");
            }
            var lengthVal = context.Pop();
            context.Push(
                Instruction.CreateNewArrayIntrinsic(
                    TypeHelpers.BoxIfReferenceType(arrayType),
                    context.GetValueType(lengthVal),
                    lengthVal));
        }
        else if (instruction.OpCode == OpCodes.Localloc)
        {
            var size = context.Pop();
            context.Push(Instruction.CreateAllocaArray(TypeEnvironment.UInt8, size));
        }
        else if (instruction.OpCode == OpCodes.Sizeof)
        {
            var measureType = Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand);
            context.Push(
                Instruction.CreateSizeOf(
                    measureType,
                    TypeEnvironment.UInt32));
        }
        else if (convTypes.ContainsKey(instruction.OpCode))
        {
            // Conversion opcodes are usually fairly straightforward.
            EmitConvertAndExtend(convTypes[instruction.OpCode], false, context);
        }
        else if (checkedConversions.ContainsKey(instruction.OpCode))
        {
            // Analyze checked conversions by analogy with their unchecked
            // counterparts.
            var pair = checkedConversions[instruction.OpCode];
            var uncheckedOp = pair.Item1;
            var sourceIsSigned = pair.Item2;
            if (sourceIsSigned)
            {
                EmitConvertToSigned(context);
            }
            else
            {
                EmitConvertToUnsigned(context);
            }
            EmitConvertAndExtend(convTypes[uncheckedOp], true, context);
        }
        else if (instruction.OpCode == OpCodes.Isinst)
        {
            var operandType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));

            var pointerOperandType = operandType as PointerType;
            if (pointerOperandType == null)
            {
                throw new InvalidProgramException(
                    "The type argument to an 'isinst' instruction must be a " +
                    $" pointer or reference; '{operandType.FullName}' isn't one.");
            }

            var arg = context.Pop();
            var argType = context.GetValueType(arg) as PointerType;
            if (argType == null)
            {
                throw new InvalidProgramException(
                    "The parameter to an 'isinst' instruction must be a " +
                    $" pointer; '{context.GetValueType(arg).FullName}' isn't one.");
            }

            context.Push(Instruction.CreateDynamicCast(pointerOperandType, arg));
        }
        else if (instruction.OpCode == OpCodes.Initobj)
        {
            var elementType = TypeHelpers.BoxIfReferenceType(
                Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand));
            var pointer = context.Pop();
            var pointerType = context.GetValueType(pointer) as PointerType;
            if (pointerType == null || pointerType.Kind == PointerKind.Box)
            {
                // Check that the pointer is actually a (reference or transient)
                // pointer.
                throw new InvalidProgramException(
                    "The parameter to an 'initobj' instruction must be a reference " +
                    $"or transient pointer; '{context.GetValueType(pointer).FullName}' is neither.");
            }

            if (pointerType.ElementType != elementType)
            {
                // Insert a reinterpret cast if necessary.
                pointerType = elementType.MakePointerType(pointerType.Kind);
                pointer = context.Emit(Instruction.CreateReinterpretCast(pointerType, pointer));
            }

            // Assign the 'default' constant to the pointer.
            context.Emit(
                Instruction.CreateStore(
                    elementType,
                    pointer,
                    context.Emit(
                        Instruction.CreateDefaultConstant(elementType))));
        }
        else if (instruction.OpCode == OpCodes.Ret)
        {
            var value = context.Pop(ReturnParameter.Type);
            context.Terminate(
                new ReturnFlow(
                    Instruction.CreateCopy(
                        graph.GetValueType(value),
                        value)));
        }
        else if (instruction.OpCode == OpCodes.Throw)
        {
            var value = context.Pop();
            context.Emit(
                Instruction.CreateThrowIntrinsic(graph.GetValueType(value), value));
            context.Terminate(UnreachableFlow.Instance);
        }
        else if (instruction.OpCode == OpCodes.Rethrow)
        {
            var handler = context.ExceptionHandlerClauses.OfType<CilCatchHandler>().First();
            var rethrownValue = handler.GetCapturedException(graph.ToImmutable());
            context.Emit(
                Instruction.CreateRethrowIntrinsic(
                    graph.GetValueType(rethrownValue),
                    rethrownValue));
            context.Terminate(UnreachableFlow.Instance);
        }
        else if (instruction.OpCode == OpCodes.Pop)
        {
            context.Pop();
        }
        else if (instruction.OpCode == OpCodes.Dup)
        {
            context.Push(context.Peek());
        }
        else if (instruction.OpCode == OpCodes.Nop)
        {
            // Do nothing I guess.
        }
        else if (instruction.OpCode == OpCodes.Br)
        {
            var args = context.EvaluationStack.Reverse().ToArray();
            context.Terminate(
                new JumpFlow(
                    CreateBranchTo(
                        (Mono.Cecil.Cil.Instruction)instruction.Operand,
                        args,
                        cilMethodBody,
                        context)));
        }
        else if (instruction.OpCode == OpCodes.Leave)
        {
            // From MSDN:
            //
            //   The leave instruction is similar to the br instruction,
            //   but it can be used to exit a try, filter, or catch block
            //   whereas the ordinary branch instructions can only be used
            //   in such a block to transfer control within it. The leave
            //   instruction empties the evaluation stack and ensures that
            //   the appropriate surrounding finally blocks are executed.
            //
            // Ensuring that the appropriate surrounding finally blocks are
            // executed is fairly hard to do using standard control flow.
            // What we'll do is label every 'leave' target with a unique
            // integer. We then update the endfinally flow of every
            // surrounding finally block's endfinally flow with a branch that
            // either runs the 'leave' target or the next finally block.
            var target = (Mono.Cecil.Cil.Instruction)instruction.Operand;

            // Figure out what the target's finally handlers are.
            var targetHandlers = new HashSet<CilFinallyHandler>(
                exceptionHandlers[target].OfType<CilFinallyHandler>());

            // By "surrounding finally handlers" we mean all finally
            // handlers for the source that are not finally handlers for
            // the target. So the first handler we *won't* run is the first
            // handler the target and source have in common.
            var surroundingHandlers = context.ExceptionHandlers
                .OfType<CilFinallyHandler>()
                .TakeWhile(handler => !targetHandlers.Contains(handler))
                .ToArray();

            if (surroundingHandlers.Length > 0)
            {
                // Acquire a token for the leave target.
                var targetBlock = branchTargets[target];
                int token;
                if (!leaveTokens.TryGetValue(targetBlock, out token))
                {
                    // Create a new token if we don't have one already.
                    // The zero value is reserved for when an exception is
                    // thrown, so don't use that one.
                    token = leaveTokens.Count + 1;
                    leaveTokens[targetBlock] = token;
                }

                // Update all surrounding finally handlers up to and including the
                // penultimate surrounding finally handler to direct control to
                // the next finally handler when the tag is encountered.
                for (int i = 0; i < surroundingHandlers.Length - 1; i++)
                {
                    surroundingHandlers[i].Flow.Destinations[token] =
                        new Branch(surroundingHandlers[i + 1].LeavePad);
                }

                // Update the last surrounding finally handler to direct control
                // to the 'leave' target when the token is encountered.
                surroundingHandlers[surroundingHandlers.Length - 1].Flow.Destinations[token] =
                    CreateBranchTo(target, EmptyArray<ValueTag>.Value, cilMethodBody, context);

                // Set the token variable.
                context.Emit(
                    Instruction.CreateStore(
                        TypeEnvironment.Int32,
                        flowTokenVariable,
                        context.Emit(
                            Instruction.CreateConstant(
                                new IntegerConstant(token),
                                TypeEnvironment.Int32))));

                // Jump to the first finally handler.
                context.Terminate(new JumpFlow(surroundingHandlers[0].LeavePad));
            }
            else
            {
                // If there is no finally handler then we can just jump to
                // the leave target.
                context.Terminate(
                    new JumpFlow(
                        CreateBranchTo(
                            target,
                            EmptyArray<ValueTag>.Value,
                            cilMethodBody,
                            context)));
            }
        }
        else if (instruction.OpCode == OpCodes.Endfinally)
        {
            if (endfinallyFlow == null)
            {
                throw new InvalidProgramException(
                    $"illegal instruction '{instruction}' appears outside of a 'finally' clause.");
            }
            context.Terminate(endfinallyFlow);
        }
        else if (instruction.OpCode == OpCodes.Brtrue)
        {
            EmitConditionalBranch(
                context.Pop(),
                (Mono.Cecil.Cil.Instruction)instruction.Operand,
                nextInstruction,
                cilMethodBody,
                context);
        }
        else if (instruction.OpCode == OpCodes.Brfalse)
        {
            EmitConditionalBranch(
                context.Pop(),
                nextInstruction,
                (Mono.Cecil.Cil.Instruction)instruction.Operand,
                cilMethodBody,
                context);
        }
        else if (instruction.OpCode == OpCodes.Switch)
        {
            EmitJumpTable(
                context.Pop(),
                (Mono.Cecil.Cil.Instruction[])instruction.Operand,
                nextInstruction,
                cilMethodBody,
                context);
        }
        else if (instruction.OpCode == OpCodes.Call
            || instruction.OpCode == OpCodes.Callvirt)
        {
            var methodRef = (Mono.Cecil.MethodReference)instruction.Operand;
            var method = Assembly.Resolve(methodRef);
            var args = PopArguments(method, context);

            // Pop the 'this' pointer from the stack.
            if (!method.IsStatic)
            {
                var thisValType = context.GetValueType(context.Peek());
                if (thisValType is PointerType)
                {
                    var thisArg = context.Pop(
                        method.ParentType.MakePointerType(((PointerType)thisValType).Kind));
                    args = new[] { thisArg }.Concat(args).ToArray();
                }
                else
                {
                    throw new NotImplementedException("Unimplemented feature: value type as 'this' argument.");
                }
            }

            context.Push(
                Instruction.CreateCall(
                    method,
                    instruction.OpCode == OpCodes.Callvirt
                        ? MethodLookup.Virtual
                        : MethodLookup.Static,
                    args));
        }
        else if (instruction.OpCode == OpCodes.Constrained)
        {
            // The 'constrained.' prefix opcode is always followed by a 'callvirt'
            // instruction. Grab that 'callvirt' instruction.
            var callInsn = nextInstruction;
            nextInstruction = callInsn.Next;

            if (callInsn.OpCode != OpCodes.Callvirt)
            {
                throw new InvalidProgramException(
                    $"Instruction '{instruction}' must be trailed by a 'callvirt' " +
                    $"instruction but was actually trailed by '{callInsn}'.");
            }

            var methodRef = (Mono.Cecil.MethodReference)callInsn.Operand;
            var method = Assembly.Resolve(methodRef);
            var args = PopArguments(method, context);

            var thisType = Assembly.Resolve((Mono.Cecil.TypeReference)instruction.Operand);
            var thisPtrArg = context.Pop(
                TypeHelpers.BoxIfReferenceType(thisType)
                .MakePointerType(PointerKind.Reference));

            context.Push(
                Instruction.CreateConstrainedCall(
                    method,
                    thisPtrArg,
                    args));
        }
        else if (instruction.OpCode == OpCodes.Newobj)
        {
            var methodRef = (Mono.Cecil.MethodReference)instruction.Operand;
            var method = Assembly.Resolve(methodRef);
            var args = PopArguments(method, context);

            if (method.ParentType.IsReferenceType())
            {
                // Reference types are created by actual 'new_object' instructions.
                context.Push(Instruction.CreateNewObject(method, args));
            }
            else
            {
                // Value types are created by allocating a temporary, initializing it and
                // loading its value.
                var alloca = GetTemporaryAlloca(method.ParentType, context.Block.Graph);
                context.Emit(
                    Instruction.CreateCall(
                        method,
                        MethodLookup.Static,
                        new[] { alloca }.Concat(args).ToArray()));
                context.Push(Instruction.CreateLoad(method.ParentType, alloca));
            }
        }
        else if (instruction.OpCode == OpCodes.Ldftn
            || instruction.OpCode == OpCodes.Ldvirtftn)
        {
            var methodRef = (Mono.Cecil.MethodReference)instruction.Operand;
            var method = Assembly.Resolve(methodRef);
            bool isVirtual = instruction.OpCode == OpCodes.Ldvirtftn;
            context.Push(
                Instruction.CreateNewDelegate(
                    TypeEnvironment.NaturalInt,
                    method,
                    isVirtual ? context.Pop(method.ParentType) : null,
                    isVirtual ? MethodLookup.Virtual : MethodLookup.Static));
        }
        else if (ClrInstructionSimplifier.TrySimplify(instruction, cilMethodBody, out simplifiedSeq))
        {
            // Process all instructions in the simplified instruction sequence.
            var simplifiedArray = simplifiedSeq.ToArray();
            for (int i = 0; i < simplifiedArray.Length; i++)
            {
                var instr = simplifiedArray[i];
                var expectedNextInstr = i == simplifiedArray.Length - 1 ? nextInstruction : simplifiedArray[i + 1];
                var nextInstr = expectedNextInstr;
                AnalyzeInstruction(instr, ref nextInstr, cilMethodBody, context);

                // Skip instructions until we get to the expected next instruction.
                while (nextInstr != expectedNextInstr && i < simplifiedArray.Length)
                {
                    i++;
                }
            }
        }
        else
        {
            throw new NotImplementedException($"Unimplemented opcode: {instruction}");
        }
    }

    private Branch CreateBranchTo(
        Mono.Cecil.Cil.Instruction firstInstruction,
        IReadOnlyList<ValueTag> arguments,
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        BasicBlockBuilder builder)
    {
        var uniformArgs = MakeArgumentsUniform(arguments, builder);
        return new Branch(
            AnalyzeBlock(
                firstInstruction,
                uniformArgs.Select(graph.GetValueType).ToArray(),
                cilMethodBody),
            uniformArgs);
    }

    private Branch CreateBranchTo(
        Mono.Cecil.Cil.Instruction firstInstruction,
        IReadOnlyList<ValueTag> arguments,
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        CilAnalysisContext context)
    {
        return CreateBranchTo(firstInstruction, arguments, cilMethodBody, context.Block);
    }

    /// <summary>
    /// Creates an entry point block that sets up stack
    /// slots for the method body's parameters and locals.
    /// </summary>
    /// <param name="cilMethodBody">
    /// The method body to create stack slots for.
    /// </param>
    private void CreateEntryPoint(Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        // Compose an extended parameter list by prepending the 'this'
        // parameter to the regular parameter list, provided that there
        // is a 'this' parameter.
        var extParameters = cilMethodBody.Method.HasThis
            ? new[] { ThisParameter }.Concat(Parameters).ToArray()
            : Parameters;

        // Grab the entry point block.
        var entryPoint = graph.EntryPoint;

        // Create a block parameter in the entry point for each
        // actual parameter in the method.
        entryPoint.Parameters = extParameters
            .Select((param, index) =>
                new BlockParameter(param.Type, param.Name.ToString()))
            .ToImmutableList();

        this.freeTemporaries = [];

        // For each parameter, allocate a stack slot and store the
        // value of the parameter in the stack slot.
        this.parameterStackSlots = [];
        for (int i = 0; i < extParameters.Count; i++)
        {
            var param = extParameters[i];

            var alloca = entryPoint.AppendInstruction(
                Instruction.CreateAlloca(param.Type),
                new ValueTag(param.Name.ToString() + "_slot"));

            entryPoint.AppendInstruction(
                Instruction.CreateStore(
                    param.Type,
                    alloca.Tag,
                    entryPoint.Parameters[i].Tag),
                new ValueTag(param.Name.ToString()));

            this.parameterStackSlots.Add(alloca);
        }

        // For each local, allocate an empty stack slot.
        this.localStackSlots = [];
        foreach (var local in cilMethodBody.Variables)
        {
            var varType = local.VariableType;
            bool isPinned = varType is Mono.Cecil.PinnedType;
            if (isPinned)
            {
                varType = ((Mono.Cecil.PinnedType)varType).ElementType;
            }

            var elemType = TypeHelpers.BoxIfReferenceType(Assembly.Resolve(varType));
            var name = "local_" + local.Index + "_slot";

            var alloca = entryPoint.AppendInstruction(
                isPinned ? Instruction.CreateAllocaPinnedIntrinsic(elemType) : Instruction.CreateAlloca(elemType),
                name);

            this.localStackSlots.Add(alloca);
        }

        // Jump to the entry point instruction.
        entryPoint.Flow = new JumpFlow(
            branchTargets[cilMethodBody.Instructions[0]].Tag);
    }

    /// <summary>
    /// Initializes exception handler data structures.
    /// </summary>
    /// <param name="cilMethodBody">A CIL method body.</param>
    /// <returns>
    /// A mapping of Cecil exception handlers to CIL analysis exception handlers.
    /// </returns>
    private IReadOnlyDictionary<Mono.Cecil.Cil.ExceptionHandler, CilExceptionHandler> CreateExceptionHandlers(
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        // Initialize exception handler data structures.
        exceptionHandlers = [];
        exceptionHandlerClauses = [];
        var ehMapping = new Dictionary<Mono.Cecil.Cil.ExceptionHandler, CilExceptionHandler>();

        // If there are no exception handlers then we can save ourselves quite
        // a bit of trouble by exiting early.
        if (!cilMethodBody.HasExceptionHandlers)
        {
            foreach (var target in branchTargets.Keys)
            {
                exceptionHandlers[target] = EmptyArray<CilExceptionHandler>.Value;
                exceptionHandlerClauses[target] = EmptyArray<CilExceptionHandler>.Value;
            }
            return ehMapping;
        }

        // Define a flow token variable. We might need it for finally flow.
        flowTokenVariable = graph.EntryPoint
            .AppendInstruction(
                Instruction.CreateAlloca(TypeEnvironment.Int32),
                "flow_token");

        // First map Cecil exception handlers to CIL analysis exception handlers.
        foreach (var handler in cilMethodBody.ExceptionHandlers)
        {
            var pad = graph.AddBasicBlock($"IL_{handler.HandlerStart.Offset.ToString("X4")}_landingpad");
            pad.AppendParameter(
                new BlockParameter(
                    TypeHelpers.BoxIfReferenceType(TypeEnvironment.CapturedException)));
            if (handler.HandlerType == Mono.Cecil.Cil.ExceptionHandlerType.Catch)
            {
                ehMapping[handler] = new CilCatchHandler(
                    pad,
                    new[]
                    {
                        TypeHelpers.BoxIfReferenceType(
                            Assembly.Resolve(handler.CatchType))
                    });
            }
            else if (handler.HandlerType == Mono.Cecil.Cil.ExceptionHandlerType.Finally)
            {
                var leavePad = graph.AddBasicBlock($"IL_{handler.HandlerStart.Offset.ToString("X4")}_leave");
                ehMapping[handler] = new CilFinallyHandler(pad, leavePad);
            }
            else
            {
                throw new NotSupportedException(
                    "Only catch and finally exception handlers are supported; " +
                    $"{handler.HandlerType} handlers are not.");
            }
        }

        // At this point, we want to create a top-level landing pad that does nothing
        // but rethrow uncaught exceptions. That'll make implementing other handlers
        // much easier.
        var toplevelLandingPad = graph.AddBasicBlock("toplevel_landingpad");
        toplevelLandingPad.AppendParameter(
            new BlockParameter(
                TypeHelpers.BoxIfReferenceType(TypeEnvironment.CapturedException)));

        // Make the top-level landing pad rethrow every exception it is passed.
        toplevelLandingPad.AppendInstruction(
            Instruction.CreateRethrowIntrinsic(
                toplevelLandingPad.Parameters[0].Type,
                toplevelLandingPad.Parameters[0].Tag));

        // Create the top-level handler data structure. The top-level handler is
        // kind of funny in the sense that it does not catch *any* exception type.
        // It's not a catch-all handler, either. That should ensure that CIL instructions
        // never directly transfer control to the top-level handler. It wouldn't be
        // wrong for them to do so anyway, but it would obscure control flow.
        // Only exception handlers should transfer control to the top-level handler.
        var toplevelHandler = new CilCatchHandler(toplevelLandingPad, EmptyArray<IType>.Value);

        // Finally iterate over all branch targets and construct exception handler lists.
        var activeHandlers = new Stack<Mono.Cecil.Cil.ExceptionHandler>();
        var activeClauses = new Stack<Mono.Cecil.Cil.ExceptionHandler>();
        foreach (var instruction in cilMethodBody.Instructions)
        {
            if (!branchTargets.ContainsKey(instruction))
            {
                continue;
            }

            // Pop handlers that are no longer active.
            while (activeHandlers.Count > 0 && activeHandlers.Peek().TryEnd == instruction)
            {
                activeHandlers.Pop();
            }

            // Pop clauses that are no longer active.
            while (activeClauses.Count > 0 && activeClauses.Peek().HandlerEnd == instruction)
            {
                activeClauses.Pop();
            }

            // Push handlers that become active.
            foreach (var handler in cilMethodBody.ExceptionHandlers)
            {
                if (handler.TryStart == instruction)
                {
                    activeHandlers.Push(handler);
                }
                if (handler.HandlerStart == instruction)
                {
                    activeClauses.Push(handler);
                }
            }

            exceptionHandlers[instruction] = activeHandlers
                .Select(h => ehMapping[h])
                .Concat(new[] { toplevelHandler })
                .ToArray();

            exceptionHandlerClauses[instruction] = activeClauses
                .Select(h => ehMapping[h])
                .ToArray();
        }

        return ehMapping;
    }

    /// <summary>
    /// Emits a binary arithmetic intrinsic operation.
    /// </summary>
    /// <param name="operatorName">The name of the operator to create.</param>
    /// <param name="isChecked">Tells if the operator checks for overflow.</param>
    /// <param name="first">The first argument to the intrinsic operation.</param>
    /// <param name="second">The second argument to the intrinsic operation.</param>
    /// <param name="context">The CIL analysis context.</param>
    private void EmitArithmeticBinary(
        string operatorName,
        bool isChecked,
        ValueTag first,
        ValueTag second,
        CilAnalysisContext context)
    {
        var firstType = context.GetValueType(first);
        var secondType = context.GetValueType(second);

        bool isRelational = ArithmeticIntrinsics.Operators.IsRelationalOperator(operatorName);

        var resultType = isRelational ? Assembly.Resolver.TypeEnvironment.Boolean : firstType;

        context.Push(
            ArithmeticIntrinsics.CreatePrototype(operatorName, isChecked, resultType, firstType, secondType)
                .Instantiate(first, second));
    }

    /// <summary>
    /// Emits a conditional branch.
    /// </summary>
    /// <param name="condition">
    /// The condition to branch on.
    /// </param>
    /// <param name="ifInstruction">
    /// The instruction to branch to if the condition is true/nonzero.
    /// </param>
    /// <param name="falseInstruction">
    /// The instruction to branch to if the condition is false/zero.
    /// </param>
    /// <param name="cilMethodBody">
    /// The method body that defines the conditional branch.
    /// </param>
    /// <param name="context">
    /// The current CIL analysis context.
    /// </param>
    private void EmitConditionalBranch(
        ValueTag condition,
        Mono.Cecil.Cil.Instruction ifInstruction,
        Mono.Cecil.Cil.Instruction falseInstruction,
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        CilAnalysisContext context)
    {
        var args = context.EvaluationStack.Reverse().ToArray();

        var conditionType = context.GetValueType(condition);
        var conditionISpec = conditionType.GetIntegerSpecOrNull();

        Constant falseConstant;
        if (conditionISpec == null)
        {
            if (conditionType is PointerType)
            {
                falseConstant = NullConstant.Instance;
            }
            else if (conditionType == TypeEnvironment.NaturalInt
                || conditionType == TypeEnvironment.NaturalUInt)
            {
                falseConstant = DefaultConstant.Instance;
            }
            else
            {
                throw new InvalidProgramException(
                    $"Cannot branch on non-pointer, non-integer type '{conditionType.FullName}' (in '{cilMethodBody.Method.FullName}').");
            }
        }
        else
        {
            falseConstant = new IntegerConstant(0).Cast(conditionISpec);
        }

        context.Terminate(
            new SwitchFlow(
                Instruction.CreateCopy(conditionType, condition),
                ImmutableList.Create(
                    new SwitchCase(
                        ImmutableHashSet.Create<Constant>(falseConstant),
                        CreateBranchTo(falseInstruction, args, cilMethodBody, context))),
                CreateBranchTo(ifInstruction, args, cilMethodBody, context)));
    }

    private void EmitConvertAndExtend(IType targetType, bool isChecked, CilAnalysisContext context)
    {
        EmitConvertTo(targetType, isChecked, context);

        // We need to take care to convert integers < 32 bits
        // to 32-bit integers; CIL never keeps < 32 bits integers
        // on the stack.
        var intSpec = targetType.GetIntegerSpecOrNull();
        if (intSpec != null && intSpec.Size < 32)
        {
            if (intSpec.IsSigned)
            {
                // Sign-extend the integer.
                EmitConvertTo(TypeEnvironment.Int32, false, context);
            }
            else
            {
                // Zero-extend, then make sure an int32 ends up on the stack.
                EmitConvertTo(TypeEnvironment.UInt32, false, context);
                EmitConvertTo(TypeEnvironment.Int32, false, context);
            }
        }
    }

    private void EmitConvertSign(
        CilAnalysisContext context,
        bool makeSigned,
        Func<int, IType> createType)
    {
        var value = context.Peek();
        var type = context.GetValueType(value);
        var spec = type.GetIntegerSpecOrNull();
        if (spec != null && spec.IsSigned != makeSigned)
        {
            var resultType = createType(spec.Size);
            if (resultType != null)
            {
                EmitConvertTo(
                    createType(spec.Size),
                    false,
                    context);
            }
        }
    }

    private void EmitConvertTo(
        IType targetType,
        bool isChecked,
        CilAnalysisContext context)
    {
        context.Push(
            EmitConvertTo(context.Pop(), targetType, isChecked, context));
    }

    private ValueTag EmitConvertTo(
        ValueTag operand,
        IType targetType,
        bool isChecked,
        CilAnalysisContext context)
    {
        return context.Emit(
            Instruction.CreateConvertIntrinsic(
                isChecked,
                targetType,
                context.GetValueType(operand),
                operand));
    }

    private void EmitConvertToSigned(
        CilAnalysisContext context)
    {
        EmitConvertSign(context, true, Assembly.Resolver.TypeEnvironment.MakeSignedIntegerType);
    }

    private void EmitConvertToUnsigned(
        CilAnalysisContext context)
    {
        EmitConvertSign(context, false, Assembly.Resolver.TypeEnvironment.MakeUnsignedIntegerType);
    }

    private void EmitImplicitBinaryConversions(
        ref ValueTag first,
        ref ValueTag second,
        CilAnalysisContext context)
    {
        var firstType = graph.GetValueType(first);
        var secondType = graph.GetValueType(second);

        if (firstType == secondType)
        {
            return;
        }

        var firstISpec = firstType.GetIntegerSpecOrNull();
        var secondISpec = secondType.GetIntegerSpecOrNull();
        if (firstISpec != null && secondISpec != null && firstISpec.Size != secondISpec.Size)
        {
            // Extend integers if necessary.
            var newSize = Math.Max(firstISpec.Size, secondISpec.Size);
            var newType = firstISpec.IsSigned
                ? TypeEnvironment.MakeSignedIntegerType(newSize)
                : TypeEnvironment.MakeUnsignedIntegerType(newSize);

            if (newType != null)
            {
                first = EmitConvertTo(first, newType, false, context);
                second = EmitConvertTo(second, newType, false, context);
            }
            return;
        }
        else if (firstISpec != null &&
            (secondType == TypeEnvironment.NaturalInt || secondType == TypeEnvironment.NaturalUInt))
        {
            if (firstISpec.Size <= 32)
            {
                // Integers of 32 bits or smaller can be extended to natural integers.
                first = EmitConvertTo(first, secondType, false, context);
                return;
            }
            else if (firstISpec.Size >= 64)
            {
                // Natural integers can be extended to integers of 64 bits or greater.
                second = EmitConvertTo(second, firstType, false, context);
                return;
            }
        }
        else if (secondISpec != null &&
            (firstType == TypeEnvironment.NaturalInt || firstType == TypeEnvironment.NaturalUInt))
        {
            if (secondISpec.Size <= 32)
            {
                // Integers of 32 bits or smaller can be extended to natural integers.
                second = EmitConvertTo(second, firstType, false, context);
                return;
            }
            else if (secondISpec.Size >= 64)
            {
                // Natural integers can be extended to integers of 64 bits or greater.
                first = EmitConvertTo(first, secondType, false, context);
                return;
            }
        }
    }

    private void EmitJumpTable(
        ValueTag condition,
        IReadOnlyList<Mono.Cecil.Cil.Instruction> labels,
        Mono.Cecil.Cil.Instruction defaultLabel,
        Mono.Cecil.Cil.MethodBody cilMethodBody,
        CilAnalysisContext context)
    {
        var args = context.EvaluationStack.Reverse().ToArray();
        var conditionType = context.GetValueType(condition);
        var conditionSpec = conditionType.GetIntegerSpecOrNull();

        var cases = ImmutableList.CreateBuilder<SwitchCase>();
        int labelCount = labels.Count;
        for (int i = 0; i < labelCount; i++)
        {
            cases.Add(
                new SwitchCase(
                    ImmutableHashSet.Create<Constant>(
                        new IntegerConstant(i, conditionSpec)),
                    CreateBranchTo(labels[i], args, cilMethodBody, context)));
        }
        context.Terminate(
            new SwitchFlow(
                Instruction.CreateCopy(conditionType, condition),
                cases.ToImmutable(),
                CreateBranchTo(defaultLabel, args, cilMethodBody, context)));
    }

    /// <summary>
    /// Emits a binary arithmetic intrinsic operation
    /// for signed integer or floating-point values.
    /// </summary>
    /// <param name="operatorName">The name of the operator to create.</param>
    /// <param name="isChecked">Tells if the operator checks for overflow.</param>
    /// <param name="context">The CIL analysis context.</param>
    private void EmitSignedArithmeticBinary(
        string operatorName,
        bool isChecked,
        CilAnalysisContext context)
    {
        EmitConvertToSigned(context);
        var second = context.Pop();
        EmitConvertToSigned(context);
        var first = context.Pop();
        EmitImplicitBinaryConversions(ref first, ref second, context);
        EmitArithmeticBinary(operatorName, isChecked, first, second, context);
    }

    /// <summary>
    /// Emits a binary arithmetic intrinsic operation
    /// for unsigned integer values.
    /// </summary>
    /// <param name="operatorName">The name of the operator to create.</param>
    /// <param name="isChecked">Tells if the operator checks for overflow.</param>
    /// <param name="context">The CIL analysis context.</param>
    private void EmitUnsignedArithmeticBinary(
        string operatorName,
        bool isChecked,
        CilAnalysisContext context)
    {
        EmitConvertToUnsigned(context);
        var second = context.Pop();
        EmitConvertToUnsigned(context);
        var first = context.Pop();
        EmitImplicitBinaryConversions(ref first, ref second, context);
        EmitArithmeticBinary(operatorName, isChecked, first, second, context);
    }

    private void FlagBranchTarget(
        Mono.Cecil.Cil.Instruction target)
    {
        if (target != null && !branchTargets.ContainsKey(target))
        {
            branchTargets[target] = graph.AddBasicBlock(
                "IL_" + target.Offset.ToString("X4"));
        }
    }

    private NamedInstructionBuilder GetParameterSlot(
        Mono.Cecil.ParameterReference parameterRef,
        Mono.Cecil.Cil.MethodBody cilMethodBody)
    {
        return parameterStackSlots[parameterRef.Index + (cilMethodBody.Method.HasThis ? 1 : 0)];
    }

    /// <summary>
    /// Reuses or creates a temporary alloca slot of a particular type.
    /// </summary>
    /// <param name="elementType">
    /// The type of type to store in the alloca slot.
    /// </param>
    /// <param name="graph">
    /// The graph that defines the alloca.
    /// </param>
    /// <returns>
    /// An alloca slot value.
    /// </returns>
    private ValueTag GetTemporaryAlloca(IType elementType, FlowGraphBuilder graph)
    {
        ValueTag candidate = null;
        foreach (var tag in freeTemporaries)
        {
            var proto = (AllocaPrototype)graph.GetInstruction(tag).Prototype;
            if (proto.ElementType == elementType)
            {
                candidate = tag;
                break;
            }
        }

        if (candidate == null)
        {
            var entryPoint = graph.EntryPoint;
            return entryPoint.AppendInstruction(Instruction.CreateAlloca(elementType), "temp_slot");
        }
        else
        {
            freeTemporaries.Remove(candidate);
            return candidate;
        }
    }

    private void LoadValue(
        ValueTag pointer,
        CilAnalysisContext context)
    {
        context.Push(
            Instruction.CreateLoad(
                ((PointerType)context.GetValueType(pointer)).ElementType,
                pointer));
    }

    private IReadOnlyList<ValueTag> MakeArgumentsUniform(
                                                                                                                                                                                IReadOnlyList<ValueTag> arguments,
        BasicBlockBuilder builder)
    {
        return arguments.Select(arg =>
        {
            var argType = graph.GetValueType(arg);
            if (argType.IsPointerType(PointerKind.Box)
                && ((PointerType)argType).ElementType != TypeEnvironment.Object)
            {
                // CIL doesn't distinguish between different types of objects on the stack
                // (there is just one object type, 'O'). Keeping types more precise within a
                // basic block is harmless, but failing to adhere to the uniform 'O' type
                // can be harmful when creating branches to basic blocks that have more than
                // one predecessor.
                //
                // For instance, suppose that we have the following situation:
                //
                //     Block 1                 Block 2
                //              \            /
                //   String box* \          / Int32 box*        <-- Stack contents
                //                \        /
                //                 v      v
                //                  Block 3
                //
                // Where Block 3 is written to take a Object box* parameter. However, that
                // information (the types of parameters taken by basic blocks) is lost during
                // the compilation process to CIL. For example, CIL for the situation above
                // might look like this:
                //
                //     block_1: ldsfld x
                //              br block_3
                //
                //     block_2: ldsfld y
                //              br block_3
                //
                //     block_3: call Console.WriteLine(object)
                //
                // If we were to leave box arguments unmodified, then the CIL analysis algorithm
                // in this file will encounter a branch to Block 3 and either conclude that
                // Block 3 takes a 'String box*' or conclude that it takes an 'Int32 box*' instead.
                // When the algorithm encounters the next branch, it will balk because 'String box*'
                // and 'Int32 box*' are not the same type.
                //
                // We avoid this messy situation by reinterpeting all box arguments as 'Object box*'.
                //
                return builder.AppendInstruction(
                    Instruction.CreateReinterpretCast(
                        TypeEnvironment.Object.MakePointerType(PointerKind.Box),
                        arg));
            }
            else if (argType.IsUnsignedIntegerType())
            {
                // We will also convert all unsigned integer types to their signed
                // counterparts, as values on the stack are implicitly signed.
                var signedType = TypeEnvironment.MakeSignedIntegerType(argType.GetIntegerSpecOrNull().Size);
                return builder.AppendInstruction(
                    Instruction.CreateConvertIntrinsic(false, signedType, argType, arg));
            }
            else
            {
                return arg;
            }
        }).ToArray();
    }

    /// <summary>
    /// Pops a value of the stack and interprets it as a pointer
    /// to a value of a particular type.
    /// </summary>
    /// <param name="elementType">
    /// The type of value the top-of-stack value should point to.
    /// </param>
    /// <param name="context">
    /// The CIL analysis context.
    /// </param>
    /// <returns>
    /// A pointer to the element type.
    /// </returns>
    private ValueTag PopPointerToType(
        IType elementType,
        CilAnalysisContext context)
    {
        var pointer = context.Pop();
        var pointerType = context.GetValueType(pointer) as PointerType;
        if (pointerType == null)
        {
            // Convert the value to a transient pointer.
            return context.Emit(
                Instruction.CreateConvertIntrinsic(
                    false,
                    elementType.MakePointerType(PointerKind.Transient),
                    context.GetValueType(pointer),
                    pointer));
        }
        else if (pointerType.ElementType != elementType)
        {
            // Emit a reinterpret cast to convert between pointers.
            return context.Emit(
                Instruction.CreateReinterpretCast(
                    elementType.MakePointerType(pointerType.Kind),
                    pointer));
        }
        else
        {
            // Exact match. No need to insert a cast.
            return pointer;
        }
    }

    /// <summary>
    /// Releases a temporary alloca, making it suitable for reuse.
    /// </summary>
    /// <param name="alloca">
    /// The temporary alloca to reuse.
    /// </param>
    private void ReleaseTemporaryAlloca(ValueTag alloca)
    {
        freeTemporaries.Add(alloca);
    }
}