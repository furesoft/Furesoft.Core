using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;

namespace Furesoft.Core.CodeDom.Compiler.Flow
{
    /// <summary>
    /// Control flow that conditionally jumps to a particular branch.
    /// </summary>
    public sealed class JumpConditionalFlow : BlockFlow
    {
        /// <summary>
        /// Creates control flow that conditionally jumps
        /// to a particular branch.
        /// </summary>
        /// <param name="branch">The branch to jump to.</param>
        /// <param name="conditionSelector">To decide which jump instruction to emit</param>
        public JumpConditionalFlow(Branch branch, object conditionSelector)
        {
            Branch = branch;
            ConditionSelector = conditionSelector;
        }

        /// <summary>
        /// Creates control flow that unconditionally jumps
        /// to a particular block, passing no arguments.
        /// </summary>
        /// <param name="target">The target block.</param>
        public JumpConditionalFlow(BasicBlockTag target, object conditionSelector)
            : this(new Branch(target), conditionSelector)
        { }

        /// <summary>
        /// Creates control flow that unconditionally jumps
        /// to a particular block, passing a list of arguments.
        /// </summary>
        /// <param name="target">The target block.</param>
        /// <param name="arguments">
        /// A list of arguments to pass to the target block.
        /// </param>
        public JumpConditionalFlow(BasicBlockTag target, object conditionSelector, IReadOnlyList<BranchArgument> arguments)
            : this(new Branch(target, arguments), conditionSelector)
        { }

        /// <summary>
        /// Creates control flow that unconditionally jumps
        /// to a particular block, passing a list of arguments.
        /// </summary>
        /// <param name="target">The target block.</param>
        /// <param name="arguments">
        /// A list of arguments to pass to the target block.
        /// </param>
        public JumpConditionalFlow(BasicBlockTag target, object conditionSelector, IReadOnlyList<ValueTag> arguments)
            : this(new Branch(target, arguments), conditionSelector)
        { }

        /// <summary>
        /// Gets the branch that is unconditionally taken by
        /// this flow.
        /// </summary>
        /// <returns>The jump branch.</returns>
        public Branch Branch { get; private set; }

        public object ConditionSelector { get; set; }

        /// <inheritdoc/>
        public override IReadOnlyList<Instruction> Instructions => EmptyArray<Instruction>.Value;

        /// <inheritdoc/>
        public override IReadOnlyList<Branch> Branches => new Branch[] { Branch };

        /// <inheritdoc/>
        public override BlockFlow WithInstructions(IReadOnlyList<Instruction> instructions)
        {
            ContractHelpers.Assert(instructions.Count == 0, "Jump flow does not take any instructions.");
            return this;
        }

        /// <inheritdoc/>
        public override BlockFlow WithBranches(IReadOnlyList<Branch> branches)
        {
            ContractHelpers.Assert(branches.Count == 1, "Jump flow takes exactly one branch.");
            var newBranch = branches[0];
            if (object.ReferenceEquals(newBranch, Branch))
            {
                return this;
            }
            else
            {
                return new JumpFlow(newBranch);
            }
        }

        /// <inheritdoc/>
        public override InstructionBuilder GetInstructionBuilder(
            BasicBlockBuilder block,
            int instructionIndex)
        {
            throw new IndexOutOfRangeException();
        }
    }
}