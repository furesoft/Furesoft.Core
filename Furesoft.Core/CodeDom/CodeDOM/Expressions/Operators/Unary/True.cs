// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Determines if an expression evaluates to true.
    /// </summary>
    public class True : PreUnaryOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "True";

        /// <summary>
        /// Create a <see cref="True"/> operator.
        /// </summary>
        public True(Expression expression)
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
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "true";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}