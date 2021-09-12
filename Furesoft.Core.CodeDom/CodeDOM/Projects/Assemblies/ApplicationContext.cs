// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Projects.Assemblies;
using Furesoft.Core.CodeDom.CodeDOM.Projects.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Projects;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Utilities;

namespace Furesoft.Core.CodeDom.CodeDOM.Projects.Assemblies
{
    /// <summary>
    /// Manages all assemblies loaded into an <see cref="AppDomain"/>.
    /// </summary>
    /// <remarks>
    /// When using reflection-only or Mono Cecil loads, different versions of the same assembly can be loaded into the same
    /// <see cref="AppDomain"/>, otherwise separate AppDomains must be used (which is not yet supported, and might never be
    /// since Mono Cecil might be the new preferred way of handling this).
    /// </remarks>
    public class ApplicationContext
    {
        #region /* STATIC MEMBERS */

        private static ApplicationContext MasterAppDomain;

        /// <summary>
        /// Get the master <see cref="ApplicationContext"/> object.
        /// </summary>
        public static ApplicationContext GetMasterInstance()
        {
            if (MasterAppDomain == null)
                MasterAppDomain = new ApplicationContext(AppDomain.CurrentDomain);
            return MasterAppDomain;
        }

        /// <summary>
        /// Use Mono Cecil to load assemblies instead of reflection.  Loaded data will use types in the Mono.Cecil
        /// namespace such as AssemblyDefinition, TypeDefinition, MethodDefinition, PropertyDefinition, FieldDefinition,
        /// etc. instead of the reflection types of Assembly, Type, MethodInfo, PropertyInfo, FieldInfo, etc.  Using
        /// Mono Cecil is faster than reflection, and gets around various possible issues, including the inability to
        /// unload reflection data from memory.
        /// </summary>
        public static bool UseMonoCecilLoads = true;

        /// <summary>
        /// Use reflection-only loads to load assemblies (ignored if using Mono Cecil loads).
        /// Reflection-only loads bypass strong name verifications, CAS policy checks, processor architecture
        /// loading rules, binding policies, don't execute any init code, and prevent automatic probing of dependencies.
        /// This can help to load assemblies that otherwise wouldn't be loadable, but it can also cause loading problems
        /// of its own, such as trying to load old framework assemblies (due to bypassing binding policies) that aren't
        /// compatible with newer and/or 64bit OSes, or illegal cross-references with normally-loaded assemblies.
        /// However, logic has been added to workaround these issues in most cases, and NOT using reflection-only loads
        /// prevents loading old versions of framework assemblies, which can cause resolve conflicts (in either case,
        /// old versions of 'mscorlib' can never be loaded into the default app domain).
        /// </summary>
        public static bool UseReflectionOnlyLoads = true;

        #endregion

        #region /* FIELDS */

        /// <summary>
        /// The .NET <see cref="AppDomain"/> object associated with this <see cref="ApplicationContext"/> instance.
        /// </summary>
        protected AppDomain _appDomain;

        /// <summary>
        /// Loaded assemblies indexed by display name, and also short name for the latest version.
        /// </summary>
        protected Dictionary<string, LoadedAssembly> _loadedAssembliesByName = new Dictionary<string, LoadedAssembly>();

        /// <summary>
        /// Loaded assemblies indexed by assembly (used only when loading with reflection, to handle already-loaded assemblies).
        /// </summary>
        protected Dictionary<Assembly, LoadedAssembly> _loadedAssembliesByAssembly = new Dictionary<Assembly, LoadedAssembly>();

        /// <summary>
        /// Map of assemblies to projects in which they are referenced (used only when loading with reflection).
        /// </summary>
        protected Dictionary<Assembly, HashSet<Project>> _assemblyToProjectMap = new Dictionary<Assembly, HashSet<Project>>();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ApplicationContext"/>.
        /// </summary>
        /// <param name="appDomain"></param>
        public ApplicationContext(AppDomain appDomain)
        {
            _appDomain = appDomain;

            // Register event handlers on the AppDomain
            if (!UseMonoCecilLoads)
            {
                _appDomain.AssemblyLoad += OnAssemblyLoad;
                _appDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
            }
        }

        #endregion

        #region /* STATIC CONSTRUCTOR */

        static ApplicationContext()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            CodeObject.ForceReference();
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The .NET <see cref="AppDomain"/> object associated with this <see cref="ApplicationContext"/> instance.
        /// </summary>
        public AppDomain AppDomain
        {
            get { return _appDomain; }
        }

        /// <summary>
        /// The unique ID of the underlying <see cref="AppDomain"/> object.
        /// </summary>
        public int ID
        {
            get { return _appDomain.Id; }
        }

        internal bool IgnoreDemandLoadErrors { get; set; }
        internal bool IgnoreDuplicateLoadErrors { get; set; }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Find any already-loaded assembly with the specified name.
        /// </summary>
        public LoadedAssembly FindLoadedAssembly(string assemblyName)
        {
            LoadedAssembly alreadyLoadedAssembly;
            _loadedAssembliesByName.TryGetValue(assemblyName.ToLower(), out alreadyLoadedAssembly);
            return alreadyLoadedAssembly;
        }

        /// <summary>
        /// Load an assembly into the current AppDomain.
        /// </summary>
        /// <param name="assemblyName">The display name, short name, or file name of the assembly.</param>
        /// <param name="hintPath">Optional full file specification of the assembly.</param>
        /// <param name="isFrameworkAssembly">True for framework assemblies.</param>
        /// <param name="alternatePaths">Optional alternate search paths for the assembly.</param>
        /// <param name="errorMessage">Returns an error message string if there was one.</param>
        /// <param name="project">The Project associated with the assembly.</param>
        /// <param name="reference">The Reference object associated with the assembly.</param>
        /// <returns>The LoadedAssembly object.</returns>
        public LoadedAssembly LoadAssembly(string assemblyName, string hintPath, bool isFrameworkAssembly,
            IEnumerable<string> alternatePaths, out string errorMessage, Project project, Reference reference)
        {
            bool alreadyLoaded;
            return LoadAssembly(assemblyName, hintPath, false, isFrameworkAssembly, alternatePaths, out alreadyLoaded, out errorMessage, project, reference);
        }

        /// <summary>
        /// Load an assembly into the current AppDomain.
        /// </summary>
        /// <param name="assemblyName">The display name, short name, or file name of the assembly.</param>
        /// <param name="hintPath">Optional full file specification of the assembly.</param>
        /// <param name="isDependency">True if the assembly is a child dependency (call originates from OnReflectionOnlyAssemblyResolve).</param>
        /// <param name="isFrameworkAssembly">True for framework assemblies.</param>
        /// <param name="alternatePaths">Optional alternate search paths for the assembly.</param>
        /// <param name="alreadyLoaded">Returns true if the assembly was already loaded.</param>
        /// <param name="errorMessage">Returns an error message string if there was one.</param>
        /// <param name="project">The Project associated with the assembly.</param>
        /// <param name="reference">The Reference object associated with the assembly.</param>
        /// <returns>The LoadedAssembly object.</returns>
        private LoadedAssembly LoadAssembly(string assemblyName, string hintPath, bool isDependency, bool isFrameworkAssembly,
            IEnumerable<string> alternatePaths, out bool alreadyLoaded, out string errorMessage, Project project, Reference reference)
        {
            LoadedAssembly loadedAssembly = null;
            errorMessage = null;

            // Only allow one thread to do this at a time for the same ApplicationContext
            lock (this)
            {
                // Initialize the assembly cache if using reflection
                if (!UseMonoCecilLoads && _loadedAssembliesByAssembly.Count == 0)
                {
                    _loadedAssembliesByName.Clear();

                    if (!UseReflectionOnlyLoads)
                    {
                        // We can save around 6MB off our working set for the assemblies used by Nova itself by using already-loaded
                        // assemblies instead of loading copies, and additional savings when loading multiple solutions during the same
                        // run of Nova.  However, we can't do this if using reflection-only loads, because references between assemblies
                        // loaded in that mode and non-reflection-only assemblies are illegal.  Also, this only makes sense when sharing
                        // a single AppDomain.
                        Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (Assembly currentAssembly in currentAssemblies)
                        {
                            LoadedAssembly newLoadedAssembly = LoadedAssembly.Create(currentAssembly, true, true);
                            _loadedAssembliesByName.Add(currentAssembly.FullName.ToLower(), newLoadedAssembly);
                            _loadedAssembliesByAssembly.Add(currentAssembly, newLoadedAssembly);
                        }
                    }
                }

                // If no alternate paths were specified, but a Project was, then use the project's output path by default
                if (alternatePaths == null & project != null)
                {
                    string outputPath = project.GetFullOutputPath();
                    if (outputPath != null)
                        alternatePaths = new List<string> { outputPath };
                }

                // Check for display name vs file spec vs short name
                bool isDisplayName = AssemblyUtil.IsDisplayName(assemblyName);
                if (isDisplayName)
                    assemblyName = AssemblyUtil.GetNormalizedDisplayName(assemblyName);
                bool isFileName = (assemblyName.IndexOf('\\') >= 0);
                if (isFileName && project != null && !Path.IsPathRooted(assemblyName))
                    assemblyName = FileUtil.CombineAndNormalizePath(project.GetDirectory(), assemblyName);
                string shortName = (isDisplayName
                                        ? assemblyName.Substring(0, assemblyName.IndexOf(','))
                                        : (isFileName ? (Path.GetFileNameWithoutExtension(assemblyName) ?? assemblyName) : assemblyName));

                if (reference != null)
                {
                    // Remember the originally requested version and/or location, if appropriate
                    if (isDisplayName && reference.RequestedVersion == null)
                        reference.RequestedVersion = AssemblyUtil.GetVersion(assemblyName);
                    if (isFileName && reference.RequestedLocation == null)
                        reference.RequestedLocation = assemblyName;
                }

                // If it's not a framework assembly, and not a file name, get the latest version from the GAC if it exists there
                if (!isFrameworkAssembly && !isFileName)
                {
                    GACEntry gacEntry = GACUtil.FindAssembly(assemblyName);
                    if (gacEntry != null)
                    {
                        if (UseMonoCecilLoads)
                        {
                            assemblyName = gacEntry.FileName;
                            isDisplayName = false;
                            isFileName = true;
                            shortName = (Path.GetFileNameWithoutExtension(assemblyName) ?? assemblyName);
                        }
                        else
                        {
                            assemblyName = gacEntry.DisplayName;
                            isDisplayName = true;
                            shortName = assemblyName.Substring(0, assemblyName.IndexOf(','));
                        }
                    }
                }

                // Return the existing assembly if it's already loaded (can occur if using reflection, OR if a previous load
                // attempt generated an error).  When using reflection, dependent assemblies should already be pre-loaded, unless
                // we're using reflection-only loads, in which case we'll get here from OnReflectionOnlyAssemblyResolve).
                // If we have a display name or file name, try that first.
                LoadedAssembly alreadyLoadedAssembly = null;
                if (isDisplayName || isFileName)
                    alreadyLoadedAssembly = FindLoadedAssembly(assemblyName.ToLower());

                // If a hint-path was provided, check if the assembly was already loaded from that path
                if (alreadyLoadedAssembly == null && hintPath != null)
                {
                    // Normalize the path to be absolute and include the filename and extension
                    hintPath = NormalizePath(hintPath, shortName, project);
                    alreadyLoadedAssembly = FindLoadedAssembly(hintPath.ToLower());
                }

                // If we failed to load by display name or file name, then also try the short name.
                // This allows references by short name to just use whatever existing version is already loaded.
                // If we're using reflection, it also supports using already-loaded assemblies with a different version.
                LoadedAssembly existingDifferentVersionAssembly = null;
                if (alreadyLoadedAssembly == null)
                {
                    alreadyLoadedAssembly = FindLoadedAssembly(shortName.ToLower());

                    // If we found the assembly by short name, and we're using reflection-only loads, and we have a display
                    // name or file name, then save the existing (perhaps different-version) assembly for later and pretend
                    // that we haven't found anything, so that we can potentially load different versions of the same assembly
                    // simultaneously.  If that fails later, we'll give up and use the existing assembly.
                    if (alreadyLoadedAssembly != null && !UseMonoCecilLoads && UseReflectionOnlyLoads && (isDisplayName || isFileName))
                    {
                        existingDifferentVersionAssembly = alreadyLoadedAssembly;
                        alreadyLoadedAssembly = null;
                    }
                }

                if (alreadyLoadedAssembly != null)
                {
                    // Log as "sharing" if the assembly was pre-loaded, and this is the first time it's been referenced
                    ReflectionLoadedAssembly reflectionLoadedAssembly = alreadyLoadedAssembly as ReflectionLoadedAssembly;
                    if (reflectionLoadedAssembly != null && (reflectionLoadedAssembly.IsPreLoaded && !reflectionLoadedAssembly.IsShared))
                    {
                        reflectionLoadedAssembly.IsShared = true;
                        if (!isDependency && Log.LogLevel >= Log.Level.Normal)
                            Log.WriteLine("Sharing assembly: " + assemblyName);
                    }

                    alreadyLoaded = true;
                    if (alreadyLoadedAssembly is ErrorLoadedAssembly)
                        errorMessage = ((ErrorLoadedAssembly)alreadyLoadedAssembly).ErrorMessage;
                    return alreadyLoadedAssembly;
                }

                alreadyLoaded = false;


                // LOAD THE ASSEMBLY:
                // Do NOT load referenced child assemblies here - that will be handled as required later, such as when the
                // types are loaded from the assembly.

                // According to this MSDN blog http://blogs.msdn.com/manishagarwal/archive/2005/09/28/474769.aspx, the search order is:
                //  - Files from the current project – indicated by {CandidateAssemblyFiles}.  * Not doing this?
                //  - $(ReferencePath) property that comes from .user/targets file.  * Yes if in Project file, but not .csproj.user file
                //  - $(HintPath) indicated by reference item.  YES.
                //  - Target framework directory.  YES (for framework assemblies).
                //  - Directories found in registry that uses AssemblyFoldersEx Registration.  * Not doing this
                //  - Registered assembly folders, indicated by {AssemblyFolders}.  * Not doing this
                //  - $(OutputPath) or $(OutDir).  YES.
                //  - GAC.  YES.

                if (!isDependency && Log.LogLevel >= Log.Level.Normal)
                    Log.WriteLine("Loading assembly: " + assemblyName);

                // Check the project for a ReferencePath, and try that first if one exists
                if (project != null)
                {
                    string referencePaths = project.ReferencePath + ";" + project.UserReferencePath;
                    foreach (string referencePath in referencePaths.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(referencePath))
                        {
                            string path = ExpandEnvironmentMacros(referencePath);

                            // Normalize the path to be absolute and include the filename and extension
                            path = NormalizePath(path, shortName, project);
                            loadedAssembly = LoadFrom(path, isFrameworkAssembly, out errorMessage, project);
                        }
                    }
                }

                // If a hint-path was provided, and the file exists, then load it (do this first to avoid the performance expense
                // of exceptions thrown by trying to load with a display name when the assembly is not in the GAC).  If we're
                // loading with a full filespec, then the hint-path should be null, and this will be bypassed.
                if (loadedAssembly == null && hintPath != null && errorMessage == null)
                {
                    // Normalize the path to be absolute and include the filename and extension
                    hintPath = NormalizePath(hintPath, shortName, project);
                    loadedAssembly = LoadFrom(hintPath, isFrameworkAssembly, out errorMessage, project);
                }

                // If the hint-path wasn't provided, or the file didn't exist, then load the assembly by the specified display
                // name or path.
                if (loadedAssembly == null && errorMessage == null)
                {
                    if (isDisplayName)
                        loadedAssembly = Load(assemblyName, isFrameworkAssembly, out errorMessage, project);
                    else if (isFileName)
                        loadedAssembly = LoadFrom(assemblyName, isFrameworkAssembly, out errorMessage, project);
                }

                // If the previous attempts have failed, then try loading the assembly from any specified alternate paths, such
                // as the project output path (there might be more than one, if we're loading a dependency and we're not sure
                // which of multiple possible projects the requesting assembly originated in).
                if (loadedAssembly == null && alternatePaths != null && errorMessage == null)
                {
                    foreach (string alternatePath in alternatePaths)
                    {
                        string alternateName = Path.Combine(alternatePath, shortName);
                        loadedAssembly = LoadFrom(alternateName, isFrameworkAssembly, out errorMessage, project);
                        if (loadedAssembly != null)
                            break;
                    }
                }

                // If everything has failed so far, look in all parent directories of the project (not positive, but VS seems
                // to do this).
                if (loadedAssembly == null && errorMessage == null && project != null)
                {
                    string path = project.GetDirectory();
                    while (true)
                    {
                        string alternateName = Path.Combine(path, shortName);
                        loadedAssembly = LoadFrom(alternateName, isFrameworkAssembly, out errorMessage, project);
                        if (loadedAssembly != null || path.Length <= 2)
                            break;
                        int lastSlash = path.LastIndexOf('\\');
                        if (lastSlash < 0)
                            break;
                        path = path.Substring(0, lastSlash);
                    }
                }

                // If we already have an existing different-version of the assembly, and we failed to load the one we wanted
                // then fall back to the existing assembly.
                if (loadedAssembly == null && existingDifferentVersionAssembly != null)
                {
                    loadedAssembly = existingDifferentVersionAssembly;
                    errorMessage = null;
                }

                if (loadedAssembly == null)
                {
                    if (errorMessage == null)
                    {
                        errorMessage = "Unable to find assembly '" + assemblyName + "' in default locations"
                                       + (hintPath != null ? " or HintPath ('" + hintPath + "')." : ".");
                    }
                    else
                        errorMessage = "Unable to load assembly '" + assemblyName + ": " + errorMessage;
                }
                else
                    ProcessLoadedAssembly(loadedAssembly, assemblyName, isDisplayName, shortName, isDependency, project);
            }
            return loadedAssembly;
        }

        private static string NormalizePath(string path, string fileName, Project project)
        {
            // Convert relative paths to absolute.  Paths can include the filename, but also allow just
            // a directory - add the filename in this case, and ".dll" if it's missing.
            if (!Path.IsPathRooted(path))
            {
                string rootPath = (project != null ? project.GetDirectory() : null);
                if (string.IsNullOrEmpty(rootPath))
                    rootPath = Directory.GetCurrentDirectory();
                path = FileUtil.CombineAndNormalizePath(rootPath, path);
            }
            if (!path.EndsWith(@"\") && !File.Exists(path))
                path += @"\";
            if (path.EndsWith(@"\"))
            {
                path += fileName;
                if (!File.Exists(path) && !fileName.EndsWith(".dll"))
                    path += ".dll";
            }
            return path;
        }

        /// <summary>
        /// Process an assembly that was just loaded.
        /// </summary>
        private void ProcessLoadedAssembly(LoadedAssembly loadedAssembly, string assemblyName, bool isDisplayName, string shortName,
            bool isDependency, Project project)
        {
            LoadedAssembly existingAssembly = null;

            // If using reflection, handle assemblies that are already loaded
            if (loadedAssembly is ReflectionLoadedAssembly)
            {
                ReflectionLoadedAssembly reflectionLoadedAssembly = (ReflectionLoadedAssembly)loadedAssembly;

                // If we were given an assembly that is already loaded, handle it - .NET reflection will use an existing version in
                // the same AppDomain instead of the requested version if not using reflection-only loads, and even loading by path
                // could result in an assembly that is already loaded, such as when a solution is re-loaded into the same AppDomain.
                if (_loadedAssembliesByAssembly.TryGetValue(reflectionLoadedAssembly.Assembly, out existingAssembly))
                {
                    // We have to replace our LoadedAssembly object so it remains unique and can be used as a dictionary key
                    loadedAssembly = existingAssembly;
                    reflectionLoadedAssembly.IsShared = true;
                }
                else
                {
                    // Add the assembly to the assembly index
                    _loadedAssembliesByAssembly.Add(reflectionLoadedAssembly.Assembly, loadedAssembly);
                }

                // Add the current project to the AssemblyToProject map if it's not a child dependency, creating a new
                // entry if necessary (this can occur if the assembly is first loaded as a dependency).
                if (!isDependency)
                    AddAssemblyToProjectMapping(reflectionLoadedAssembly.Assembly, project);
            }

            if (existingAssembly == null)
            {
                // Also add the assembly under it's display name if we loaded by filename, or if the loaded display name is
                // different from the requested one (this is possible when using reflection) - we might have asked for a 3.0
                // assembly, but been given a (previously unloaded) 4.0 assembly if we have other 4.0 assemblies loaded.
                // Skip this step if the assembly display name had no Version specified.
                if (!isDisplayName || (!StringUtil.NNEqualsIgnoreCase(loadedAssembly.FullName, assemblyName) && AssemblyUtil.HasVersion(assemblyName)))
                {
                    // There might already be an entry under the name if an error occurred on a previous load attempt
                    // (such as in OnReflectionOnlyAssemblyResolve), so remove any existing entry first.
                    string key = AssemblyUtil.GetNormalizedDisplayName(loadedAssembly.FullName).ToLower();
                    _loadedAssembliesByName.Remove(key);
                    _loadedAssembliesByName.Add(key, loadedAssembly);
                }
                // Finally, if we loaded by display name, also add the assembly under it's filename
                if (isDisplayName)
                {
                    string key = loadedAssembly.Location.ToLower();
                    //_loadedAssembliesByName.Remove(key);
                    _loadedAssembliesByName.Add(key, loadedAssembly);
                }
            }

            string requestedVersion = AssemblyUtil.GetVersion(assemblyName);
            string loadedVersion = loadedAssembly.GetVersion();
            if (requestedVersion != null && requestedVersion != loadedVersion && Log.LogLevel >= Log.Level.Normal)
                Log.WriteLine("\t    USING: " + loadedAssembly.FullName);

            // Add the loaded assembly to the name index under the requested display or file name
            _loadedAssembliesByName.Add(assemblyName.ToLower(), loadedAssembly);

            // If using reflection, add the assembly under it's short name
            if (loadedAssembly is ReflectionLoadedAssembly)
            {
                // Add the loaded assembly to the dictionary under its simple name (we don't want to use the full file path,
                // because we can't support loading the same assembly from different locations unless we use a separate AppDomain
                // for each project - which would boost memory requirements 100X for large solutions).
                // We might have to support this eventually in order to support different versions of the same assembly used by
                // different projects (like System.dll v2.0 vs System.dll v1.1) BUT only if we're not using reflection-only loads.
                // For now, if the short name already exists due to another version of the same assembly being loaded, leave it
                // alone if it's for a newer version, otherwise replace it with the new one.
                if (shortName != assemblyName)
                {
                    // Normalize the key to lower-case to avoid possible mismatches in references from different projects
                    string key = shortName.ToLower();
                    if (_loadedAssembliesByName.TryGetValue(key, out existingAssembly))
                    {
                        // If the existing assembly is older, replace it with the new one
                        if (GACUtil.CompareVersions(existingAssembly.GetVersion(), loadedAssembly.GetVersion()) < 0)
                        {
                            _loadedAssembliesByName.Remove(key);
                            _loadedAssembliesByName.Add(key, loadedAssembly);
                        }
                    }
                    else
                        _loadedAssembliesByName.Add(key, loadedAssembly);
                }
            }
        }

        /// <summary>
        /// Expand all "$(name)" macros in the specified string by replacing with matching environment variables.
        /// </summary>
        public static string ExpandEnvironmentMacros(string value)
        {
            string result = "";
            int index = 0;
            while (index < value.Length)
            {
                int macroIndex = value.IndexOf("$(", index);
                if (macroIndex >= 0)
                {
                    int startIndex = macroIndex + 2;
                    int endIndex = value.IndexOf(")", startIndex);
                    if (endIndex == -1)
                        endIndex = value.Length;
                    string variableName = value.Substring(startIndex, endIndex - startIndex);
                    string variableValue = null;
                    try { variableValue = Environment.GetEnvironmentVariable(variableName); }
                    catch { }
                    if (variableValue != null)
                        result += value.Substring(0, macroIndex) + variableValue;
                    index = endIndex;
                }
                else
                {
                    result += value.Substring(index);
                    break;
                }
            }
            return result;
        }

        private void AddAssemblyToProjectMapping(Assembly assembly, Project project)
        {
            if (project != null)
            {
                HashSet<Project> projects;
                if (_assemblyToProjectMap.TryGetValue(assembly, out projects))
                    projects.Add(project);
                else
                    _assemblyToProjectMap.Add(assembly, new HashSet<Project> { project });
            }
        }

        protected static LoadedAssembly Load(string assemblyName, bool isFrameworkAssembly, out string errorMessage, Project project)
        {
            errorMessage = null;
            try
            {
                if (UseMonoCecilLoads)
                {
                    // Cecil doesn't support loading assemblies by display name - look in the GAC and use the file name
                    GACEntry gacEntry = GACUtil.FindAssembly(assemblyName);
                    if (gacEntry != null)
                        return LoadFromInternal(gacEntry.FileName, isFrameworkAssembly, out errorMessage, project);
                }
                else
                {
                    Assembly assembly = UseReflectionOnlyLoads ? Assembly.ReflectionOnlyLoad(assemblyName) : Assembly.Load(assemblyName);
                    return LoadedAssembly.Create(assembly, isFrameworkAssembly);
                }
            }
            catch (FileNotFoundException)
            {
                // Just return null if file not found
            }
            catch (Exception ex)
            {
                errorMessage = "EXCEPTION loading assembly: " + ex.Message;
            }
            return null;
        }

        protected static LoadedAssembly LoadFrom(string assemblyFileName, bool isFrameworkAssembly, out string errorMessage, Project project)
        {
            string extension = Path.GetExtension(assemblyFileName);
            if (extension != ".dll" && extension != ".exe")
            {
                if (File.Exists(assemblyFileName + ".dll"))
                    assemblyFileName += ".dll";
                else if (File.Exists(assemblyFileName + ".exe"))
                    assemblyFileName += ".exe";
            }
            return LoadFromInternal(assemblyFileName, isFrameworkAssembly, out errorMessage, project);
        }

        private static LoadedAssembly LoadFromInternal(string assemblyFileName, bool isFrameworkAssembly, out string errorMessage, Project project)
        {
            errorMessage = null;
            if (File.Exists(assemblyFileName))
            {
                try
                {
                    if (UseMonoCecilLoads)
                    {
                        MonoCecilAssemblyResolver assemblyResolver = project.AssemblyResolver;
                        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyFileName, new ReaderParameters { AssemblyResolver = assemblyResolver });
                        LoadedAssembly loadedAssembly = LoadedAssembly.Create(assembly, isFrameworkAssembly);
                        Log.DetailWriteLine("\tLOADED: " + loadedAssembly.Location);
                        return loadedAssembly;
                    }
                    else
                    {
                        Assembly assembly = UseReflectionOnlyLoads ? Assembly.ReflectionOnlyLoadFrom(assemblyFileName) : Assembly.LoadFrom(assemblyFileName);
                        return LoadedAssembly.Create(assembly, isFrameworkAssembly);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "EXCEPTION loading assembly: " + ex.Message;
                }
            }
            return null;
        }

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            // Log the actual loading of any assemblies
            Log.DetailWriteLine("\tLOADED: " + args.LoadedAssembly.Location);
        }

        private Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // This event should only fire if we're using reflection to load assemblies in reflection-only mode.
            // It will fire whenever a referenced "child" assembly needs to be loaded, and we have to find it and load it
            // here.  This will generally occur when we are loading types from a parent assembly, but some loads won't
            // trigger until a later point in time, when retrieving additional reflection data.
            // Unfortunately, we have no context as to which FrameworkContext or Project the requesting assembly belongs
            // to, so we have to maintain a mapping of all referenced assemblies to projects and use that as best as we
            // can to determine the appropriate FrameworkContext and/or Project output path (even though this is a one-to-
            // many mapping).

            // Only allow one thread to do this at a time for the same ApplicationContext
            lock (this)
            {
                LoadedAssembly loadedAssembly = null;
                bool alreadyLoaded = false;
                string errorMessage = null;
                string assemblyName = AssemblyUtil.GetNormalizedDisplayName(args.Name);
                string shortName = assemblyName.Substring(0, assemblyName.IndexOf(','));

                // Get the requesting assembly, and use it's location as the first alternate search path
                Assembly requestingAssembly = args.RequestingAssembly;
                List<string> alternatePaths = new List<string> { Path.GetDirectoryName(requestingAssembly.Location) };

                // Determine all projects which reference the requesting assembly
                HashSet<Project> projects;
                if (_assemblyToProjectMap.TryGetValue(requestingAssembly, out projects))
                {
                    // Add the output directories of all parent projects as alternate search paths
                    foreach (Project project in projects)
                    {
                        string outputPath = project.GetFullOutputPath();
                        if (outputPath != null)
                            alternatePaths.Add(outputPath);
                    }

                    // Try to determine the target framework, and if we can, and the assembly is a framework assembly
                    // of the target, then load it from there.
                    FrameworkContext frameworkContext = null;
                    Project relatedProject = null;
                    foreach (Project project in projects)
                    {
                        if (frameworkContext == null)
                        {
                            frameworkContext = project.FrameworkContext;
                            relatedProject = project;
                        }
                        else if (frameworkContext != project.FrameworkContext)
                        {
                            // If there are multiple possible target frameworks, use none
                            frameworkContext = null;
                            break;
                        }
                    }
                    if (frameworkContext != null && frameworkContext.IsFrameworkAssembly(shortName))
                    {
                        loadedAssembly = frameworkContext.LoadAssembly(shortName, null, alternatePaths, out errorMessage, null, null, true);

                        // If the version requested is greater than that returned, invalidate it (which will cause the
                        // correct version to be loaded from the GAC down below).  This can occur with the .NETPortable
                        // framework, which is in a v4.0 directory with v4.0 file versions, but v2.0 assemblies.  If a
                        // referenced assembly references a v4.0 framework assembly, it will throw an exception if we
                        // try to feed it a v2.0 assembly instead.
                        if (GACUtil.CompareVersions(AssemblyUtil.GetVersion(assemblyName), loadedAssembly.GetVersion()) > 0)
                            loadedAssembly = null;
                        else
                        {
                            // Add the newly loaded framework assembly to the project mapping if it's not already there,
                            // so that if it depends on other framework assemblies, they can be properly treated as such.
                            Assembly newAssembly = ((ReflectionLoadedAssembly)loadedAssembly).Assembly;
                            if (!_assemblyToProjectMap.ContainsKey(newAssembly))
                                AddAssemblyToProjectMapping(((ReflectionLoadedAssembly)loadedAssembly).Assembly, relatedProject);
                        }
                    }
                }

                if (loadedAssembly == null)
                {
                    // If it wasn't a framework assembly, try loading it by display name
                    loadedAssembly = LoadAssembly(assemblyName, null, true, false, alternatePaths, out alreadyLoaded, out errorMessage, null, null);
                    if (loadedAssembly == null && AssemblyUtil.IsDisplayName(assemblyName))
                    {
                        // If loading the assembly failed, and the name was a display name, try loading again with just the
                        // short name.  For example, a referenced assembly might in turn reference an old framework assembly,
                        // such as System.Drawing 1.0, when it's not available on the current machine (frameworks prior to 2.0
                        // aren't supported on Windows 7).  This will work around this problem, probably without any negative
                        // side effects.
                        string dummyErrorMessage;
                        loadedAssembly = LoadAssembly(shortName, null, true, false, alternatePaths, out alreadyLoaded, out dummyErrorMessage, null, null);
                        if (loadedAssembly == null)
                        {
                            // If loading with the short name failed, make a last attempt to find the assembly in the GAC.  This
                            // will pick up any old framework assemblies (such as 1.0 assemblies unsupported by the current OS)
                            // that weren't already loaded as direct references elsewhere.
                            GACEntry gacEntry = GACUtil.FindAssembly(shortName);
                            if (gacEntry != null)
                                loadedAssembly = LoadAssembly(gacEntry.DisplayName, null, true, false, null, out alreadyLoaded, out dummyErrorMessage, null, null);
                        }
                    }
                }
                if (loadedAssembly == null)
                {
                    // Store a fake assembly in the name index to prevent duplicate failure exceptions
                    if (!alreadyLoaded)
                        _loadedAssembliesByName.Add(assemblyName.ToLower(), LoadedAssembly.Create(errorMessage));

                    if (!IgnoreDemandLoadErrors && !(IgnoreDuplicateLoadErrors && alreadyLoaded))
                        Log.WriteLine(errorMessage);
                }
                return (loadedAssembly is ReflectionLoadedAssembly ? ((ReflectionLoadedAssembly)loadedAssembly).Assembly : null);
            }
        }

        /// <summary>
        /// Unload the <see cref="ApplicationContext"/> (unloads all loaded assemblies if not using reflection).
        /// </summary>
        public void Unload()
        {
            if (UseMonoCecilLoads)
            {
                _loadedAssembliesByName.Clear();
                FrameworkContext.UnloadAll();
            }
            else
            {
                // If we're using reflection, then unless we're using an alternate AppDomain, we can't unload it, and therefore can't
                // actually unload any loaded assemblies.  So, we don't want to clear the loaded assembly collections, because if a new
                // Solution is loaded, this would result in both the needless reloading of assemblies plus possible errors if an attempt
                // is made to load an already-loaded assembly from a different location.
                if (!_appDomain.IsDefaultAppDomain())
                {
                    _loadedAssembliesByName.Clear();
                    _loadedAssembliesByAssembly.Clear();
                    _assemblyToProjectMap.Clear();
                    FrameworkContext.UnloadAll();
                    _appDomain.AssemblyLoad -= OnAssemblyLoad;
                    _appDomain.ReflectionOnlyAssemblyResolve -= OnReflectionOnlyAssemblyResolve;
                    try
                    {
                        AppDomain.Unload(_appDomain);
                    }
                    catch { }
                }
            }
        }

        #endregion
    }
}
