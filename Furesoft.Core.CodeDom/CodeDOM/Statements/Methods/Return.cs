// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Indicates that execution should return from the current method, and has an optional <see cref="Expression"/>
    /// to be evaluated as the return value.
    /// </summary>
    public class Return : Statement
    {
        #region /* FIELDS */

        protected Expression _expression;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Return"/>.
        /// </summary>
        public Return(Expression expression)
        {
            Expression = expression;

            // Force parens on any binary operator expression with precedence greater than 200, or the Conditional operator
            if ((expression is BinaryOperator && ((BinaryOperator)expression).GetPrecedence() > 200) || expression is Conditional)
                expression.HasParens = true;
        }

        /// <summary>
        /// Create a <see cref="Return"/>.
        /// </summary>
        public Return()
            : this(null)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The return <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Return clone = (Return)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "return";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="Return"/>.
        /// </summary>
        public static Return Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Return(parser, parent);
        }

        protected Return(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'return'
            SetField(ref _expression, Expression.Parse(parser, this, true), false);
            ParseTerminator(parser);

            // Get rid of parens around the expression if they serve no purpose
            if (AutomaticFormattingCleanup && !parser.IsGenerated && _expression != null
                && !((_expression is BinaryOperator && ((BinaryOperator)_expression).GetPrecedence() > 200) || _expression is Conditional))
                _expression.HasParens = false;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_expression != null)
                _expression = (Expression)_expression.Resolve(ResolveCategory.Expression, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_expression != null && _expression.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Evaluate the <see cref="Expression"/> to a type or namespace.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/>, <see cref="UnresolvedRef"/>, or <see cref="NamespaceRef"/>.</returns>
        public override SymbolicRef EvaluateTypeOrNamespace(bool withoutConstants)
        {
            return (_expression != null ? _expression.EvaluateTypeOrNamespace(withoutConstants) : TypeRef.ObjectRef);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return (_expression != null); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_expression != null)
                {
                    if (value)
                        _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_expression != null)
                _expression.AsText(writer, flags);
        }

        #endregion
    }
}
