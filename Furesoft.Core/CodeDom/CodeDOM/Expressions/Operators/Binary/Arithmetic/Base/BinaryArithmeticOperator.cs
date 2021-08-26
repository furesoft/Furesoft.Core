// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// The common base class of all binary arithmetic operators (<see cref="Add"/>, <see cref="Subtract"/>, <see cref="Multiply"/>, <see cref="Divide"/>, <see cref="Mod"/>).
    /// </summary>
    public abstract class BinaryArithmeticOperator : BinaryOperator
    {
        protected BinaryArithmeticOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected BinaryArithmeticOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }  // Default to NO parens for binary arithmetic operators
        }
    }
}