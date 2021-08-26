// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a user-overloaded 'true' or 'false' operator.
    /// </summary>
    /// <remarks>
    /// If a 'true' or 'false' operator is overloaded, then the other one must also be overloaded.
    /// </remarks>
    public class BooleanOperatorDecl : OperatorDecl
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="BooleanOperatorDecl"/>.
        /// </summary>
        public BooleanOperatorDecl(string symbol, Modifiers modifiers, CodeObject body, ParameterDecl parameter)
            : base(symbol, (TypeRef)TypeRef.BoolRef.Clone(), modifiers, body, new[] { parameter })
        { }

        /// <summary>
        /// Create a <see cref="BooleanOperatorDecl"/>.
        /// </summary>
        public BooleanOperatorDecl(string symbol, Modifiers modifiers, ParameterDecl parameter)
            : base(symbol, (TypeRef)TypeRef.BoolRef.Clone(), modifiers, new[] { parameter })
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse a <see cref="BooleanOperatorDecl"/>.
        /// </summary>
        public BooleanOperatorDecl(Parser parser, CodeObject parent, ParseFlags flags)
            : base(parser, parent, true, flags)
        { }

        #endregion
    }
}
