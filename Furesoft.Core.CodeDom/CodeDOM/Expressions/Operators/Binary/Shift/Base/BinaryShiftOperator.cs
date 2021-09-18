// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="LeftShift"/> and <see cref="RightShift"/>.
    /// </summary>
    public abstract class BinaryShiftOperator : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        protected BinaryShiftOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PROPERTIES */

        #endregion

        #region /* PARSING */

        protected BinaryShiftOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
