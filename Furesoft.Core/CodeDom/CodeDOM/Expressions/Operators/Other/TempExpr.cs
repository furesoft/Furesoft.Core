// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Used temporarily during the parsing process for special situations - specifically,
    /// as a temporary holder of compiler directives that need to be returned to a higher level.
    /// </summary>
    public class TempExpr : Expression
    {
        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        { }

        #endregion
    }
}
