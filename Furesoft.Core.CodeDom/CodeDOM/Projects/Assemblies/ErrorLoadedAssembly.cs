using Furesoft.Core.CodeDom.CodeDOM.Projects.Assemblies;
// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM.Projects.Assemblies
{
    /// <summary>
    /// Represents an assembly that failed to load.
    /// </summary>
    public class ErrorLoadedAssembly : LoadedAssembly
    {
        #region /* FIELDS */

        protected string _errorMessage;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ErrorLoadedAssembly"/>.
        /// </summary>
        public ErrorLoadedAssembly(string errorMessage)
            : base(false)
        {
            _errorMessage = errorMessage;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The error message associated with this <see cref="LoadedAssembly"/> (if any).
        /// </summary>
        public string ErrorMessage { get { return _errorMessage; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get a string description of this <see cref="LoadedAssembly"/>.
        /// </summary>
        public override string ToString()
        {
            return _errorMessage;
        }

        #endregion
    }
}
