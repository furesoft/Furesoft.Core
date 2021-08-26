// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all relational operators (<see cref="Equal"/>, <see cref="NotEqual"/>,
    /// <see cref="GreaterThan"/>, <see cref="LessThan"/>, <see cref="GreaterThanEqual"/>, <see cref="LessThanEqual"/>).
    /// </summary>
    public abstract class RelationalOperator : BinaryBooleanOperator
    {
        #region /* CONSTRUCTORS */

        protected RelationalOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PARSING */

        protected RelationalOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
