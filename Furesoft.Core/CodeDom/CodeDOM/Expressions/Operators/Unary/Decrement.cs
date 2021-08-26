// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Decrements an <see cref="Expression"/>, which should evaluate to a <see cref="VariableRef"/> (or a property or indexer access).
    /// </summary>
    public class Decrement : PreUnaryOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Decrement";

        /// <summary>
        /// Create a <see cref="Decrement"/> operator.
        /// </summary>
        public Decrement(Expression expression)
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
        public const string ParseToken = "--";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        protected Decrement(Parser parser, CodeObject parent)
            : base(parser, parent, false)
        { }

        /// <summary>
        /// Parse a <see cref="Decrement"/> operator.
        /// </summary>
        public static Decrement Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If we have an unused left expression, abort
            // (this is to give the PostDecrement operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return null;
            return new Decrement(parser, parent);
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
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, true, Parse);
        }
    }
}