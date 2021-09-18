// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;
using System.Reflection;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="IndexerDecl"/> or a <see cref="PropertyInfo"/> for an indexer.
    /// </summary>
    public class IndexerRef : PropertyRef
    {
        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(IndexerDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(IndexerDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(PropertyInfo propertyInfo, bool isFirstOnLine)
            : base(propertyInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(PropertyInfo propertyInfo)
            : base(propertyInfo, false)
        { }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(IndexerDecl.ParseToken);
        }
    }
}