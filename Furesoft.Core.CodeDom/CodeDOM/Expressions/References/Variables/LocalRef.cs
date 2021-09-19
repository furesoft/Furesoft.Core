using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables.Base;
using Nova.CodeDOM;

// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables
{
    /// <summary>
    /// Represents a reference to a <see cref="LocalDecl"/>.
    /// </summary>
    public class LocalRef : VariableRef
    {
        /// <summary>
        /// Create a <see cref="LocalRef"/>.
        /// </summary>
        public LocalRef(LocalDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="LocalRef"/>.
        /// </summary>
        public LocalRef(LocalDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// True if the referenced <see cref="LocalDecl"/> is const.
        /// </summary>
        public override bool IsConst
        {
            get { return ((LocalDecl)_reference).IsConst; }
        }
    }
}