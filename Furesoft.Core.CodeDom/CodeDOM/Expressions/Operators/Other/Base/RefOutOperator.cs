// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of the <see cref="Ref"/> and <see cref="Out"/> pseudo-operators.
    /// </summary>
    public abstract class RefOutOperator : SingleArgumentOperator
    {
        /// <summary>
        /// Create a Ref/Out operator instance.
        /// </summary>
        /// <param name="variable">An expression that evaluates to a <see cref="VariableRef"/>.</param>
        protected RefOutOperator(Expression variable)
            : base(variable)
        { }

        /// <summary>
        /// Create a Ref/Out operator instance.
        /// </summary>
        protected RefOutOperator(VariableDecl variableDecl)
            : base(variableDecl.CreateRef())
        { }

        protected RefOutOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the argument has parens around it.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }
    }
}