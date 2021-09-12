using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary
{
    /// <summary>
    /// The Cast operator casts an Expression to the specified Type.
    /// </summary>
    public class Cast : PreUnaryOperator
    {
        #region /* FIELDS */

        protected Expression _type;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Cast"/> operator.
        /// </summary>
        /// <param name="type">An <see cref="Expression"/> that evaluates to a <see cref="TypeRef"/> representing the target type.</param>
        /// <param name="expression">The <see cref="Expression"/> to be cast.</param>
        public Cast(Expression type, Expression expression)
            : base(expression)
        {
            Type = type;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The cast target type.
        /// </summary>
        public Expression Type
        {
            get { return _type; }
            set { SetField(ref _type, value, true); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="UnaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return NamePrefix + Modifiers.Explicit;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Cast clone = (Cast)base.Clone();
            clone.CloneField(ref clone._type, _type);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of the <see cref="Cast"/> operator.
        /// </summary>
        public const string ParseTokenStart = ParseTokenStartGroup;

        /// <summary>
        /// The token used to parse the end of the <see cref="Cast"/> operator.
        /// </summary>
        public const string ParseTokenEnd = ParseTokenEndGroup;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 300 (ConstructorDecl uses 0, MethodDecl uses 50, LambdaExpression uses 100, Call uses 200, Expression parens uses 400)
            Parser.AddOperatorParsePoint(ParseTokenStart, 300, Precedence, LeftAssociative, true, Parse);
        }

        /// <summary>
        /// Parse a <see cref="Cast"/> operator.
        /// </summary>
        public static Cast Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a pattern of "(Type)", otherwise abort (so the expression parens logic can try parsing it).
            // Do NOT set the ParseFlags.Type flag here, because it might not be a type (it might be "(A * B)").
            // Also, verify that we're not inside a directive expression - casts aren't legal there.
            if (TypeRefBase.PeekType(parser, parser.PeekNextToken(), false, flags) && !parser.InDirectiveExpression)
            {
                Token last = parser.LastPeekedToken;
                if (last != null && last.Text == ParseTokenEnd)
                {
                    // Verify that the cast is either followed by a non-symbol, or various legal symbols (those that
                    // can only be unary operators).
                    Token next = parser.PeekNextToken();
                    if (next != null)
                    {
                        if (!next.IsSymbol || next.Text == ParseTokenStartGroup || next.Text == Complement.ParseToken
                            || next.Text == Increment.ParseToken || next.Text == Decrement.ParseToken || next.Text == Not.ParseToken)
                            return new Cast(parser, parent);

                        // In the case of the Negative and Positive unary operators following the Cast,
                        // it's impossible to be sure they aren't actually binary operators, such as "(v1)-v2" or
                        // "(v1)-(v2)" (yes, programmers actually write code like that for some reason!).
                        // For now, assume if the operator is followed by a space and/or a '(', it's a binary
                        // operator (so we don't have a Cast).  This will cover most cases, but not 100%.
                        // Any parse issues could be easily worked around by adding parens around the entire right
                        // expression being cast, such as "(v1)(-(v2))" to force a cast, or around either both sides
                        // or neither side of a binary operator to avoid a cast.
                        if (next.Text == Positive.ParseToken || next.Text == Negative.ParseToken)
                        {
                            next = parser.PeekNextToken();
                            if (next.Text != ParseTokenStartGroup && next.LeadingWhitespace.Length == 0)
                                return new Cast(parser, parent);
                        }
                    }

                    // Otherwise, fail and treat it as a grouped expression instead.
                }
            }
            return null;
        }

        protected Cast(Parser parser, CodeObject parent)
            : base(parser, parent, true)
        {
            parser.NextToken();  // Move past '('
            SetField(ref _type, Parse(parser, this, false, ParseTokenEnd, ParseFlags.Type), false);
            ParseExpectedToken(parser, ParseTokenEnd);  // Move past ')'
            SetField(ref _expression, Parse(parser, this), false);
            if (_expression != null)
            {
                // Move any EOL or Postfix annotations from the expression up to the parent if there are no
                // parens in the way - this "normalizes" the annotations to the highest node on the line.
                if (_expression.HasEOLOrPostAnnotations && parent != parser.GetNormalizationBlocker())
                    MoveEOLAndPostAnnotations(_expression);
            }
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _type = (Expression)_type.Resolve(ResolveCategory.Type, flags);
            return base.Resolve(resolveCategory, flags);
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_type != null && _type.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Evaluate to the type of the cast, but also include any constant value in the expression
            TypeRefBase newType = _type.EvaluateType(withoutConstants);
            if (_expression != null && _expression.IsConst)
            {
                // Use IsConst first, as it's faster and avoids possible infinite loops, but if it's const, then
                // we have to evaluate the expression.
                // Because we can't create constant instances of nullable types (see TypeUtil.ChangeType:840), don't
                // even try, or we'll end up with a non-nullable constant - it's better to lose the constant and keep
                // the type correct, since this can for example effect the evaluated type of Conditionals.
                TypeRefBase expressionType = _expression.EvaluateType(withoutConstants);
                if (newType is TypeRef && !newType.IsNullableType && expressionType != null && expressionType.IsConst)
                    newType = new TypeRef((TypeRef)newType, expressionType.GetConstantValue());
            }
            return newType;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            return _type.FindTypeArgument(typeParameterRef, originatingChild);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_type == null || (!_type.IsFirstOnLine && _type.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _type != null)
                {
                    _type.IsFirstOnLine = false;
                    _type.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextOperator(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            writer.Write(ParseTokenStart);
            if (_type != null)
                _type.AsText(writer, passFlags);
            writer.Write(ParseTokenEnd);
        }

        #endregion
    }
}
