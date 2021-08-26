// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a call to a constructor in the base class (constructor initializer).
    /// </summary>
    public class BaseInitializer : ConstructorInitializer
    {
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

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

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
    }
}