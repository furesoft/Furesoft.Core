// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Negates an <see cref="Expression"/>.
    /// </summary>
    public class Negative : PreUnaryOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "UnaryNegation";

        /// <summary>
        /// Create a <see cref="Negative"/> operator.
        /// </summary>
        public Negative(Expression expression)
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
        /// The internal name of the <see cref="UnaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return InternalName;
        }

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = Subtract.ParseToken;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        protected Negative(Parser parser, CodeObject parent)
            : base(parser, parent, false)
        { }

        /// <summary>
        /// Parse a <see cref="Negative"/> operator.
        /// </summary>
        public static Negative Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Negative(parser, parent);
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
            // Use a parse-priority of 100 (Subtract uses 0)
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, true, Parse);
        }
    }
}