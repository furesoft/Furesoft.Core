// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="ThisRef"/> and <see cref="BaseRef"/>.
    /// </summary>
    public abstract class SelfRef : SymbolicRef
    {
        protected SelfRef(bool isFirstOnLine)
            : base((INamedCodeObject)null, isFirstOnLine)
        { }

        protected SelfRef()
            : base((INamedCodeObject)null, false)
        { }

        protected SelfRef(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// The keyword associated with the <see cref="SelfRef"/>.
        /// </summary>
        public abstract string Keyword
        {
            get;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(Keyword);
        }
    }
}