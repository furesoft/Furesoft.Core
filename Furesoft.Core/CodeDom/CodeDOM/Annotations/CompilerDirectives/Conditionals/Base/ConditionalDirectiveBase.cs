// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/>, <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, and <see cref="EndIfDirective"/>.
    /// </summary>
    public class ConditionalDirectiveBase : CompilerDirective
    {
        #region /* CONSTRUCTORS */

        protected ConditionalDirectiveBase()
        { }

        #endregion

        #region /* PARSING */

        protected ConditionalDirectiveBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
