// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Nova.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="ICustomAttributeProvider"/>.
    /// </summary>
    public static class ICustomAttributeProviderUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Determine if the ICustomAttributeProvider has a custom attribute.
        /// </summary>
        public static bool HasCustomAttribute(ICustomAttributeProvider customAttributeProvider, string name)
        {
            return Enumerable.Any(customAttributeProvider.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; });
        }

        /// <summary>
        /// Get all custom attributes with the specified name from the ICustomAttributeProvider.
        /// </summary>
        public static List<CustomAttribute> GetCustomAttributes(ICustomAttributeProvider customAttributeProvider, string name)
        {
            return Enumerable.ToList(Enumerable.Where(customAttributeProvider.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; }));
        }

        /// <summary>
        /// Get the custom attribute with the specified name from the ICustomAttributeProvider.  If there are multiple attributes with the name, the first one is returned.
        /// </summary>
        public static CustomAttribute GetCustomAttribute(ICustomAttributeProvider customAttributeProvider, string name)
        {
            return Enumerable.FirstOrDefault(customAttributeProvider.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; });
        }

        #endregion
    }
}
