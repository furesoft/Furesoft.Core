// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a call to a constructor in the base class (constructor initializer).
    /// </summary>
    public class BaseInitializer : ConstructorInitializer
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="BaseInitializer"/> operator.
        /// </summary>
        public BaseInitializer(SymbolicRef symbolicRef, params Expression[] parameters)
            : base(symbolicRef, parameters)
        { }

        /// <summary>
        /// Create a <see cref="BaseInitializer"/> operator.
        /// </summary>
        public BaseInitializer(ConstructorRef constructorRef, params Expression[] parameters)
            : base(constructorRef, parameters)
        { }

        /// <summary>
        /// Create a <see cref="BaseInitializer"/> operator.
        /// </summary>
        public BaseInitializer(ConstructorDecl constructorDecl, params Expression[] parameters)
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
        public const string ParseToken = BaseRef.ParseToken;

        /// <summary>
        /// Parse a <see cref="BaseInitializer"/> operator.
        /// </summary>
        public BaseInitializer(Parser parser, CodeObject parent)
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

        #endregion
    }
}
