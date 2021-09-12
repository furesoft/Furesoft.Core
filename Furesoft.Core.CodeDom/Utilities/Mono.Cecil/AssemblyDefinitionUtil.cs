// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Furesoft.Core.CodeDom.Utilities.Mono.Cecil
{
    /// <summary>
    /// Static helper methods for <see cref="AssemblyDefinition"/>.
    /// </summary>
    public static class AssemblyDefinitionUtil
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
            return assemblyName.Contains(VersionTag);
        }

        /// <summary>
        /// Get a shorter display name for an assembly, truncating anything after the PublicKeyToken,
        /// such as Culture and processorArchitecture.
        /// </summary>
        public static string GetShortDisplayName(string assemblyName)
        {
            const string publicKey = "PublicKeyToken=";
            int keyIndex = assemblyName.IndexOf(publicKey);
            int endIndex = assemblyName.IndexOf(',', keyIndex + publicKey.Length);
            if (endIndex > 0)
                assemblyName = assemblyName.Substring(0, endIndex);
            return assemblyName;
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
        /// Determine if an <see cref="AssemblyDefinition"/> has a custom attribute.
        /// </summary>
        public static bool HasCustomAttribute(AssemblyDefinition assemblyDefinition, string name)
        {
            return Enumerable.Any(assemblyDefinition.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; });
        }

        /// <summary>
        /// Get all custom attributes with the specified name from the <see cref="AssemblyDefinition"/>.
        /// </summary>
        public static List<CustomAttribute> GetCustomAttributes(AssemblyDefinition assemblyDefinition, string name)
        {
            return Enumerable.ToList(Enumerable.Where(assemblyDefinition.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; }));
        }

        /// <summary>
        /// Get the custom attributewith the specified name from the <see cref="AssemblyDefinition"/>.  If there are multiple attributes with the name, the first one is returned.
        /// </summary>
        public static CustomAttribute GetCustomAttribute(AssemblyDefinition assemblyDefinition, string name)
        {
            return Enumerable.FirstOrDefault(assemblyDefinition.CustomAttributes, delegate(CustomAttribute customAttribute) { return customAttribute.Constructor.DeclaringType.Name == name; });
        }

        #endregion
    }
}
