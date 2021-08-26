// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Performs a conditional (logical) 'or' of two boolean <see cref="Expression"/>s.
    /// </summary>
    public class Or : BinaryBooleanOperator
    {
        /// <summary>
        /// Create an <see cref="Or"/> operator.
        /// </summary>
        public Or(Expression left, Expression right)
            : base(left, right)
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
        public const string ParseToken = "||";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 385;

        protected Or(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse an <see cref="Or"/> operator.
        /// </summary>
        public static Or Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Or(parser, parent);
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