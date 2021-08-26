// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Explicitly turns ON overflow checking (exceptions) for an <see cref="Expression"/>.
    /// </summary>
    public class Checked : CheckedOperator
    {
        /// <summary>
        /// Create a <see cref="Checked"/> operator with the specified <see cref="Expression"/>.
        /// </summary>
        public Checked(Expression expression)
            : base(expression)
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
        public const string ParseToken = "checked";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        protected Checked(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.NotAType);
        }

        /// <summary>
        /// Parse a <see cref="Checked"/> operator.
        /// </summary>
        public static Checked Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Checked(parser, parent);
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
            // Use a parse-priority of 100 (CheckedBlock uses 0)
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, false, Parse);
        }
    }
}