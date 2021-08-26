// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// This interface is implemented by <see cref="MultiFieldDecl"/>, <see cref="MultiLocalDecl"/>, <see cref="MultiEnumMemberDecl"/>.
    /// </summary>
    public interface IMultiVariableDecl : IEnumerable<VariableDecl>
    {
        /// <summary>
        /// The number of <see cref="VariableDecl"/>s.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get the <see cref="VariableDecl"/> at the specified index.
        /// </summary>
        VariableDecl this[int index] { get; }
    }
}