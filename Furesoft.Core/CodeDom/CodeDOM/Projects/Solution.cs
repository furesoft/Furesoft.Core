// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Utilities;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a collection of <see cref="Project"/> objects that are logically grouped (and compiled) together.
    /// </summary>
    public class Solution : CodeObject, INamedCodeObject, IFile
    {
        public const string HideSolutionNodeProperty = "HideSolutionNode";
        public const string NestedProjectsGlobalSection = "NestedProjects";
        public const string PlatformAnyCPU = "Any CPU";
        public const string PlatformDotNet = ".NET";
        public const string PlatformMixed = "Mixed Platforms";
        public const string ProjectConfigurationPlatformsGlobalSection = "ProjectConfigurationPlatforms";
        public const string SolutionConfigurationPlatformsGlobalSection = "SolutionConfigurationPlatforms";
        public const string SolutionFileExtension = ".sln";
        public const string SolutionItems = "Solution Items";
        public const string SolutionItemsProjectSection = "SolutionItems";
        public const string SolutionOptionsExtension = ".nuo";
        public const string SolutionPropertiesGlobalSection = "SolutionProperties";
        public const string SourceCodeControlGlobalSection = "SourceCodeControl";
        public const string WebsitePropertiesProjectSection = "WebsiteProperties";
        protected const string GlobalEnd = "EndGlobal";
        protected const string GlobalSectionEnd = "EndGlobalSection";
        protected const string GlobalSectionStart = "GlobalSection(";
        protected const string GlobalStart = "Global";
        protected const string PostProject = "postProject";
        protected const string PostSolution = "postSolution";
        protected const string PreProject = "preProject";
        protected const string PreSolution = "preSolution";
        protected const string ProductReleasePrefix = "# ";
        protected const string ProjectEnd = "EndProject";
        protected const string ProjectSectionEnd = "EndProjectSection";
        protected const string ProjectSectionStart = "ProjectSection(";
        protected const string ProjectStart = "Project(";
        protected const string VisualStudioSolutionFileHeader = "Microsoft Visual Studio Solution File";

        /// <summary>
        /// The active configuration (usually 'Debug' or 'Release').
        /// </summary>
        protected string _activeConfiguration;

        /// <summary>
        /// The active platform (usually 'Any CPU', 'x86', or 'Mixed Platforms').
        /// </summary>
        protected string _activePlatform;

        /// <summary>
        /// All 'listed' annotations (<see cref="Message"/>s and special <see cref="Comment"/>s) in this <see cref="Solution"/>.
        /// </summary>
        protected ObservableCollection<CodeAnnotation> _codeAnnotations = new ObservableCollection<CodeAnnotation>();

        protected Dictionary<Annotation, CodeAnnotation> _codeAnnotationsDictionary = new Dictionary<Annotation, CodeAnnotation>();

        /// <summary>
        /// The full file name.
        /// </summary>
        protected string _fileName;

        /// <summary>
        /// The version of the solution file.
        /// (11.00 = VS2010 = v10, 10.00 = VS2008 = v9, 9.00 = VS2005 = v8, 8.00 = VS2003 = v7.1, ? = VS2002 = v7)
        /// </summary>
        protected string _formatVersion;

        /// <summary>
        /// Global sections (blocks of configuration data).
        /// </summary>
        protected List<GlobalSection> _globalSections = new List<GlobalSection>();

        /// <summary>
        /// True if the <see cref="Solution"/> is newly created and hasn't been saved yet.
        /// </summary>
        protected bool _isNew;

        /// <summary>
        /// The name of the <see cref="Solution"/>.
        /// </summary>
        protected string _name;

        /// <summary>
        /// The product release.
        /// (Visual Studio 2010, Visual C# Express 2008, etc.)
        /// </summary>
        protected string _productRelease;

        /// <summary>
        /// Project entries (these represent the entries in the solution file).
        /// </summary>
        protected List<ProjectEntry> _projectEntries = new List<ProjectEntry>();

        /// <summary>
        /// All projects in this solution (will be sorted alphabetically).
        /// </summary>
        protected ChildList<Project> _projects;

        /// <summary>
        /// Callback used to indicate status changes to the UI while loading.
        /// </summary>
        protected Action<LoadStatus, CodeObject> _statusCallback;

        /// <summary>
        /// Create a new <see cref="Solution"/> object.
        /// </summary>
        /// <remarks>
        /// To load an existing solution, use <see cref="Load(string, LoadOptions, Action{LoadStatus, CodeObject})"/>.
        /// </remarks>
        public Solution(string fileName, Action<LoadStatus, CodeObject> statusCallback)
        {
            _name = Path.GetFileNameWithoutExtension(fileName);
            _isNew = true;
            _fileName = fileName;
            if (!Path.HasExtension(fileName))
                _fileName += SolutionFileExtension;
            FileEncoding = Encoding.UTF8;  // Default to UTF8 encoding with a BOM
            FileHasUTF8BOM = true;
            _projects = new ChildList<Project>(this);
            _statusCallback = statusCallback;

            // Default to VS2010
            _formatVersion = "11.00";
            _productRelease = "Visual Studio 2010";

            _globalSections.Add(new GlobalSection(SolutionConfigurationPlatformsGlobalSection, true));
            _globalSections.Add(new GlobalSection(ProjectConfigurationPlatformsGlobalSection, false));
            _globalSections.Add(new GlobalSection(SolutionPropertiesGlobalSection, true, new KeyValuePair<string, string>(HideSolutionNodeProperty, "FALSE")));

            // Determine the active configuration and platform (either specified, from previously saved settings, or defaulted).
            // This is done here even though the Solution is new, because even if it's a dummy solution for a directly-loaded
            // project, we still want to be able to change the active configuration/platform and re-load it.
            DetermineActiveConfigurationAndPlatform();
        }

        /// <summary>
        /// Create a new <see cref="Solution"/> object.
        /// </summary>
        /// <remarks>
        /// To load an existing solution, use <see cref="Load(string, LoadOptions, Action{LoadStatus, CodeObject})"/>.
        /// </remarks>
        public Solution(string fileName)
            : this(fileName, null)
        { }

        static Solution()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            ForceReference();
        }

        /// <summary>
        /// The active configuration (usually 'Debug' or 'Release').
        /// </summary>
        /// <remarks>
        /// After changing the active configuration and/or platform, the solution should be re-loaded
        /// so that files can be parsed according to any defined compiler directive symbols, and so
        /// any generated code-behind files can be loaded from the proper output directory.
        /// </remarks>
        public string ActiveConfiguration
        {
            get { return _activeConfiguration; }
            set { _activeConfiguration = value; }
        }

        /// <summary>
        /// The active platform (usually 'Any CPU', 'x86', or 'Mixed Platforms').
        /// </summary>
        /// <remarks>
        /// After changing the active configuration and/or platform, the solution should be re-loaded
        /// so that files can be parsed according to any defined compiler directive symbols, and so
        /// any generated code-behind files can be loaded from the proper output directory.
        /// </remarks>
        public string ActivePlatform
        {
            get { return _activePlatform; }
            set { _activePlatform = value; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "solution"; }
        }

        /// <summary>
        /// All 'listed' annotations (<see cref="Message"/>s and special <see cref="Comment"/>s) for the entire <see cref="Solution"/>.
        /// </summary>
        public ObservableCollection<CodeAnnotation> CodeAnnotations
        {
            get { return _codeAnnotations; }
        }

        /// <summary>
        /// The encoding of the file (normally UTF8).
        /// </summary>
        public Encoding FileEncoding { get; set; }

        /// <summary>
        /// True if the associated file exists.
        /// </summary>
        public bool FileExists
        {
            get { return File.Exists(_fileName); }
        }

        /// <summary>
        /// True if the file has a UTF8 byte-order-mark.
        /// </summary>
        public bool FileHasUTF8BOM { get; set; }

        /// <summary>
        /// The associated file name of the <see cref="Solution"/>.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Always <c>true</c> for a <see cref="Solution"/>.
        /// </summary>
        public bool FileUsingTabs
        {
            get { return true; }
            set { throw new Exception("Solution files always use tabs!"); }
        }

        /// <summary>
        /// The version number of the solution file format.
        /// </summary>
        public string FormatVersion
        {
            get { return _formatVersion; }
        }

        /// <summary>
        /// The <see cref="GlobalSection"/>s of the associated '.sln' file.
        /// </summary>
        public List<GlobalSection> GlobalSections
        {
            get { return _globalSections; }
        }

        /// <summary>
        /// True if the <see cref="Solution"/> is newly created and hasn't been saved yet.
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
        }

        /// <summary>
        /// The name of the <see cref="Solution"/> (does not include the file path or extension).
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The product release number.
        /// </summary>
        public string ProductRelease
        {
            get { return _productRelease; }
        }

        /// <summary>
        /// The <see cref="ProjectEntry"/>s of the associated '.sln' file.
        /// </summary>
        public List<ProjectEntry> ProjectEntries
        {
            get { return _projectEntries; }
        }

        /// <summary>
        /// The child <see cref="Project"/>s.
        /// </summary>
        public ChildList<Project> Projects
        {
            get { return _projects; }
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution ('.sln') file.</param>
        /// <param name="activeConfiguration">The active configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="activePlatform">The active platform to use (such as 'Any CPU', 'x86', 'Mixed Platforms', etc.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Project file, which is useful
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, string activeConfiguration, string activePlatform, LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            Solution solution = null;
            try
            {
                if (statusCallback != null)
                    statusCallback(LoadStatus.Loading, null);
                Stopwatch overallStopWatch = new Stopwatch();
                overallStopWatch.Start();
                GC.Collect();
                long startBytes = GC.GetTotalMemory(true);

                // Handle a relative path to the file
                if (!Path.IsPathRooted(fileName))
                    fileName = FileUtil.CombineAndNormalizePath(Environment.CurrentDirectory, fileName);

                // Parse the solution, projects, and code units
                Unrecognized.Count = 0;
                solution = Parse(fileName, activeConfiguration, activePlatform, statusCallback);
                if (solution != null)
                {
                    Log.WriteLine("Loaded all projects, elapsed time: " + overallStopWatch.Elapsed.TotalSeconds.ToString("N3"));
                    if (statusCallback != null)
                        statusCallback(LoadStatus.ProjectsLoaded, null);
                    Log.WriteLine("Loaded solution '" + solution.Name + "', total elapsed time: " + overallStopWatch.Elapsed.TotalSeconds.ToString("N3"));

                    if (loadOptions.HasFlag(LoadOptions.ParseSources))
                    {
                        if (statusCallback != null)
                            statusCallback(LoadStatus.Parsing, null);
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        ParseFlags parseFlags = (loadOptions.HasFlag(LoadOptions.DoNotParseBodies) ? ParseFlags.SkipMethodBodies : ParseFlags.None);
                        foreach (Project project in solution.Projects)
                            project.ParseCodeUnits(parseFlags);
                        if (Unrecognized.Count > 0)
                            Log.WriteLine("UNRECOGNIZED OBJECT COUNT: " + Unrecognized.Count);
                        Log.WriteLine("Parsed solution '" + solution.Name + "', elapsed time: " + stopWatch.Elapsed.TotalSeconds.ToString("N3"));
                    }

                    long memoryUsage = GC.GetTotalMemory(true) - startBytes;
                    Log.WriteLine(string.Format("Total elapsed time: {0:N3}, memory usage: {1} MBs", overallStopWatch.Elapsed.TotalSeconds, memoryUsage / (1024 * 1024)));

                    if (statusCallback != null)
                        statusCallback(LoadStatus.LoggingResults, null);
                    solution.LogMessageCounts(loadOptions.HasFlag(LoadOptions.LogMessages));

                    solution._statusCallback = null;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading solution");
            }
            return solution;
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution ('.sln') file.</param>
        /// <param name="activeConfiguration">The active configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="activePlatform">The active platform to use (such as 'Any CPU', 'x86', 'Mixed Platforms', etc.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Project file, which is useful
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, string activeConfiguration, string activePlatform, LoadOptions loadOptions)
        {
            return Load(fileName, activeConfiguration, activePlatform, loadOptions, null);
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution ('.sln') file.</param>
        /// <param name="activeConfiguration">The active configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="activePlatform">The active platform to use (such as 'Any CPU', 'x86', 'Mixed Platforms', etc.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Solution and Project files, which
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, string activeConfiguration, string activePlatform)
        {
            return Load(fileName, activeConfiguration, activePlatform, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution ('.sln') file.</param>
        /// <param name="activeConfiguration">The active configuration to use (usually 'Debug' or 'Release').</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Solution and Project files and resolving References, which
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, string activeConfiguration)
        {
            return Load(fileName, activeConfiguration, null, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution (".sln") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Solution and Project files, which
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            return Load(fileName, null, null, loadOptions, statusCallback);
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution (".sln") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Solution and Project files, which
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName, LoadOptions loadOptions)
        {
            return Load(fileName, null, null, loadOptions, null);
        }

        /// <summary>
        /// Load the specified <see cref="Solution"/> file, including all child <see cref="Project"/>s and code files.
        /// </summary>
        /// <param name="fileName">The solution (".sln") file.</param>
        /// <returns>The resulting <see cref="Solution"/> object, or null if the file didn't exist.</returns>
        /// <remarks>
        /// A 'Complete' load of a Solution goes through the following steps:
        ///   - Parse the Solution file, creating a Solution object and a list of ProjectEntry objects.
        ///   - Parse each Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Parse all code files for each project (in dependency order).
        /// The 'LoadOnly' option stops this process after parsing the Solution and Project files, which
        /// is useful if only the solution and/or project files are being viewed, analyzed, or edited.
        /// </remarks>
        public static Solution Load(string fileName)
        {
            return Load(fileName, null, null, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Add a <see cref="Project"/> to the <see cref="Solution"/>, keeping the <see cref="Projects"/> collection sorted alphabetically.
        /// </summary>
        public void AddProject(Project project)
        {
            if (project != null)
            {
                // Insert the Project at the proper index according to its name.
                // Duplicate names shouldn't exist, but technically could, so just insert any duplicate matches following the existing one.
                int index = _projects.BinarySearch(project);
                _projects.Insert((index < 0 ? ~index : index + 1), project);

                // Add to the Solution's configuration platforms
                foreach (Project.Configuration configuration in project.Configurations)
                    AddProjectConfiguration(configuration);
            }
        }

        /// <summary>
        /// Add the configuration and platform from the specified <see cref="Project.Configuration"/>.
        /// </summary>
        public void AddProjectConfiguration(Project.Configuration configuration)
        {
            // Add to the Solution's configuration platform section
            string platform = (configuration.Platform ?? PlatformAnyCPU);
            if (platform == Project.PlatformAnyCPU)
                platform = PlatformAnyCPU;
            string configurationPlatform = configuration.Name + "|" + platform;
            FindGlobalSection(SolutionConfigurationPlatformsGlobalSection).AddKeyValue(configurationPlatform, configurationPlatform);

            // Add default mappings to the Project's configuration platform mappings
            GlobalSection globalSection = FindGlobalSection(ProjectConfigurationPlatformsGlobalSection);
            Project project = (Project)configuration.Parent;
            string projectGuid = project.ProjectGuid.ToString("B").ToUpper();
            string projectConfigurationPlatform = configuration.Name;
            if (project.TypeGuid != Project.SetupProjectType)
                projectConfigurationPlatform += "|" + platform;
            string prefix = projectGuid + "." + configurationPlatform;
            globalSection.AddKeyValue(prefix + ".ActiveCfg", projectConfigurationPlatform);
            if (platform != PlatformDotNet)
                globalSection.AddKeyValue(prefix + ".Build.0", projectConfigurationPlatform);
            if (project.ProjectTypeGuids != null && project.ProjectTypeGuids.Count > 0 && project.ProjectTypeGuids.Contains(Project.VisualDBToolsProjectType))
                globalSection.AddKeyValue(prefix + ".Deploy.0", projectConfigurationPlatform);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(_name, this);
        }

        /// <summary>
        /// Add a listed annotation to the <see cref="Solution"/>.
        /// </summary>
        public void AnnotationAdded(Annotation annotation, Project project, CodeUnit codeUnit, bool sendStatus)
        {
            try
            {
                CodeAnnotation codeAnnotation = new CodeAnnotation(annotation, project, codeUnit);
                // The following line has been seen to throw an exception when the LOST COMMENT logic in the Token finalizer is
                // triggered, complaining that this is executed during the processing of a CollectionChanged event on the same
                // collection, even though this situation couldn't be found in any thread in the debugger.  So, we wrap this
                // method in a try/catch and swallow any such (very-low frequency) event - we'll still get a Log message for
                // the lost comment in the Output window (lost comments are also only tracked in Debug builds).
                lock (this)
                {
                    _codeAnnotations.Add(codeAnnotation);
                    _codeAnnotationsDictionary.Add(annotation, codeAnnotation);
                }

                // Send a status change if loading and an annotation (message) is added to a file (solution, project, reference,
                // or code unit), so that the UI can indicate any errors in the file tree.
                if (sendStatus && _statusCallback != null)
                    _statusCallback(LoadStatus.ObjectAnnotated, annotation.Parent);
            }
            catch { }
        }

        /// <summary>
        /// Remove a listed annotation from the <see cref="Solution"/>.
        /// </summary>
        public void AnnotationRemoved(Annotation annotation)
        {
            lock (this)
            {
                CodeAnnotation codeAnnotation;
                if (_codeAnnotationsDictionary.TryGetValue(annotation, out codeAnnotation))
                {
                    _codeAnnotations.Remove(codeAnnotation);
                    _codeAnnotationsDictionary.Remove(annotation);
                }
            }
        }

        /// <summary>
        /// Create and add a new <see cref="Project"/> to the solution by filename.
        /// </summary>
        public Project CreateProject(string fileName)
        {
            Project project = new Project(fileName, this);
            AddProject(project);
            _projectEntries.Add(new ProjectEntry(Project.CSProjectType, project));
            return project;
        }

        /// <summary>
        /// Find any GlobalSection with the specified name.
        /// </summary>
        public GlobalSection FindGlobalSection(string name)
        {
            return Enumerable.FirstOrDefault(_globalSections, delegate (GlobalSection globalSection) { return globalSection.Name == name; });
        }

        /// <summary>
        /// Find a project by name.
        /// </summary>
        public Project FindProject(string name)
        {
            return Enumerable.FirstOrDefault(_projects, delegate (Project project) { return StringUtil.NNEqualsIgnoreCase(project.Name, name); });
        }

        /// <summary>
        /// Find a project by its GUID.
        /// </summary>
        public Project FindProject(Guid projectGuid)
        {
            return Enumerable.FirstOrDefault(_projects, delegate (Project project) { return project.ProjectGuid == projectGuid; });
        }

        /// <summary>
        /// Find a project by its assembly name.
        /// </summary>
        public Project FindProjectByAssemblyName(string assemblyName)
        {
            return Enumerable.FirstOrDefault(_projects, delegate (Project project) { return StringUtil.NNEqualsIgnoreCase(project.AssemblyName, assemblyName); });
        }

        /// <summary>
        /// Find a project by its full file name.
        /// </summary>
        public Project FindProjectByFileName(string fullFileName)
        {
            return Enumerable.FirstOrDefault(_projects, delegate (Project project) { return StringUtil.NNEqualsIgnoreCase(project.FileName, fullFileName); });
        }

        /// <summary>
        /// Find any ProjectEntry with the specified name.
        /// </summary>
        public ProjectEntry FindProjectEntry(string name)
        {
            return Enumerable.FirstOrDefault(_projectEntries, delegate (ProjectEntry projectEntry) { return projectEntry.Name == name; });
        }

        /// <summary>
        /// Find any ProjectEntry with the specified Guid.
        /// </summary>
        public ProjectEntry FindProjectEntry(Guid guid)
        {
            return Enumerable.FirstOrDefault(_projectEntries, delegate (ProjectEntry projectEntry) { return projectEntry.Guid == guid; });
        }

        /// <summary>
        /// Determine lists of unique configurations and platforms from the appropriate global section.
        /// </summary>
        /// <param name="configurations">The list of configurations.</param>
        /// <param name="platforms">The list of platforms.</param>
        public void GetConfigurationsAndPlatforms(out List<string> configurations, out List<string> platforms)
        {
            configurations = null;
            platforms = null;
            GlobalSection globalSection = FindGlobalSection(SolutionConfigurationPlatformsGlobalSection);
            List<string> uniqueConfigurations = new List<string>();
            List<string> uniquePlatforms = new List<string>();
            foreach (KeyValuePair<string, string> keyValuePair in globalSection.KeyValuePairs)
            {
                string[] parts = keyValuePair.Key.Split('|');
                if (!uniqueConfigurations.Contains(parts[0]))
                    uniqueConfigurations.Add(parts[0]);
                if (parts.Length > 1 && !uniquePlatforms.Contains(parts[1]))
                    uniquePlatforms.Add(parts[1]);
            }
            if (uniqueConfigurations.Count > 0)
                configurations = uniqueConfigurations;
            if (uniquePlatforms.Count > 0)
                platforms = uniquePlatforms;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName(bool descriptive)
        {
            return _name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return _name;
        }

        /// <summary>
        /// Calculate message counts.
        /// </summary>
        public void GetMessageCounts(out int errorCount, out int warningCount, out int commentCount)
        {
            // Calculate message counts
            errorCount = warningCount = commentCount = 0;
            foreach (CodeAnnotation codeAnnotation in _codeAnnotations)
            {
                if (codeAnnotation.Annotation is Message)
                {
                    Message message = (Message)codeAnnotation.Annotation;
                    if (message.Severity == MessageSeverity.Error)
                        ++errorCount;
                    else if (message.Severity == MessageSeverity.Warning)
                        ++warningCount;
                }
                else if (codeAnnotation.Annotation is Comment)
                    ++commentCount;
            }
        }

        /// <summary>
        /// Get the configuration and platform for the specified project for the given solution configuration and platform.
        /// </summary>
        /// <param name="solutionConfiguration">The solution configuration.</param>
        /// <param name="solutionPlatform">The solution platform.</param>
        /// <param name="project">The <see cref="Project"/>.</param>
        /// <param name="projectConfiguration">The configuration of the <see cref="Project"/>.</param>
        /// <param name="projectPlatform">The platform of the <see cref="Project"/> (can be null).</param>
        public void GetProjectConfiguration(string solutionConfiguration, string solutionPlatform, Project project, out string projectConfiguration, out string projectPlatform)
        {
            projectConfiguration = null;
            projectPlatform = null;
            GlobalSection globalSection = FindGlobalSection(ProjectConfigurationPlatformsGlobalSection);
            if (globalSection != null)
            {
                string result = globalSection.FindValue(project.ProjectGuid.ToString("B").ToUpper() + "." + solutionConfiguration + "|" + solutionPlatform + ".ActiveCfg");
                // If no match was found, and the platform wasn't "Any CPU", then try that
                if (result == null && solutionPlatform != PlatformAnyCPU)
                    result = globalSection.FindValue(project.ProjectGuid.ToString("B").ToUpper() + "." + solutionConfiguration + "|" + PlatformAnyCPU + ".ActiveCfg");
                if (result != null)
                {
                    string[] parts = result.Split('|');
                    projectConfiguration = parts[0];
                    if (parts.Length > 1)
                    {
                        projectPlatform = parts[1];
                        if (projectPlatform == PlatformAnyCPU)
                            projectPlatform = Project.PlatformAnyCPU;
                    }
                }
            }
        }

        /// <summary>
        /// Get any Solution folders for the specified Project.
        /// </summary>
        public string GetSolutionFolders(Project project)
        {
            string solutionFolders = null;
            GlobalSection nestedProjects = FindGlobalSection(NestedProjectsGlobalSection);
            if (nestedProjects != null)
            {
                Guid projectGuid = project.ProjectGuid;
                while (true)
                {
                    string parentGuid = nestedProjects.FindValue(projectGuid.ToString("B").ToUpper());
                    if (parentGuid == null)
                        break;
                    ProjectEntry parentProject = FindProjectEntry(Guid.Parse(parentGuid));
                    if (parentProject == null)
                        break;
                    solutionFolders = (string.IsNullOrEmpty(solutionFolders) ? parentProject.Name : parentProject.Name + "\\" + solutionFolders);
                    projectGuid = parentProject.Guid;
                }
            }
            return solutionFolders;
        }

        /// <summary>
        /// Get the directory for the specified web site project.
        /// </summary>
        public string GetWebSiteDirectory(string projectName)
        {
            ProjectEntry projectEntry = FindProjectEntry(projectName);
            if (projectEntry != null)
            {
                string relativePath = null;
                if (projectEntry.FileName.StartsWith("http:"))
                {
                    ProjectSection projectSection = projectEntry.FindProjectSection(WebsitePropertiesProjectSection);
                    if (projectSection != null)
                        relativePath = projectSection.FindValue("SlnRelativePath").Trim('"');
                }
                else
                    relativePath = projectEntry.FileName;
                if (relativePath != null)
                    return Path.Combine(Path.GetDirectoryName(FileName) ?? "", relativePath.TrimEnd('\\'));
            }
            return null;
        }

        /// <summary>
        /// Log the specified exception and message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachException(Exception ex, string message, MessageSource source)
        {
            message = LogException(ex, message);
            AttachMessage(message, MessageSeverity.Error, source);
        }

        /// <summary>
        /// Log the specified text message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachMessage(string message, MessageSeverity severity, MessageSource source, string toolTip)
        {
            LogMessage(message, severity, toolTip);
            AttachMessage(message, severity, source);
        }

        /// <summary>
        /// Log the specified text message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachMessage(string message, MessageSeverity severity, MessageSource source)
        {
            LogAndAttachMessage(message, severity, source, null);
        }

        /// <summary>
        /// Log the specified exception and message.
        /// </summary>
        public string LogException(Exception ex, string message)
        {
            return Log.Exception(ex, message + " solution '" + _name + "'");
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity, string toolTip)
        {
            string prefix = (severity == MessageSeverity.Error ? "ERROR: " : (severity == MessageSeverity.Warning ? "Warning: " : ""));
            Log.WriteLine(prefix + "Solution '" + _name + "': " + message, toolTip != null ? toolTip.TrimEnd() : null);
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity)
        {
            LogMessage(message, severity, null);
        }

        /// <summary>
        /// Log message counts, and optionally errors and warnings (or all messages if detail logging is on).
        /// </summary>
        public void LogMessageCounts(bool logMessages)
        {
            // Calculate and log message counts
            int errorCount, warningCount, commentCount;
            GetMessageCounts(out errorCount, out warningCount, out commentCount);
            Log.WriteLine(string.Format("{0:N0} messages ({1:N0} errors; {2:N0} warnings; {3:N0} comments)", _codeAnnotations.Count, errorCount, warningCount, commentCount));

            // Log errors and warnings if requested
            if (logMessages)
            {
                foreach (CodeAnnotation codeAnnotation in _codeAnnotations)
                {
                    // Log all messages if the LogLevel is Detailed, log Warnings if Normal, and Errors if Minimal
                    Message message = codeAnnotation.Annotation as Message;
                    if (Log.LogLevel >= Log.Level.Detailed || (message != null
                        && ((Log.LogLevel >= Log.Level.Normal && message.Severity == MessageSeverity.Warning)
                        || (Log.LogLevel >= Log.Level.Minimal && message.Severity == MessageSeverity.Error))))
                        Log.WriteLine(codeAnnotation.ToString());
                }
            }
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(_name, this);
        }

        /// <summary>
        /// Save the <see cref="Solution"/>.
        /// </summary>
        public void Save()
        {
            SaveAs(CodeUnit.GetSaveFileName(_fileName));
        }

        /// <summary>
        /// Save the <see cref="Solution"/> plus all <see cref="Project"/>s and all <see cref="CodeUnit"/>s.
        /// </summary>
        public void SaveAll()
        {
            Save();
            foreach (Project project in Projects)
                project.SaveAll();
        }

        /// <summary>
        /// Save the <see cref="Solution"/> to the specified file name.
        /// </summary>
        public void SaveAs(string fileName)
        {
            Log.WriteLine("Saving solution to '" + fileName + "' ...");

            // VS solution files use tabs, but this is handled by the rendering routines.
            try
            {
                using (CodeWriter writer = new CodeWriter(fileName, FileEncoding, FileHasUTF8BOM, false))
                    AsText(writer, RenderFlags.None);
            }
            catch (Exception ex)
            {
                LogException(ex, "writing");
            }
            _isNew = false;
        }

        /// <summary>
        /// Unload the <see cref="Solution"/> - unload all <see cref="Project"/>s, clear all <see cref="CodeAnnotation"/>s, etc.
        /// </summary>
        public void Unload()
        {
            lock (this)
            {
                _codeAnnotations.Clear();
                _codeAnnotationsDictionary.Clear();
                foreach (Project project in _projects)
                    project.Unload();
            }
        }

        protected override void NotifyListedAnnotationAdded(Annotation annotation)
        {
            AnnotationAdded(annotation, null, null, true);
        }

        protected override void NotifyListedAnnotationRemoved(Annotation annotation)
        {
            AnnotationRemoved(annotation);
        }

        /// <summary>
        /// Parse a solution from a standard VS solution file.
        /// </summary>
        /// <param name="fileName">The solution file name.</param>
        /// <param name="activeConfiguration">The active configuration to be used (will default if null).</param>
        /// <param name="activePlatform">The active platform to be used (will default if null).</param>
        /// <param name="statusCallback">An action to be executed as status changes occur during processing.</param>
        protected Solution(string fileName, string activeConfiguration, string activePlatform, Action<LoadStatus, CodeObject> statusCallback)
        {
            _name = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
            Log.WriteLine("Loading solution '" + _name + "' ...");
            _fileName = fileName;
            _activeConfiguration = activeConfiguration;
            _activePlatform = activePlatform;
            _projects = new ChildList<Project>(this);
            _statusCallback = statusCallback;
            if (statusCallback != null)
                statusCallback(LoadStatus.ObjectCreated, this);

            try
            {
                // Open the file and store the encoding and BOM status for use when saving
                FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] bom = new byte[3];
                fileStream.Read(bom, 0, 3);
                if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    FileHasUTF8BOM = true;
                fileStream.Position = 0;

                // Parse the file using a StreamReader
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    reader.Peek();  // Peek at the first char so that the encoding is determined
                    FileEncoding = reader.CurrentEncoding;

                    // Parse projects and other settings from the solution file
                    bool lookingForHeader = true;
                    do
                    {
                        string line = ReadLine(reader);
                        if (line == null) break;
                        if (lookingForHeader)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            // Validate the file format
                            if (!line.StartsWith(VisualStudioSolutionFileHeader))
                            {
                                LogAndAttachMessage("Solution file format not recognized!", MessageSeverity.Error, MessageSource.Parse);
                                return;
                            }

                            // Extract the format version number
                            _formatVersion = line.Substring(line.LastIndexOf(' ') + 1);

                            lookingForHeader = false;
                            continue;
                        }

                        // Check for product release comment
                        if (line.StartsWith(ProductReleasePrefix))
                            _productRelease = line.Substring(ProductReleasePrefix.Length);
                        // Check for a project entry
                        else if (line.StartsWith(ProjectStart))
                            ParseProject(reader, line);
                        // Check for a global section
                        else if (line.StartsWith(GlobalSectionStart))
                        {
                            GlobalSection globalSection = new GlobalSection(reader, line, this);
                            _globalSections.Add(globalSection);
                        }
                        else if (line == GlobalStart || line == GlobalEnd)
                        {
                            // Just ignore these lines
                        }
                        else if (!string.IsNullOrEmpty(line))
                            UnrecognizedLine(line);
                    }
                    while (true);
                }

                // Determine the active configuration and platform (either specified, from previously saved settings, or defaulted).
                DetermineActiveConfigurationAndPlatform();
                Log.WriteLine("Active Configuration and Platform: " + _activeConfiguration + " - " + _activePlatform);

                if (statusCallback != null)
                    statusCallback(LoadStatus.SolutionLoaded, null);

                // Now that we've parsed all of the ProjectEntries, create the Project objects (this is done separately so that
                // the UI can know how many projects there are before they start getting created).
                foreach (ProjectEntry projectEntry in _projectEntries)
                {
                    if (!projectEntry.IsFolder)
                    {
                        string projectFileName = projectEntry.FileName;
                        if (projectFileName != null && !projectFileName.StartsWith("http:") && !Path.IsPathRooted(projectFileName))
                            projectFileName = FileUtil.CombineAndNormalizePath(Path.GetDirectoryName(_fileName), projectFileName);
                        Project project = Project.Parse(projectEntry.Name, projectFileName, projectEntry.TypeGuid, projectEntry.Guid, this, statusCallback);
                        AddProject(project);
                        if (statusCallback != null)
                            statusCallback(LoadStatus.ProjectParsed, project);
                        projectEntry.Project = project;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing", MessageSource.Parse);
            }
        }

        /// <summary>
        /// Parse a solution from a file.
        /// </summary>
        /// <param name="fileName">The solution file name.</param>
        /// <param name="activeConfiguration">The active configuration to be used (will default if null).</param>
        /// <param name="activePlatform">The active platform to be used (will default if null).</param>
        /// <param name="statusCallback">An action to be executed as status changes occur during processing.</param>
        public static Solution Parse(string fileName, string activeConfiguration, string activePlatform, Action<LoadStatus, CodeObject> statusCallback)
        {
            if (File.Exists(fileName))
                return new Solution(fileName, activeConfiguration, activePlatform, statusCallback);

            Log.WriteLine("ERROR: Solution file '" + fileName + "' does not exist.");
            return null;
        }

        /// <summary>
        /// Parse a solution from a file.
        /// </summary>
        /// <param name="fileName">The solution file name.</param>
        /// <param name="activeConfiguration">The active configuration to be used (will default if null).</param>
        /// <param name="activePlatform">The active platform to be used (will default if null).</param>
        public static Solution Parse(string fileName, string activeConfiguration, string activePlatform)
        {
            return Parse(fileName, activeConfiguration, activePlatform, null);
        }

        /// <summary>
        /// Parse a solution from a file.
        /// </summary>
        /// <param name="fileName">The solution file name.</param>
        /// <param name="activeConfiguration">The active configuration to be used (will default if null).</param>
        public static Solution Parse(string fileName, string activeConfiguration)
        {
            return Parse(fileName, activeConfiguration, null, null);
        }

        /// <summary>
        /// Parse a solution from a file.
        /// </summary>
        /// <param name="fileName">The solution file name.</param>
        public static Solution Parse(string fileName)
        {
            return Parse(fileName, null, null, null);
        }

        /// <summary>
        /// Load user options (such as active configuration/platform) from any '.nuo' file.
        /// </summary>
        public void LoadUserOptions()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(Path.ChangeExtension(FileName, SolutionOptionsExtension)))
                    {
                        bool firstElement = true;
                        //int version = 0;

                        // Read the next node
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element)
                            {
                                if (firstElement)
                                {
                                    firstElement = false;
                                    if (xmlReader.Name != "Furesoft.Core.CodeDomSolutionOptions")
                                        return;
                                    //if (xmlReader.MoveToAttribute("Version"))
                                    //    version = xmlReader.Value.ParseInt();
                                }
                                else if (xmlReader.Name == "ActiveConfiguration" && !xmlReader.IsEmptyElement)
                                    ActiveConfiguration = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "ActivePlatform" && !xmlReader.IsEmptyElement)
                                    ActivePlatform = xmlReader.ReadString().Trim();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Save user options (such as active configuration/platform) to a '.nuo' file.
        /// </summary>
        public void SaveUserOptions()
        {
            try
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(Path.ChangeExtension(FileName, SolutionOptionsExtension), Encoding.ASCII))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Furesoft.Core.CodeDomSolutionOptions");
                    xmlWriter.WriteAttributeString("Version", "1");
                    xmlWriter.WriteElementString("ActiveConfiguration", ActiveConfiguration);
                    xmlWriter.WriteElementString("ActivePlatform", ActivePlatform);
                    xmlWriter.WriteEndDocument();
                }
            }
            catch { }
        }

        /// <summary>
        /// Determine the active configuration and platform (either from previously saved settings, or defaults).
        /// </summary>
        protected void DetermineActiveConfigurationAndPlatform()
        {
            if (_activeConfiguration == null)
            {
                // First, create lists of valid configurations and platforms from the appropriate global section (we can't build it
                // from the project files, because we have to do this before we read them, so we have to rely on the global section in
                // the solution file being correct).
                GlobalSection globalSection = FindGlobalSection(SolutionConfigurationPlatformsGlobalSection);
                List<string> uniqueConfigurations = new List<string>();
                List<string> uniquePlatforms = new List<string>();
                foreach (KeyValuePair<string, string> keyValuePair in globalSection.KeyValuePairs)
                {
                    string[] parts = keyValuePair.Key.Split('|');
                    if (!uniqueConfigurations.Contains(parts[0]))
                        uniqueConfigurations.Add(parts[0]);
                    if (parts.Length > 1 && !uniquePlatforms.Contains(parts[1]))
                        uniquePlatforms.Add(parts[1]);
                }

                // Try to read any previously saved active configuration and platform from the '.nuo' settings file (if any).
                LoadUserOptions();

                // If there weren't any saved settings or they weren't valid, default the configuration to 'Debug' or whatever is first,
                // and default the platform to 'Any CPU' or whatever is first.
                if (_activeConfiguration == null && uniqueConfigurations.Count > 0)
                {
                    if (uniqueConfigurations.Contains(Project.ConfigurationDebug))
                        _activeConfiguration = Project.ConfigurationDebug;
                    else
                        _activeConfiguration = uniqueConfigurations[0];
                }
                if (_activePlatform == null && uniquePlatforms.Count > 0)
                {
                    if (uniquePlatforms.Contains(PlatformAnyCPU))
                        _activePlatform = PlatformAnyCPU;
                    else
                        _activePlatform = uniquePlatforms[0];
                }
            }
        }

        /// <summary>
        /// Parse a project.
        /// </summary>
        protected void ParseProject(StreamReader reader, string line)
        {
            try
            {
                int start = ProjectStart.Length;
                int end = line.IndexOf(')', start);
                if (end > 0)
                {
                    // Parse the ProjectEntry object
                    Guid typeGuid = Guid.Parse(line.Substring(start, end - start).Trim().Trim('"'));
                    string[] args = line.Substring(end + 3).Split(',');
                    string projectName = (args.Length > 0 ? args[0].Trim().Trim('"') : null);
                    string projectFileName = (args.Length > 1 ? args[1].Trim().Trim('"') : null);
                    Guid projectGuid = Guid.Parse(args.Length > 2 ? args[2].Trim().Trim('"') : "");
                    ProjectEntry projectEntry = new ProjectEntry(reader, typeGuid, projectName, projectFileName, projectGuid, this);
                    _projectEntries.Add(projectEntry);
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing Project entry for", MessageSource.Parse);
            }
        }

        protected string ReadLine(StreamReader reader)
        {
            string line = reader.ReadLine();
            if (!string.IsNullOrEmpty(line))
                line = line.Trim();
            return line;
        }

        protected void UnrecognizedLine(string line)
        {
            LogAndAttachMessage("Unrecognized line while parsing: '" + line + "'", MessageSeverity.Error, MessageSource.Parse);
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public override bool IsRenderable
        {
            get { return false; }
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.Description))
                writer.Write(_name);
            else
            {
                // Write the file header
                writer.WriteLine();
                writer.WriteLine("Microsoft Visual Studio Solution File, Format Version " + _formatVersion);
                if (_productRelease != null)
                    writer.WriteLine(ProductReleasePrefix + _productRelease);

                // Write all project entries
                foreach (ProjectEntry projectEntry in _projectEntries)
                    projectEntry.AsText(writer);

                writer.WriteLine(GlobalStart);

                // Determine all unique configuration platforms across all projects so that we can generate the related global sections
                HashSet<string> uniqueConfigurations = new HashSet<string>();
                HashSet<string> uniquePlatforms = new HashSet<string>();
                bool hasNullPlatform = false;
                foreach (Project project in _projects)
                {
                    foreach (Project.Configuration projectConfiguration in project.Configurations)
                    {
                        uniqueConfigurations.Add(projectConfiguration.Name);
                        string platform = projectConfiguration.Platform;
                        if (platform == null)
                        {
                            uniquePlatforms.Add(PlatformDotNet);
                            hasNullPlatform = true;
                        }
                        else
                        {
                            if (platform == Project.PlatformAnyCPU)
                                platform = PlatformAnyCPU;
                            uniquePlatforms.Add(platform);
                        }
                    }
                }
                // If we have multiple platforms, and more than one project, add 'Mixed Platforms'
                if (uniquePlatforms.Count > (hasNullPlatform ? 2 : 1) && Projects.Count > 1)
                    uniquePlatforms.Add(PlatformMixed);
                List<string> uniqueConfigurationPlatforms = Enumerable.ToList((Enumerable.SelectMany<string, string, string>(uniqueConfigurations,
                    delegate (string configuration) { return uniquePlatforms; }, delegate (string configuration, string platform) { return configuration + "|" + platform; })));
                uniqueConfigurationPlatforms.Sort();

                // Write out all global sections
                foreach (GlobalSection globalSection in _globalSections)
                {
                    // Re-write the Solution's configuration platforms section based upon the project configurations,
                    // because it should always be safe to do this.
                    if (globalSection.Name == SolutionConfigurationPlatformsGlobalSection)
                    {
                        GlobalSection.AsTextHeader(writer, SolutionConfigurationPlatformsGlobalSection, true);
                        foreach (string configurationPlatform in uniqueConfigurationPlatforms)
                            writer.WriteLine("\t\t" + configurationPlatform + " = " + configurationPlatform);
                        GlobalSection.AsTextFooter(writer);
                    }
                    else
                        globalSection.AsText(writer);
                }

                writer.WriteLine(GlobalEnd);
            }
        }

        protected void AsTextProjectEnd(CodeWriter writer)
        {
            writer.WriteLine(ProjectEnd);
        }

        protected void AsTextProjectStart(CodeWriter writer, Guid typeGuid, string name, string fileName, Guid projectGuid)
        {
            writer.WriteLine(ProjectStart + "\"" + typeGuid.ToString("B").ToUpper() + "\") = \"" + name + "\", \""
                + FileUtil.MakeRelative(FileName, fileName) + "\", \"" + projectGuid.ToString("B").ToUpper() + "\"");
        }

        /// <summary>
        /// Represents a project entry in a <see cref="Solution"/> file.
        /// </summary>
        public class ProjectEntry : CodeObject
        {
            public string FileName;
            public Guid Guid;
            public string Name;

            /// <summary>
            /// The associated <see cref="Project"/> object.
            /// </summary>
            public Project Project;

            public List<ProjectSection> ProjectSections = new List<ProjectSection>();
            public Guid TypeGuid;

            /// <summary>
            /// Create a <see cref="ProjectEntry"/>.
            /// </summary>
            public ProjectEntry(Guid typeGuid, Project project)
            {
                TypeGuid = typeGuid;
                Name = project.Name;
                FileName = project.FileName;
                Guid = project.ProjectGuid;
                Project = project;
                Parent = project.Solution;
            }

            /// <summary>
            /// Determine if the ProjectEntry represents a folder instead of an actual project.
            /// </summary>
            public bool IsFolder
            {
                get { return (TypeGuid == Project.FolderType); }
            }

            /// <summary>
            /// The parent <see cref="Project"/>.
            /// </summary>
            public Solution ParentSolution
            {
                get { return _parent as Solution; }
            }

            /// <summary>
            /// Find any ProjectSection with the specified name.
            /// </summary>
            public ProjectSection FindProjectSection(string name)
            {
                return Enumerable.FirstOrDefault(ProjectSections, delegate (ProjectSection projectSection) { return projectSection.Name == name; });
            }

            /// <summary>
            /// Parse from the specified <see cref="StreamReader"/>.
            /// </summary>
            public ProjectEntry(StreamReader reader, Guid typeGuid, string name, string fileName, Guid guid, Solution parent)
            {
                Parent = parent;
                TypeGuid = typeGuid;
                Name = name;
                FileName = fileName;
                Guid = guid;

                do
                {
                    string line = ParentSolution.ReadLine(reader);
                    if (line == null || line == ProjectEnd) break;
                    if (line.StartsWith(ProjectSectionStart))
                        ProjectSections.Add(new ProjectSection(reader, line, parent));
                    else
                        parent.UnrecognizedLine(line);
                }
                while (true);
            }

            /// <summary>
            /// Write to the specified <see cref="CodeWriter"/>.
            /// </summary>
            public void AsText(CodeWriter writer)
            {
                ParentSolution.AsTextProjectStart(writer, TypeGuid, Name, FileName, Guid);
                foreach (ProjectSection projectSection in ProjectSections)
                    projectSection.AsText(writer);
                ParentSolution.AsTextProjectEnd(writer);
            }
        }

        /// <summary>
        /// Represents a project-level section of configuration data in a solution file.
        /// </summary>
        public class ProjectSection
        {
            public bool IsPreProject;
            public List<KeyValuePair<string, string>> KeyValues = new List<KeyValuePair<string, string>>();
            public string Name;

            /// <summary>
            /// Create a <see cref="ProjectSection"/>.
            /// </summary>
            public ProjectSection(string name, bool isPreProject)
            {
                Name = name;
                IsPreProject = isPreProject;
            }

            /// <summary>
            /// Find the value with the specified key.
            /// </summary>
            public string FindValue(string key)
            {
                return Enumerable.FirstOrDefault(KeyValues, delegate (KeyValuePair<string, string> keyValue) { return keyValue.Key == key; }).Value;
            }

            /// <summary>
            /// Parse a <see cref="ProjectSection"/>.
            /// </summary>
            public ProjectSection(StreamReader reader, string line, Solution parent)
            {
                if (line.StartsWith(ProjectSectionStart))
                {
                    int start = ProjectSectionStart.Length;
                    int end = line.IndexOf(')', start);
                    if (end > 0)
                    {
                        Name = line.Substring(start, end - start);
                        IsPreProject = (line.Substring(end).Contains(PreProject));
                    }
                    else
                        parent.UnrecognizedLine(line);
                    do
                    {
                        line = parent.ReadLine(reader);
                        if (line == null || line == ProjectSectionEnd) break;
                        end = line.IndexOf(" = ");
                        if (end > 0)
                            KeyValues.Add(new KeyValuePair<string, string>(line.Substring(0, end), line.Substring(end + 3)));
                        else
                            parent.UnrecognizedLine(line);
                    }
                    while (true);
                }
            }

            public static void AsTextFooter(CodeWriter writer)
            {
                writer.WriteLine("\t" + ProjectSectionEnd);
            }

            public static void AsTextHeader(CodeWriter writer, string name, bool isPreProject)
            {
                writer.WriteLine("\t" + ProjectSectionStart + name + ") = " + (isPreProject ? PreProject : PostProject));
            }

            public void AsText(CodeWriter writer)
            {
                AsTextHeader(writer, Name, IsPreProject);
                foreach (KeyValuePair<string, string> keyValuePair in KeyValues)
                    writer.WriteLine("\t\t" + keyValuePair.Key + " = " + keyValuePair.Value);
                AsTextFooter(writer);
            }
        }

        /// <summary>
        /// Represents a global-level section of configuration data in a solution file.
        /// </summary>
        public class GlobalSection
        {
            /// <summary>
            /// True if the <see cref="GlobalSection"/> is 'pre-solution' (otherwise, it is 'post-solution').
            /// </summary>
            public bool IsPreSolution;

            /// <summary>
            /// The name of the <see cref="GlobalSection"/>.
            /// </summary>
            public string Name;

            // Store the key/values in a Dictionary (for quick lookups and no duplicates), but also keep a List of KeyValuePairs
            // to preserve the order.  Also, the stupid VSS data has duplicate keys (appears to be a mistake, such as 'CanCheckoutShared'
            // missing a number on the end), so we ignore collisions when adding to the dictionary and such duplicate keys are ony
            // accessible via the List.
            private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

            private readonly List<KeyValuePair<string, string>> _keyValuePairs = new List<KeyValuePair<string, string>>();

            /// <summary>
            /// Create a <see cref="GlobalSection"/>.
            /// </summary>
            public GlobalSection(string name, bool isPreSolution, params KeyValuePair<string, string>[] keyValuePairs)
            {
                Name = name;
                IsPreSolution = isPreSolution;
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                    AddKeyValue(keyValuePair.Key, keyValuePair.Value);
            }

            /// <summary>
            /// The list of key/value pairs ordered as they appear in the solution file.
            /// </summary>
            public List<KeyValuePair<string, string>> KeyValuePairs
            {
                get { return _keyValuePairs; }
            }

            /// <summary>
            /// Add a key-value pair.
            /// </summary>
            public void AddKeyValue(string key, string value, bool allowDuplicateKeys)
            {
                bool keyExists = _dictionary.ContainsKey(key);
                if (!keyExists || allowDuplicateKeys)
                {
                    if (!keyExists)
                        _dictionary.Add(key, value);
                    _keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            /// <summary>
            /// Add a key-value pair.
            /// </summary>
            public void AddKeyValue(string key, string value)
            {
                AddKeyValue(key, value, false);
            }

            /// <summary>
            /// Find the value with the specified key (only looks in the dictionary, so won't find duplicate keys).
            /// </summary>
            public string FindValue(string key)
            {
                string value;
                _dictionary.TryGetValue(key, out value);
                return value;
            }

            /// <summary>
            /// Remove the key-value pair with the specified key.
            /// </summary>
            public void RemoveKey(string key)
            {
                string value;
                if (_dictionary.TryGetValue(key, out value))
                {
                    _dictionary.Remove(key);
                    _keyValuePairs.RemoveAll(delegate (KeyValuePair<string, string> keyValuePair) { return keyValuePair.Key == key && keyValuePair.Value == value; });
                }
            }

            /// <summary>
            /// Parse a <see cref="GlobalSection"/> from the specified <see cref="StreamReader"/>.
            /// </summary>
            public GlobalSection(StreamReader reader, string line, Solution parent)
            {
                if (line.StartsWith(GlobalSectionStart))
                {
                    bool allowDuplicateKeys = true;
                    int start = GlobalSectionStart.Length;
                    int end = line.IndexOf(')', start);
                    if (end > 0)
                    {
                        Name = line.Substring(start, end - start);
                        if (Name == SolutionConfigurationPlatformsGlobalSection || Name == ProjectConfigurationPlatformsGlobalSection)
                            allowDuplicateKeys = false;
                        IsPreSolution = (line.Substring(end).Contains(PreSolution));
                    }
                    else
                        parent.UnrecognizedLine(line);
                    do
                    {
                        line = parent.ReadLine(reader);
                        if (line == null || line == GlobalSectionEnd) break;
                        end = line.IndexOf(" = ");
                        if (end > 0)
                            AddKeyValue(line.Substring(0, end), line.Substring(end + 3), allowDuplicateKeys);
                        else
                            parent.UnrecognizedLine(line);
                    }
                    while (true);
                }
            }

            public static void AsTextFooter(CodeWriter writer)
            {
                writer.WriteLine("\t" + GlobalSectionEnd);
            }

            public static void AsTextHeader(CodeWriter writer, string name, bool isPreSolution)
            {
                writer.WriteLine("\t" + GlobalSectionStart + name + ") = " + (isPreSolution ? PreSolution : PostSolution));
            }

            public void AsText(CodeWriter writer)
            {
                AsTextHeader(writer, Name, IsPreSolution);
                foreach (KeyValuePair<string, string> keyValuePair in _keyValuePairs)
                    writer.WriteLine("\t\t" + keyValuePair.Key + " = " + keyValuePair.Value);
                AsTextFooter(writer);
            }
        }
    }

    /// <summary>
    /// Represents a 'listed' code <see cref="Annotation"/> and its location (<see cref="Project"/> and <see cref="CodeUnit"/>).
    /// Listed annotations include all <see cref="Message"/>s (error, warning, suggestion, etc) and also special <see cref="Comment"/>s.
    /// </summary>
    /// <remarks>
    /// <see cref="CodeAnnotation"/> objects are maintained at the <see cref="Solution"/> level in order to provide a single collection of
    /// all messages for the entire solution (for display in an output window).
    /// </remarks>
    public class CodeAnnotation
    {
        private readonly Annotation _annotation;
        private readonly CodeUnit _codeUnit;
        private readonly Project _project;

        /// <summary>
        /// Create a <see cref="CodeAnnotation"/>.
        /// </summary>
        public CodeAnnotation(Annotation annotation, Project project, CodeUnit codeUnit)
        {
            _annotation = annotation;
            _project = project;
            _codeUnit = codeUnit;
        }

        /// <summary>
        /// The associated <see cref="Annotation"/>.
        /// </summary>
        public Annotation Annotation
        {
            get { return _annotation; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return (_annotation is Message ? ((Message)_annotation).Category : "ToDo"); }
        }

        /// <summary>
        /// The associated <see cref="CodeUnit"/>.
        /// </summary>
        public CodeUnit CodeUnit
        {
            get { return _codeUnit; }
        }

        /// <summary>
        /// The associated line number and column in "9999/99" format (if any, otherwise empty string).
        /// </summary>
        public string LineCol
        {
            get
            {
                int lineNumber = _annotation.LineNumber;
                return (lineNumber > 0) ? (lineNumber + "-" + _annotation.ColumnNumber) : "";
            }
        }

        /// <summary>
        /// The associated <see cref="Project"/>.
        /// </summary>
        public Project Project
        {
            get { return _project; }
        }

        /// <summary>
        /// Format the <see cref="CodeAnnotation"/> as a string.
        /// </summary>
        /// <returns>The text representation of the <see cref="CodeAnnotation"/>.</returns>
        public override string ToString()
        {
            string result = "";
            bool prefix = false;
            if (_project != null)
            {
                result += _project.Name;
                prefix = true;
            }
            if (_codeUnit != null)
            {
                if (prefix)
                    result += ": ";
                result += _codeUnit.Name;
                prefix = true;
            }
            if (prefix)
            {
                int lineNumber = _annotation.LineNumber;
                if (lineNumber > 0)
                    result += "(" + lineNumber + "," + _annotation.ColumnNumber + ")";
                result += ": ";
            }
            result += _annotation.AsString();
            return result;
        }
    }

    /// <summary>
    /// Load options - used when loading a Solution, Project, or CodeUnit.
    /// </summary>
    [Flags]
    public enum LoadOptions
    {
        /// <summary>
        /// No options specified - loading will still occur, but no parsing, or extra logging.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Parse all <see cref="CodeUnit"/>s.
        /// </summary>
        ParseSources = 0x01,

        /// <summary>
        /// Log messages after loading/parsing (errors, warnings, others - depending upon the log level).
        /// </summary>
        /// <remarks>
        /// This option only determines whether or not messages are logged using the <see cref="Log"/> class (to the <see cref="Console"/>,
        /// or intercepted by <see cref="Log.SetLogWriteLineCallback"/>.  Regardless, <see cref="Message"/>s are always created and propagated
        /// up to the <see cref="Solution.CodeAnnotations"/> collection.
        /// </remarks>
        LogMessages = 0x08,

        /// <summary>
        /// Do not parse method bodies (useful if you only need types and member signatures, and not code in methods).
        /// </summary>
        DoNotParseBodies = 0x10,

        /// <summary>
        /// Perform complete processing.
        /// </summary>
        Complete = ParseSources,

        /// <summary>
        /// Load, but don't parse sources (useful for working on <see cref="Solution"/> or <see cref="Project"/> files only).
        /// </summary>
        LoadOnly = None
    }

    /// <summary>
    /// Used for status callbacks during the load process to monitor progress and update any UI.
    /// </summary>
    public enum LoadStatus
    {
        /// <summary>Starting to load a <see cref="Solution"/> or <see cref="Project"/>.</summary>
        Loading,

        /// <summary>Created a new <see cref="Solution"/>, <see cref="Project"/>, <see cref="Reference"/>, or <see cref="CodeUnit"/> object.</summary>
        ObjectCreated,

        /// <summary>A listed <see cref="Annotation"/> (such as an error or warning <see cref="Message"/>) was added to an object.</summary>
        ObjectAnnotated,

        /// <summary>A <see cref="Solution"/> file was loaded, and the active configuration and platform have been set - projects will be loaded next.</summary>
        SolutionLoaded,

        /// <summary>A <see cref="Project"/> file has been parsed, and the configuration and platform have been set.</summary>
        ProjectParsed,

        /// <summary>All <see cref="Project"/>s have been loaded.</summary>
        ProjectsLoaded,

        /// <summary>Starting parsing of all <see cref="CodeUnit"/>s.</summary>
        Parsing,

        /// <summary>Starting logging of message counts, and messages (if requested).</summary>
        LoggingResults
    }
}