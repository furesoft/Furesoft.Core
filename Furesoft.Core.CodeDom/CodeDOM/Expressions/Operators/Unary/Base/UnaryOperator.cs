using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base
{
    /// <summary>
    /// The common base class of all unary operators (<see cref="PreUnaryOperator"/>, <see cref="PostUnaryOperator"/>).
    /// </summary>
    public abstract class UnaryOperator : Operator
    {
        #region /* FIELDS */

        protected Expression _expression;

        // If the operator is overloaded, a hidden reference (OperatorRef) to the overloaded
        // operator declaration is stored here.
        protected SymbolicRef _operatorRef;

        #endregion

        #region /* CONSTRUCTORS */

        protected UnaryOperator(Expression expression)
        {
            Expression = expression;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The associated <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
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
            // If the expression is const, then the result will be const
            get { return (_expression != null && _expression.IsConst); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="UnaryOperator"/>.
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
            UnaryOperator clone = (UnaryOperator)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            clone.CloneField(ref clone._operatorRef, _operatorRef);
            return clone;
        }

        #endregion

        #region /* PARSING */

        protected UnaryOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Get rid of parens around the expression if they're not necessary
            if (AutomaticFormattingCleanup && !parser.IsGenerated && _expression != null
                && !(_expression is Operator && ((Operator)_expression).GetPrecedence() > GetPrecedence()))
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
            return ResolveOverload();
        }

        /// <summary>
        /// Resolve any overload for the operator.
        /// </summary>
        public Operator ResolveOverload()
        {
            if (_operatorRef == null)
            {
                // After the operand has been resolved, we need to check for any overloaded operator
                // that matches the type.  Get the internal name of the operator, and skip if it's null
                // or if the operand is null.
                string name = GetInternalName();
                if (name != null && _expression != null)
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
            // If we have a reference to an overloaded operator declaration, use its return type
            if (_operatorRef is OperatorRef)
                return ((OperatorRef)_operatorRef).GetReturnType();

            // By default, unary operations evaluate to the type of the expression they operate on
            return _expression.EvaluateType(withoutConstants);
        }

        #endregion

        #region /* FORMATTING */

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

        protected override void DefaultFormatField(CodeObject field)
        {
            base.DefaultFormatField(field);

            // Force parens around the expression if it's an operator with lower precedence than the current one,
            // otherwise parens aren't necessary so force them off.
            Expression expression = (Expression)field;
            expression.HasParens = (expression is Operator && ((Operator)expression).GetPrecedence() > GetPrecedence());
        }

        #endregion
    }
}
