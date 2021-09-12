// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an <see cref="Assembly"/> that has been loaded into memory using .NET reflection.
    /// </summary>
    public class ReflectionLoadedAssembly : LoadedAssembly
    {
        #region /* FIELDS */

        protected Assembly _assembly;
        protected bool _isPreLoaded;
        protected bool _isShared;
        protected Type[] _loadedTypes;
        protected List<CustomAttributeData> _internalsVisibleToAttributes;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ReflectionLoadedAssembly"/>.
        /// </summary>
        public ReflectionLoadedAssembly(Assembly assembly, bool isFrameworkAssembly, bool isPreLoaded)
            : base(isFrameworkAssembly)
        {
            _isFrameworkAssembly = isFrameworkAssembly;
            _assembly = assembly;
            _isPreLoaded = isPreLoaded;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The associated <see cref="Assembly"/> object.
        /// </summary>
        public Assembly Assembly { get { return _assembly; } }

        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        public override string FullName { get { return AssemblyUtil.GetNormalizedDisplayName(_assembly.FullName); } }

        /// <summary>
        /// The location of the assembly.
        /// </summary>
        public override string Location { get { return _assembly.Location; } }

        /// <summary>
        /// True if this assembly was pre-loaded.
        /// </summary>
        public bool IsPreLoaded { get { return _isPreLoaded; } }

        /// <summary>
        /// True if a pre-loaded assembly for the current app domain is "shared".
        /// </summary>
        public bool IsShared { get { return _isShared; } set { _isShared = value; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine if internal types in the assembly are visible to the specified project.
        /// </summary>
        public override bool AreInternalTypesVisibleTo(Project project)
        {
            // Cache this because it seems to take a long time for Reflection
            if (_internalsVisibleToAttributes == null)
                _internalsVisibleToAttributes = AssemblyUtil.GetCustomAttributes(_assembly, AssemblyUtil.InternalsVisibleToAttributeName);
            if (_internalsVisibleToAttributes != null)
            {
                foreach (CustomAttributeData internalsVisibleToAttribute in _internalsVisibleToAttributes)
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
            return AssemblyUtil.GetVersion(_assembly.FullName);
        }

        /// <summary>
        /// Get the types for this assembly.
        /// </summary>
        public Type[] GetTypes(bool includePrivateTypes)
        {
            if (_loadedTypes == null)
                _loadedTypes = (includePrivateTypes ? _assembly.GetTypes() : _assembly.GetExportedTypes());
            return _loadedTypes;
        }

        /// <summary>
        /// Load the specified type into the appropriate namespace under the specified root namespace.
        /// </summary>
        protected static void LoadType(Type type, RootNamespace rootNamespace)
        {
            // Find the namespace by its full name, or create it if it doesn't exist
            string namespaceFullName = type.Namespace;
            Namespace @namespace = (namespaceFullName == null ? rootNamespace : rootNamespace.FindOrCreateChildNamespace(namespaceFullName));

            // Add the type to the namespace
            @namespace.Add(type);
        }

        /// <summary>
        /// Load all of the (non-nested) types in the assembly into the appropriate namespaces under
        /// the specified root namespace, hiding any of the specified types.
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
                Type[] types;
                try
                {
                    types = GetTypes(includePrivateTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // If one or more types fail to load, log it, but still load the ones that worked
                    errorMessage = "EXCEPTION loading types from '" + FullName
                        + "': Unable to load one or more of the requested types.  LoaderException: " + ex.LoaderExceptions[0];
                    types = ex.Types;
                }
                foreach (Type type in types)
                {
                    if (type != null)  // Nulls can occur if types fail to load above
                    {
                        // Skip nested types (they will be discovered later via reflection on the parent type),
                        // and also any types being hidden for compatibility with old assemblies.
                        if (!type.IsNested && (hideTypes == null || !hideTypes.Contains(type.FullName ?? (type.Namespace + "." + type.Name))))
                        {
                            LoadType(type, rootNamespace);
                            ++typeCount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "EXCEPTION loading types from '" + FullName + "': " + ex.Message;

                if (ex is FileLoadException)
                {
                    // A file load exception probably means that we couldn't load a required dependent assembly
                    // while inside the GetExportedTypes() call, giving us zero types.  If we call GetTypes(),
                    // we'll get a TypeLoadException instead, along with a list of loaded types and exceptions.
                    try
                    {
                        // We know that OnReflectionOnlyAssemblyResolve is going to fire many times and usually
                        // fail, so force it to just return null and not log any errors.  We prevent tons of
                        // exceptions slowing things down by remembering assemblies that failed to load.
                        ApplicationContext.GetMasterInstance().IgnoreDemandLoadErrors = true;
                        GetTypes(includePrivateTypes);
                    }
                    catch (Exception ex2)
                    {
                        if (ex2 is ReflectionTypeLoadException)
                        {
                            // Get all of the public types that were able to load
                            ReflectionTypeLoadException loadException = (ReflectionTypeLoadException)ex2;
                            foreach (Type type in loadException.Types)
                            {
                                try
                                {
                                    // Skip unloaded, non-public, and nested types (using a trick of looking for a '+'
                                    // in the name) in order to avoid a possible exception due to unresolved dependencies.
                                    if (type != null && type.IsPublic && !StringUtil.Contains(type.Name, '+'))
                                    {
                                        LoadType(type, rootNamespace);
                                        ++typeCount;
                                    }
                                }
                                catch
                                {
                                    // Ignore the type if an exception is thrown (should be rare)
                                }
                            }
                        }
                    }
                    finally
                    {
                        ApplicationContext.GetMasterInstance().IgnoreDemandLoadErrors = false;
                    }
                }
            }
            return typeCount;
        }

        /// <summary>
        /// Get all referenced assemblies for this assembly.
        /// </summary>
        public AssemblyName[] GetReferencedAssemblies()
        {
            return _assembly.GetReferencedAssemblies();
        }

        /// <summary>
        /// Get a string description of this <see cref="LoadedAssembly"/>.
        /// </summary>
        public override string ToString()
        {
            return _assembly.FullName;
        }

        #endregion
    }
}
