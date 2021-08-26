// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a call to another constructor in the same class (constructor initializer).
    /// </summary>
    public class ThisInitializer : ConstructorInitializer
    {
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
        public const string ParseToken = ThisRef.ParseToken;

        /// <summary>
        /// Parse a <see cref="ThisInitializer"/> operator.
        /// </summary>
        public ThisInitializer(Parser parser, CodeObject parent)
            : base(parser, parent, ParseToken)
        { }
    }
}