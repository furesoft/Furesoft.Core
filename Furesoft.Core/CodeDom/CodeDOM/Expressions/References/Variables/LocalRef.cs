// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="LocalDecl"/>.
    /// </summary>
    public class LocalRef : VariableRef
    {
        #region /* CONSTRUCTORS */

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

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if the referenced <see cref="LocalDecl"/> is const.
        /// </summary>
        public override bool IsConst
        {
            get { return ((LocalDecl)_reference).IsConst; }
        }

        #endregion

        #region /* METHODS */

        #endregion
    }
}
