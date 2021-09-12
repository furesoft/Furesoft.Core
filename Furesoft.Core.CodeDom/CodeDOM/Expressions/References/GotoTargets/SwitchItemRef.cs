using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets
{
    /// <summary>
    /// Represents a reference to a <see cref="SwitchItem"/> (<see cref="Case"/> or <see cref="Default"/>).
    /// </summary>
    public class SwitchItemRef : GotoTargetRef
    {
        /// <summary>
        /// Create a <see cref="SwitchItemRef"/>.
        /// </summary>
        public SwitchItemRef(SwitchItem declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="SwitchItemRef"/>.
        /// </summary>
        public SwitchItemRef(SwitchItem declaration)
            : base(declaration, false)
        { }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            ((SwitchItem)_reference).AsTextGotoTarget(writer, flags & ~RenderFlags.UpdateLineCol);
        }
    }
}