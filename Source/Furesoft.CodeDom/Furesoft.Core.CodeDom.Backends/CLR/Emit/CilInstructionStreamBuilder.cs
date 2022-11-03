using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.Target;
using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.CodeDom.Backends.CLR.Emit;

/// <summary>
/// An instruction stream builder for CIL instructions.
/// </summary>
public sealed class CilInstructionStreamBuilder : StackInstructionStreamBuilder<CilCodegenInstruction>
{
    /// <summary>
    /// Creates a CIL instruction stream builder.
    /// </summary>
    /// <param name="selector">An instruction selector.</param>
    private CilInstructionStreamBuilder(CilInstructionSelector selector)
        : base(selector)
    { }

    private CilInstructionSelector CilSelector => (CilInstructionSelector)InstructionSelector;

    /// <summary>
    /// Creates a CIL instruction stream builder.
    /// </summary>
    /// <param name="selector">
    /// The instruction selector to use.
    /// </param>
    public static CilInstructionStreamBuilder Create(CilInstructionSelector selector)
    {
        return new CilInstructionStreamBuilder(selector);
    }

    /// <inheritdoc/>
    protected override IEnumerable<ValueTag> GetStackContentsOnEntry(BasicBlock block)
    {
        if (block.IsEntryPoint)
        {
            return Enumerable.Empty<ValueTag>();
        }
        else
        {
            return block.ParameterTags;
        }
    }

    /// <inheritdoc/>
    protected override bool ShouldMaterializeOnUse(NamedInstruction instruction)
    {
        if (CilSelector.AllocaToVariableMapping.ContainsKey(instruction))
        {
            // Materialize ldloca instructions on use.
            return true;
        }

        // Materialize trivial constants on use.
        var proto = instruction.Prototype as ConstantPrototype;
        return proto != null && proto.Value != DefaultConstant.Instance;
    }
}