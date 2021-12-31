using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Backends.CLR
{
    /// <summary>
    /// Subtyping rules for the CLR's type system.
    /// </summary>
    public sealed class ClrSubtypingRules : SubtypingRules
    {
        /// <summary>
        /// An instance of the CLR subtyping rules.
        /// </summary>
        public static readonly ClrSubtypingRules Instance =
            new ClrSubtypingRules();

        private ClrSubtypingRules()
        {
        }

        /// <inheritdoc/>
        public override ImpreciseBoolean IsSubtypeOf(IType subtype, IType supertype)
        {
            if (subtype == supertype)
            {
                return ImpreciseBoolean.True;
            }

            // TODO: refine this!
            return ImpreciseBoolean.Maybe;
        }
    }
}