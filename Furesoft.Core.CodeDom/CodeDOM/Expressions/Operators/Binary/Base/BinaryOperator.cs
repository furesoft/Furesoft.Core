// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary operators (<see cref="BinaryArithmeticOperator"/>, <see cref="BinaryBitwiseOperator"/>,
    /// <see cref="BinaryBooleanOperator"/>, <see cref="BinaryShiftOperator"/>, <see cref="Assignment"/>, <see cref="As"/>, <see cref="Dot"/>,
    /// <see cref="IfNullThen"/>, <see cref="Lookup"/>).
    /// </summary>
    public abstract class BinaryOperator : Operator
    {
        #region /* FIELDS */

        protected Expression _left;
        protected Expression _right;

        // If the operator is overloaded, a hidden reference (OperatorRef) to the overloaded
        // operator declaration is stored here.
        protected SymbolicRef _operatorRef;

        #endregion

        #region /* CONSTRUCTORS */

        protected BinaryOperator(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        protected BinaryOperator()
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The left-side <see cref="Expression"/>.
        /// </summary>
        public Expression Left
        {
            get { return _left; }
            set { SetField(ref _left, value, true); }
        }

        /// <summary>
        /// The right-side <see cref="Expression"/>.
        /// </summary>
        public Expression Right
        {
            get { return _right; }
            set { SetField(ref _right, value, true); }
        }

        /// <summary>
        /// A hidden OperatorRef to an overloaded operator declaration (if any).
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return _operatorRef; }
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            // If both sides are const, then the result will be const
            get { return (_left != null && _left.IsConst && _right != null && _right.IsConst); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public virtual string GetInternalName()
        {
            return null;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            BinaryOperator clone = (BinaryOperator)base.Clone();
            clone.CloneField(ref clone._left, _left);
            clone.CloneField(ref clone._right, _right);
            clone.CloneField(ref clone._operatorRef, _operatorRef);
            return clone;
        }

        #endregion

        #region /* PARSING */

        protected BinaryOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            IsFirstOnLine = false;
            Expression left = parser.RemoveLastUnusedExpression();
            MoveFormatting(left);
            SetField(ref _left, left, false);

            Token operatorToken = parser.Token;
            if (left != null)
            {
                // Move any comments before the operator to the left expression as post comments
                left.MoveCommentsAsPost(parser.LastToken);
                // Move any formatting on the operator to the left expression
                left.MoveFormatting(operatorToken);

                // Get rid of parens around the left expression if they're not necessary
                if (AutomaticFormattingCleanup && !parser.IsGenerated && left is Operator)
                {
                    if (((Operator)left).GetPrecedence() <= 100 || left.GetType() == GetType())
                        left.HasParens = false;
                }
            }
            parser.NextToken();  // Skip past the operator

            // Move any EOL comment on the operator as an infix EOL comment
            if (operatorToken.HasTrailingComments)
                MoveEOLCommentAsInfix(operatorToken);

            // Abort parsing if a ')' is encountered, to improve parsing of bad/unrecognized code
            if (parser.TokenText != ParseTokenEndGroup)
                SetField(ref _right, Parse(parser, this), false);
            if (_right != null)
            {
                // Move any EOL or Postfix annotations from the right side up to the parent if there are no
                // parens in the way - this "normalizes" the annotations to the highest node on the line.
                if (_right.HasEOLOrPostAnnotations && parent != parser.GetNormalizationBlocker())
                    MoveEOLAndPostAnnotations(_right);

                // Move newlines immediately after an operator to before it if auto-cleanup is on
                // (exception: don't do this for any Assignment operators).
                if (AutomaticFormattingCleanup && !parser.IsGenerated && _right.NewLines > 0 && left != null && left.NewLines == 0 && !(this is Assignment))
                {
                    left.NewLines = _right.NewLines;
                    _right.NewLines = 0;

                    // If the operator has an infix EOL comment, move it to the left expression
                    if (HasInfixComments)
                        left.MoveEOLComment(this);
                    // Move any (non-EOL) comments after the operator as post comments on the left expression
                    if (operatorToken.HasTrailingComments)
                        _left.MoveCommentsAsPost(operatorToken);
                    // Move any prefix annotations on the right expression to be post annotations on the left
                    left.MovePrefixAnnotationsAsPost(_right);
                }
                else
                {
                    // Move any (non-EOL) comments after the operator to the right expression
                    if (operatorToken.HasTrailingComments)
                        _right.MoveCommentsToLeftMost(operatorToken, false);
                }
            }

            // Set the parent starting token to the beginning of the expression
            parser.ParentStartingToken = startingToken;
        }

        /// <summary>
        /// Move any comments from the specified <see cref="Token"/> to the left-most sub-expression.
        /// </summary>
        public override void MoveCommentsToLeftMost(Token token, bool skipParens)
        {
            if ((HasParens && !skipParens) || _left == null)
                MoveAllComments(token);
            else
                _left.MoveCommentsToLeftMost(token, false);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_left != null)
                _left = (Expression)_left.Resolve(ResolveCategory.Expression, flags);
            if (_right != null)
                _right = (Expression)_right.Resolve(ResolveCategory.Expression, flags);
            return ResolveOverload();
        }

        /// <summary>
        /// Resolve any overload for the operator.
        /// </summary>
        public Operator ResolveOverload()
        {
            if (_operatorRef == null)
            {
                // After the operands have been resolved, we need to check for any overloaded operator
                // that matches the types.  Get the internal name of the operator, and skip if it's null
                // or if either of the operands is null.
                string name = GetInternalName();
                if (name != null && _left != null && _right != null)
                {
                    // Determine if an overloaded operator exists - create an UnresolvedRef, which will be
                    // resolved below as an operator overload declaration reference.  If it fails to resolve,
                    // null is returned, and no errors are logged.
                    SetField(ref _operatorRef, new UnresolvedRef(name, ResolveCategory.OperatorOverload, LineNumber, ColumnNumber), false);
                }
            }
            if (_operatorRef != null)
                _operatorRef = (SymbolicRef)_operatorRef.Resolve(ResolveCategory.OperatorOverload, ResolveFlags.Quiet);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_left != null && _left.HasUnresolvedRef())
                return true;
            if (_right != null && _right.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // If we have a reference to an overloaded operator declaration, use its return type
            if (_operatorRef is OperatorRef)
                return ((OperatorRef)_operatorRef).GetReturnType();

            if (_left == null || _right == null)
                return null;

            // By default, determine a common type (using implicit conversions) that can handle the
            // result of the operation (various binary operators will override this behavior).
            return TypeRef.GetCommonType(_left.EvaluateType(withoutConstants), _right.EvaluateType(withoutConstants));
        }

        protected virtual object EvaluateConstants(object leftConstant, object rightConstant)
        {
            // By default, assume the operator doesn't have a constant result (subclasses will override this as appropriate)
            return null;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return true; }  // Default to using parens for binary operators
        }

        protected override void DefaultFormatField(CodeObject field)
        {
            base.DefaultFormatField(field);

            Expression expression = (Expression)field;
            if (field == _left && expression is Operator)
            {
                // Force parens around the left expression if it's an operator with precedence greater than 100, and is also
                // lower than the current operator's precedence or is the same but isn't the same operator.
                if (!expression.HasParens)
                {
                    int leftPrecedence = ((Operator)expression).GetPrecedence();
                    int precedence = GetPrecedence();
                    if (leftPrecedence > 100 && (leftPrecedence > precedence || (leftPrecedence == precedence && expression.GetType() != GetType())))
                        expression.HasParens = true;
                }
                else
                {
                    // Default parens to off for the child expression if it has a precedence of 100 or is the same operator as the parent
                    if (!expression.IsGroupingSet && (((Operator)expression).GetPrecedence() <= 100 || expression.GetType() == GetType()))
                        expression.SetFormatFlag(FormatFlags.Grouping, false);
                }
            }
            if (field == _right && expression is Operator)
            {
                // Force parens around the right expression if it's an operator with the same or lower precedence than the
                // current one, and not 500 or higher (Assignment and other special operators).
                if (!expression.HasParens)
                {
                    int rightPrecedence = ((Operator)expression).GetPrecedence();
                    if (rightPrecedence >= GetPrecedence() && rightPrecedence < 500)
                        expression.HasParens = true;
                }
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_left == null || (!_left.IsFirstOnLine && _left.IsSingleLine))
                    && (_right == null || (!_right.IsFirstOnLine && _right.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_left != null)
                    _left.IsSingleLine = value;
                if (_right != null)
                    _right.IsSingleLine = value;
            }
        }

        #endregion

        #region /* RENDERING */

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            // Increase the indent level for any binary operator expressions that wrap, unless the parent
            // is the same operator (required for right-associative operators, such as Assignment - left
            // associative operators would only indent once anyway, due to the drawing order).
            base.AsText(writer, flags | (_parent == null || _parent.GetType() != GetType() ? RenderFlags.IncreaseIndent : 0));
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (_left != null)
                _left.AsText(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            AsTextInfixComments(writer, 0, flags | RenderFlags.PrefixSpace);
            if (_right != null)
                _right.AsText(writer, passFlags | RenderFlags.PrefixSpace);
        }

        #endregion
    }
}
