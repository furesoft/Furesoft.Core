﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals
{
    /// <summary>
    /// Used as a child of a <see cref="Switch"/>.  Includes a constant expression plus a body (a statement or block).
    /// </summary>
    public class Case : SwitchItem
    {
        #region /* FIELDS */

        protected Expression _constantExpression;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Case"/>.
        /// </summary>
        public Case(Expression constant, CodeObject body)
            : base(body)
        {
            ConstantExpression = constant;
        }

        /// <summary>
        /// Create a <see cref="Case"/>.
        /// </summary>
        public Case(Expression constant)
            : base(null)
        {
            ConstantExpression = constant;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The constant <see cref="Expression"/> of the <see cref="Case"/>.
        /// </summary>
        public Expression ConstantExpression
        {
            get { return _constantExpression; }
            set { SetField(ref _constantExpression, value, true); }
        }

        /// <summary>
        /// The name of the <see cref="Case"/>.
        /// </summary>
        public override string Name
        {
            get { return ParseToken + " " + (_constantExpression != null ? _constantExpression.AsString() : null); }
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
            Case clone = (Case)base.Clone();
            clone.CloneField(ref clone._constantExpression, _constantExpression);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "case";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(Switch));
        }

        /// <summary>
        /// Parse a <see cref="Case"/>.
        /// </summary>
        public static Case Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Case(parser, parent);
        }

        protected Case(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'case'
            SetField(ref _constantExpression, Expression.Parse(parser, this, true, ParseTokenTerminator), false);
            ParseTerminatorAndBody(parser);  // Parse ':' and body (if any)
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Normally, the constant expressions of all Cases in a Switch are resolved by the Switch itself (in order
            // to allow forward references for 'goto case ...' statements with single-pass resolving).  But, just in case
            // we're an orphaned Case without a Switch parent, resolve the constant expression here.
            if (!(Parent is Switch))
                ResolveConstantExpression(flags);
            return base.Resolve(ResolveCategory.CodeObject, flags);
        }

        /// <summary>
        /// Resolve the constant expression of the <see cref="Case"/>.
        /// </summary>
        public CodeObject ResolveConstantExpression(ResolveFlags flags)
        {
            // Allow any expression - non-constant expressions will be flagged during the analysis phase
            if (_constantExpression != null)
                _constantExpression = (Expression)_constantExpression.Resolve(ResolveCategory.Expression, flags);
            return _constantExpression;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_constantExpression == null || (!_constantExpression.IsFirstOnLine && _constantExpression.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _constantExpression != null)
                {
                    _constantExpression.IsFirstOnLine = false;
                    _constantExpression.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_constantExpression != null)
                _constantExpression.AsText(writer, flags);
        }

        #endregion
    }
}
