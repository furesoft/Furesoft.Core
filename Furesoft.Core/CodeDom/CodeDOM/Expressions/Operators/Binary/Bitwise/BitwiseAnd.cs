// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Performs a boolean AND operation on two <see cref="Expression"/>s.
    /// </summary>
    public class BitwiseAnd : BinaryBitwiseOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "BitwiseAnd";

        /// <summary>
        /// Create a <see cref="BitwiseAnd"/> operator.
        /// </summary>
        public BitwiseAnd(Expression left, Expression right)
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
        /// The internal name of the <see cref="BinaryOperator"/>.
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
        public const string ParseToken = "&";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 350;

        protected BitwiseAnd(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="BitwiseAnd"/> operator.
        /// </summary>
        public static BitwiseAnd Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the AddressOf operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new BitwiseAnd(parser, parent);
            return null;
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