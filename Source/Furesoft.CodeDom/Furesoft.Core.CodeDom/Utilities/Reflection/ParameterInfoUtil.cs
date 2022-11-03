// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Linq;
using System.Reflection;

namespace Furesoft.Core.CodeDom.Utilities.Reflection;

/// <summary>
/// Static helper methods for <see cref="ParameterInfo"/>.
/// </summary>
public static class ParameterInfoUtil
{
    #region /* STATIC HELPER METHODS */

    /// <summary>
    /// The name of the 'params' attribute.
    /// </summary>
    public const string ParamArrayAttributeName = "ParamArrayAttribute";

    /// <summary>
    /// Check if the ParameterInfo has the 'params' modifier.
    /// </summary>
    public static bool IsParams(ParameterInfo parameterInfo)
    {
        return HasCustomAttribute(parameterInfo, ParamArrayAttributeName);
    }

    /// <summary>
    /// Check if the ParameterInfo has the 'ref' modifier.
    /// </summary>
    public static bool IsRef(ParameterInfo parameterInfo)
    {
        // A 'ref' will generally NOT have the "[In]" and "[Out]" attributes, so assume 'ref' for an IsByRef type where
        // the parameter is NOT an 'out'.
        return (parameterInfo.ParameterType.IsByRef && !(parameterInfo.IsOut && !parameterInfo.IsIn));
    }

    /// <summary>
    /// Check if the ParameterInfo has the 'out' modifier.
    /// </summary>
    public static bool IsOut(ParameterInfo parameterInfo)
    {
        // The IsIn and IsOut properties reflect the "[In]" and "[Out]" attributes, NOT the C# 'ref' and 'out' keywords.
        // An 'out' parameter will always have an IsByRef type, and should always have the "[Out]" attribute to distinguish
        // it from a 'ref'.  To be thorough, also ensure that there isn't an "[In]" attribute.
        return (parameterInfo.ParameterType.IsByRef && parameterInfo.IsOut && !parameterInfo.IsIn);
    }

    /// <summary>
    /// Determine if a ParameterInfo has a custom attribute.
    /// </summary>
    /// <remarks>
    /// This method must be used instead of the built-in GetCustomAttributes() call when working with
    /// members of reflection-only assemblies (otherwise, the custom attribute type would be instantiated,
    /// which is illegal).
    /// </remarks>
    public static bool HasCustomAttribute(ParameterInfo parameterInfo, string name)
    {
        return Enumerable.Any(CustomAttributeData.GetCustomAttributes(parameterInfo),
            delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; });
    }

    /// <summary>
    /// Get the category name.
    /// </summary>
    public static string GetCategory(ParameterInfo parameterInfo)
    {
        return "parameter";
    }

    #endregion
}
