// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an <see cref="AssemblyDefinition"/> that has been loaded into memory using Mono.Cecil.
    /// </summary>
    public class MonoCecilLoadedAssembly : LoadedAssembly
    {
        #region /* FIELDS */

        protected AssemblyDefinition _assemblyDefinition;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="MonoCecilLoadedAssembly"/>.
        /// </summary>
        public MonoCecilLoadedAssembly(AssemblyDefinition assemblyDefinition, bool isFrameworkAssembly)
            : base(isFrameworkAssembly)
        {
            _isFrameworkAssembly = isFrameworkAssembly;
            _assemblyDefinition = assemblyDefinition;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The associated <see cref="AssemblyDefinition"/> object.
        /// </summary>
        public AssemblyDefinition AssemblyDefinition { get { return _assemblyDefinition; } }

        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        public override string FullName { get { return AssemblyUtil.GetNormalizedDisplayName(_assemblyDefinition.Name.FullName); } }

        /// <summary>
        /// The location of the assembly.
        /// </summary>
        public override string Location { get { return _assemblyDefinition.MainModule.FullyQualifiedName; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine if internal types in the assembly are visible to the specified project.
        /// </summary>
        public override bool AreInternalTypesVisibleTo(Project project)
        {
            List<CustomAttribute> internalsVisibleToAttributes = AssemblyDefinitionUtil.GetCustomAttributes(_assemblyDefinition, AssemblyUtil.InternalsVisibleToAttributeName);
            if (internalsVisibleToAttributes != null)
            {
                foreach (CustomAttribute internalsVisibleToAttribute in internalsVisibleToAttributes)
                {
                    string argument = internalsVisibleToAttribute.ConstructorArguments[0].Value.ToString();
                    if (argument == project.Name || argument.StartsWith(project.Name + ","))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the version of the assembly.
        /// </summary>
        public override string GetVersion()
        {
            return AssemblyUtil.GetVersion(_assemblyDefinition.Name.FullName);
        }

        /// <summary>
        /// Get the type definitions for this assembly.
        /// </summary>
        public IEnumerable<TypeDefinition> GetTypes(bool includePrivateTypes)
        {
            return (includePrivateTypes ? Enumerable.Where(_assemblyDefinition.MainModule.GetTypes(), delegate(TypeDefinition typeDefinition) { return !typeDefinition.IsNested; })
                : Enumerable.Where(_assemblyDefinition.MainModule.GetTypes(), delegate(TypeDefinition typeDefinition) { return typeDefinition.IsPublic && !typeDefinition.IsNested; }));
        }

        /// <summary>
        /// Load all of the (non-nested) type definitions in the assembly into the appropriate namespaces under
        /// the specified root namespace.
        /// </summary>
        public override int LoadTypes(bool includePrivateTypes, RootNamespace rootNamespace, out string errorMessage, HashSet<string> hideTypes)
        {
            int typeCount = 0;
            errorMessage = null;
            try
            {
                Log.DetailWriteLine("\tLoading types for: " + FullName);

                // Get all of the public types and iterate through them.  Optionally load private types in
                // order to detect illegal references to them (or their namespaces) during analysis.
                IEnumerable<TypeDefinition> types = GetTypes(includePrivateTypes);
                foreach (TypeDefinition typeDefinition in types)
                {
                    // Find the namespace by its full name, or create it if it doesn't exist
                    string namespaceFullName = typeDefinition.Namespace;
                    Namespace @namespace = (string.IsNullOrEmpty(namespaceFullName) ? rootNamespace : rootNamespace.FindOrCreateChildNamespace(namespaceFullName));

                    // Add the type to the namespace
                    @namespace.Add(typeDefinition);
                    ++typeCount;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "EXCEPTION loading types from '" + FullName + "': " + ex.Message;
            }
            return typeCount;
        }

        /// <summary>
        /// Get a string description of this <see cref="LoadedAssembly"/>.
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}
