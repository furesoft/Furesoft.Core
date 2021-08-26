// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Returns the system type object for the specified type.
    /// </summary>
    public class TypeOf : TypeOperator
    {
        /// <summary>
        /// Create a <see cref="TypeOf"/> operator - the expression must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        public TypeOf(Expression type)
            : base(type)
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
        public const string ParseToken = "typeof";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        protected TypeOf(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.Type);
        }

        /// <summary>
        /// Parse a <see cref="TypeOf"/> operator.
        /// </summary>
        public static TypeOf Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new TypeOf(parser, parent);
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