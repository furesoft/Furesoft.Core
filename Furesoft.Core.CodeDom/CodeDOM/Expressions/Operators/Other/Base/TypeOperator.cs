// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of the <see cref="TypeOf"/>, <see cref="SizeOf"/>, and <see cref="DefaultValue"/> operators.
    /// </summary>
    public abstract class TypeOperator : SingleArgumentOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a type operator - the expression must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected TypeOperator(Expression type)
            : base(type)
        { }

        #endregion

        #region /* PARSING */

        protected TypeOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
