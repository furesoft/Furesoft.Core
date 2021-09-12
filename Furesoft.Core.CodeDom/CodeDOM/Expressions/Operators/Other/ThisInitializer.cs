// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
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
