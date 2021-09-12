// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Derives from a Mono Cecil <see cref="BaseAssemblyResolver"/> in order to resolve assemblies on demand
    /// in the scope of a certain <see cref="Project"/>.
    /// </summary>
    public class MonoCecilAssemblyResolver : BaseAssemblyResolver
    {
        #region /* FIELDS */

        /// <summary>
        /// The <see cref="Project"/> that this assembly resolver is associated with.
        /// </summary>
        protected Project _project;

        /// <summary>
        /// Cache for assemblies that fail to resolve.
        /// </summary>
        protected HashSet<string> _failedToLoad = new HashSet<string>();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="MonoCecilAssemblyResolver"/>.
        /// </summary>
        public MonoCecilAssemblyResolver(Project project)
        {
            _project = project;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The associated <see cref="Project"/> object.
        /// </summary>
        public Project Project { get { return _project; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Resolve the requested assembly name in the current <see cref="Project"/> scope.
        /// </summary>
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            AssemblyDefinition assembly = null;

            // Only allow one thread to do this at a time for the same Project
            lock (_project)
            {
                // Attempt to load the assembly using the Project's FrameworkContext
                string fullName = name.FullName;
                string errorMessage;
                LoadedAssembly loadedAssembly = _project.LoadAssembly(fullName, null, out errorMessage, null);
                if (loadedAssembly is MonoCecilLoadedAssembly)
                    assembly = ((MonoCecilLoadedAssembly)loadedAssembly).AssemblyDefinition;

                // As a fallback, let Mono try to give it a shot, but don't try if we've already done so (to avoid too many exceptions)
                if (assembly == null && !_failedToLoad.Contains(fullName))
                {
                    try
                    {
                        assembly = base.Resolve(name);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, "loading assembly '" + fullName + "'");
                    }
                    if (assembly != null)
                        Log.WriteLine("WARNING: Assembly was loaded by fallback to Mono loader: " + fullName);
                    else
                        _failedToLoad.Add(fullName);
                }
            }

            return assembly;
        }

        /// <summary>
        /// Reset any cached data regarding assemblies that failed to resolve.
        /// </summary>
        public void Reset()
        {
            _failedToLoad.Clear();
        }

        #endregion
    }
}
