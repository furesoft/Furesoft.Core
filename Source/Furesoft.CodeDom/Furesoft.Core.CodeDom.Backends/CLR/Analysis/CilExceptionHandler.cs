using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler;

namespace Furesoft.Core.CodeDom.Backends.CLR.Analysis;

/// <summary>
/// A CIL exception handler that has typical 'catch' behavior:
/// it is triggered when an exception is thrown and does not
/// have special interactions with 'leave' instructions.
/// </summary>
public sealed class CilCatchHandler : CilExceptionHandler
{
    private IReadOnlyList<IType> handledTypes;

    private BasicBlockTag landingPadTag;

    /// <summary>
    /// Creates an exception handler that will catch
    /// any exception.
    /// </summary>
    /// <param name="landingPad">
    /// The landing pad to redirect flow to when an exception gets thrown.
    /// </param>
    public CilCatchHandler(
        BasicBlockTag landingPad)
        : this(landingPad, null)
    { }

    /// <summary>
    /// Creates an exception handler that will catch
    /// only exceptions that inherit from a list of types.
    /// </summary>
    /// <param name="landingPad">
    /// The landing pad to redirect flow to when an exception gets thrown.
    /// </param>
    /// <param name="handledExceptionTypes">
    /// The list of exception types that are handled.
    /// Subtypes of these types are also handled.
    /// </param>
    public CilCatchHandler(
        BasicBlockTag landingPad,
        IReadOnlyList<IType> handledExceptionTypes)
    {
        this.landingPadTag = landingPad;
        this.handledTypes = handledExceptionTypes;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<IType> HandledExceptionTypes => handledTypes;

    /// <inheritdoc/>
    public override BasicBlockTag LandingPad => landingPadTag;

    /// <summary>
    /// Gets the exception captured by this catch handler.
    /// </summary>
    /// <param name="graph">The control-flow graph that defines the landing pad.</param>
    /// <returns>A value that identifies the exception captured by this catch handler.</returns>
    public ValueTag GetCapturedException(FlowGraph graph)
    {
        return graph.GetBasicBlock(landingPadTag).Parameters[0];
    }
}

/// <summary>
/// Describes a CIL exception handler.
/// </summary>
public abstract class CilExceptionHandler
{
    /// <summary>
    /// Gets the list of types supported by this exception handler.
    /// This property is <c>null</c> if the handler catches all exceptions.
    /// </summary>
    /// <value>A list of exception types or <c>null</c>.</value>
    public abstract IReadOnlyList<IType> HandledExceptionTypes { get; }

    /// <summary>
    /// Tells if this exception handler will catch any exception.
    /// </summary>
    public bool IsCatchAll => HandledExceptionTypes == null;

    /// <summary>
    /// Gets the landing pad basic block to which flow is
    /// redirected when an exception is thrown.
    /// </summary>
    /// <value>A basic block tag.</value>
    public abstract BasicBlockTag LandingPad { get; }
}

/// <summary>
/// A CIL exception handler that has typical 'finally' behavior:
/// it intercepts 'leave' branches.
/// </summary>
public sealed class CilFinallyHandler : CilExceptionHandler
{
    private BasicBlockTag landingPadTag;

    /// <summary>
    /// Creates a 'finally' exception handler.
    /// </summary>
    /// <param name="landingPad">
    /// The landing pad to redirect flow to when an exception gets thrown.
    /// </param>
    /// <param name="leavePad">
    /// A special landing pad with zero parameters that is jumped
    /// to when the finally handler is to be run in the absence
    /// of an exception. The leave pad corresponds to the happy path.
    /// </param>
    public CilFinallyHandler(BasicBlockTag landingPad, BasicBlockTag leavePad)
    {
        this.landingPadTag = landingPad;
        this.LeavePad = leavePad;
        this.Flow = new EndFinallyFlow();
    }

    /// <inheritdoc/>
    public override IReadOnlyList<IType> HandledExceptionTypes => null;

    /// <inheritdoc/>
    public override BasicBlockTag LandingPad => landingPadTag;

    /// <summary>
    /// A special landing pad with zero parameters that is jumped
    /// to when the finally handler is to be run in the absence
    /// of an exception. The leave pad corresponds to the happy path.
    /// </summary>
    /// <value>The leave pad.</value>
    public BasicBlockTag LeavePad { get; private set; }

    internal EndFinallyFlow Flow { get; private set; }
}

/// <summary>
/// Represents the flow produced by an endfinally instruction.
/// Endfinally flow transfers control to either the target of
/// the last leave instruction or to an intervening finally
/// handler.
///
/// Endfinally flow is transient: it exists solely as a placeholder
/// for switch flow, which is constructed at the same time as
/// the CFG. Once the entire CFG has been constructed, endfinally
/// flow is lowered to equivalent switch flow.
/// </summary>
internal sealed class EndFinallyFlow : BlockFlow
{
    public EndFinallyFlow()
    {
        this.Destinations = [];
    }

    public override IReadOnlyList<Branch> Branches
    {
        get
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Gets or sets the default branch.
    /// </summary>
    /// <value>The default branch.</value>
    public Branch DefaultBranch { get; set; }

    /// <summary>
    /// A mapping of integer token values to branches.
    /// </summary>
    /// <value>A mapping of tokens to branches.</value>
    public Dictionary<int, Branch> Destinations { get; private set; }

    public override IReadOnlyList<Instruction> Instructions
    {
        get
        {
            throw new InvalidOperationException();
        }
    }

    public override InstructionBuilder GetInstructionBuilder(BasicBlockBuilder block, int instructionIndex)
    {
        throw new IndexOutOfRangeException();
    }

    /// <summary>
    /// Lowers this endfinally flow to switch flow.
    /// </summary>
    /// <param name="token">
    /// An instruction that produces the token value
    /// to switch on.
    /// </param>
    /// <returns>Equivalent switch flow.</returns>
    public SwitchFlow ToSwitchFlow(
        Instruction token)
    {
        return new SwitchFlow(
            token,
            Destinations
                .Select(
                    pair => new SwitchCase(
                        new IntegerConstant(pair.Key),
                        pair.Value))
                .ToArray(),
            DefaultBranch);
    }

    public override BlockFlow WithBranches(IReadOnlyList<Branch> branches)
    {
        throw new InvalidOperationException();
    }

    public override BlockFlow WithInstructions(IReadOnlyList<Instruction> instructions)
    {
        throw new InvalidOperationException();
    }
}