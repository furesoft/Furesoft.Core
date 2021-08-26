// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all prefix unary operators (<see cref="Cast"/>, <see cref="Complement"/>,
    /// <see cref="Decrement"/>, <see cref="Increment"/>, <see cref="Negative"/>, <see cref="Not"/>, <see cref="Positive"/>).
    /// </summary>
    public abstract class PreUnaryOperator : UnaryOperator
    {
        #region /* CONSTRUCTORS */

        protected PreUnaryOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected PreUnaryOperator(Parser parser, CodeObject parent, bool skipParsing)
            : base(parser, parent)
        {
            if (!skipParsing)
            {
                parser.NextToken();  // Skip past the operator
                SetField(ref _expression, Parse(parser, this), false);

                // Move any EOL or Postfix annotations from the expression up to the parent if there are no
                // parens in the way - this "normalizes" the annotations to the highest node on the line.
                if (_expression != null && _expression.HasEOLOrPostAnnotations && parent != parser.GetNormalizationBlocker())
                    MoveEOLAndPostAnnotations(_expression);
            }
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            if (_expression != null)
                _expression.AsText(writer, flags);
        }

        #endregion
    }
}
