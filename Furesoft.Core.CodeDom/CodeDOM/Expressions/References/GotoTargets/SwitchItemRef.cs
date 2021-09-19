using Furesoft.Core.CodeDom.Rendering;

namespace Nova.CodeDOM
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