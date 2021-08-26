// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// The common base class of the <see cref="Checked"/> and <see cref="Unchecked"/> operators.
    /// </summary>
    public abstract class CheckedOperator : SingleArgumentOperator
    {
        protected CheckedOperator(Expression expression)
            : base(expression)
        { }

        protected CheckedOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}