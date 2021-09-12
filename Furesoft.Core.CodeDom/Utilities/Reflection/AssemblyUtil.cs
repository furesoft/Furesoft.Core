// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Furesoft.Core.CodeDom.Utilities.Reflection
{
    /// <summary>
    /// Static helper methods for <see cref="Assembly"/>.
    /// </summary>
    public static class AssemblyUtil
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The name of the version part of a display name.
        /// </summary>
        public const string VersionTag = "Version=";

        /// <summary>
        /// The name of the internals-visible-to attribute.
        /// </summary>
        public const string InternalsVisibleToAttributeName = "InternalsVisibleToAttribute";

        #endregion

        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Determine if the specified assembly name is a display name.
        /// </summary>
        public static bool IsDisplayName(string assemblyName)
        {
            return assemblyName.Contains(",");
        }

        /// <summary>
        /// Get a normalized version of the specified assembly display name, including the name, Version (if any),
        /// Culture (if any), PublicKeyToken (null if none), and processorArchitecture (if not the default MSIL).
        /// </summary>
        public static string GetNormalizedDisplayName(string assemblyName)
        {
            AssemblyName name = new AssemblyName(assemblyName);
            string result = name.FullName;
            // Remove any PublicKeyToken tag if the value is null (assembly references might omit it)
            const string nullKey = ", PublicKeyToken=null";
            int index = result.IndexOf(nullKey);
            return (index > 0 ? result.Remove(index, nullKey.Length) : result);
        }

        /// <summary>
        /// Determine if the specified assembly name has a version number.
        /// </summary>
        public static bool HasVersion(string assemblyName)
        {
            return assemblyName.Contains(VersionTag);
        }

        /// <summary>
        /// Get the version number from an assembly display name.
        /// </summary>
        public static string GetVersion(string assemblyName)
        {
            int versionIndex = assemblyName.IndexOf(VersionTag);
            if (versionIndex > 0)
            {
                versionIndex += VersionTag.Length;
                int commaIndex = assemblyName.IndexOf(",", versionIndex);
                if (commaIndex < versionIndex)
                    commaIndex = assemblyName.Length;
                return assemblyName.Substring(versionIndex, commaIndex - versionIndex);
            }
            return null;
        }

        /// <summary>
        /// Determine if an <see cref="Assembly"/> has a custom attribute.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// reflection-only assemblies (otherwise, the custom attribute type would be instantiated, which is illegal).
        /// </remarks>
        public static bool HasCustomAttribute(Assembly assembly, string name)
        {
            return Enumerable.Any(CustomAttributeData.GetCustomAttributes(assembly),
                delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; });
        }

        /// <summary>
        /// Get all custom attributes with the specified name from the <see cref="Assembly"/>.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// reflection-only assemblies (otherwise, the custom attribute type would be instantiated, which is illegal).
        /// </remarks>
        public static List<CustomAttributeData> GetCustomAttributes(Assembly assembly, string name)
        {
            return Enumerable.ToList(Enumerable.Where(CustomAttributeData.GetCustomAttributes(assembly),
                delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; }));
        }

        /// <summary>
        /// Get the custom attributewith the specified name from the <see cref="Assembly"/>.  If there are multiple attributes with the name, the first one is returned.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// reflection-only assemblies (otherwise, the custom attribute type would be instantiated, which is illegal).
        /// </remarks>
        public static CustomAttributeData GetCustomAttribute(Assembly assembly, string name)
        {
            return Enumerable.FirstOrDefault(CustomAttributeData.GetCustomAttributes(assembly),
                delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; });
        }

        #endregion
    }
}
