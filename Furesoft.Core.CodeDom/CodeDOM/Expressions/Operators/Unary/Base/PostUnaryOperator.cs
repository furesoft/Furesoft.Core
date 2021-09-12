// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all postfix unary operators (<see cref="PostIncrement"/> and <see cref="PostDecrement"/>).
    /// </summary>
    public abstract class PostUnaryOperator : UnaryOperator
    {
        #region /* CONSTRUCTORS */

        protected PostUnaryOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected PostUnaryOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);

            parser.NextToken();  // Skip past the operator
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            _expression.AsText(writer, flags);
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
        }

        #endregion
    }
}
