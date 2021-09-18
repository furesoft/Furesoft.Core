// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="SwitchItem"/> (<see cref="Case"/> or <see cref="Default"/>).
    /// </summary>
    public class SwitchItemRef : GotoTargetRef
    {
        #region /* CONSTRUCTORS */

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

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            ((SwitchItem)_reference).AsTextGotoTarget(writer, flags &~ RenderFlags.UpdateLineCol);
        }

        #endregion
    }
}
