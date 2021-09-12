// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Mono.Cecil;

namespace Furesoft.Core.CodeDom.Utilities.Mono.Cecil
{
    /// <summary>
    /// Static helper methods for <see cref="GenericParameter"/>.
    /// </summary>
    public static class GenericParameterUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Get the category name.
        /// </summary>
        public static string GetCategory(GenericParameter thisGenericParameter)
        {
            return "type parameter";
        }

        #endregion
    }
}
