// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Mono.Cecil;
using Furesoft.Core.CodeDom.Utilities.Mono.Cecil;

namespace Furesoft.Core.CodeDom.Utilities.Mono.Cecil
{
    /// <summary>
    /// Static helper methods for <see cref="ParameterDefinition"/>.
    /// </summary>
    public static class ParameterDefinitionUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// The name of the 'params' attribute.
        /// </summary>
        public const string ParamArrayAttributeName = "ParamArrayAttribute";

        /// <summary>
        /// Check if the ParameterDefinition has the 'params' modifier.
        /// </summary>
        public static bool IsParams(ParameterDefinition parameterDefinition)
        {
            return ICustomAttributeProviderUtil.HasCustomAttribute(parameterDefinition, ParamArrayAttributeName);
        }

        /// <summary>
        /// Check if the ParameterDefinition has the 'ref' modifier.
        /// </summary>
        public static bool IsRef(ParameterDefinition parameterDefinition)
        {
            // A 'ref' will generally NOT have the "[In]" and "[Out]" attributes, so assume 'ref' for an IsByRef type where
            // the parameter is NOT an 'out'.
            return (parameterDefinition.ParameterType.IsByReference && !(parameterDefinition.IsOut && !parameterDefinition.IsIn));
        }

        /// <summary>
        /// Check if the ParameterDefinition has the 'out' modifier.
        /// </summary>
        public static bool IsOut(ParameterDefinition parameterDefinition)
        {
            // The IsIn and IsOut properties reflect the "[In]" and "[Out]" attributes, NOT the C# 'ref' and 'out' keywords.
            // An 'out' parameter will always have an IsByRef type, and should always have the "[Out]" attribute to distinguish
            // it from a 'ref'.  To be thorough, also ensure that there isn't an "[In]" attribute.
            return (parameterDefinition.ParameterType.IsByReference && parameterDefinition.IsOut && !parameterDefinition.IsIn);
        }

        /// <summary>
        /// Get the category name.
        /// </summary>
        public static string GetCategory(ParameterDefinition parameterDefinition)
        {
            return "parameter";
        }

        #endregion
    }
}
