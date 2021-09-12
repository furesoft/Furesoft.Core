// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Mono.Cecil;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a collection of <see cref="CodeUnit"/> objects (files) that
    /// are compiled together into some type of target (such as a .DLL or .EXE file).
    /// </summary>
    public class Project : CodeObject, INamedCodeObject, IFile, IComparable<Project>
    {
        /// <summary>
        /// Set to load internal types in addition to public types when loading types from referenced assemblies and
        /// projects, even when there isn't any InternalsVisibleTo attribute.  This allows resolving namespaces and types
        /// that would otherwise not be found, allowing code analysis to then flag such references as illegal (not accessible).
        /// This option will slow things down a bit and use up more memory.
        /// </summary>
        public static bool LoadInternalTypes;

        public const string ConfigurationDebug = "Debug";
        public const string ConfigurationRelease = "Release";
        public const string CSharpFileExtension = ".cs";
        public const string CSharpProjectFileExtension = ".csproj";
        public const int DefaultBaseAddress = 0x400000;
        public const int DefaultFileAlignment = 512;
        public const string DefaultFramework = FrameworkContext.DotNetFramework;
        public const string DesignerCSharpGeneratedExtension = DesignerGeneratedExtension + CSharpFileExtension;
        public const string DesignerGeneratedExtension = ".Designer";
        public const string GeneratedFileExtension = ".g";
        public const string MsCorLib = "mscorlib";
        public const string PlatformAnyCPU = "AnyCPU";
        public const string PlatformX64 = "x64";
        public const string PlatformX86 = "x86";
        public const string PropertiesFolder = "Properties";
        public const string ReferencesFolder = "References";
        public const string SystemCore = "System.Core";
        public const string VBFileExtension = ".vb";
        public const string VBProjectFileExtension = ".vbproj";
        public const string WorkflowCodeBesideFileExtension = ".xoml";
        public const string WorkflowCSharpCodeBesideFileExtension = WorkflowCodeBesideFileExtension + CSharpFileExtension;
        public const string XamlCSharpCodeBehindExtension = XamlFileExtension + CSharpFileExtension;
        public const string XamlCSharpGeneratedExtension = GeneratedFileExtension + CSharpFileExtension;
        public const string XamlFileExtension = ".xaml";
        public static readonly Guid ASPMVCProjectType = new Guid("{603C0E0B-DB56-11DC-BE95-000D561079B0}");
        public static readonly Guid CPPProjectType = new Guid("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        public static readonly Guid CSProjectType = new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        public static readonly Guid CSSharePointProjectType = new Guid("{593B0543-81F6-4436-BA1E-4747859CAAE2}");
        public static readonly Guid CSWorkflowProjectType = new Guid("{14822709-B5A1-4724-98CA-57A101D1B079}");
        public static readonly Guid FolderType = new Guid("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
        public static readonly Guid MvcProjectType = new Guid("{F85E285D-A4E0-4152-9332-AB1D724D3325}");
        public static readonly Guid SetupProjectType = new Guid("{54435603-DBB4-11D2-8724-00A0C9A8B90C}");
        public static readonly Guid SilverlightProjectType = new Guid("{A1591282-1198-4647-A2B1-27E5FF5F6F3B}");
        public static readonly Guid TestProjectType = new Guid("{3AC096D0-A1C2-E12C-1390-A8335801FDAB}");
        public static readonly Guid VBProjectType = new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
        public static readonly Guid VBSharePointProjectType = new Guid("{EC05E597-79D4-47f3-ADA0-324C4F7C7484}");
        public static readonly Guid VBWorkflowProjectType = new Guid("{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}");
        public static readonly Guid VisualDBToolsProjectType = new Guid("{C252FEB5-A946-4202-B1D4-9916A0590387}");
        public static readonly Guid VstoProjectType = new Guid("{BAA0C2D2-18E2-41B9-852F-F413020CAA33}");
        public static readonly Guid WCFProjectType = new Guid("{3D9AD99F-2412-4246-B90B-4EAA41C64699}");
        public static readonly Guid WebApplicationProjectType = new Guid("{349C5851-65DF-11DA-9384-00065B846F21}");
        public static readonly Guid WebSiteProjectType = new Guid("{E24C65DC-7377-472B-9ABA-BC803B73C61A}");
        public static readonly Guid WPFProjectType = new Guid("{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}");

        protected string _appDesignerFolder;
        protected string _assemblyName;
        protected string _assemblyOriginatorKeyFile;

        /// <summary>
        /// The <see cref="MonoCecilAssemblyResolver"/> for this project (only used if using Mono Cecil to load assemblies).
        /// </summary>
        protected MonoCecilAssemblyResolver _assemblyResolver;

        /// <summary>
        /// All code units in this project.
        /// </summary>
        protected ChildList<CodeUnit> _codeUnits;

        protected string _configurationName;

        /// <summary>
        /// The project configurations.
        /// </summary>
        protected ChildList<Configuration> _configurations;

        protected bool? _createTestPage;

        /// <summary>
        /// The currently active project configuration.
        /// </summary>
        protected Configuration _currentConfiguration;

        protected string _defaultTargets;

        /// <summary>
        /// A cached dictionary of projects which the current one depends on (used for performance).
        /// </summary>
        protected HashSet<Project> _dependsOn;

        protected string _deploymentDirectory;
        protected bool? _enableOutOfBrowser;
        protected int _fileAlignment;

        /// <summary>
        /// File items (source files, content files, resource files, etc).
        /// </summary>
        protected ChildList<FileItem> _fileItems;

        protected string _fileName;
        protected string _fileUpgradeFlags;

        /// <summary>
        /// The <see cref="FrameworkContext"/> for this project, which keeps track of all loaded assemblies.
        /// </summary>
        protected FrameworkContext _frameworkContext;

        protected bool? _generateSilverlightManifest;

        /// <summary>
        /// All project-global attributes (those with a target of <b>assembly</b> or <b>module</b>) defined in any <see cref="CodeUnit"/> (usually AssemblyInfo.cs).
        /// </summary>
        protected List<Attribute> _globalAttributes = new List<Attribute>();

        /// <summary>
        /// The project's global namespace.
        /// </summary>
        protected RootNamespace _globalNamespace;

        protected bool _isNew;
        protected string _linkedServerProject;

        /// <summary>
        /// Assembly/root namespace combinations for which types have already been loaded for this project.
        /// </summary>
        protected HashSet<string> _loadedAssemblies = new HashSet<string>();

        protected bool? _mvcBuildViews;
        protected string _name;
        protected string _namespace;

        protected bool? _nonShipping;

        protected bool? _noStandardLibraries;

        /// <summary>
        /// True if the project type is not currently supported.
        /// </summary>
        protected bool _notSupported;

        protected string _oldToolsVersion;

        protected string _outOfBrowserSettingsFile;

        protected string _outputPath;

        protected OutputTypes _outputType;

        protected string _platform;

        // Normally at the configuration level, but can also be specified at the project level
        protected string _productVersion;

        protected Guid _projectGuid;

        protected string _projectType;

        protected List<Guid> _projectTypeGuids;

        protected string _referencePath;

        /// <summary>
        /// References to other projects, assemblies, or COM objects.
        /// </summary>
        protected ChildList<Reference> _references;

        protected string _rootNamespace;

        protected string _sccAuxPath;

        protected string _sccLocalPath;

        protected string _sccProjectName;

        protected string _sccProvider;

        protected string _schemaVersion;

        protected bool? _signAssembly;

        protected string _silverlightAppEntry;

        protected bool? _silverlightApplication;

        protected string _silverlightApplicationList;

        protected string _silverlightManifestTemplate;

        protected string _silverlightVersion;

        protected string _startArguments;

        protected string _startupObject;

        protected string _supportedCultures;

        protected string _targetFrameworkIdentifier;

        protected string _targetFrameworkProfile;

        protected string _targetFrameworkVersion;

        protected string _testPageFileName;

        protected bool? _throwErrorsInValidation;

        protected string _toolsVersion;

        // True if newly created and not saved yet
        protected Guid _typeGuid;

        /// <summary>
        /// Unhandled (unparsed or unrecognized) XML data in the project file.
        /// </summary>
        protected List<UnhandledData> _unhandledData = new List<UnhandledData>();

        protected string _upgradeBackupLocation;
        protected bool? _useIISExpress;
        protected bool? _usePlatformExtensions;

        /// <summary>
        /// The ReferencePath from any "*.csproj.user" file.
        /// </summary>
        protected string _userReferencePath;

        protected bool? _validateXaml;
        protected int? _warningLevel;  // Why does this show up sometimes at the main level?
        protected string _xapFilename;
        protected bool? _xapOutputs;

        /// <summary>
        /// Create a new <see cref="Project"/> with the specified file name and parent <see cref="Solution"/>.
        /// </summary>
        /// <remarks>
        /// To load an existing project, use <see cref="Load(string, LoadOptions, Action{LoadStatus, CodeObject})"/>.
        /// </remarks>
        public Project(string fileName, Solution solution)
        {
            _parent = solution;
            _name = Path.GetFileNameWithoutExtension(fileName);
            _isNew = true;
            _fileName = fileName;
            if (!Path.HasExtension(fileName))
                _fileName += CSharpProjectFileExtension;
            FileEncoding = Encoding.UTF8;  // Default to UTF8 encoding with a BOM
            FileHasUTF8BOM = true;

            _toolsVersion = "4.0";  // Visual Studio 2010
            _defaultTargets = "Build";
            _namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            _productVersion = "9.0.21022";  // Visual Studio 2010
            _schemaVersion = "2.0";
            _projectGuid = Guid.NewGuid();
            _outputType = OutputTypes.Library;  // Default to library
            _appDesignerFolder = PropertiesFolder;
            _rootNamespace = _name;
            _assemblyName = _rootNamespace;
            _targetFrameworkVersion = "4.0";  // Default to .NETFramework 4.0
            _fileAlignment = DefaultFileAlignment;

            Initialize();
            AddDefaultConfigurations();
            _configurationName = ((solution != null && solution.ActiveConfiguration != null) ? solution.ActiveConfiguration : ConfigurationDebug);
            _platform = PlatformAnyCPU;
            _currentConfiguration = FindConfiguration(_configurationName, _platform);
        }

        static Project()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            ForceReference();
        }

        public string AppDesignerFolder
        {
            get { return _appDesignerFolder; }
            set { _appDesignerFolder = value; }
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>
        /// The <see cref="MonoCecilAssemblyResolver"/> for this project.
        /// </summary>
        public MonoCecilAssemblyResolver AssemblyResolver
        {
            get
            {
                if (_assemblyResolver == null)
                    _assemblyResolver = new MonoCecilAssemblyResolver(this);
                return _assemblyResolver;
            }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "project"; }
        }

        /// <summary>
        /// All of <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public ChildList<CodeUnit> CodeUnits
        {
            get { return _codeUnits; }
        }

        /// <summary>
        /// The name of the current <see cref="Configuration"/>.
        /// </summary>
        public string ConfigurationName
        {
            get { return (_currentConfiguration != null ? _currentConfiguration.Name : _configurationName); }
        }

        /// <summary>
        /// The platform of the current <see cref="Configuration"/>.
        /// </summary>
        public string ConfigurationPlatform
        {
            get { return (_currentConfiguration != null ? _currentConfiguration.Platform : _platform); }
        }

        /// <summary>
        /// All of the <see cref="Configuration"/>s for the <see cref="Project"/>.
        /// </summary>
        public ChildList<Configuration> Configurations
        {
            get { return _configurations; }
        }

        public bool? CreateTestPage
        {
            get { return _createTestPage; }
            set { _createTestPage = value; }
        }

        /// <summary>
        /// The currently active <see cref="Configuration"/>.
        /// </summary>
        public Configuration CurrentConfiguration
        {
            get { return _currentConfiguration; }
            set { _currentConfiguration = value; }
        }

        public string DefaultTargets
        {
            get { return _defaultTargets; }
            set { _defaultTargets = value; }
        }

        public string DeploymentDirectory
        {
            get { return _deploymentDirectory; }
            set { _deploymentDirectory = value; }
        }

        public bool? EnableOutOfBrowser
        {
            get { return _enableOutOfBrowser; }
            set { _enableOutOfBrowser = value; }
        }

        public int FileAlignment
        {
            get { return _fileAlignment; }
            set { _fileAlignment = value; }
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
        /// All of <see cref="FileItem"/>s in the <see cref="Project"/>.
        /// </summary>
        public ChildList<FileItem> FileItems
        {
            get { return _fileItems; }
        }

        /// <summary>
        /// The associated file name of the <see cref="Project"/>.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Always <c>false</c> for a <see cref="Project"/>.  Can't be set.
        /// </summary>
        public bool FileUsingTabs
        {
            get { return false; }
            set { throw new Exception("Project files never use tabs!"); }
        }

        /// <summary>
        /// The <see cref="FrameworkContext"/> for this project, which controls all loaded assemblies.
        /// </summary>
        public FrameworkContext FrameworkContext
        {
            get
            {
                // Create the load context if we don't have one yet (using the current AppDomain for now)
                if (_frameworkContext == null)
                    _frameworkContext = FrameworkContext.Get(TargetFrameworkIdentifier, TargetFrameworkVersion, TargetFrameworkProfile, ApplicationContext.GetMasterInstance());
                return _frameworkContext;
            }
        }

        public bool? GenerateSilverlightManifest
        {
            get { return _generateSilverlightManifest; }
            set { _generateSilverlightManifest = value; }
        }

        /// <summary>
        /// All project (assembly) level attributes defined in any code unit (such as AssemblyInfo.cs).
        /// </summary>
        public List<Attribute> GlobalAttributes
        {
            get { return _globalAttributes; }
        }

        /// <summary>
        /// The global namespace for the project.
        /// </summary>
        public RootNamespace GlobalNamespace
        {
            get { return _globalNamespace; }
        }

        /// <summary>
        /// True if the <see cref="Project"/> is newly created and hasn't been saved yet.
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
        }

        /// <summary>
        /// True if the project is a 'website' project - meaning a web-based project that has no project file
        /// and appears in the solution tree as an 'http:\' URL instead of just a project name.
        /// </summary>
        public bool IsWebSiteProject
        {
            get { return (_typeGuid == WebSiteProjectType); }
        }

        public string LinkedServerProject
        {
            get { return _linkedServerProject; }
            set { _linkedServerProject = value; }
        }

        public bool? MvcBuildViews
        {
            get { return _mvcBuildViews; }
            set { _mvcBuildViews = value; }
        }

        /// <summary>
        /// The name of the <see cref="Project"/> (does not include the file path or extension).
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool? NonShippping
        {
            get { return _nonShipping; }
            set { _nonShipping = value; }
        }

        public bool? NoStandardLibraries
        {
            get { return _noStandardLibraries; }
            set { _noStandardLibraries = value; }
        }

        /// <summary>
        /// True if the project type is not currently supported.
        /// </summary>
        public bool NotSupported
        {
            get { return _notSupported; }
        }

        public string OutOfBrowserSettingsFile
        {
            get { return _outOfBrowserSettingsFile; }
            set { _outOfBrowserSettingsFile = value; }
        }

        /// <summary>
        /// The output path of the <see cref="Project"/>.
        /// </summary>
        public string OutputPath
        {
            get { return (_currentConfiguration != null ? _currentConfiguration.OutputPath ?? _outputPath : _outputPath); }
        }

        public OutputTypes OutputType
        {
            get { return _outputType; }
            set { _outputType = value; }
        }

        public string ProductVersion
        {
            get { return _productVersion; }
            set { _productVersion = value; }
        }

        public Guid ProjectGuid
        {
            get { return _projectGuid; }
        }

        public string ProjectType
        {
            get { return _projectType; }
            set { _projectType = value; }
        }

        public List<Guid> ProjectTypeGuids
        {
            get { return _projectTypeGuids; }
            set { _projectTypeGuids = value; }
        }

        public string ReferencePath
        {
            get { return _referencePath; }
            set { _referencePath = value; }
        }

        /// <summary>
        /// All of the references for the <see cref="Project"/>.
        /// </summary>
        public ChildList<Reference> References
        {
            get { return _references; }
        }

        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }

        public string SccAuxPath
        {
            get { return _sccAuxPath; }
            set { _sccAuxPath = value; }
        }

        public string SccLocalPath
        {
            get { return _sccLocalPath; }
            set { _sccLocalPath = value; }
        }

        public string SccProjectName
        {
            get { return _sccProjectName; }
            set { _sccProjectName = value; }
        }

        public string SccProvider
        {
            get { return _sccProvider; }
            set { _sccProvider = value; }
        }

        public string SchemaVersion
        {
            get { return _schemaVersion; }
            set { _schemaVersion = value; }
        }

        public bool? SignAssembly
        {
            get { return _signAssembly; }
            set { _signAssembly = value; }
        }

        public string SilverlightAppEntry
        {
            get { return _silverlightAppEntry; }
            set { _silverlightAppEntry = value; }
        }

        public bool? SilverlightApplication
        {
            get { return _silverlightApplication; }
            set { _silverlightApplication = value; }
        }

        public string SilverlightApplicationList
        {
            get { return _silverlightApplicationList; }
            set { _silverlightApplicationList = value; }
        }

        public string SilverlightManifestTemplate
        {
            get { return _silverlightManifestTemplate; }
            set { _silverlightManifestTemplate = value; }
        }

        public string SilverlightVersion
        {
            get { return _silverlightVersion; }
            set { _silverlightVersion = value; }
        }

        /// <summary>
        /// The parent <see cref="Solution"/>.
        /// </summary>
        public Solution Solution
        {
            get { return _parent as Solution; }
            set { _parent = value; }
        }

        public string StartArguments
        {
            get { return _startArguments; }
            set { _startArguments = value; }
        }

        public string StartupObject
        {
            get { return _startupObject; }
            set { _startupObject = value; }
        }

        public string SupportedCultures
        {
            get { return _supportedCultures; }
            set { _supportedCultures = value; }
        }

        public string TargetFrameworkIdentifier
        {
            get
            {
                if (_targetFrameworkIdentifier != null)
                    return _targetFrameworkIdentifier;
                if (_silverlightVersion != null)
                    return FrameworkContext.SilverlightFramework;
                if (_targetFrameworkProfile != null && _targetFrameworkProfile.StartsWith("Profile"))  // Profile1..4
                    return FrameworkContext.PortableLibraryFramework;
                return DefaultFramework;
            }
            set
            {
                if (_targetFrameworkIdentifier != value)
                {
                    _targetFrameworkIdentifier = value;

                    // Reset the FrameworkContext, and update any assembly references accordingly
                    _frameworkContext = null;
                    foreach (Reference reference in _references)
                        reference.Resolve();
                }
            }
        }

        public string TargetFrameworkProfile
        {
            get { return _targetFrameworkProfile; }
            set
            {
                if (_targetFrameworkProfile != value)
                {
                    _targetFrameworkProfile = value;

                    // Reset the FrameworkContext, and update any assembly references accordingly
                    _frameworkContext = null;
                    foreach (Reference reference in _references)
                        reference.Resolve();
                }
            }
        }

        public string TargetFrameworkVersion
        {
            get
            {
                if (_targetFrameworkVersion != null)
                    return _targetFrameworkVersion;
                // Default the target framework version to the ToolsVersion
                if (_toolsVersion != null)
                    return _toolsVersion;
                // Worst case, default to 2.0 (VS 2005 default)
                return "2.0";
            }
            set
            {
                if (_targetFrameworkVersion != value)
                {
                    _targetFrameworkVersion = value;

                    // Reset the FrameworkContext, and update any assembly references accordingly
                    _frameworkContext = null;
                    foreach (Reference reference in _references)
                        reference.Resolve();
                }
            }
        }

        public string TestPageFileName
        {
            get { return _testPageFileName; }
            set { _testPageFileName = value; }
        }

        public bool? ThrowErrorsInValidation
        {
            get { return _throwErrorsInValidation; }
            set { _throwErrorsInValidation = value; }
        }

        public string ToolsVersion
        {
            get { return _toolsVersion; }
            set { _toolsVersion = value; }
        }

        /// <summary>
        /// The GUID representing the type of the <see cref="Project"/>.
        /// </summary>
        public Guid TypeGuid
        {
            get { return _typeGuid; }
        }

        public bool? UseIISExpress
        {
            get { return _useIISExpress; }
            set { _useIISExpress = value; }
        }

        public bool? UsePlatformExtensions
        {
            get { return _usePlatformExtensions; }
            set { _usePlatformExtensions = value; }
        }

        /// <summary>
        /// The ReferencePath from any "*.csproj.user" file.
        /// </summary>
        public string UserReferencePath
        {
            get { return _userReferencePath; }
        }

        public bool? ValidateXaml
        {
            get { return _validateXaml; }
            set { _validateXaml = value; }
        }

        public int? WarningLevel
        {
            get { return _warningLevel; }
            set { _warningLevel = value; }
        }

        public string XapFilename
        {
            get { return _xapFilename; }
            set { _xapFilename = value; }
        }

        public bool? XapOutputs
        {
            get { return _xapOutputs; }
            set { _xapOutputs = value; }
        }

        public string XmlNamespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project (".csproj") file.</param>
        /// <param name="configuration">The project configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="platform">The project platform to use (such as 'AnyCPU', 'x86', etc.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, string configuration, string platform, LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            Project project = null;
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

                // Abort if the file doesn't exist - otherwise, the Parse() method will end up returning a valid but empty object
                // with an error message attached (which is done so errors can appear inside a loaded Solution tree).
                if (!File.Exists(fileName))
                {
                    Log.WriteLine("ERROR: Project file '" + fileName + "' does not exist.");
                    return null;
                }

                Log.WriteLine("Loading project '" + Path.GetFileNameWithoutExtension(fileName) + "' ...");

                // Create a dummy solution for the project file, since it is being loaded directly
                Solution solution = new Solution(Path.ChangeExtension(fileName, Solution.SolutionFileExtension), statusCallback);
                if (statusCallback != null)
                    statusCallback(LoadStatus.ObjectCreated, solution);

                // Parse and resolve the project and code units
                Unrecognized.Count = 0;
                project = Parse(fileName, solution, statusCallback);
                solution.AddProject(project);

                // Change the default configuration and platform if a configuration was specified, and change the solution
                // configuration to match.
                if (configuration != null)
                    project._currentConfiguration = project.FindConfiguration(configuration, platform);
                solution.ActiveConfiguration = project.ConfigurationName;
                solution.ActivePlatform = (project.ConfigurationPlatform == PlatformAnyCPU ? Solution.PlatformAnyCPU : project.ConfigurationPlatform);
                Log.WriteLine("Active Configuration and Platform: " + project.ConfigurationName + " - " + project.ConfigurationPlatform);

                if (statusCallback != null)
                    statusCallback(LoadStatus.ProjectParsed, project);

                if (loadOptions.HasFlag(LoadOptions.ResolveSources))
                    project.LoadReferencedAssembliesAndTypes();
                Log.WriteLine("Loaded project '" + project.Name + "', total elapsed time: " + overallStopWatch.Elapsed.TotalSeconds.ToString("N3"));
                if (statusCallback != null)
                    statusCallback(LoadStatus.ProjectsLoaded, null);

                // Parse and (optionally) resolve all code units in the project
                if (loadOptions.HasFlag(LoadOptions.ParseSources) || loadOptions.HasFlag(LoadOptions.ResolveSources))
                    project.ParseAndResolveCodeUnits(loadOptions, statusCallback);

                long memoryUsage = GC.GetTotalMemory(true) - startBytes;
                Log.WriteLine(string.Format("Total elapsed time: {0:N3}, memory usage: {1} MBs", overallStopWatch.Elapsed.TotalSeconds, memoryUsage / (1024 * 1024)));

                if (statusCallback != null)
                    statusCallback(LoadStatus.LoggingResults, null);
                solution.LogMessageCounts(loadOptions.HasFlag(LoadOptions.LogMessages));
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading project");
            }
            return project;
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project (".csproj") file.</param>
        /// <param name="configuration">The project configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="platform">The project platform to use (such as 'AnyCPU', 'x86', etc.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, string configuration, string platform, LoadOptions loadOptions)
        {
            return Load(fileName, configuration, platform, loadOptions, null);
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project (".csproj") file.</param>
        /// <param name="configuration">The project configuration to use (usually 'Debug' or 'Release').</param>
        /// <param name="platform">The project platform to use (such as 'AnyCPU', 'x86', etc.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, string configuration, string platform)
        {
            return Load(fileName, configuration, platform, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project (".csproj") file.</param>
        /// <param name="configuration">The project configuration to use (usually 'Debug' or 'Release').</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, string configuration)
        {
            return Load(fileName, configuration, null, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project ('.csproj') file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            return Load(fileName, null, null, loadOptions, statusCallback);
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project ('.csproj') file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName, LoadOptions loadOptions)
        {
            return Load(fileName, null, null, loadOptions, null);
        }

        /// <summary>
        /// Load the specified <see cref="Project"/> file and all child code files directly (without a <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The project ('.csproj') file.</param>
        /// <returns>The resulting <see cref="Project"/> object.</returns>
        /// <remarks>
        /// Loading a Project directly goes through the following steps:
        ///   - Parse the Project file, creating a Project object and loading the lists of References and code files (CodeUnits).
        ///   - Resolve all References for the project.
        ///   - Load all referenced assemblies.
        ///   - Load all types from all referenced assemblies.
        ///   - Parse all code files.
        ///   - Resolve all symbolic references in all code files.
        /// The 'LoadOnly' option stops this process after parsing the Project file and resolving References, which is useful
        /// if only the project file itself is being viewed, analyzed, or edited.  The 'DoNotResolve' option skips resolving
        /// symbolic references, which is useful if that step is not required, such as when only formatting code which doesn't
        /// rely on resolved references.
        /// </remarks>
        public static Project Load(string fileName)
        {
            return Load(fileName, null, null, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The required target framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        /// <param name="specificVersion">True if the specific version specified in the display name should be used.</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path, which can be
        /// either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path instead.
        /// </summary>
        public void AddAssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden, string hintPath, bool specificVersion)
        {
            AddReference(new AssemblyReference(name, alias, requiredTargetFrameworkVersion, isHidden, hintPath, specificVersion));
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The required target framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path, which can be
        /// either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path instead.
        /// </summary>
        public void AddAssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden, string hintPath)
        {
            AddReference(new AssemblyReference(name, alias, requiredTargetFrameworkVersion, isHidden, hintPath));
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The required target framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path, which can be
        /// either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path instead.
        /// </summary>
        public void AddAssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden)
        {
            AddReference(new AssemblyReference(name, alias, requiredTargetFrameworkVersion, isHidden));
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The required target framework version for the assembly (only used for framework assemblies).</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path, which can be
        /// either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path instead.
        /// </summary>
        public void AddAssemblyReference(string name, string alias, string requiredTargetFrameworkVersion)
        {
            AddReference(new AssemblyReference(name, alias, requiredTargetFrameworkVersion));
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        public void AddAssemblyReference(string name, string hintPath)
        {
            AddReference(new AssemblyReference(name, hintPath));
        }

        /// <summary>
        /// Add an assembly reference by name.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        public void AddAssemblyReference(string name)
        {
            AddReference(new AssemblyReference(name));
        }

        /// <summary>
        /// Add a <see cref="CodeUnit"/> to the <see cref="Project"/>, keeping the <see cref="CodeUnits"/> collection sorted alphabetically.
        /// </summary>
        public void AddCodeUnit(CodeUnit codeUnit)
        {
            // Insert the CodeUnit at the proper index according to its path and filename.
            // Duplicate paths shouldn't exist, but names might for CodeUnits that aren't mapped to files, so just insert
            // any duplicate matches following the existing one.
            int index = _codeUnits.BinarySearch(codeUnit);
            _codeUnits.Insert((index < 0 ? ~index : index + 1), codeUnit);
        }

        /// <summary>
        /// Add a new <see cref="Configuration"/> to the <see cref="Project"/>.
        /// </summary>
        public void AddConfiguration(Configuration configuration)
        {
            _configurations.Add(configuration);
            if (Solution != null)
                Solution.AddProjectConfiguration(configuration);
        }

        /// <summary>
        /// Add default assembly references to a newly created project (System, System.Core, System.Data,
        /// System.Data.DataSetExtensions, System.Xml, System.Xml.Linq, Microsoft.CSharp).
        /// </summary>
        public void AddDefaultAssemblyReferences()
        {
            AddAssemblyReference("System");
            AddAssemblyReference("System.Core");
            AddAssemblyReference("System.Data");
            AddAssemblyReference("System.Data.DataSetExtensions");
            AddAssemblyReference("System.Xml");
            AddAssemblyReference("System.Xml.Linq");
            AddAssemblyReference("Microsoft.CSharp");
        }

        /// <summary>
        /// Add default configurations to a newly created project (Debug and Release for AnyCPU).
        /// </summary>
        public void AddDefaultConfigurations()
        {
            AddConfiguration(new Configuration(ConfigurationDebug, PlatformAnyCPU, true, DebugTypes.full, false, "DEBUG;TRACE"));
            AddConfiguration(new Configuration(ConfigurationRelease, PlatformAnyCPU, false, DebugTypes.pdbonly, true, "TRACE"));
        }

        /// <summary>
        /// Add an existing file to the project.
        /// </summary>
        /// <returns>The new CodeUnit object.</returns>
        public CodeUnit AddFile(string fileName, bool isGenerated, Action<LoadStatus, CodeObject> statusCallback)
        {
            bool noStdLib = (_currentConfiguration != null && _currentConfiguration.NoStdLib);

            // All projects reference 'mscorlib' implicitly, so add it now if we don't have any references yet.
            // We must do this here and not in the constructor of Project, because the type of the 'mscorlib'
            // library is determined by the targeted framework version, which must be parsed or set first.
            if (References.Count == 0 && !noStdLib)
                AddImplicitMscorlibReference();

            CodeUnit codeUnit = new CodeUnit(fileName, this) { IsGenerated = isGenerated };
            if (!codeUnit.FileExists)
            {
                string message = (codeUnit.IsGenerated
                    ? "Generated file '" + codeUnit.FileName + "' is missing - do a full build to create all generated files."
                    : "File '" + codeUnit.FileName + "' doesn't exist!");
                LogMessage(message, MessageSeverity.Error);
                codeUnit.AttachMessage(message, MessageSeverity.Error, MessageSource.Load);
            }

            AddCodeUnit(codeUnit);
            if (statusCallback != null)
                statusCallback(LoadStatus.ObjectCreated, codeUnit);

            return codeUnit;
        }

        /// <summary>
        /// Add an existing file to the project.
        /// </summary>
        /// <returns>The new CodeUnit object.</returns>
        public CodeUnit AddFile(string fileName, bool isGenerated)
        {
            return AddFile(fileName, isGenerated, null);
        }

        /// <summary>
        /// Add an existing file to the project.
        /// </summary>
        /// <returns>The new CodeUnit object.</returns>
        public CodeUnit AddFile(string fileName)
        {
            return AddFile(fileName, false, null);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(_name, this);
        }

        /// <summary>
        /// Add a listed annotation to the <see cref="Project"/>.
        /// </summary>
        public void AnnotationAdded(Annotation annotation, CodeUnit codeUnit, bool sendStatus)
        {
            // Update the list of assembly-level attributes
            if (annotation is Attribute)
            {
                lock (this)
                    _globalAttributes.Add((Attribute)annotation);
            }
            // Pass the annotation to the solution
            else if (_parent != null)
                Solution.AnnotationAdded(annotation, this, codeUnit, sendStatus);
        }

        /// <summary>
        /// Remove a listed annotation from the <see cref="Project"/>.
        /// </summary>
        public void AnnotationRemoved(Annotation annotation)
        {
            // Update the list of assembly-level attributes
            if (annotation is Attribute)
            {
                lock (this)
                    _globalAttributes.Remove((Attribute)annotation);
            }
            // Pass the annotation to the solution
            else if (_parent != null)
                Solution.AnnotationRemoved(annotation);
        }

        /// <summary>
        /// Determine if internal types in the project are visible to the specified project.
        /// </summary>
        public bool AreInternalTypesVisibleTo(Project project)
        {
            // This shouldn't BUT apparently CAN throw an exception if there's a problem loading types from the assembly
            bool visibleTo = false;
            try
            {
                lock (this)
                    visibleTo = Enumerable.Any(_globalAttributes, delegate (Attribute attribute) { return attribute.Target == AttributeTarget.Assembly && GetInternalsVisibleToProject(attribute) == project; });
            }
            catch { }
            return visibleTo;
        }

        /// <summary>
        /// Compare one <see cref="Project"/> to another.
        /// </summary>
        public int CompareTo(Project project)
        {
            // Sort by name only (not path)
            return _name.CompareTo(project.Name);
        }

        /// <summary>
        /// Create a new <see cref="CodeUnit"/>, and add it to the <see cref="Project"/> along with a corresponding <see cref="FileItem"/>.
        /// </summary>
        /// <returns>The new <see cref="CodeUnit"/> object.</returns>
        public CodeUnit CreateCodeUnit(string fileName)
        {
            CodeUnit codeUnit = new CodeUnit(fileName, this);
            AddCodeUnit(codeUnit);
            _fileItems.Add(new FileItem(BuildActions.Compile, codeUnit.FileName));
            return codeUnit;
        }

        /// <summary>
        /// Create a new <see cref="CodeUnit"/> for a code fragment, and add it to the <see cref="Project"/>.
        /// </summary>
        /// <param name="codeFragment">The code fragment.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The new CodeUnit object.</returns>
        public CodeUnit CreateCodeUnit(string codeFragment, string fileName)
        {
            CodeUnit codeUnit = new CodeUnit(fileName, codeFragment, this);
            AddCodeUnit(codeUnit);
            return codeUnit;
        }

        /// <summary>
        /// Define a compiler directive symbol for the current configuration.
        /// </summary>
        public void DefineCompilerDirectiveSymbol(string name)
        {
            if (_currentConfiguration != null)
                _currentConfiguration.DefineConstant(name);
        }

        /// <summary>
        /// Find a namespace or type with the fully-specified name.
        /// </summary>
        /// <returns>A <see cref="Namespace"/>, <see cref="TypeDecl"/>, or <see cref="TypeDefinition"/>/<see cref="Type"/> object.</returns>
        public object Find(string fullName)
        {
            RootNamespace rootNamespace = GetRootNamespace(ref fullName);
            return rootNamespace.Find(fullName);
        }

        /// <summary>
        /// Find a <see cref="CodeUnit"/> by name.
        /// </summary>
        public CodeUnit FindCodeUnit(string name)
        {
            return Enumerable.FirstOrDefault(_codeUnits, delegate (CodeUnit codeUnit) { return StringUtil.NNEqualsIgnoreCase(codeUnit.Name, name); });
        }

        /// <summary>
        /// Find any configuration with the specified configuration name and platform name.
        /// </summary>
        /// <returns>The <see cref="Configuration"/> object if found, otherwise null.</returns>
        public Configuration FindConfiguration(string configurationName, string platformName)
        {
            foreach (Configuration configuration in _configurations)
            {
                if (configuration.Name == configurationName && (configuration.Platform == platformName
                    || configuration.Platform == null || (configuration.Platform == PlatformAnyCPU && platformName == null)))
                    return configuration;
            }
            return null;
        }

        /// <summary>
        /// Find the <see cref="Namespace"/> with the fully-specified name.
        /// </summary>
        public Namespace FindNamespace(string namespaceFullName)
        {
            RootNamespace rootNamespace = GetRootNamespace(ref namespaceFullName);
            return rootNamespace.FindNamespace(namespaceFullName);
        }

        /// <summary>
        /// Find a namespace or type in the global namespace with the fully specified name, returning a <see cref="SymbolicRef"/> to it.
        /// </summary>
        /// <param name="name">The namespace or type name (may include namespace and/or parent type prefixes).</param>
        /// <param name="isFirstOnLine">True if the returned <see cref="SymbolicRef"/> should be formatted as first-on-line.</param>
        /// <returns>A <see cref="NamespaceRef"/>, <see cref="TypeRef"/>, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public SymbolicRef FindRef(string name, bool isFirstOnLine)
        {
            RootNamespace rootNamespace = GetRootNamespace(ref name);
            object obj = rootNamespace.Find(name);
            if (obj is Namespace)
                return new NamespaceRef((Namespace)obj, isFirstOnLine);
            if (obj is TypeDecl)
                return new TypeRef((TypeDecl)obj, isFirstOnLine);
            if (obj is TypeDefinition)
                return new TypeRef((TypeDefinition)obj, isFirstOnLine);
            if (obj is Type)
                return new TypeRef((Type)obj, isFirstOnLine);
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.NamespaceOrType);
        }

        /// <summary>
        /// Find a namespace or type in the global namespace with the fully specified name, returning a <see cref="SymbolicRef"/> to it.
        /// </summary>
        /// <param name="name">The namespace or type name (may include namespace and/or parent type prefixes).</param>
        /// <returns>A <see cref="NamespaceRef"/>, <see cref="TypeRef"/>, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public SymbolicRef FindRef(string name)
        {
            return FindRef(name, false);
        }

        /// <summary>
        /// Find the <see cref="RootNamespace"/> for the <see cref="Reference"/> with the specified alias name.
        /// </summary>
        public RootNamespace FindReferenceAliasNamespace(string referenceAliasName)
        {
            foreach (Reference reference in References)
            {
                if (reference.Alias == referenceAliasName)
                    return reference.AliasNamespace;
            }
            return null;
        }

        /// <summary>
        /// Get an enumerator for all <see cref="TypeDecl"/>s declared in the <see cref="Project"/>
        /// (does not include TypeDecls imported from other Projects).
        /// </summary>
        public IEnumerable<TypeDecl> GetAllDeclaredTypeDecls(bool includeNestedTypes)
        {
            return Enumerable.SelectMany<CodeUnit, TypeDecl>(_codeUnits, delegate (CodeUnit codeUnit) { return codeUnit.GetTypeDecls(true, includeNestedTypes); });
        }

        /// <summary>
        /// Get an enumerator for all <see cref="TypeDecl"/>s declared in the <see cref="Project"/>
        /// (does not include TypeDecls imported from other Projects).
        /// </summary>
        public IEnumerable<TypeDecl> GetAllDeclaredTypeDecls()
        {
            return Enumerable.SelectMany<CodeUnit, TypeDecl>(_codeUnits, delegate (CodeUnit codeUnit) { return codeUnit.GetTypeDecls(true, false); });
        }

        /// <summary>
        /// Get the directory of the <see cref="Project"/> (handles website directories).
        /// </summary>
        public string GetDirectory()
        {
            return (IsWebSiteProject ? (_parent != null ? Solution.GetWebSiteDirectory(_name) : null) : Path.GetDirectoryName(_fileName));
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
        /// Get the full output path of the <see cref="Project"/>.
        /// </summary>
        public string GetFullOutputPath()
        {
            string outputPath = OutputPath;
            if (outputPath != null)
            {
                if (!Path.IsPathRooted(outputPath))
                    outputPath = FileUtil.CombineAndNormalizePath(GetDirectory(), outputPath);
            }
            return outputPath;
        }

        /// <summary>
        /// Get any Solution folders for this Project.
        /// </summary>
        public string GetSolutionFolders()
        {
            return (Solution != null ? Solution.GetSolutionFolders(this) : null);
        }

        /// <summary>
        /// Get the description of the target framework.
        /// </summary>
        public string GetTargetFrameworkDescription()
        {
            string targetFramework = null;
            if (IsWebSiteProject)
            {
                if (_parent != null)
                {
                    Solution.ProjectEntry projectEntry = Solution.FindProjectEntry(_name);
                    if (projectEntry != null)
                    {
                        Solution.ProjectSection projectSection = projectEntry.FindProjectSection(Solution.WebsitePropertiesProjectSection);
                        if (projectSection != null)
                        {
                            string targetFrameworkMoniker = projectSection.FindValue("TargetFrameworkMoniker");
                            if (targetFrameworkMoniker != null)
                                targetFramework = Uri.UnescapeDataString(targetFrameworkMoniker.Trim('"')).Replace(",Version=", " ");
                        }
                    }
                }
            }
            else
            {
                if (TargetFrameworkIdentifier != null && TargetFrameworkVersion != null)
                {
                    targetFramework = TargetFrameworkIdentifier + " v" + TargetFrameworkVersion
                        + (_targetFrameworkProfile != null ? " " + _targetFrameworkProfile + " Profile" : "");
                }
            }
            return targetFramework;
        }

        /// <summary>
        /// Determine if a compiler directive symbol is defined for the current configuration.
        /// </summary>
        public bool IsCompilerDirectiveSymbolDefined(string name)
        {
            return (_currentConfiguration != null && _currentConfiguration.IsConstantDefined(name));
        }

        /// <summary>
        /// Determine if this project depends on the specified project.
        /// </summary>
        public bool IsDependentOn(Project project)
        {
            if (_dependsOn == null)
            {
                // Create a cached dictionary of dependencies if it doesn't exist yet
                _dependsOn = new HashSet<Project>();
                AddDependencies(this);
            }
            return _dependsOn.Contains(project);
        }

        /// <summary>
        /// Load an assembly.
        /// </summary>
        /// <param name="assemblyName">The display name, file name, or short name of the assembly.</param>
        /// <param name="hintPath">Optional full file specification of the assembly.</param>
        /// <param name="errorMessage">Returns an error message string if there was one.</param>
        /// <param name="reference">The Reference object associated with the assembly.</param>
        /// <returns>The LoadedAssembly object.</returns>
        public LoadedAssembly LoadAssembly(string assemblyName, string hintPath, out string errorMessage, Reference reference)
        {
            // Only allow one thread to do this at a time for the same FrameworkContext
            LoadedAssembly loadedAssembly;
            lock (this)
                loadedAssembly = _frameworkContext.LoadAssembly(assemblyName, hintPath, null, out errorMessage, this, reference);
            return loadedAssembly;
        }

        /// <summary>
        /// Load all externally referenced assemblies.
        /// </summary>
        public void LoadReferencedAssemblies()
        {
            // Don't load referenced assemblies for unsupported project types
            if (_notSupported)
                return;

            Log.DetailWriteLine("Loading referenced assemblies for project: " + _name);

            // Reset the mono resolver to clear any cached errors for assemblies that failed to load
            if (ApplicationContext.UseMonoCecilLoads)
                AssemblyResolver.Reset();

            // Don't ignore any errors while loading
            ApplicationContext.GetMasterInstance().IgnoreDuplicateLoadErrors = false;

            // Load all referenced assemblies first, before loading any types.  This is necessary so that framework "reference
            // assemblies" are given priority over runtime assemblies in the GAC for directly-referenced assemblies.  It also
            // allows for any errors finding direct references to be reported (and displayed) very quickly.
            foreach (Reference reference in _references)
                reference.Load();

            // Once we're done loading, we want to ignore any load errors for already-failed loads
            // that might occur later when drilling down into the metadata.
            ApplicationContext.GetMasterInstance().IgnoreDuplicateLoadErrors = true;
        }

        /// <summary>
        /// Load all referenced assemblies, then load all types from them.
        /// </summary>
        public void LoadReferencedAssembliesAndTypes(bool quiet)
        {
            Stopwatch stopWatch = new Stopwatch();
            if (!quiet)
                stopWatch.Start();
            ResolveReferences();
            LoadReferencedAssemblies();
            if (!quiet)
            {
                Log.WriteLine("Loaded all referenced assemblies, elapsed time: " + stopWatch.Elapsed.TotalSeconds.ToString("N3"));
                stopWatch.Restart();
            }
            int typeCount = LoadTypesFromReferencedAssemblies();
            if (!quiet)
                Log.WriteLine("Loaded types from all referenced assemblies (" + typeCount.ToString("N0") + " total types), elapsed time: " + stopWatch.Elapsed.TotalSeconds.ToString("N3"));
        }

        /// <summary>
        /// Load all referenced assemblies, then load all types from them.
        /// </summary>
        public void LoadReferencedAssembliesAndTypes()
        {
            LoadReferencedAssembliesAndTypes(false);
        }

        /// <summary>
        /// Load all of the (non-nested) types in the specified assembly into the appropriate namespaces under
        /// the specified root namespace, hiding any of the specified types.
        /// </summary>
        public int LoadTypes(LoadedAssembly loadedAssembly, RootNamespace rootNamespace, out string errorMessage, HashSet<string> hideTypes)
        {
            // Abort if we've already loaded the types for this assembly for this project
            string key = rootNamespace.Name + "::" + loadedAssembly;
            lock (_loadedAssemblies)
            {
                if (!_loadedAssemblies.Add(key))
                {
                    errorMessage = null;
                    return 0;
                }
            }

            // Check if we need to include internal types (the 'AreInternalTypesVisibleTo()' call shouldn't BUT apparently
            // CAN throw an exception if there's a problem loading types from the assembly, so do this here).
            bool includePrivateTypes = (LoadInternalTypes || loadedAssembly.AreInternalTypesVisibleTo(this));

            // Load the types from the assembly
            return loadedAssembly.LoadTypes(includePrivateTypes, rootNamespace, out errorMessage, hideTypes);
        }

        /// <summary>
        /// Load all of the (non-nested) types in the specified assembly into the appropriate namespaces under
        /// the specified root namespace, hiding any of the specified types.
        /// </summary>
        public int LoadTypes(LoadedAssembly loadedAssembly, RootNamespace rootNamespace, out string errorMessage)
        {
            return LoadTypes(loadedAssembly, rootNamespace, out errorMessage, null);
        }

        /// <summary>
        /// Load types from all externally referenced assemblies.
        /// </summary>
        public int LoadTypesFromReferencedAssemblies()
        {
            // Don't ignore any errors while loading
            ApplicationContext.GetMasterInstance().IgnoreDuplicateLoadErrors = false;

            // Load all types from all referenced assemblies (extracted to a subroutine so Solution can use it)
            int typeCount = LoadTypesFromReferenceAssembliesInternal();

            // Once we're done loading, we want to ignore any load errors for already-failed loads
            // that might occur later when drilling down into the metadata.
            ApplicationContext.GetMasterInstance().IgnoreDuplicateLoadErrors = true;

            // Initialize all static TypeRefs for the current Project - this is NOT THREAD SAFE, and also these types should really
            // be project-specific instead of static, but they're much more convenient static, will work fine if all projects have the
            // same target platform, and only causes cosmetic display of wrong source types if they don't (they will still compare OK).
            TypeRef.InitializeTypeRefs(this);

            return typeCount;
        }

        /// <summary>
        /// Load all of the types in all referenced projects into this one.
        /// </summary>
        public void LoadTypesFromReferencedProjects()
        {
            Log.DetailWriteLine("Loading types from referenced projects for project: " + _name);

            foreach (Reference reference in _references)
            {
                if (reference is ProjectReference)
                {
                    // Load all accessible types from the referenced project
                    ProjectReference projectReference = (ProjectReference)reference;
                    if (!projectReference.TreatAsAssemblyReference)
                    {
                        Project project = projectReference.ReferencedProject;
                        if (project != null)
                        {
                            Log.DetailWriteLine("\tAdding types from: " + project.Name);
                            _globalNamespace.AddFromOtherProject(project, project.GlobalNamespace, LoadInternalTypes || project.AreInternalTypesVisibleTo(this));
                        }
                    }
                }
            }
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
            LogMessage(message, severity, null);
            AttachMessage(message, severity, source);
        }

        /// <summary>
        /// Log the specified exception and message.
        /// </summary>
        public string LogException(Exception ex, string message)
        {
            return Log.Exception(ex, message + " project '" + _name + "'");
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity, string toolTip)
        {
            string prefix = (severity == MessageSeverity.Error ? "ERROR: " : (severity == MessageSeverity.Warning ? "Warning: " : ""));
            Log.WriteLine(prefix + "Project '" + _name + "': " + message, toolTip != null ? toolTip.TrimEnd() : null);
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity)
        {
            LogMessage(message, severity, null);
        }

        /// <summary>
        /// Parse and resolve all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ParseAndResolveCodeUnits(LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            if (statusCallback != null)
                statusCallback(LoadStatus.Parsing, null);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            ParseCodeUnits(loadOptions.HasFlag(LoadOptions.DoNotParseBodies) ? ParseFlags.SkipMethodBodies : ParseFlags.None);
            if (Unrecognized.Count > 0)
                Log.WriteLine("UNRECOGNIZED OBJECT COUNT: " + Unrecognized.Count);
            Log.WriteLine("Parsed project '" + Name + "', elapsed time: " + stopWatch.Elapsed.TotalSeconds.ToString("N3"));

            if (loadOptions.HasFlag(LoadOptions.ResolveSources))
            {
                if (statusCallback != null)
                    statusCallback(LoadStatus.Resolving, null);
                stopWatch.Restart();
                Resolver.ResolveAttempts = Resolver.ResolveFailures = 0;
                ResolveCodeUnits(loadOptions.HasFlag(LoadOptions.DoNotResolveBodies) ? ResolveFlags.SkipMethodBodies : ResolveFlags.None);
                Log.WriteLine(string.Format("Resolved project '{0}', elapsed time: {1:N3}, ResolveAttempts = {2:N0}, ResolveFailures = {3:N0}",
                    Name, stopWatch.Elapsed.TotalSeconds, Resolver.ResolveAttempts, Resolver.ResolveFailures));
            }
        }

        /// <summary>
        /// Parse and resolve all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ParseAndResolveCodeUnits(LoadOptions loadOptions)
        {
            ParseAndResolveCodeUnits(loadOptions, null);
        }

        /// <summary>
        /// Parse and resolve all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ParseAndResolveCodeUnits()
        {
            ParseAndResolveCodeUnits(LoadOptions.Complete, null);
        }

        /// <summary>
        /// Parse the specified name into a <see cref="NamespaceRef"/> or <see cref="TypeRef"/>, or a <see cref="Dot"/> or <see cref="Lookup"/> expression that evaluates to one.
        /// </summary>
        public Expression ParseName(string fullName)
        {
            RootNamespace rootNamespace = GetRootNamespace(ref fullName);
            return rootNamespace.ParseName(fullName);
        }

        /// <summary>
        /// Remove the specified <see cref="CodeUnit"/> from the <see cref="Project"/>, also removing the corresponding <see cref="FileItem"/>.
        /// </summary>
        /// <param name="codeUnit">The <see cref="CodeUnit"/> to be removed.</param>
        public void RemoveCodeUnit(CodeUnit codeUnit)
        {
            _codeUnits.Remove(codeUnit);
            _fileItems.RemoveAll(delegate (FileItem x) { return x.FileName == codeUnit.FileName; });
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(_name, this);
        }

        /// <summary>
        /// Rename the file name of the specified <see cref="CodeUnit"/> in the <see cref="Project"/>, also renaming the corresponding <see cref="FileItem"/>.
        /// </summary>
        /// <param name="codeUnit">The <see cref="CodeUnit"/> to be renamed.</param>
        /// <param name="newfileName">The new file name.</param>
        public void RenameCodeUnit(CodeUnit codeUnit, string newfileName)
        {
            foreach (FileItem fileItem in _fileItems)
            {
                if (fileItem.FileName == codeUnit.FileName)
                    fileItem.FileName = newfileName;
            }
            codeUnit.Name = Path.GetFileName(newfileName);
            codeUnit.FileName = newfileName;
        }

        /// <summary>
        /// Resolve all references.
        /// </summary>
        public void ResolveReferences()
        {
            // Resolve references to other projects and assembly files
            foreach (Reference reference in _references)
                reference.Resolve();
        }

        /// <summary>
        /// Save the <see cref="Project"/>.
        /// </summary>
        public void Save()
        {
            SaveAs(CodeUnit.GetSaveFileName(_fileName));
        }

        /// <summary>
        /// Save the <see cref="Project"/> plus all <see cref="CodeUnit"/>s.
        /// </summary>
        public void SaveAll()
        {
            Save();
            foreach (CodeUnit codeUnit in CodeUnits)
                codeUnit.Save();
        }

        /// <summary>
        /// Save the <see cref="Project"/> to the specified file name.
        /// </summary>
        public void SaveAs(string fileName)
        {
            // Don't try to save web projects
            if (!IsWebSiteProject)
            {
                try
                {
                    Log.DetailWriteLine("Saving project to '" + fileName + "' ...");

                    // VS project files are XML, are normally UTF8, don't use tabs, and use 2-space indents
                    // It's "preferred" to use XmlWriter.Create(), BUT adds a UTF-8 BOM to the file, which Visual Studio apparently
                    // does not normally use, despite the 'utf-8' encoding in the XML header.  Using 'new XmlTextWriter' for now
                    // because it's simpler and avoids the BOM problem.
                    //StreamWriter textWriter = new StreamWriter(fileName, false, FileEncoding);
                    //XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true };
                    //using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings))
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(fileName, FileEncoding))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.WriteStartDocument();
                        AsText(xmlWriter);
                        xmlWriter.WriteEndDocument();
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, "writing");
                }
            }
            _isNew = false;
        }

        /// <summary>
        /// Set the current configuration using the specified index.
        /// </summary>
        public void SetCurrentConfiguration(int index)
        {
            _currentConfiguration = _configurations[0];
        }

        /// <summary>
        /// Un-define a compiler direcive symbol for the current configuration.
        /// </summary>
        public void UndefineCompilerDirectiveSymbol(string name)
        {
            if (_currentConfiguration != null)
                _currentConfiguration.UndefineConstant(name);
        }

        /// <summary>
        /// Unload the project - clear its global namespace and loaded assemblies.
        /// </summary>
        public void Unload()
        {
            _globalNamespace.RemoveAll();
            _loadedAssemblies.Clear();
            foreach (Reference reference in _references)
                reference.Unload();
        }

        protected internal int LoadTypesFromReferenceAssembliesInternal()
        {
            Log.DetailWriteLine("Loading types from referenced assemblies for project: " + _name);

            // Load all types from all referenced assemblies.  When using reflection-only loads, this will also cause the
            // loading of additional assemblies via the OnReflectionOnlyAssemblyResolve event.
            return Enumerable.Sum(_references, delegate (Reference reference) { return reference.LoadTypes(); });
        }

        /// <summary>
        /// Add the implicit reference to 'mscorlib'.
        /// </summary>
        protected void AddImplicitMscorlibReference()
        {
            References.Add(new AssemblyReference(MsCorLib) { IsHidden = true });
        }

        /// <summary>
        /// Add an implicit reference to System.Core if required.
        /// </summary>
        protected void AddImplicitSystemCoreReferenceIfNecessary()
        {
            // Visual Studio 2010 adds an implicit reference to System.Core when targeting framework 3.5 or
            // higher if it doesn't already exist (it also has a bug where it auto-adds the reference to new
            // projects, and lets you remove it, but trying to add it back produces an error).
            // Apparently, the ProductVersion isn't updated for converted projects (only the SLN is updated), so
            // go by the FormatVersion of the solution file instead (11.00 = VS2010 = v10, 10.00 = VS2008 = v9,
            // 9.00 = VS2005 = v8, 8.00 = VS2003 = v7.1, ? = VS2002 = v7).
            bool isVS2010orLater = (_parent == null || GACUtil.CompareVersions(Solution.FormatVersion, "11.00") >= 0);
            if (isVS2010orLater && _targetFrameworkVersion != null)
            {
                if (!Enumerable.Any(References, delegate (Reference referenceBase) { return referenceBase is AssemblyReference && StringUtil.NNEqualsIgnoreCase(referenceBase.ShortName, SystemCore); })
                    && GACUtil.CompareVersions(_targetFrameworkVersion, "3.5") >= 0)
                    AddAssemblyReference(SystemCore, null, null, true);
            }
        }

        /// <summary>
        /// Add a reference object.
        /// </summary>
        protected void AddReference(Reference reference, Action<LoadStatus, CodeObject> statusCallback)
        {
            bool noStdLib = (_currentConfiguration != null && _currentConfiguration.NoStdLib);

            // All projects reference 'mscorlib' implicitly, so add it now if we don't have any references yet.
            // We must do this here and not in the constructor of Project, because the type of the 'mscorlib'
            // library is determined by the targeted framework version, which must be parsed or set first.
            if (References.Count == 0 && !noStdLib)
                AddImplicitMscorlibReference();

            // Ignore any manually added 'mscorlib' reference
            if (!StringUtil.NNEqualsIgnoreCase(reference.Name, MsCorLib) || noStdLib)
            {
                References.Add(reference);
                if (statusCallback != null)
                    statusCallback(LoadStatus.ObjectCreated, reference);
            }
        }

        /// <summary>
        /// Add a reference object.
        /// </summary>
        protected void AddReference(Reference reference)
        {
            AddReference(reference, null);
        }

        /// <summary>
        /// Determine the <see cref="RootNamespace"/> from any '::' prefix on the specified name, defaulting to the global namespace.
        /// </summary>
        protected RootNamespace GetRootNamespace(ref string name)
        {
            RootNamespace rootNamespace = null;
            int index = name.IndexOf(Lookup.ParseToken);
            if (index > 0)
            {
                string rootNamespaceName = name.Substring(0, index);
                name = name.Substring(index + Lookup.ParseToken.Length);
                rootNamespace = FindReferenceAliasNamespace(rootNamespaceName);
            }
            return (rootNamespace ?? _globalNamespace);
        }

        protected void Initialize()
        {
            _globalNamespace = new RootNamespace(ExternAlias.GlobalName, this);  // Setup the 'global' namespace
            _configurations = new ChildList<Configuration>(this);
            _references = new ChildList<Reference>(this);
            _codeUnits = new ChildList<CodeUnit>(this);
            _fileItems = new ChildList<FileItem>(this);
        }

        protected override void NotifyListedAnnotationAdded(Annotation annotation)
        {
            if (_parent != null)
                Solution.AnnotationAdded(annotation, this, null, true);
        }

        protected override void NotifyListedAnnotationRemoved(Annotation annotation)
        {
            if (_parent != null)
                Solution.AnnotationRemoved(annotation);
        }

        private void AddDependencies(Project project)
        {
            foreach (Reference reference in project.References)
            {
                if (reference is ProjectReference)
                {
                    Project referencedProject = ((ProjectReference)reference).ReferencedProject;
                    if (referencedProject != null)
                    {
                        _dependsOn.Add(referencedProject);
                        AddDependencies(referencedProject);
                    }
                }
            }
        }

        // Get a friend Project from an InternalsVisibleTo attribute
        private Project GetInternalsVisibleToProject(Attribute attribute)
        {
            if (_parent != null)
            {
                // Find the attribute call expression
                Call internalsCall = attribute.FindAttributeExpression(AssemblyUtil.InternalsVisibleToAttributeName) as Call;
                if (internalsCall != null && internalsCall.ArgumentCount > 0)
                {
                    // The parameter should be a string constant - however, it might be built using a string concatenation
                    // operator, and we want this code to work without resolving or evaluating expressions.  We only need
                    // the assembly name, which should always be a string literal, so check for an Add operator and if we've
                    // got one, just grab the left side.  Treat the result as a Literal, and trim leading '"'s.
                    Expression expression = internalsCall.Arguments[0];
                    if (expression is Add)
                        expression = ((Add)expression).Left;
                    Literal literal = expression as Literal;
                    if (literal != null)
                    {
                        string friendAssemblyName = literal.Text.Trim('"');
                        // Ignore any strong-name key
                        int commaIndex = friendAssemblyName.IndexOf(',');
                        if (commaIndex > 0)
                            friendAssemblyName = friendAssemblyName.Substring(0, commaIndex);
                        // Find the project by its assembly name
                        return Solution.FindProjectByAssemblyName(friendAssemblyName);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Parse a project from a standard VS project file.
        /// </summary>
        protected Project(string name, string fileName, Guid typeGuid, Guid projectGuid, Solution solution, bool isSupported, Action<LoadStatus, CodeObject> statusCallback)
        {
            Log.DetailWriteLine("Loading project '" + name + "' ...");

            // Initialize the project object
            _parent = solution;
            _name = name;
            _fileName = fileName;
            _typeGuid = typeGuid;
            _projectGuid = projectGuid;
            _notSupported = !(isSupported || IsWebSiteProject);
            _configurations = new ChildList<Configuration>(this);
            Initialize();
            if (statusCallback != null)
                statusCallback(LoadStatus.ObjectCreated, this);

            // Special handling for web site projects
            if (IsWebSiteProject)
            {
                ParseWebSiteProject(name, solution, statusCallback);
                return;
            }

            // Check that the file exists (to avoid an exception)
            if (!File.Exists(_fileName))
            {
                LogAndAttachMessage("Project file '" + _fileName + "' doesn't exist!", MessageSeverity.Error, MessageSource.Parse);
                return;
            }

            try
            {
                // Parse the project file
                bool firstElement = true;

                // Open the file and store the encoding and BOM status for use when saving
                FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] bom = new byte[3];
                fileStream.Read(bom, 0, 3);
                if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    FileHasUTF8BOM = true;
                fileStream.Position = 0;
                StreamReader streamReader = new StreamReader(fileStream);
                streamReader.Peek();  // Peek at the first char so that the encoding is determined
                FileEncoding = streamReader.CurrentEncoding;

                // Parse the file using an XmlReader
                using (XmlReader xmlReader = XmlReader.Create(streamReader))
                {
                    string projectPath = Path.GetDirectoryName(_fileName);
                    string xmlns = null;
                    Locations location = Locations.BeforeProperties;
                    bool lastElementWasItemGroupHeader = false;
                    string unhandledData = null;
                    HashSet<string> generatedFiles = new HashSet<string>();

                    // Read the next node
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            // Ignore the declaration node for now
                        }
                        else if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            if (firstElement)
                            {
                                firstElement = false;
                                if (xmlReader.Name == "VisualStudioProject")
                                {
                                    LogAndAttachMessage("Project files prior to VS 2005 aren't supported - upgrade the project file with VS first.", MessageSeverity.Error, MessageSource.Parse);
                                    return;
                                }
                                if (xmlReader.Name != "Project")
                                {
                                    LogAndAttachMessage("Project file format not recognized or not supported!", MessageSeverity.Error, MessageSource.Parse);
                                    return;
                                }
                                if (xmlReader.MoveToAttribute("ToolsVersion"))
                                    _toolsVersion = xmlReader.Value;
                                if (xmlReader.MoveToAttribute("DefaultTargets"))
                                    _defaultTargets = xmlReader.Value;
                                if (xmlReader.MoveToAttribute("xmlns"))
                                {
                                    _namespace = xmlReader.Value;
                                    xmlns = " xmlns=\"" + _namespace + "\"";
                                }
                            }
                            else if (xmlReader.Name == "PropertyGroup" && !xmlReader.IsEmptyElement)
                            {
                                if (!xmlReader.HasAttributes)
                                    location = Locations.MainProperties;
                                else
                                {
                                    xmlReader.MoveToFirstAttribute();
                                    if (xmlReader.Name == "Condition" && xmlReader.Value.Contains("$(Configuration)"))
                                    {
                                        AddConfiguration(new Configuration(xmlReader, this));
                                        string configurationName;
                                        string platform;
                                        solution.GetProjectConfiguration(solution.ActiveConfiguration, solution.ActivePlatform, this, out configurationName, out platform);
                                        _currentConfiguration = FindConfiguration(configurationName ?? _configurationName, platform ?? _platform);
                                    }
                                    else if (xmlReader.Name == "Label" && xmlReader.Value == "Globals")  // Used by C++ projects (.vcxproj)
                                        location = Locations.MainProperties;
                                    else
                                    {
                                        xmlReader.MoveToElement();
                                        unhandledData = xmlReader.ReadOuterXml();
                                    }
                                }
                            }
                            else if (xmlReader.Name == "ItemGroup" && !xmlReader.IsEmptyElement)
                            {
                                location = Locations.Items;
                                lastElementWasItemGroupHeader = true;
                                continue;
                            }
                            else if (location == Locations.MainProperties)
                            {
                                if (xmlReader.Name == "Configuration" && _configurationName == null)
                                    _configurationName = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "Platform" && _platform == null)
                                    _platform = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "ProductVersion")
                                    _productVersion = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "SchemaVersion")
                                    _schemaVersion = xmlReader.ReadString().Trim();
                                else if (StringUtil.NNEqualsIgnoreCase(xmlReader.Name, "ProjectGuid"))  // C++ uses 'ProjectGUID'
                                    _projectGuid = Guid.Parse(xmlReader.ReadString().Trim());
                                else if (xmlReader.Name == "ProjectTypeGuids")
                                {
                                    string[] guids = xmlReader.ReadString().Trim().Split(';');
                                    if (guids != null)
                                    {
                                        _projectTypeGuids = new List<Guid>();
                                        foreach (string guid in guids)
                                            _projectTypeGuids.Add(Guid.Parse(guid));
                                    }
                                }
                                else if (xmlReader.Name == "OutputType")
                                    _outputType = StringUtil.ParseEnum(xmlReader.ReadString(), OutputTypes.Library);
                                else if (xmlReader.Name == "OutputPath" && _outputPath == null)
                                    _outputPath = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "StartupObject")
                                    _startupObject = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "NoStandardLibraries")
                                    _noStandardLibraries = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "AppDesignerFolder")
                                    _appDesignerFolder = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "RootNamespace")
                                    _rootNamespace = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "AssemblyName")
                                    _assemblyName = xmlReader.ReadString().Trim();
                                else if (xmlReader.Name == "DeploymentDirectory")
                                    _deploymentDirectory = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "StartArguments")
                                    _startArguments = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "TargetFrameworkIdentifier")
                                    _targetFrameworkIdentifier = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "TargetFrameworkVersion")
                                    _targetFrameworkVersion = xmlReader.ReadString().Substring(1);  // Skip "v"
                                else if (xmlReader.Name == "TargetFrameworkProfile")
                                    _targetFrameworkProfile = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "FileAlignment")
                                    _fileAlignment = StringUtil.ParseInt(xmlReader.ReadString());
                                else if (xmlReader.Name == "WarningLevel")
                                    _warningLevel = StringUtil.ParseInt(xmlReader.ReadString());
                                else if (xmlReader.Name == "SignAssembly")
                                    _signAssembly = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "AssemblyOriginatorKeyFile")
                                    _assemblyOriginatorKeyFile = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "ReferencePath")
                                    _referencePath = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SccProjectName")
                                    _sccProjectName = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SccLocalPath")
                                    _sccLocalPath = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SccAuxPath")
                                    _sccAuxPath = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SccProvider")
                                    _sccProvider = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "FileUpgradeFlags")
                                    _fileUpgradeFlags = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "OldToolsVersion")
                                    _oldToolsVersion = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "UpgradeBackupLocation")
                                    _upgradeBackupLocation = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "ProjectType")
                                    _projectType = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SilverlightVersion")
                                    _silverlightVersion = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SilverlightApplication")
                                    _silverlightApplication = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "SupportedCultures")
                                    _supportedCultures = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "XapOutputs")
                                    _xapOutputs = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "GenerateSilverlightManifest")
                                    _generateSilverlightManifest = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "XapFilename")
                                    _xapFilename = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SilverlightManifestTemplate")
                                    _silverlightManifestTemplate = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "SilverlightAppEntry")
                                    _silverlightAppEntry = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "TestPageFileName")
                                    _testPageFileName = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "CreateTestPage")
                                    _createTestPage = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "ValidateXaml")
                                    _validateXaml = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "EnableOutOfBrowser")
                                    _enableOutOfBrowser = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "OutOfBrowserSettingsFile")
                                    _outOfBrowserSettingsFile = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "UsePlatformExtensions")
                                    _usePlatformExtensions = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "ThrowErrorsInValidation")
                                    _throwErrorsInValidation = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "LinkedServerProject")
                                    _linkedServerProject = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "MvcBuildViews")
                                    _mvcBuildViews = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "UseIISExpress")
                                    _useIISExpress = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "SilverlightApplicationList")
                                    _silverlightApplicationList = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "Nonshipping")
                                    _nonShipping = StringUtil.ParseBool(xmlReader.ReadString());
                                else
                                {
                                    if (_configurations.Count == 0)
                                        unhandledData = xmlReader.ReadOuterXml();
                                    else
                                    {
                                        unhandledData = "<PropertyGroup>" + xmlReader.ReadOuterXml();
                                        while (xmlReader.NodeType != XmlNodeType.EndElement || xmlReader.Name != "PropertyGroup")
                                            unhandledData += xmlReader.ReadOuterXml();
                                        unhandledData += "</PropertyGroup>";
                                        location = (_codeUnits.Count == 0 ? Locations.AfterProperties : Locations.AfterItems);
                                    }
                                }
                            }
                            else if (location == Locations.Items)
                            {
                                if (xmlReader.Name == "Reference")
                                    AddReference(new AssemblyReference(xmlReader, this), statusCallback);
                                else if (xmlReader.Name == "ProjectReference")
                                    AddReference(new ProjectReference(xmlReader, this), statusCallback);
                                else
                                {
                                    BuildActions buildAction = StringUtil.ParseEnum(xmlReader.Name, BuildActions.Unrecognized);
                                    if (buildAction != BuildActions.Unrecognized && xmlReader.HasAttributes)
                                    {
                                        xmlReader.MoveToFirstAttribute();
                                        if (xmlReader.Name == "Include")
                                            ParseFileItem(xmlReader, buildAction, lastElementWasItemGroupHeader, projectPath, generatedFiles, statusCallback);
                                        else
                                        {
                                            xmlReader.MoveToElement();
                                            unhandledData = xmlReader.ReadOuterXml();
                                        }
                                    }
                                    else
                                        unhandledData = xmlReader.ReadOuterXml();
                                }
                            }
                            else
                                unhandledData = xmlReader.ReadOuterXml();
                            lastElementWasItemGroupHeader = false;
                        }
                        else if (xmlReader.NodeType == XmlNodeType.EndElement)
                        {
                            if (xmlReader.Name == "PropertyGroup")
                                location = (_codeUnits.Count == 0 ? Locations.AfterProperties : Locations.AfterItems);
                            else if (xmlReader.Name == "ItemGroup")
                                location = Locations.AfterItems;
                        }
                        else if (xmlReader.NodeType == XmlNodeType.Comment)
                            _unhandledData.Add(new UnhandledData("  <!--" + xmlReader.Value + "-->", location));
                        else if (xmlReader.NodeType != XmlNodeType.Whitespace && xmlReader.NodeType != XmlNodeType.SignificantWhitespace)
                            unhandledData = xmlReader.ReadOuterXml();
                        if (unhandledData != null)
                        {
                            string rawData = unhandledData.Replace(xmlns, "");
                            if (!string.IsNullOrEmpty(rawData))
                                _unhandledData.Add(new UnhandledData("  " + rawData, location));
                            unhandledData = null;
                        }
                    }
                }

                // Add an implicit reference to System.Core under the proper circumstances
                AddImplicitSystemCoreReferenceIfNecessary();
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing", MessageSource.Parse);
            }
        }

        /// <summary>
        /// Parse a project from a file.
        /// </summary>
        public static Project Parse(string fileName, Solution solution, Action<LoadStatus, CodeObject> statusCallback)
        {
            // Determine the project type GUID
            Guid typeGuid;
            if (fileName.EndsWith(CSharpProjectFileExtension))
                typeGuid = CSProjectType;
            else if (fileName.EndsWith(VBProjectFileExtension))
                typeGuid = VBProjectType;
            else
                typeGuid = new Guid();

            return Parse(Path.GetFileNameWithoutExtension(fileName), fileName, typeGuid, Guid.Empty, solution, statusCallback);
        }

        /// <summary>
        /// Parse a project from a file.
        /// </summary>
        public static Project Parse(string fileName, Solution solution)
        {
            return Parse(fileName, solution, null);
        }

        /// <summary>
        /// Parse a project from a file.
        /// </summary>
        public static Project Parse(string name, string fileName, Guid typeGuid, Guid projectGuid, Solution solution, Action<LoadStatus, CodeObject> statusCallback)
        {
            // Create the project object
            Project project;
            if (typeGuid == CSProjectType)
                project = new Project(name, fileName, typeGuid, projectGuid, solution, true, statusCallback);
            else if (typeGuid == VBProjectType)
            {
                // VB projects aren't fully supported, but we parse the project file in order to get the OutputPath
                // and AssemblyName, which are needed to find and load the output assembly in order to resolve symbols.
                project = new Project(name, fileName, typeGuid, projectGuid, solution, false, statusCallback);
            }
            else
            {
                // We also parse other project file types in order to get the OutputPath and AssemblyName if possible,
                // so we can find and load the output assembly in order to resolve symbols.
                project = new Project(name, fileName, typeGuid, projectGuid, solution, false, statusCallback);
            }

            if (project.NotSupported)
                project.AttachMessage("Project type isn't fully supported (source files won't be parsed)", MessageSeverity.Warning, MessageSource.Parse);
            else
            {
                // Load user settings from any ".csproj.user" file
                string userSettingsFile = Path.ChangeExtension(fileName, ".csproj.user");
                project.ParseUserSettings(userSettingsFile);
            }

            return project;
        }

        /// <summary>
        /// Parse a project from a file.
        /// </summary>
        public static Project Parse(string name, string fileName, Guid typeGuid, Guid projectGuid, Solution solution)
        {
            return Parse(name, fileName, typeGuid, projectGuid, solution, null);
        }

        /// <summary>
        /// Parse all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ParseCodeUnits(ParseFlags flags)
        {
            foreach (CodeUnit codeUnit in _codeUnits)
                codeUnit.Parse(flags);
        }

        /// <summary>
        /// Parse file items.
        /// </summary>
        protected void ParseFileItem(XmlReader xmlReader, BuildActions buildAction, bool isFirstInGroup, string projectPath,
            HashSet<string> generatedFiles, Action<LoadStatus, CodeObject> statusCallback)
        {
            try
            {
                // Parse the file item
                FileItem fileItem = new FileItem(xmlReader, buildAction, projectPath, isFirstInGroup, this);
                _fileItems.Add(fileItem);

                // Special handling for source files
                string fileName = fileItem.FileName;
                bool lookForGeneratedFile = false;
                bool shouldHaveGeneratedFile = false;
                int extensionLength = 0;
                if (buildAction == BuildActions.Compile)
                {
                    // Add a CodeUnit for the source file
                    AddFile(fileName, false, statusCallback);

                    // If it's a ".xaml.cs" file, it SHOULD have a generated CS file
                    if (fileName.EndsWith(XamlCSharpCodeBehindExtension))
                    {
                        lookForGeneratedFile = shouldHaveGeneratedFile = true;
                        extensionLength = XamlCSharpCodeBehindExtension.Length;
                    }
                }
                else if (buildAction == BuildActions.Page)
                {
                    // If it's a ".xaml" file, it MIGHT have a generated CS file (it would be nice to know
                    // this for sure, but we'd have to parse and analyze the header of the XAML file).
                    // Look for a generated file, but don't log any messages if it's not found.
                    if (fileName.EndsWith(XamlFileExtension))
                    {
                        lookForGeneratedFile = true;
                        extensionLength = XamlFileExtension.Length;
                    }
                }
                if (lookForGeneratedFile)
                {
                    // Get the relativized path
                    fileName = FileUtil.RemoveCommonRootPath(fileName, projectPath + @"\");
                    int volumeSep = fileName.IndexOf(Path.VolumeSeparatorChar);
                    if (volumeSep > 0)
                        fileName = fileName.Substring(volumeSep + 1);

                    // Change the extension, and build the full path, using the platform if any (such as 'x86'),
                    // and the configuration name (such 'Debug' or 'Release') as currently saved in the project file.
                    // The 'obj' path will always use a subdirectory for the platform if it's not 'AnyCPU', but the 'bin'
                    // path (OutputPath) may or may not.
                    string platform = (ConfigurationPlatform ?? _platform);
                    fileName = projectPath + @"\obj\" + (platform != PlatformAnyCPU ? platform + @"\" : "")
                        + ConfigurationName + @"\" + fileName.Substring(0, fileName.Length - extensionLength) + XamlCSharpGeneratedExtension;
                    if (shouldHaveGeneratedFile || File.Exists(fileName))
                    {
                        // Only add the file if we haven't already, since there are 2 ways to get here from above
                        if (!generatedFiles.Contains(fileName))
                        {
                            generatedFiles.Add(fileName);
                            AddFile(fileName, true, statusCallback);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing file item in ", MessageSource.Parse);
            }
        }

        /// <summary>
        /// Parse user settings from any ".csproj.user" file.
        /// </summary>
        protected void ParseUserSettings(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            try
            {
                // Open the file and store the encoding and BOM status for use when saving
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] bom = new byte[3];
                fileStream.Read(bom, 0, 3);
                //if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                //    FileHasUTF8BOM = true;
                fileStream.Position = 0;
                StreamReader streamReader = new StreamReader(fileStream);
                streamReader.Peek();  // Peek at the first char so that the encoding is determined
                //FileEncoding = streamReader.CurrentEncoding;

                // Parse the file using an XmlReader
                using (XmlReader xmlReader = XmlReader.Create(streamReader))
                {
                    Locations location = Locations.BeforeProperties;

                    // Read the next node
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            // Ignore the declaration node for now
                        }
                        else if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "PropertyGroup" && !xmlReader.IsEmptyElement)
                            {
                                if (!xmlReader.HasAttributes)
                                    location = Locations.MainProperties;
                                else
                                {
                                    xmlReader.MoveToFirstAttribute();
                                    if (xmlReader.Name == "Condition" && xmlReader.Value.Contains("$(Configuration)"))
                                    {
                                        location = Locations.ConfigurationProperties;
                                        // Setup to read configuration-specific properties here
                                    }
                                    else
                                        xmlReader.MoveToElement();
                                }
                            }
                            else if (location == Locations.MainProperties)
                            {
                                //if (xmlReader.Name == "ProjectView")
                                //    _projectView = xmlReader.ReadString().Trim();
                                if (xmlReader.Name == "ReferencePath")
                                    _userReferencePath = xmlReader.ReadString().Trim();
                            }
                        }
                        else if (xmlReader.NodeType == XmlNodeType.EndElement)
                        {
                            if (xmlReader.Name == "PropertyGroup")
                                location = Locations.AfterProperties;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "parsing user settings for");
            }
        }

        /// <summary>
        /// Parse any 'Web.Config' file for configuration and references.
        /// </summary>
        /// <returns>True if any references were parsed.</returns>
        protected void ParseWebConfig(string path, bool isMasterFile, Action<LoadStatus, CodeObject> statusCallback)
        {
            string webConfigFile = Path.Combine(path, "Web.Config");
            if (File.Exists(webConfigFile))
            {
                LogMessage("Parsing '" + webConfigFile + "'...", MessageSeverity.Information);
                try
                {
                    // Parse the file using an XmlReader
                    FileStream fileStream = new FileStream(webConfigFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamReader streamReader = new StreamReader(fileStream);
                    using (XmlReader xmlReader = XmlReader.Create(streamReader))
                    {
                        bool inCompilation = false;
                        bool inAssemblies = false;

                        // Read the next node
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element)
                            {
                                if (xmlReader.Name == "compilation")
                                {
                                    inCompilation = !xmlReader.IsEmptyElement;
                                    // Don't read this config data from the master file - only a local one
                                    if (!isMasterFile && xmlReader.HasAttributes)
                                    {
                                        if (xmlReader.MoveToAttribute("debug"))
                                        {
                                            _configurationName = (StringUtil.ParseBool(xmlReader.Value) ? ConfigurationDebug : ConfigurationRelease);
                                            AddDefaultConfigurations();
                                            _configurations[0].OutputPath = "Bin";
                                            _configurations[1].OutputPath = "Bin";
                                            string configurationName = null;
                                            string platform;
                                            if (Solution != null)
                                                Solution.GetProjectConfiguration(Solution.ActiveConfiguration, Solution.ActivePlatform, this, out configurationName, out platform);
                                            _platform = PlatformAnyCPU;  // Always AnyCPU for web projects
                                            _currentConfiguration = FindConfiguration(configurationName ?? _configurationName, _platform);
                                        }
                                        if (xmlReader.MoveToAttribute("targetFramework"))
                                            _targetFrameworkVersion = xmlReader.Value;
                                    }
                                }
                                else if (inCompilation && xmlReader.Name == "assemblies" && !xmlReader.IsEmptyElement)
                                    inAssemblies = true;
                                else if (inAssemblies && xmlReader.Name == "add")
                                {
                                    if (xmlReader.HasAttributes)
                                    {
                                        if (xmlReader.MoveToAttribute("assembly"))
                                        {
                                            string assemblyName = xmlReader.Value;
                                            // Just ignore any '*', as we'll always look in the BIN directory anyway
                                            if (assemblyName != "*")
                                                AddReference(new AssemblyReference(assemblyName), statusCallback);
                                        }
                                    }
                                }
                            }
                            else if (xmlReader.NodeType == XmlNodeType.EndElement)
                            {
                                if (xmlReader.Name == "assemblies")
                                    inAssemblies = false;
                                else if (xmlReader.Name == "compilation")
                                    inCompilation = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogAndAttachException(ex, "parsing 'Web.Config' for ", MessageSource.Parse);
                }
            }
        }

        protected void ParseWebSiteProject(string name, Solution solution, Action<LoadStatus, CodeObject> statusCallback)
        {
            try
            {
                string path = solution.GetWebSiteDirectory(name);
                if (path != null)
                {
                    // Parse the local 'Web.Config' file
                    ParseWebConfig(path, false, statusCallback);

                    // Parse the master 'Web.Config' file
                    string runtimeVersion = TargetFrameworkVersion;
                    // For 3.0 or 3.5, use the 2.0 runtime location
                    if (GACUtil.CompareVersions(runtimeVersion, "4.0") < 0)
                        runtimeVersion = "2.0";
                    string runtimePath = FrameworkContext.GetRuntimeLocation(runtimeVersion) + @"Config\";
                    ParseWebConfig(runtimePath, true, statusCallback);

                    // Load any project references from the solution file entry
                    List<string> projectReferenceFileNames = new List<string>();
                    Solution.ProjectEntry projectEntry = solution.FindProjectEntry(name);
                    if (projectEntry != null)
                    {
                        Solution.ProjectSection projectSection = projectEntry.FindProjectSection(Solution.WebsitePropertiesProjectSection);
                        if (projectSection != null)
                        {
                            string projectReferencesRaw = projectSection.FindValue("ProjectReferences");
                            if (projectReferencesRaw != null)
                            {
                                string[] projectReferences = projectReferencesRaw.Trim('"').TrimEnd(';').Split(';');
                                foreach (string projectReference in projectReferences)
                                {
                                    // We can't look up project references by GUID here, because there might be forward references.
                                    // So, add the reference using the GUID as the name, and we'll resolve it later.
                                    string[] items = projectReference.Split('|');
                                    AddReference(new ProjectReference(items[0], items[1], Guid.Parse(items[0])), statusCallback);
                                    projectReferenceFileNames.Add(items[1]);
                                }
                            }
                        }
                    }

                    // Load any "BIN" references from the output path
                    string outputPath = Path.Combine(path, OutputPath);
                    if (Directory.Exists(outputPath))
                    {
                        foreach (string assemblyFile in Directory.EnumerateFiles(outputPath, "*.dll"))
                        {
                            if (!projectReferenceFileNames.Contains(Path.GetFileName(assemblyFile)))
                                AddReference(new AssemblyReference(Path.GetFileNameWithoutExtension(assemblyFile)), statusCallback);
                        }
                    }

                    // Load any source files from the physical path
                    foreach (string sourceFile in Directory.EnumerateFiles(path, "*" + CSharpFileExtension, SearchOption.AllDirectories))
                        AddFile(sourceFile, false, statusCallback);
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing website", MessageSource.Parse);
            }
        }

        /// <summary>
        /// Resolve all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ResolveCodeUnits(ResolveFlags flags)
        {
            // Do a 3-phase resolve if a specific phase wasn't specified by a higher level (the first phase stops at base lists
            // of type decls, the second at method/property bodies, and the third does the bodies - this resolves all base classes
            // and signatures first in order to resolve all references in a single attempt).
            if ((flags & (ResolveFlags.Phase1 | ResolveFlags.Phase2 | ResolveFlags.Phase3)) == 0)
            {
                foreach (CodeUnit codeUnit in _codeUnits)
                    codeUnit.Resolve(flags | ResolveFlags.Phase1);
                foreach (CodeUnit codeUnit in _codeUnits)
                    codeUnit.Resolve(flags | ResolveFlags.Phase2);
                foreach (CodeUnit codeUnit in _codeUnits)
                    codeUnit.Resolve(flags | ResolveFlags.Phase3);
            }
            else
            {
                foreach (CodeUnit codeUnit in _codeUnits)
                    codeUnit.Resolve(flags);
            }
        }

        /// <summary>
        /// Resolve all <see cref="CodeUnit"/>s in the <see cref="Project"/>.
        /// </summary>
        public void ResolveCodeUnits()
        {
            ResolveCodeUnits(ResolveFlags.None);
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            // Stop at the project level - there's nothing to resolve from here up
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
                writer.Write(GetRenderName());
            else
                base.AsText(writer, flags);
        }

        public void AsText(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(null, "Project", _namespace);
            if (_toolsVersion != null)
                xmlWriter.WriteAttributeString("ToolsVersion", _toolsVersion);
            xmlWriter.WriteAttributeString("DefaultTargets", _defaultTargets);

            WriteUnhandledData(xmlWriter, Locations.BeforeProperties);

            xmlWriter.WriteStartElement("PropertyGroup");

            xmlWriter.WriteStartElement("Configuration");
            xmlWriter.WriteAttributeString("Condition", " '$(Configuration)' == '' ");
            xmlWriter.WriteValue(ConfigurationName);
            xmlWriter.WriteEndElement();

            string platform = ConfigurationPlatform;
            if (platform != null)
            {
                xmlWriter.WriteStartElement("Platform");
                xmlWriter.WriteAttributeString("Condition", " '$(Platform)' == '' ");
                xmlWriter.WriteValue(_platform);
                xmlWriter.WriteEndElement();
            }

            if (_productVersion != null)
                xmlWriter.WriteElementString("ProductVersion", _productVersion);
            if (_schemaVersion != null)
                xmlWriter.WriteElementString("SchemaVersion", _schemaVersion);
            xmlWriter.WriteElementString("ProjectGuid", _projectGuid.ToString("B").ToUpper());
            if (_projectTypeGuids != null)
            {
                string guids = "";
                foreach (Guid guid in _projectTypeGuids)
                    guids = StringUtil.Append(guids, ";", guid.ToString("B").ToUpper());
                xmlWriter.WriteElementString("ProjectTypeGuids", guids);
            }
            xmlWriter.WriteElementString("OutputType", _outputType.ToString());
            if (_startupObject != null)
                xmlWriter.WriteElementString("StartupObject", _startupObject);
            if (_noStandardLibraries.HasValue)
                xmlWriter.WriteElementString("NoStandardLibraries", _noStandardLibraries.ToString().ToLower());
            if (_appDesignerFolder != null)
                xmlWriter.WriteElementString("AppDesignerFolder", _appDesignerFolder);
            if (_rootNamespace != null)
                xmlWriter.WriteElementString("RootNamespace", _rootNamespace);
            xmlWriter.WriteElementString("AssemblyName", _assemblyName);
            if (_deploymentDirectory != null)
                xmlWriter.WriteElementString("DeploymentDirectory", _deploymentDirectory);
            if (_startArguments != null)
                xmlWriter.WriteElementString("StartArguments", _startArguments);
            if (_targetFrameworkIdentifier != null)
                xmlWriter.WriteElementString("TargetFrameworkIdentifier", _targetFrameworkIdentifier);
            if (_targetFrameworkVersion != null)
                xmlWriter.WriteElementString("TargetFrameworkVersion", "v" + _targetFrameworkVersion);
            if (_targetFrameworkProfile != null)
                xmlWriter.WriteElementString("TargetFrameworkProfile", _targetFrameworkProfile);
            if (_fileAlignment > 0)
                xmlWriter.WriteElementString("FileAlignment", _fileAlignment.ToString());
            if (_warningLevel.HasValue)
                xmlWriter.WriteElementString("WarningLevel", _warningLevel.GetValueOrDefault().ToString());
            if (_signAssembly.HasValue)
                xmlWriter.WriteElementString("SignAssembly", _signAssembly.ToString().ToLower());
            if (_assemblyOriginatorKeyFile != null)
                xmlWriter.WriteElementString("AssemblyOriginatorKeyFile", _assemblyOriginatorKeyFile);
            if (_referencePath != null)
                xmlWriter.WriteElementString("ReferencePath", _referencePath);
            if (_sccProjectName != null)
                xmlWriter.WriteElementString("SccProjectName", _sccProjectName);
            if (_sccLocalPath != null)
                xmlWriter.WriteElementString("SccLocalPath", _sccLocalPath);
            if (_sccAuxPath != null)
                xmlWriter.WriteElementString("SccAuxPath", _sccAuxPath);
            if (_sccProvider != null)
                xmlWriter.WriteElementString("SccProvider", _sccProvider);
            if (_fileUpgradeFlags != null)
                xmlWriter.WriteElementString("FileUpgradeFlags", _fileUpgradeFlags);
            if (_oldToolsVersion != null)
                xmlWriter.WriteElementString("OldToolsVersion", _oldToolsVersion);
            if (_upgradeBackupLocation != null)
                xmlWriter.WriteElementString("UpgradeBackupLocation", _upgradeBackupLocation);
            if (_projectType != null)
                xmlWriter.WriteElementString("ProjectType", _projectType);
            if (_silverlightVersion != null)
            {
                xmlWriter.WriteElementString("SilverlightVersion", _silverlightVersion);
                xmlWriter.WriteElementString("SilverlightApplication", _silverlightApplication.ToString().ToLower());
            }
            if (_supportedCultures != null)
                xmlWriter.WriteElementString("SupportedCultures", _supportedCultures);
            if (_xapOutputs.HasValue)
                xmlWriter.WriteElementString("XapOutputs", _xapOutputs.ToString().ToLower());
            if (_generateSilverlightManifest.HasValue)
                xmlWriter.WriteElementString("GenerateSilverlightManifest", _generateSilverlightManifest.ToString().ToLower());
            if (_xapFilename != null)
                xmlWriter.WriteElementString("XapFilename", _xapFilename);
            if (_silverlightManifestTemplate != null)
                xmlWriter.WriteElementString("SilverlightManifestTemplate", _silverlightManifestTemplate);
            if (_silverlightAppEntry != null)
                xmlWriter.WriteElementString("SilverlightAppEntry", _silverlightAppEntry);
            if (_testPageFileName != null)
                xmlWriter.WriteElementString("TestPageFileName", _testPageFileName);
            if (_createTestPage.HasValue)
                xmlWriter.WriteElementString("CreateTestPage", _createTestPage.ToString().ToLower());
            if (_validateXaml.HasValue)
                xmlWriter.WriteElementString("ValidateXaml", _validateXaml.ToString().ToLower());
            if (_enableOutOfBrowser.HasValue)
                xmlWriter.WriteElementString("EnableOutOfBrowser", _enableOutOfBrowser.ToString().ToLower());
            if (_outOfBrowserSettingsFile != null)
                xmlWriter.WriteElementString("OutOfBrowserSettingsFile", _outOfBrowserSettingsFile);
            if (_usePlatformExtensions.HasValue)
                xmlWriter.WriteElementString("UsePlatformExtensions", _usePlatformExtensions.ToString().ToLower());
            if (_throwErrorsInValidation.HasValue)
                xmlWriter.WriteElementString("ThrowErrorsInValidation", _throwErrorsInValidation.ToString().ToLower());
            if (_linkedServerProject != null)
                xmlWriter.WriteElementString("LinkedServerProject", _linkedServerProject);
            if (_mvcBuildViews.HasValue)
                xmlWriter.WriteElementString("MvcBuildViews", _mvcBuildViews.ToString().ToLower());
            if (_useIISExpress.HasValue)
                xmlWriter.WriteElementString("UseIISExpress", _useIISExpress.ToString().ToLower());
            if (_silverlightApplicationList != null)
                xmlWriter.WriteElementString("SilverlightApplicationList", _silverlightApplicationList);
            if (_nonShipping.HasValue)
                xmlWriter.WriteElementString("Nonshipping", _nonShipping.ToString().ToLower());

            WriteUnhandledData(xmlWriter, Locations.MainProperties);

            xmlWriter.WriteEndElement();

            // Write project configurations
            foreach (Configuration projectConfiguration in _configurations)
                projectConfiguration.AsText(xmlWriter);

            WriteUnhandledData(xmlWriter, Locations.AfterProperties);

            // Write all references
            if (Enumerable.Any(_references, delegate (Reference reference) { return !reference.IsHidden; }))
            {
                xmlWriter.WriteStartElement("ItemGroup");
                foreach (Reference reference in _references)
                {
                    if (!reference.IsHidden)
                        reference.AsText(xmlWriter);
                }
                xmlWriter.WriteEndElement();
            }

            // Write all file items
            if (_fileItems.Count > 0)
            {
                xmlWriter.WriteStartElement("ItemGroup");
                bool first = true;
                foreach (FileItem fileItem in _fileItems)
                {
                    if (fileItem.IsFirstInGroup && !first)
                    {
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("ItemGroup");
                    }
                    fileItem.AsText(xmlWriter);
                    first = false;
                }
                xmlWriter.WriteEndElement();
            }

            // Write any unhandled file items
            if (Enumerable.Any(_unhandledData, delegate (UnhandledData unhandledData) { return unhandledData.Location == Locations.Items; }))
            {
                xmlWriter.WriteStartElement("ItemGroup");
                WriteUnhandledData(xmlWriter, Locations.Items);
                xmlWriter.WriteEndElement();
            }

            WriteUnhandledData(xmlWriter, Locations.AfterItems);

            xmlWriter.WriteEndElement();
        }

        public string GetRenderName()
        {
            if (IsWebSiteProject)
            {
                string name = _fileName;
                if (name.StartsWith("http:"))
                {
                    if (!name.EndsWith("/"))
                        name += "/";
                }
                else if (Path.IsPathRooted(name))
                {
                    if (name.EndsWith("\\"))
                        name = name.Substring(0, name.Length - 1);
                    int index = name.LastIndexOf('\\');
                    if (index > 0)
                        name = Path.GetPathRoot(name) + "..." + name.Substring(index);
                    name += "\\";
                }
                return name;
            }
            return _name;
        }

        protected void WriteRaw(XmlWriter xmlWriter, string rawData, string @namespace)
        {
            // This is workaround for a nasty bug in the XmlWriter where WriteRaw() turns off formatting, which MS refuses to fix.
            // Another option would be to always retain the unrecognized element (using ReadInnerXml to read the content, instead
            // of ReadOuterXml to read the element plus content), writing it with WriteStart/EndElement and using WriteRaw() only
            // to write the content (because formatting is turned off only until the parent element is ended).
            // See: https://connect.microsoft.com/VisualStudio/feedback/details/677081/xmlwriter-writeraw-permanently-disables-all-formatting
            string wrappedData = "<R xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">" + rawData.Trim('\r', '\n', ' ') + "</R>";
            XmlDocument xmlDocument = new XmlDocument { PreserveWhitespace = true };
            xmlDocument.LoadXml(wrappedData);
            XmlNode wrappedNode = xmlDocument.FirstChild.FirstChild;
            if (wrappedNode != null)
                wrappedNode.WriteTo(xmlWriter);
        }

        protected void WriteUnhandledData(XmlWriter xmlWriter, Locations location)
        {
            foreach (UnhandledData unhandledData in _unhandledData)
            {
                if (unhandledData.Location == location)
                    WriteRaw(xmlWriter, unhandledData.Data, _namespace);
            }
        }

        /// <summary>
        /// Enumeration of build actions.
        /// </summary>
        public enum BuildActions
        {
            Unrecognized,
            None,
            Compile,
            Content,
            EmbeddedResource,
            Resource,
            ApplicationDefinition,
            Page,
            SplashScreen,
            DesignData,
            DesignDataWithDesignTimeCreatableTypes,
            EntityDeploy,

            // Used internally (not in VS UI)
            AppDesigner,

            Folder
        }

        /// <summary>
        /// Enumeration of copy actions.
        /// </summary>
        public enum CopyActions { None, Always, PreserveNewest }

        /// <summary>
        /// Enumeration of debug types.
        /// </summary>
        public enum DebugTypes { none, full, pdbonly }

        /// <summary>
        /// Enumeration of error reporting types.
        /// </summary>
        public enum ErrorReporting { none, prompt, send, queue }

        /// <summary>
        /// Enumeration of types of serialization generation.
        /// </summary>
        public enum GenerateSerializationTypes { Auto, Off, On }

        /// <summary>
        /// Enumeration of output types.
        /// </summary>
        public enum OutputTypes { WinExe, Exe, Library }

        /// <summary>
        /// Configuration settings for a <see cref="Project"/>.
        /// </summary>
        public class Configuration : CodeObject
        {
            public string _defineConstants;
            public bool AllowUnsafeBlocks;
            public int BaseAddress = DefaultBaseAddress;
            public bool CheckForOverflowUnderflow;
            public string CodeAnalysisRules;
            public string CodeAnalysisRuleSet;
            public string ConfigurationOverrideFile;
            public bool DebugSymbols;
            public DebugTypes DebugType;
            public string DocumentationFile;
            public bool? EnableUnmanagedDebugging;
            public ErrorReporting ErrorReport;
            public int FileAlignment = DefaultFileAlignment;
            public GenerateSerializationTypes GenerateSerializationAssemblies;
            public bool? IncrementalBuild;

            // Values: Auto, Off, On
            public string LangVersion;

            public string Name;
            public bool NoConfig;
            public bool NoStdLib;
            public string NoWarn;

            // Values: none, full, pdbonly
            public bool Optimize;

            public string OutputPath;
            public string Platform;
            public string PlatformTarget;

            public bool RegisterForComInterop;

            // OLD?
            public bool RemoveIntegerChecks;

            public bool RunCodeAnalysis;

            public bool TreatWarningsAsErrors;

            // Values: default, ISO-1, ISO-2, 3
            // Values: 512, 1024, 2048, 4096, 8192
            public bool UseVSHostingProcess = true;

            // Values: none, prompt, send, queue
            public int WarningLevel = 4;

            public string WarningsAsErrors;
            // VB only?  Remove if false.

            /// <summary>
            /// Unhandled (unparsed or unrecognized) XML data in the project configuration.
            /// </summary>
            protected List<UnhandledData> _unhandledData = new List<UnhandledData>();

            /// <summary>
            /// A set of all defined constants for quick lookups.
            /// </summary>
            private HashSet<string> _constants = new HashSet<string>();

            /// <summary>
            /// Create a <see cref="Configuration"/>.
            /// </summary>
            public Configuration(string name, string platform, bool debugSymbols, DebugTypes debugType, bool optimize, string defineConstants)
            {
                Name = name;
                Platform = platform;
                DebugSymbols = debugSymbols;
                DebugType = debugType;
                Optimize = optimize;
                OutputPath = @"bin\" + name + @"\";
                DefineConstants = defineConstants;
            }

            /// <summary>
            /// The defined constants for this configuration as a set of strings.
            /// </summary>
            public HashSet<string> Constants
            {
                get { return _constants; }
            }

            /// <summary>
            /// The defined constants for this configuration as a delimited string.
            /// </summary>
            public string DefineConstants
            {
                get { return _defineConstants; }
                set
                {
                    _defineConstants = value;
                    _constants.Clear();
                    if (!string.IsNullOrEmpty(value))
                    {
                        foreach (string constant in value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                            _constants.Add(constant);
                    }
                }
            }

            /// <summary>
            /// The parent <see cref="Project"/>.
            /// </summary>
            public Project ParentProject
            {
                get { return _parent as Project; }
            }

            /// <summary>
            /// Define a constant.
            /// </summary>
            public void DefineConstant(string name)
            {
                _constants.Add(name);
                _defineConstants = StringUtil.ToString(_constants, ";");
            }

            /// <summary>
            /// Determine if the specified constant is defined.
            /// </summary>
            public bool IsConstantDefined(string name)
            {
                return (_constants.Contains(name) || name == "USING_NOVA" || name == "USING_NOVA_2");
            }

            /// <summary>
            /// Un-define a constant.
            /// </summary>
            public void UndefineConstant(string name)
            {
                _constants.Remove(name);
                _defineConstants = StringUtil.ToString(_constants, ";");
            }

            /// <summary>
            /// Parse from the specified <see cref="XmlReader"/>.
            /// </summary>
            public Configuration(XmlReader xmlReader, Project parent)
            {
                Parent = parent;
                string xmlns = " xmlns=\"" + parent._namespace + "\"";

                // Parse the configuration name and Platform from the Condition attribute
                string condition = xmlReader.Value;
                int start = condition.IndexOf("==");
                if (start > 0)
                {
                    start = condition.IndexOf('\'', start + 2);
                    if (start > 0)
                    {
                        ++start;
                        int end = condition.IndexOf('\'', start);
                        if (end > 0)
                        {
                            string value = condition.Substring(start, end - start);
                            string[] values = value.Split('|');
                            Name = values[0];
                            if (values.Length > 1)
                                Platform = values[1];
                        }
                    }
                }

                // Parse all child tags
                while (!(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "PropertyGroup") && xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == "DebugSymbols")
                            DebugSymbols = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "DebugType")
                            DebugType = StringUtil.ParseEnum(xmlReader.ReadString(), DebugTypes.none);
                        else if (xmlReader.Name == "Optimize")
                            Optimize = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "OutputPath")
                            OutputPath = xmlReader.ReadString().Trim();
                        else if (xmlReader.Name == "EnableUnmanagedDebugging")
                            EnableUnmanagedDebugging = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "DefineConstants")
                            DefineConstants = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "NoStdLib")
                            NoStdLib = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "NoConfig")
                            NoConfig = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "ErrorReport")
                            ErrorReport = StringUtil.ParseEnum(xmlReader.ReadString(), ErrorReporting.prompt);
                        else if (xmlReader.Name == "WarningLevel")
                            WarningLevel = StringUtil.ParseInt(xmlReader.ReadString());
                        else if (xmlReader.Name == "TreatWarningsAsErrors")
                            TreatWarningsAsErrors = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "IncrementalBuild")
                            IncrementalBuild = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "AllowUnsafeBlocks")
                            AllowUnsafeBlocks = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "AllowUnsafeBlocks")
                            AllowUnsafeBlocks = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "DocumentationFile")
                            DocumentationFile = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "PlatformTarget")
                            PlatformTarget = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "RunCodeAnalysis")
                            RunCodeAnalysis = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "CodeAnalysisRules")
                            CodeAnalysisRules = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "CodeAnalysisRuleSet")
                            CodeAnalysisRuleSet = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "NoWarn")
                            NoWarn = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "WarningsAsErrors")
                            WarningsAsErrors = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "RegisterForComInterop")
                            RegisterForComInterop = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "GenerateSerializationAssemblies")
                            GenerateSerializationAssemblies = StringUtil.ParseEnum(xmlReader.ReadString(), GenerateSerializationTypes.Auto);
                        else if (xmlReader.Name == "LangVersion")
                            LangVersion = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "CheckForOverflowUnderflow")
                            CheckForOverflowUnderflow = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "FileAlignment")
                            FileAlignment = StringUtil.ParseInt(xmlReader.ReadString());
                        else if (xmlReader.Name == "BaseAddress")
                            BaseAddress = StringUtil.ParseInt(xmlReader.ReadString());
                        else if (xmlReader.Name == "UseVSHostingProcess")
                            UseVSHostingProcess = StringUtil.ParseBool(xmlReader.ReadString());
                        else if (xmlReader.Name == "ConfigurationOverrideFile")
                            ConfigurationOverrideFile = StringUtil.EmptyAsNull(xmlReader.ReadString());
                        else if (xmlReader.Name == "RemoveIntegerChecks")
                            RemoveIntegerChecks = StringUtil.ParseBool(xmlReader.ReadString());
                        else
                        {
                            string unhandledData = xmlReader.ReadOuterXml().Replace(xmlns, "");
                            if (!string.IsNullOrEmpty(unhandledData))
                                _unhandledData.Add(new UnhandledData("    " + unhandledData, Locations.ConfigurationProperties));
                        }
                    }
                }
            }

            /// <summary>
            /// Save to the specified <see cref="XmlWriter"/>.
            /// </summary>
            public void AsText(XmlWriter xmlWriter)
            {
                Project parentProject = ParentProject;

                xmlWriter.WriteStartElement("PropertyGroup");
                string configuration = (Platform != null ? " '$(Configuration)|$(Platform)' == '" + Name + "|" + Platform + "' "
                    : " '$(Configuration)' == '" + Name + "' ");
                xmlWriter.WriteAttributeString("Condition", configuration);

                if (DebugSymbols)
                    xmlWriter.WriteElementString("DebugSymbols", "true");
                if (DebugType != DebugTypes.none)
                    xmlWriter.WriteElementString("DebugType", DebugType.ToString());
                xmlWriter.WriteElementString("Optimize", Optimize.ToString().ToLower());
                if (OutputPath != null)
                    xmlWriter.WriteElementString("OutputPath", OutputPath);
                if (EnableUnmanagedDebugging.HasValue)
                    xmlWriter.WriteElementString("EnableUnmanagedDebugging", EnableUnmanagedDebugging.ToString().ToLower());
                if (DefineConstants != null)
                    xmlWriter.WriteElementString("DefineConstants", DefineConstants);
                if (NoStdLib)
                    xmlWriter.WriteElementString("NoStdLib", NoStdLib.ToString().ToLower());
                if (NoConfig)
                    xmlWriter.WriteElementString("NoConfig", NoConfig.ToString().ToLower());
                if (ErrorReport != ErrorReporting.none)
                    xmlWriter.WriteElementString("ErrorReport", ErrorReport.ToString());
                xmlWriter.WriteElementString("WarningLevel", WarningLevel.ToString());
                if (TreatWarningsAsErrors)
                    xmlWriter.WriteElementString("TreatWarningsAsErrors", TreatWarningsAsErrors.ToString().ToLower());
                if (IncrementalBuild.HasValue)
                    xmlWriter.WriteElementString("IncrementalBuild", IncrementalBuild.ToString().ToLower());
                if (AllowUnsafeBlocks)
                    xmlWriter.WriteElementString("AllowUnsafeBlocks", AllowUnsafeBlocks.ToString().ToLower());
                if (DocumentationFile != null)
                    xmlWriter.WriteElementString("DocumentationFile", (parentProject != null ? FileUtil.RemoveCommonRootPath(DocumentationFile, parentProject.FileName) : DocumentationFile));
                if (PlatformTarget != null)
                    xmlWriter.WriteElementString("PlatformTarget", PlatformTarget);
                if (RunCodeAnalysis)
                    xmlWriter.WriteElementString("RunCodeAnalysis", RunCodeAnalysis.ToString().ToLower());
                if (CodeAnalysisRules != null)
                    xmlWriter.WriteElementString("CodeAnalysisRules", CodeAnalysisRules);
                if (CodeAnalysisRuleSet != null)
                    xmlWriter.WriteElementString("CodeAnalysisRuleSet", CodeAnalysisRuleSet);
                if (NoWarn != null)
                    xmlWriter.WriteElementString("NoWarn", NoWarn);
                if (WarningsAsErrors != null)
                    xmlWriter.WriteElementString("WarningsAsErrors", WarningsAsErrors);
                if (RegisterForComInterop)
                    xmlWriter.WriteElementString("RegisterForComInterop", RegisterForComInterop.ToString().ToLower());
                if (GenerateSerializationAssemblies != GenerateSerializationTypes.Auto)
                    xmlWriter.WriteElementString("GenerateSerializationAssemblies", GenerateSerializationAssemblies.ToString());
                if (LangVersion != null && LangVersion != "default")
                    xmlWriter.WriteElementString("LangVersion", LangVersion);
                if (CheckForOverflowUnderflow)
                    xmlWriter.WriteElementString("CheckForOverflowUnderflow", CheckForOverflowUnderflow.ToString().ToLower());
                if (FileAlignment != DefaultFileAlignment)
                    xmlWriter.WriteElementString("FileAlignment", FileAlignment.ToString());
                if (BaseAddress != DefaultBaseAddress)
                    xmlWriter.WriteElementString("BaseAddress", BaseAddress.ToString());
                if (!UseVSHostingProcess)
                    xmlWriter.WriteElementString("UseVSHostingProcess", UseVSHostingProcess.ToString().ToLower());
                if (ConfigurationOverrideFile != null)
                    xmlWriter.WriteElementString("ConfigurationOverrideFile", ConfigurationOverrideFile);
                if (RemoveIntegerChecks)
                    xmlWriter.WriteElementString("RemoveIntegerChecks", RemoveIntegerChecks.ToString().ToLower());

                if (parentProject != null)
                {
                    foreach (UnhandledData unhandledData in _unhandledData)
                        parentProject.WriteRaw(xmlWriter, unhandledData.Data, parentProject._namespace);
                }

                xmlWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Represents a file item in a <see cref="Project"/> file.
        /// </summary>
        public class FileItem : CodeObject
        {
            public bool AutoGen;

            /// <summary>
            /// The build action for the file.
            /// Values: None, Compile, Content, EmbeddedResource, Resource, ApplicationDefinition, Page, SplashScreen, DesignData, DesignDataWithDesignTimeCreatableTypes, EntityDeploy
            /// </summary>
            public BuildActions BuildAction;

            /// <summary>
            /// The copy action for the file.  Values: None, Always, PreserveNewest
            /// </summary>
            public CopyActions CopyToOutputDirectory;

            public string CustomToolNamespace;

            public string DependentUpon;

            public bool DesignTime;

            public bool DesignTimeSharedInput;

            /// <summary>
            /// The file name.
            /// </summary>
            public string FileName;

            public string Generator;

            /// <summary>
            /// True if the file appears first in the current group in the <see cref="Project"/> file.
            /// </summary>
            public bool IsFirstInGroup;

            public string LastGenOutput;

            /// <summary>
            /// The link for the file (if any).
            /// </summary>
            public string Link;

            public string SubType;

            /// <summary>
            /// Unhandled (unparsed or unrecognized) XML data in the file item.
            /// </summary>
            protected List<UnhandledData> _unhandledData = new List<UnhandledData>();

            /// <summary>
            /// Create a <see cref="FileItem"/>.
            /// </summary>
            public FileItem(BuildActions buildAction, string fileName)
            {
                BuildAction = buildAction;
                FileName = fileName;
            }

            /// <summary>
            /// The parent <see cref="Project"/>.
            /// </summary>
            public Project ParentProject
            {
                get { return _parent as Project; }
            }

            /// <summary>
            /// Parse from the specified <see cref="XmlReader"/>.
            /// </summary>
            public FileItem(XmlReader xmlReader, BuildActions buildAction, string projectPath, bool isFirstInGroup, Project parent)
            {
                Parent = parent;
                IsFirstInGroup = isFirstInGroup;
                BuildAction = buildAction;
                FileName = xmlReader.Value;
                if (!Path.IsPathRooted(FileName))
                    FileName = projectPath + @"\" + FileName;
                xmlReader.MoveToContent();

                // Parse any child elements
                if (!xmlReader.IsEmptyElement)
                {
                    string xmlns = " xmlns=\"" + parent._namespace + "\"";
                    while (!(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == buildAction.ToString()) && xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "Link")
                                Link = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            else if (xmlReader.Name == "CopyToOutputDirectory")
                                CopyToOutputDirectory = StringUtil.ParseEnum(xmlReader.ReadString(), CopyActions.None);
                            else if (xmlReader.Name == "AutoGen")
                                AutoGen = StringUtil.ParseBool(xmlReader.ReadString());
                            else if (xmlReader.Name == "DesignTime")
                                DesignTime = StringUtil.ParseBool(xmlReader.ReadString());
                            else if (xmlReader.Name == "DesignTimeSharedInput")
                                DesignTimeSharedInput = StringUtil.ParseBool(xmlReader.ReadString());
                            else if (xmlReader.Name == "DependentUpon")
                                DependentUpon = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            else if (xmlReader.Name == "Generator")
                            {
                                if (Generator == null)  // Only take the first one in case of dups
                                    Generator = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            }
                            else if (xmlReader.Name == "LastGenOutput")
                                LastGenOutput = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            else if (xmlReader.Name == "SubType")
                            {
                                if (SubType == null)  // Only take the first one in case of dups)
                                    SubType = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            }
                            else if (xmlReader.Name == "CustomToolNamespace")
                                CustomToolNamespace = StringUtil.EmptyAsNull(xmlReader.ReadString());
                            else
                            {
                                string unhandledData = xmlReader.ReadOuterXml().Replace(xmlns, "");
                                if (!string.IsNullOrEmpty(unhandledData))
                                    _unhandledData.Add(new UnhandledData("    " + unhandledData, Locations.ConfigurationProperties));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Save to the specified <see cref="XmlWriter"/>.
            /// </summary>
            public void AsText(XmlWriter xmlWriter)
            {
                Project parentProject = ParentProject;

                xmlWriter.WriteStartElement(BuildAction.ToString());
                xmlWriter.WriteAttributeString("Include", (parentProject != null ? FileUtil.RemoveCommonRootPath(FileName, parentProject.FileName) : FileName));

                if (Link != null)
                    xmlWriter.WriteElementString("Link", Link);
                if (CopyToOutputDirectory != CopyActions.None)
                    xmlWriter.WriteElementString("CopyToOutputDirectory", CopyToOutputDirectory.ToString());
                if (AutoGen)
                    xmlWriter.WriteElementString("AutoGen", AutoGen.ToString());
                if (DesignTime)
                    xmlWriter.WriteElementString("DesignTime", DesignTime.ToString());
                if (DesignTimeSharedInput)
                    xmlWriter.WriteElementString("DesignTimeSharedInput", DesignTimeSharedInput.ToString());
                if (DependentUpon != null)
                    xmlWriter.WriteElementString("DependentUpon", DependentUpon);
                if (Generator != null)
                    xmlWriter.WriteElementString("Generator", Generator);
                if (LastGenOutput != null)
                    xmlWriter.WriteElementString("LastGenOutput", LastGenOutput);
                if (SubType != null)
                    xmlWriter.WriteElementString("SubType", SubType);
                if (CustomToolNamespace != null)
                    xmlWriter.WriteElementString("CustomToolNamespace", CustomToolNamespace);

                if (parentProject != null)
                {
                    foreach (UnhandledData unhandledData in _unhandledData)
                        parentProject.WriteRaw(xmlWriter, unhandledData.Data, parentProject._namespace);
                }

                xmlWriter.WriteEndElement();
            }
        }

        public enum Locations { BeforeProperties, MainProperties, ConfigurationProperties, AfterProperties, Items, AfterItems }

        /// <summary>
        /// Represents unhandled data in a <see cref="Project"/> file.
        /// </summary>
        public class UnhandledData
        {
            /// <summary>
            /// The unhandled data.
            /// </summary>
            public string Data;

            /// <summary>
            /// The location of the unhandled data.
            /// </summary>
            public Locations Location;

            /// <summary>
            /// Create an <see cref="UnhandledData"/> object.
            /// </summary>
            public UnhandledData(string data, Locations location)
            {
                Data = data;
                Location = location;
            }
        }
    }
}