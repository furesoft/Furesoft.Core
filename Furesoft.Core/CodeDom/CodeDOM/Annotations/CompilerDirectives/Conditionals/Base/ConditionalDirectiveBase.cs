// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/>, <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, and <see cref="EndIfDirective"/>.
    /// </summary>
    public class ConditionalDirectiveBase : CompilerDirective
    {
        protected ConditionalDirectiveBase()
        { }

        protected ConditionalDirectiveBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}