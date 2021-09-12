using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base
{
    /// <summary>
    /// The common base class of all operators with a fixed single argument (<see cref="Ref"/>, <see cref="Out"/>,
    /// <see cref="Checked"/>, <see cref="Unchecked"/>, <see cref="TypeOf"/>, <see cref="SizeOf"/>, <see cref="DefaultValue"/>).
    /// </summary>
    public abstract class SingleArgumentOperator : Operator
    {
        #region /* FIELDS */

        protected Expression _expression;

        #endregion

        #region /* CONSTRUCTORS */

        protected SingleArgumentOperator(Expression expression)
        {
            Expression = expression;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The <see cref="Expression"/> being operated on.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            SingleArgumentOperator clone = (SingleArgumentOperator)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        #endregion

        #region /* PARSING */

        protected SingleArgumentOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseKeywordAndArgument(Parser parser, ParseFlags flags)
        {
            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            parser.NextToken();  // Move past the keyword

            // If the argument has parens, it's a normal operator, like 'typeof()', otherwise it's a top-level
            // operator (ref/out) and we have to parse it as such.
            if (HasArgumentParens)
            {
                ParseExpectedToken(parser, ParseTokenStartGroup);  // Move past '('
                SetField(ref _expression, Parse(parser, this, false, ParseTokenEndGroup, flags), false);
                ParseExpectedToken(parser, ParseTokenEndGroup);  // Move past ')'
            }
            else
                SetField(ref _expression, Parse(parser, this, true, null, flags), false);

            // Set the parent starting token to the beginning of the expression
            parser.ParentStartingToken = startingToken;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_expression != null)
                _expression = (Expression)_expression.Resolve(resolveCategory, flags);
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
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // By default, these operations evaluate to the type of the expression
            return (_expression != null ? _expression.EvaluateType(withoutConstants) : null);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the argument has parens around it.
        /// </summary>
        public virtual bool HasArgumentParens
        {
            get { return true; }  // Default is argument has parens
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
                if (value && _expression != null)
                {
                    _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool hasParens = HasArgumentParens;
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            writer.Write(hasParens ? ParseTokenStartGroup : " ");
            if (_expression != null)
                _expression.AsText(writer, passFlags);
            if (hasParens)
                writer.Write(ParseTokenEndGroup);
        }

        #endregion
    }
}
