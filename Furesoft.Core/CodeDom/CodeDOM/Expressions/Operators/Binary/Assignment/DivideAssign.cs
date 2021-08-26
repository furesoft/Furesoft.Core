// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Divides one <see cref="Expression"/> by another, and assigns the result to the left <see cref="Expression"/>.
    /// The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </summary>
    public class DivideAssign : Assignment
    {
        /// <summary>
        /// Create a <see cref="DivideAssign"/> operator.
        /// </summary>
        public DivideAssign(Expression left, Expression right)
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
            return Divide.InternalName;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "/=";

        protected DivideAssign(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="DivideAssign"/> operator.
        /// </summary>
        public static new DivideAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DivideAssign(parser, parent);
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}