// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of the <see cref="Checked"/> and <see cref="Unchecked"/> operators.
    /// </summary>
    public abstract class CheckedOperator : SingleArgumentOperator
    {
        #region /* CONSTRUCTORS */

        protected CheckedOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected CheckedOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
