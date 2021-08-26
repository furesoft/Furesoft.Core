// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="ExternAlias"/> namespace statement, or
    /// to the 'global' namespace.
    /// </summary>
    public class ExternAliasRef : SymbolicRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ExternAliasRef"/>.
        /// </summary>
        public ExternAliasRef(ExternAlias externAlias, bool isFirstOnLine)
            : base(externAlias, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="ExternAliasRef"/>.
        /// </summary>
        public ExternAliasRef(ExternAlias externAlias)
            : base(externAlias, false)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="ExternAliasRef"/>.
        /// </summary>
        public override string Name
        {
            get { return ((ExternAlias)_reference).Name; }
        }

        /// <summary>
        /// The root-level namespace associated with the extern alias statement.
        /// </summary>
        public RootNamespace RootNamespace
        {
            get { return ((ExternAlias)_reference).RootNamespaceRef.Reference as RootNamespace; }
        }

        /// <summary>
        /// True if the extern alias is 'global'.
        /// </summary>
        public bool IsGlobal
        {
            get { return ((ExternAlias)_reference).IsGlobal; }
        }

        /// <summary>
        /// The referenced ExternAlias code object.
        /// </summary>
        public ExternAlias ExternAlias
        {
            get { return (ExternAlias)_reference; }
        }

        #endregion
    }
}
