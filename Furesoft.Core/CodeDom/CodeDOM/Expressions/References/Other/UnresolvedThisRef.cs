// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// This is a special class used for explicit interface implementations of indexers, and also
    /// for an explicit reference to an indexer member of a type (these are normally implicit, but explicit
    /// references are legal in doc comments).
    /// </summary>
    /// <remarks>
    /// It has a Name of "Item", but it displays as "this".  When resolved, it is replaced with an <see cref="IndexerRef"/>.
    /// </remarks>
    public class UnresolvedThisRef : UnresolvedRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="UnresolvedThisRef"/>.
        /// </summary>
        public UnresolvedThisRef(bool isFirstOnLine)
            : base(IndexerDecl.IndexerName, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedThisRef"/>.
        /// </summary>
        public UnresolvedThisRef()
            : base(IndexerDecl.IndexerName, false)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedThisRef"/>.
        /// </summary>
        protected internal UnresolvedThisRef(ThisRef thisRef)
            : this(thisRef.IsFirstOnLine)
        {
            SetLineCol(thisRef);
        }

        /// <summary>
        /// Create an <see cref="UnresolvedThisRef"/>.
        /// </summary>
        protected internal UnresolvedThisRef(Token token)
            : this(token.IsFirstOnLine)
        {
            SetLineCol(token);
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(ThisRef.ParseToken);
            AsTextTypeArguments(writer, _typeArguments, flags);
            AsTextArrayRanks(writer, flags);
        }

        #endregion
    }
}
