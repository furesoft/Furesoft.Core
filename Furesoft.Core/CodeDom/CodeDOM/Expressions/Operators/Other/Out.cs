// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Allows a variable being passed as a parameter to be marked as an 'out' parameter.
    /// This is a special pseudo-operator that is only for use in this special case.
    /// </summary>
    public class Out : RefOutOperator
    {
        /// <summary>
        /// Create an <see cref="Out"/> operator for the specified parameter expression.
        /// </summary>
        /// <param name="variable">An expression that evaluates to a <see cref="VariableRef"/>.</param>
        public Out(Expression variable)
            : base(variable)
        { }

        /// <summary>
        /// Create an <see cref="Out"/> operator for the specified <see cref="VariableDecl"/>.
        /// </summary>
        /// <param name="variableDecl">The <see cref="VariableDecl"/> being passed as a parameter (a reference to it will be created).</param>
        public Out(VariableDecl variableDecl)
            : base(variableDecl)
        { }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ParameterDecl.ParseTokenOut;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 920;

        protected Out(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.NotAType);
        }

        /// <summary>
        /// Parse an <see cref="Out"/> operator.
        /// </summary>
        public static Out Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Out(parser, parent);
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}