// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an <see cref="Assembly"/> that has been loaded into memory.
    /// </summary>
    public abstract class LoadedAssembly
    {
        #region /* FIELDS */

        protected bool _isFrameworkAssembly;

        #endregion

        #region /* STATICS */

        public static LoadedAssembly Create(Assembly assembly, bool isFrameworkAssembly, bool isPreLoaded)
        {
            return new ReflectionLoadedAssembly(assembly, isFrameworkAssembly, isPreLoaded);
        }

        public static LoadedAssembly Create(Assembly assembly, bool isFrameworkAssembly)
        {
            return new ReflectionLoadedAssembly(assembly, isFrameworkAssembly, false);
        }

        public static LoadedAssembly Create(AssemblyDefinition assembly, bool isFrameworkAssembly)
        {
            return new MonoCecilLoadedAssembly(assembly, isFrameworkAssembly);
        }

        public static LoadedAssembly Create(string errorMessage)
        {
            return new ErrorLoadedAssembly(errorMessage);
        }

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="LoadedAssembly"/>.
        /// </summary>
        protected LoadedAssembly(bool isFrameworkAssembly)
        {
            _isFrameworkAssembly = isFrameworkAssembly;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if this is a framework assembly.
        /// </summary>
        public bool IsFrameworkAssembly { get { return _isFrameworkAssembly; } }

        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        public virtual string FullName { get { return null; } }

        /// <summary>
        /// The location of the assembly.
        /// </summary>
        public virtual string Location { get { return null; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine if internal types in the assembly are visible to the specified project.
        /// </summary>
        public virtual bool AreInternalTypesVisibleTo(Project project)
        {
            return false;
        }

        /// <summary>
        /// Get the version of the assembly.
        /// </summary>
        public virtual string GetVersion()
        {
            return null;
        }

        /// <summary>
        /// Load all of the (non-nested) type definitions in the assembly into the appropriate namespaces under
        /// the specified root namespace.
        /// </summary>
        /// <returns>An error message if an error occurs, otherwise null.</returns>
        public virtual int LoadTypes(bool includePrivateTypes, RootNamespace rootNamespace, out string errorMessage, HashSet<string> hideTypes)
        {
            errorMessage = null;
            return 0;
        }

        /// <summary>
        /// Load all of the (non-nested) type definitions in the assembly into the appropriate namespaces under
        /// the specified root namespace.
        /// </summary>
        /// <returns>An error message if an error occurs, otherwise null.</returns>
        public virtual int LoadTypes(bool includePrivateTypes, RootNamespace rootNamespace, out string errorMessage)
        {
            return LoadTypes(includePrivateTypes, rootNamespace, out errorMessage, null);
        }

        #endregion
    }
}
