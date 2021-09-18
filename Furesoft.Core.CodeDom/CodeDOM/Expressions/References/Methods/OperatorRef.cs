// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="OperatorDecl"/> or a <see cref="MethodInfo"/> for an overloaded operator.
    /// </summary>
    public class OperatorRef : MethodRef
    {
        /// <summary>
        /// Create an <see cref="OperatorRef"/> from an <see cref="OperatorDecl"/>.
        /// </summary>
        public OperatorRef(OperatorDecl methodBase, bool isFirstOnLine)
            : base(methodBase, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OperatorRef"/> from an <see cref="OperatorDecl"/>.
        /// </summary>
        public OperatorRef(OperatorDecl methodBase)
            : base(methodBase, false)
        { }

        /// <summary>
        /// Create an <see cref="OperatorRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public OperatorRef(MethodInfo methodInfo, bool isFirstOnLine)
            : base(methodInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OperatorRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public OperatorRef(MethodInfo methodInfo)
            : base(methodInfo, false)
        { }

        // OperatorRefs should only be rendered as a Description (not directly), and they never have type arguments.
        // Descriptions are handled in SymbolicRef rendering, so there's no need to do anything here - the MethodRef
        // base will render the name if an OperatorRef is rendered not in Description mode for some reason.
    }
}