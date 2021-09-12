using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// Represents a call to another constructor in the same class (constructor initializer).
    /// </summary>
    public class ThisInitializer : ConstructorInitializer
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ThisInitializer"/> operator.
        /// </summary>
        public ThisInitializer(SymbolicRef symbolicRef, params Expression[] parameters)
            : base(symbolicRef, parameters)
        { }

        /// <summary>
        /// Create a <see cref="ThisInitializer"/> operator.
        /// </summary>
        public ThisInitializer(ConstructorRef constructorRef, params Expression[] parameters)
            : base(constructorRef, parameters)
        { }

        /// <summary>
        /// Create a <see cref="ThisInitializer"/> operator.
        /// </summary>
        public ThisInitializer(ConstructorDecl constructorDecl, params Expression[] parameters)
            : base(constructorDecl, parameters)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ThisRef.ParseToken;

        /// <summary>
        /// Parse a <see cref="ThisInitializer"/> operator.
        /// </summary>
        public ThisInitializer(Parser parser, CodeObject parent)
            : base(parser, parent, ParseToken)
        { }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            base.Resolve(ResolveCategory.Constructor, flags);
            return this;
        }

        protected override void ResolveInvokedExpression(ResolveCategory resolveCategory, ResolveFlags flags, out SymbolicRef oldInvokedRef, out SymbolicRef newInvokedRef)
        {
            // Resolve the invoked (called) expression
            if (_expression != null)
            {
                oldInvokedRef = _expression.SkipPrefixes() as SymbolicRef;

                // Special handling for ": this()" on a struct, since it has no default constructor (it's implicit)
                if (ArgumentCount == 0 && _parent is ConstructorDecl && _parent.Parent != null && _parent.Parent is StructDecl)
                    _expression = _parent.Parent.CreateRef();
                else
                    _expression = (Expression)_expression.Resolve(ResolveCategory.Constructor, flags);

                newInvokedRef = _expression.SkipPrefixes() as SymbolicRef;
            }
            else
                oldInvokedRef = newInvokedRef = null;
        }

        #endregion
    }
}
