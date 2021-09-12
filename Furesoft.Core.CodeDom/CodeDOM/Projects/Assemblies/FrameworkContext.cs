// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a group of loaded assemblies for a particular target framework version, loaded into a particular <see cref="AppDomain"/>.
    /// </summary>
    /// <remarks>
    /// Assemblies for different target frameworks (.NETFramework, Silverlight) must be managed with separate <see cref="FrameworkContext"/>s
    /// so that when a framework assembly is loaded by simple name it will load the correct one and not mix assemblies from
    /// different target frameworks in the same project.  Perhaps a single FrameworkContext could be shared for different
    /// target framework versions when using reflection-only loads or Mono Cecil loads, since different versions of the same
    /// assembly can be loaded simultaneously, but separate FrameworkContexts are used currently.
    /// Even if different FrameworkContexts exist for different target framework versions, they can generally share a single
    /// (current) AppDomain when using reflection-only or Mono Cecil loads, or if all projects target the same framework version
    /// and don't load different versions of the same assembly.  The code also supports using an existing version of an assembly
    /// that differs from the requested one, which will often work without errors.
    /// Another complication is that the .NET 3.0 and 3.5 frameworks are "partial" releases that also use assemblies from
    /// previous releases, so their FrameworkContext objects must work together with those for the earlier versions (they can't
    /// be self-contained, because in some situations that would result in an attempt to load the same version of the same
    /// assembly twice into the same AppDomain).  The .NET 4.0 release is complete, and doesn't have this problem.
    /// Actually, there is another exception - if a framework assembly doesn't exist in the current framework context, then in
    /// that case, it's OK to look in an earlier version.  For example, some assemblies are versioned in the name, such as
    /// Microsoft.Build.Utilities.v3.5, so there's no name conflict between releases.
    /// </remarks>
    public class FrameworkContext
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The name of the .NET framework.
        /// </summary>
        public const string DotNetFramework = ".NETFramework";

        /// <summary>
        /// The name of the Silverlight framework.
        /// </summary>
        public const string SilverlightFramework = "Silverlight";

        /// <summary>
        /// The name of the Portable Library framework.
        /// </summary>
        public const string PortableLibraryFramework = ".NETPortable";

        #endregion

        #region /* STATIC MEMBERS */

        private static readonly Dictionary<string, FrameworkContext> FrameworkContexts = new Dictionary<string, FrameworkContext>();

        /// <summary>
        /// Get the <see cref="FrameworkContext"/> object for the specified target framework version.
        /// </summary>
        public static FrameworkContext Get(string targetFramework, string targetFrameworkVersion, string targetFrameworkProfile, ApplicationContext applicationDomain)
        {
            FrameworkContext loadContext = null;
            if (targetFramework != null && targetFrameworkVersion != null)
            {
                string key = targetFramework + "-" + targetFrameworkVersion + "-" + targetFrameworkProfile + "-" + applicationDomain.ID;
                if (!FrameworkContexts.TryGetValue(key, out loadContext))
                {
                    loadContext = new FrameworkContext(targetFramework, targetFrameworkVersion, targetFrameworkProfile, applicationDomain);
                    FrameworkContexts.Add(key, loadContext);
                }
            }
            return loadContext;
        }

        /// <summary>
        /// Unload all <see cref="FrameworkContext"/> instances.
        /// </summary>
        public static void UnloadAll()
        {
            foreach (KeyValuePair<string, FrameworkContext> keyValuePair in FrameworkContexts)
                keyValuePair.Value.Unload();
            FrameworkContexts.Clear();
        }

        /// <summary>
        /// Get the .NET runtime location for the requested version.
        /// </summary>
        public static string GetRuntimeLocation(string version)
        {
            string path = Environment.GetEnvironmentVariable("FrameworkDir") ?? Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\";
            if (version.StartsWith("4"))
                path += @"v4.0.30319\";
            else if (version.StartsWith("3.5"))
                path += @"v3.5\";
            else if (version.StartsWith("3"))
                path += @"v3.0\";
            else if (version.StartsWith("2"))
                path += @"v2.0.50727\";
            //else if (version.StartsWith("1.1"))
            //    path += @"v1.1.4322\";
            //else if (version.StartsWith("1"))
            //    path += @"v1.0.3705\";
            else
                path += @"v4.0.30319\";
            return path;
        }

        #endregion

        #region /* FIELDS */

        /// <summary>
        /// The target framework for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        protected readonly string _targetFramework;

        /// <summary>
        /// The target framework version for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        protected readonly string _targetFrameworkVersion;

        /// <summary>
        /// The target framework profile for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        protected readonly string _targetFrameworkProfile;

        /// <summary>
        /// The associated non-profile framework if this one has a profile (otherwise null).
        /// </summary>
        protected readonly FrameworkContext _nonProfileFramework;

        /// <summary>
        /// The previous version framework to this one (null if none).
        /// </summary>
        protected readonly FrameworkContext _previousFramework;

        /// <summary>
        /// The <see cref="ApplicationContext"/> object associated with this <see cref="FrameworkContext"/> instance.
        /// </summary>
        protected ApplicationContext _applicationContext;

        /// <summary>
        /// The path of the assemblies for the target framework.
        /// </summary>
        protected string _frameworkAssembliesPath;

        /// <summary>
        /// A dictionary of names of the assemblies for the target framework, mapped to the <see cref="LoadedAssembly"/> if loaded.
        /// </summary>
        protected readonly Dictionary<string, LoadedAssembly> _frameworkAssemblyNames = new Dictionary<string, LoadedAssembly>();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="FrameworkContext"/>.
        /// </summary>
        public FrameworkContext(string targetFramework, string targetFrameworkVersion, string targetFrameworkProfile, ApplicationContext applicationContext)
        {
            _targetFramework = targetFramework;
            _targetFrameworkVersion = targetFrameworkVersion;
            _targetFrameworkProfile = targetFrameworkProfile;
            _applicationContext = applicationContext;

            // Get the associated non-profile framework if this one is a profile
            if (_targetFrameworkProfile != null)
                _nonProfileFramework = Get(_targetFramework, _targetFrameworkVersion, null, _applicationContext);

            // Get the program files path
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetEnvironmentVariable("ProgramFiles");

            // Get the path to the framework assemblies, using "reference assemblies" if possible - if they don't exist
            // (they won't if Visual Studio isn't installed on the machine), then fall back to the runtime assemblies.
            // Starting with 4.0, the .NETFramework assemblies are in their own subdirectory with Silverlight assemblies in
            // another, and all assemblies are present.  Prior to that, the 3.5 and 3.0 assemblies are in the parent folder,
            // and these versions contain only the new assemblies for that release.  For 2.0, no reference assemblies exist,
            // so the runtime assemblies have to be used.
            string path = null;
            int compare20 = GACUtil.CompareVersions(_targetFrameworkVersion, "2.0");
            if (compare20 > 0)
            {
                path = programFiles + @"\Reference Assemblies\Microsoft\Framework\";
                // The .NETFramework reference assemblies are under their own sub-directory as of 4.0 (or 3.5 for Profile
                // assemblies), while the Silverlight reference assemblies are under their own sub-directory as of 3.0.
                if (_targetFramework != DotNetFramework || GACUtil.CompareVersions(_targetFrameworkVersion, "4.0") >= 0
                    || (_targetFrameworkProfile != null && GACUtil.CompareVersions(_targetFrameworkVersion, "3.5") >= 0))
                    path += _targetFramework + @"\";
                path += "v" + _targetFrameworkVersion + @"\";
                if (_targetFrameworkProfile != null)
                    path += @"Profile\" + _targetFrameworkProfile + @"\";

                // Fall back to the runtime assemblies if the reference assemblies weren't found
                if (!Directory.Exists(path))
                    path = GetRuntimeLocation(_targetFrameworkVersion);

                // Get a reference to the previous (non-profile) framework version (back to 2.0)
                if (GACUtil.CompareVersions(_targetFrameworkVersion, "4.0") > 0)
                    _previousFramework = Get(_targetFramework, "4.0", null, _applicationContext);
                else if (GACUtil.CompareVersions(_targetFrameworkVersion, "3.5") > 0)
                    _previousFramework = Get(_targetFramework, "3.5", null, _applicationContext);
                else if (GACUtil.CompareVersions(_targetFrameworkVersion, "3.0") > 0)
                    _previousFramework = Get(_targetFramework, "3.0", null, _applicationContext);
                else if (GACUtil.CompareVersions(_targetFrameworkVersion, "2.0") > 0)
                    _previousFramework = Get(_targetFramework, "2.0", null, _applicationContext);
            }
            else if (compare20 == 0)
            {
                // The Silverlight 2.0 reference assemblies are stuffed in the SDK folder
                if (_targetFramework == SilverlightFramework)
                    path = programFiles + @"\Microsoft SDKs\Silverlight\v2.0\Reference Assemblies\";
                else  // .NETFramework
                {
                    // The .NET 2.0 framework doesn't have any reference assemblies, so use the runtime ones
                    path = GetRuntimeLocation("2.0");
                }

                // Don't bother setting the 1.1 or 1.0 framework as a previous version to 2.0
            }
            else
                Log.WriteLine("ERROR: Target framework " + GetDescription() + " isn't currently supported!");

            // Load all of the assembly names
            if (path != null)
            {
                _frameworkAssembliesPath = path;
                if (Directory.Exists(_frameworkAssembliesPath))
                {
                    foreach (string fileName in Enumerable.Where(Directory.EnumerateFiles(_frameworkAssembliesPath), delegate(string fileName) { return fileName.EndsWith(".dll"); }))
                    {
                        string assemblyName = Path.GetFileNameWithoutExtension(fileName);
                        if (assemblyName != null)
                            _frameworkAssemblyNames.Add(assemblyName.ToLower(), null);
                    }
                }
                else
                    Log.WriteLine("ERROR: Assemblies not found for " + GetDescription() + "!", "Path: " + _frameworkAssembliesPath);
            }
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The target framework for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        public string TargetFramework
        {
            get { return _targetFramework; }
        }

        /// <summary>
        /// The target framework version for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        public string TargetFrameworkVersion
        {
            get { return _targetFrameworkVersion; }
        }

        /// <summary>
        /// The target framework profile for this <see cref="FrameworkContext"/> instance.
        /// </summary>
        public string TargetFrameworkProfile
        {
            get { return _targetFrameworkProfile; }
        }

        /// <summary>
        /// The <see cref="ApplicationContext"/> object associated with this <see cref="FrameworkContext"/> instance.
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return _applicationContext; }
        }

        /// <summary>
        /// The path of the reference assemblies for the target framework.
        /// </summary>
        public string ReferenceAssembliesPath
        {
            get { return _frameworkAssembliesPath; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the full description of this target framework.
        /// </summary>
        public string GetDescription()
        {
            return _targetFramework + " v" + _targetFrameworkVersion
                + (_targetFrameworkProfile != null ? " " + _targetFrameworkProfile + " Profile" : "");
        }

        /// <summary>
        /// Determine if the specified assembly exists in the targeted framework.
        /// </summary>
        public bool IsFrameworkAssembly(string assemblyName)
        {
            bool found = _frameworkAssemblyNames.ContainsKey(assemblyName.ToLower());
            if (!found && _targetFramework == DotNetFramework && _targetFrameworkProfile == null)
            {
                // If we didn't find the assembly in the current framework version, we also have to recursively
                // check any previous versions back to at least 2.0.  This handles partial releases (such as 3.5
                // and 3.0) which add-on to the previous release, and also a few assemblies that are versioned in
                // the name (such as Microsoft.Build.Tasks.vX.X), and so are unique across framework versions.
                if (_previousFramework != null)
                    found = _previousFramework.IsFrameworkAssembly(assemblyName);
            }
            return found;
        }

        /// <summary>
        /// Load an assembly into the current AppDomain, treating any matching short name as a framework assembly
        /// for the targeted framework (non-matching names or display names are loaded normally).
        /// </summary>
        /// <param name="assemblyName">The display name, short name, or file name of the assembly.</param>
        /// <param name="hintPath">Optional full file specification of the assembly.</param>
        /// <param name="alternatePaths">Optional alternate search paths for the assembly.</param>
        /// <param name="errorMessage">Returns an error message string if there was one.</param>
        /// <param name="project">The Project associated with the assembly.</param>
        /// <param name="reference">The Reference object associated with the assembly.</param>
        /// <param name="frameworkOnly">Only load the assembly if it's a member of the current framework, otherwise return null.</param>
        /// <returns>The LoadedAssembly object.</returns>
        public LoadedAssembly LoadAssembly(string assemblyName, string hintPath, IEnumerable<string> alternatePaths, out string errorMessage,
            Project project, Reference reference, bool frameworkOnly)
        {
            LoadedAssembly loadedAssembly;
            errorMessage = null;

            // Special handling for 'mscorlib' if we're using reflection
            bool isMsCorLib = StringUtil.NNEqualsIgnoreCase(assemblyName, Project.MsCorLib);
            if ((isMsCorLib || assemblyName.StartsWith("mscorlib,")) && !ApplicationContext.UseMonoCecilLoads)
            {
                // If it's 'mscorlib' and we're using reflection, get the latest version from the GAC that doesn't exceed the
                // target framework (we need a display name for mscorlib, because loading the reference assembly by name doesn't
                // work, since we can only "load" the one that's actually already loaded).
                if (isMsCorLib)
                {
                    GACEntry gacEntry = GACUtil.FindAssembly(assemblyName, (project != null ? project.TargetFrameworkVersion : null));
                    if (gacEntry != null)
                        assemblyName = gacEntry.DisplayName;
                }
                loadedAssembly = _applicationContext.LoadAssembly(assemblyName, hintPath, true, alternatePaths, out errorMessage, project, reference);
            }
            else
            {
                // Check if the assembly belongs to the targeted framework
                bool isFileName = (assemblyName.IndexOf('\\') >= 0 || assemblyName.EndsWith(".dll"));
                if (isFileName && project != null && !Path.IsPathRooted(assemblyName))
                    assemblyName = FileUtil.CombineAndNormalizePath(project.GetDirectory(), assemblyName);
                string shortName = (assemblyName.IndexOf(',') >= 0 ? assemblyName.Substring(0, assemblyName.IndexOf(','))
                    : (isFileName ? (Path.GetFileNameWithoutExtension(assemblyName) ?? assemblyName) : assemblyName));
                if (_frameworkAssemblyNames.TryGetValue(shortName.ToLower(), out loadedAssembly))
                {
                    // If the framework assembly hasn't been loaded yet, do it now
                    string path = Path.Combine(_frameworkAssembliesPath, shortName + ".dll");

                    // If this is a framework 'profile', load the assembly using the associated non-profile framework
                    // if possible, so that they can share the assembly (which probably has the same version).
                    if (_targetFrameworkProfile != null)
                    {
                        // Make the location always show the original attempted location
                        if (reference != null)
                            reference.RequestedLocation = path;

                        if (loadedAssembly == null)
                        {
                            if (_nonProfileFramework != null && _nonProfileFramework.IsFrameworkAssembly(shortName))
                                loadedAssembly = _nonProfileFramework.LoadAssembly(shortName, hintPath, alternatePaths, out errorMessage, project, reference, true);
                        }
                    }

                    // Otherwise, load the framework assembly normally
                    if (loadedAssembly == null)
                        loadedAssembly = _applicationContext.LoadAssembly(path, null, true, alternatePaths, out errorMessage, project, reference);

                    _frameworkAssemblyNames[shortName.ToLower()] = loadedAssembly;
                }
                else
                {
                    // Also check any chained older frameworks for .NET 3.5 or 3.0 (we don't have to do this for Client Profiles, because
                    // they contain all appropriate assemblies - including older ones).
                    if (_targetFramework == DotNetFramework && _targetFrameworkProfile == null)
                    {
                        // If we didn't find the assembly in the current framework version, we also have to recursively
                        // check any previous versions back to at least 2.0.  This handles partial releases (such as 3.5
                        // and 3.0) which add-on to the previous release, and also a few assemblies that are versioned in
                        // the name (such as Microsoft.Build.Tasks.vX.X), and so are unique across framework versions.
                        if (_previousFramework != null)
                            loadedAssembly = _previousFramework.LoadAssembly(assemblyName, hintPath, alternatePaths, out errorMessage, project, reference, true);
                    }

                    // If it's not a framework assembly, load it normally (unless told not to)
                    if (loadedAssembly == null && !frameworkOnly)
                        loadedAssembly = _applicationContext.LoadAssembly(assemblyName, hintPath, false, alternatePaths, out errorMessage, project, reference);
                }
            }

            return loadedAssembly;
        }

        /// <summary>
        /// Load an assembly into the current AppDomain, treating any matching short name as a framework assembly
        /// for the targeted framework (non-matching names or display names are loaded normally).
        /// </summary>
        /// <param name="assemblyName">The display name, short name, or file name of the assembly.</param>
        /// <param name="hintPath">Optional full file specification of the assembly.</param>
        /// <param name="alternatePaths">Optional alternate search paths for the assembly.</param>
        /// <param name="errorMessage">Returns an error message string if there was one.</param>
        /// <param name="project">The Project associated with the assembly.</param>
        /// <param name="reference">The Reference object associated with the assembly.</param>
        /// <returns>The LoadedAssembly object.</returns>
        public LoadedAssembly LoadAssembly(string assemblyName, string hintPath, IEnumerable<string> alternatePaths, out string errorMessage,
            Project project, Reference reference)
        {
            return LoadAssembly(assemblyName, hintPath, alternatePaths, out errorMessage, project, reference, false);
        }

        /// <summary>
        /// Unload the <see cref="FrameworkContext"/> (clears the map of loaded framework assemblies).
        /// </summary>
        public void Unload()
        {
            _applicationContext = null;
            _frameworkAssemblyNames.Clear();
        }

        #endregion
    }
}
