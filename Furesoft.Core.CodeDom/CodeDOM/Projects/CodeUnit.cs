// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares a unit of independent code that belongs to the root-level namespace (also known as a "compilation unit").
    /// Usually is the contents of a source file, but it can optionally be in-memory only.
    /// A <see cref="CodeUnit"/> is essentially a <see cref="NamespaceDecl"/> of the global namespace.
    /// </summary>
    /// <remarks>
    /// The format of a code unit is (in order):
    ///     - Zero or more "extern alias" directives
    ///     - Zero or more "using" directives (or "using aliasname = ...")
    ///     - Zero or more global attributes
    ///     - Zero or more namespace member declarations (child namespaces and/or type declarations)
    /// Of course, comments and preprocessor directives may be mixed in.
    /// 
    /// The term "code unit" is used because the code might not be mapped to a file, and because it's shorter
    /// and more generic than the term "compilation unit" (which is specifically associated with compilation).
    /// 
    /// Note that multiple CodeUnits can exist that map to the same physical file, in the case where the same file is a member
    /// of more than one project in the same solution.  This is necessary, because each CodeUnit might have different compiler
    /// directives defined, resulting in a different parse tree.  If saved to the file, each CodeUnit should produce the exact
    /// same save text, so no problems will result even though they write to the same file.
    /// </remarks>
    public class CodeUnit : NamespaceDecl, INamedCodeObject, IFile, IComparable
    {
        #region /* STATIC FIELDS */

        /// <summary>
        /// Determines if changes are saved to a separate ".Nova.cs" file instead of the original.
        /// </summary>
        public static bool SaveChangesToSeparateFile;

        /// <summary>
        /// Determines if messages in workflow code-beside files (".xoml.cs") are listed.
        /// </summary>
        public static bool ListWorkflowFileErrors;

        #endregion

        #region /* FIELDS */

        protected string _name;                    // Name of source file or in-memory source
        protected bool _isNew;                     // True if newly created and not saved yet
        protected string _fileName;                // The file name (if any)
        protected string _code;                    // Optional in-memory source string (in lieu of a file)
        protected int _totalLines;                 // Total number of text lines in the source (when first parsed)
        protected int _SLOC;                       // "Source Lines Of Code" in the source (when first parsed)
        protected bool _isWorkflowCodeBesideFile;  // True if this is a Workflow code-beside file

        /// <summary>
        /// Compiler directive symbols defined in the current file.
        /// </summary>
        protected HashSet<string> _compilerDirectiveSymbols = new HashSet<string>();

        /// <summary>
        /// Generated 'extern alias global' statement.
        /// </summary>
        protected ExternAlias _globalAlias;

        /// <summary>
        /// All 'listed' code annotations (<see cref="Message"/>s and special <see cref="Comment"/>s) for this <see cref="CodeUnit"/>.
        /// </summary>
        protected List<Annotation> _listedAnnotations = new List<Annotation>();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a new <see cref="CodeUnit"/> with the specified file name (or text source) and parent <see cref="Project"/>.
        /// </summary>
        public CodeUnit(string fileName, string code, Project project)
            : base(new NamespaceRef(project.GlobalNamespace) { IsGenerated = true })
        {
            Name = Path.GetFileName(fileName);
            _isNew = true;
            _fileName = fileName;
            if (!Path.HasExtension(fileName))
                _fileName += Project.CSharpFileExtension;
            FileEncoding = Encoding.UTF8;  // Default to UTF8 encoding with a BOM
            FileHasUTF8BOM = true;
            _parent = project;
            _code = code;
        }

        /// <summary>
        /// Create a new <see cref="CodeUnit"/> with the specified file name and parent <see cref="Project"/>.
        /// </summary>
        public CodeUnit(string fileName, Project project)
            : this(fileName, null, project)
        { }

        #endregion

        #region /* STATIC CONSTRUCTOR */

        static CodeUnit()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            ForceReference();
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="CodeUnit"/>.  If associated with a file, this is the file name and extension.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _isWorkflowCodeBesideFile = _name.EndsWith(Project.WorkflowCSharpCodeBesideFileExtension);
            }
        }

        /// <summary>
        /// True if the <see cref="CodeUnit"/> is newly created and hasn't been saved yet.
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
        }

        /// <summary>
        /// True if the <see cref="CodeUnit"/> is associated with a file (as opposed to being only in memory).
        /// </summary>
        public bool IsFile
        {
            get { return (_code == null); }
        }

        /// <summary>
        /// The associated file name of the <see cref="CodeUnit"/>.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// True if the associated file exists.
        /// </summary>
        public bool FileExists
        {
            get { return File.Exists(_fileName); }
        }

        /// <summary>
        /// The encoding of the file (normally UTF8).
        /// </summary>
        public Encoding FileEncoding { get; set; }

        /// <summary>
        /// True if the file has a UTF8 byte-order-mark.
        /// </summary>
        public bool FileHasUTF8BOM { get; set; }

        /// <summary>
        /// True if the associated file is formatted using tabs, otherwise false (using spaces).
        /// </summary>
        public bool FileUsingTabs { get; set; }

        /// <summary>
        /// True if the <see cref="CodeUnit"/> contains C# code.
        /// </summary>
        public bool IsCSharp
        {
            get
            {
                // Treat no extension as C# so that in-memory code can omit the extension
                string extension = Path.GetExtension(Name);
                return (string.IsNullOrEmpty(extension) || extension == Project.CSharpFileExtension);
            }
        }

        /// <summary>
        /// The associated text source code if no file is being used.
        /// </summary>
        public string Code
        {
            get { return _code; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "file"; }
        }

        /// <summary>
        /// The parent <see cref="Project"/>.
        /// </summary>
        public Project Project
        {
            get { return _parent as Project; }
        }

        /// <summary>
        /// The implied global extern alias to the global <see cref="RootNamespace"/>.
        /// </summary>
        public ExternAlias GlobalAlias
        {
            get
            {
                if (_globalAlias == null && _parent != null)
                    _globalAlias = new ExternAlias(Project.GlobalNamespace) { Parent = this, IsGenerated = true };
                return _globalAlias;
            }
        }

        /// <summary>
        /// True for all <see cref="BlockStatement"/>s that have a header (all except <see cref="CodeUnit"/> and <see cref="BlockDecl"/>).
        /// </summary>
        public override bool HasHeader
        {
            get { return false; }
        }

        /// <summary>
        /// True if a <see cref="BlockStatement"/> is at the top level (those that have no header and no indent).
        /// For example, a <see cref="CodeUnit"/>, a <see cref="BlockDecl"/> with no parent, or a <see cref="DocComment"/> parent.
        /// </summary>
        public override bool IsTopLevel
        {
            get { return true; }
        }

        /// <summary>
        /// All 'listed' code annotations (<see cref="Message"/>s and special <see cref="Comment"/>s) for this CodeUnit.
        /// </summary>
        public List<Annotation> ListedAnnotations
        {
            get { return _listedAnnotations; }
        }

        /// <summary>
        /// Total number of text lines in the source (when first parsed).
        /// </summary>
        public int TotalLines
        {
            get { return _totalLines; }
        }

        /// <summary>
        /// "Source Lines Of Code" in the source (when first parsed).
        /// </summary>
        public int SLOC
        {
            get { return _SLOC; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Load a <see cref="CodeUnit"/> directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        ///   - Resolve all symbolic references in the code file.
        /// The 'DoNotResolve' option skips resolving symbolic references, which is useful if that step is not required, such as when
        /// only formatting code which doesn't rely on resolved references.  The 'LoadOnly' option does not apply in this case, and is
        /// ignored if specified.
        /// Note that any external references in the <see cref="CodeUnit"/> will fail to resolve, since there is no Project and thus no assembly
        /// or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create a
        /// dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit Load(string fileName, LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            CodeUnit codeUnit = null;
            try
            {
                // Handle a relative path to the file
                if (!Path.IsPathRooted(fileName))
                    fileName = FileUtil.CombineAndNormalizePath(Environment.CurrentDirectory, fileName);

                // Abort if the file doesn't exist - otherwise, the Parse() method will end up returning a valid but empty object
                // with an error message attached (which is done so errors can appear inside a loaded Solution tree).
                if (!File.Exists(fileName))
                {
                    Log.WriteLine("ERROR: File '" + fileName + "' does not exist.");
                    return null;
                }

                // Create a dummy project for the file, since it is being loaded directly
                Project project = new Project(Path.ChangeExtension(fileName, Project.CSharpProjectFileExtension), null);
                if (statusCallback != null)
                    statusCallback(LoadStatus.ObjectCreated, project);

                // Create, parse and resolve the code unit, and log statistics
                codeUnit = new CodeUnit(fileName, project);
                codeUnit.ParseResolveLog(loadOptions, statusCallback);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading file");
            }
            return codeUnit;
        }

        /// <summary>
        /// Load a <see cref="CodeUnit"/> directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        ///   - Resolve all symbolic references in the code file.
        /// The 'DoNotResolve' option skips resolving symbolic references, which is useful if that step is not required, such as when
        /// only formatting code which doesn't rely on resolved references.  The 'LoadOnly' option does not apply in this case, and is
        /// ignored if specified.
        /// Note that any external references in the <see cref="CodeUnit"/> will fail to resolve, since there is no Project and thus no assembly
        /// or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create a
        /// dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit Load(string fileName, LoadOptions loadOptions)
        {
            return Load(fileName, loadOptions, null);
        }

        /// <summary>
        /// Load a <see cref="CodeUnit"/> directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        ///   - Resolve all symbolic references in the code file.
        /// The 'DoNotResolve' option skips resolving symbolic references, which is useful if that step is not required, such as when
        /// only formatting code which doesn't rely on resolved references.  The 'LoadOnly' option does not apply in this case, and is
        /// ignored if specified.
        /// Note that any external references in the <see cref="CodeUnit"/> will fail to resolve, since there is no Project and thus no assembly
        /// or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create a
        /// dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit Load(string fileName)
        {
            return Load(fileName, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load a code unit directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <param name="name">An optional name for the code fragment.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed, and optionally resolved.
        /// Note that any external references in the code fragment will fail to resolve, since there is no <see cref="Project"/> and
        /// thus no assembly or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment, string name, LoadOptions loadOptions)
        {
            CodeUnit codeUnit = null;
            try
            {
                // Create a dummy project for the fragment
                Project project = new Project(name + Project.CSharpProjectFileExtension, null);

                // Create, parse and resolve the code fragment, and log statistics
                codeUnit = new CodeUnit(name, codeFragment, project);
                codeUnit.ParseResolveLog(loadOptions, null);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading fragment");
            }
            return codeUnit;
        }

        /// <summary>
        /// Load a code unit directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <param name="name">An optional name for the code fragment.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed, and optionally resolved.
        /// Note that any external references in the code fragment will fail to resolve, since there is no <see cref="Project"/> and
        /// thus no assembly or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment, string name)
        {
            return LoadFragment(codeFragment, name, LoadOptions.Complete);
        }

        /// <summary>
        /// Load a code unit directly from the specified source file (without a <see cref="Project"/> or <see cref="Solution"/>).
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed, and optionally resolved.
        /// Note that any external references in the code fragment will fail to resolve, since there is no <see cref="Project"/> and
        /// thus no assembly or project references.  Also, any compiler directive symbols will be undefined.  To work around these issues, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing and resolving them.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment)
        {
            return LoadFragment(codeFragment, null, LoadOptions.Complete);
        }

        /// <summary>
        /// Parse and optionally resolve the <see cref="CodeUnit"/> (or fragment), and log statistics if requested.
        /// </summary>
        public void ParseResolveLog(LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            Stopwatch overallStopWatch = new Stopwatch();
            overallStopWatch.Start();
            GC.Collect();
            long startBytes = GC.GetTotalMemory(true);

            // Parse and resolve the code unit
            if (statusCallback != null)
                statusCallback(LoadStatus.Parsing, null);
            Unrecognized.Count = 0;
            Parse(loadOptions.HasFlag(LoadOptions.DoNotParseBodies) ? ParseFlags.SkipMethodBodies : ParseFlags.None);
            if (Unrecognized.Count > 0)
                Log.WriteLine("UNRECOGNIZED OBJECT COUNT: " + Unrecognized.Count);
            Log.WriteLine("Parsed " + (IsFile ? "file" : "fragment") + " '" + Name + "', total elapsed time: " + overallStopWatch.Elapsed.TotalSeconds.ToString("N3"));

            if (loadOptions.HasFlag(LoadOptions.ResolveSources))
            {
                if (statusCallback != null)
                    statusCallback(LoadStatus.Resolving, null);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Restart();
                Resolver.ResolveAttempts = Resolver.ResolveFailures = 0;
                Resolve();
                Log.WriteLine(string.Format("Resolved " + (IsFile ? "file" : "fragment") + " '{0}', elapsed time: {1:N3}, ResolveAttempts = {2:N0}, ResolveFailures = {3:N0}",
                    Name, stopWatch.Elapsed.TotalSeconds, Resolver.ResolveAttempts, Resolver.ResolveFailures));
            }

            long memoryUsage = GC.GetTotalMemory(true) - startBytes;
            Log.WriteLine(string.Format("Total elapsed time: {0:N3}, memory usage: {1} MBs", overallStopWatch.Elapsed.TotalSeconds, memoryUsage / (1024 * 1024)));

            if (statusCallback != null)
                statusCallback(LoadStatus.LoggingResults, null);
            LogMessageCounts(loadOptions.HasFlag(LoadOptions.LogMessages));
        }

        /// <summary>
        /// Calculate message counts.
        /// </summary>
        public void GetMessageCounts(out int errorCount, out int warningCount, out int commentCount)
        {
            // Calculate message counts
            errorCount = warningCount = commentCount = 0;
            foreach (Annotation annotation in _listedAnnotations)
            {
                if (annotation is Message)
                {
                    Message message = (Message)annotation;
                    if (message.Severity == MessageSeverity.Error)
                        ++errorCount;
                    else if (message.Severity == MessageSeverity.Warning)
                        ++warningCount;
                }
                else if (annotation is Comment)
                    ++commentCount;
            }
        }

        /// <summary>
        /// Log message counts, and optionally errors and warnings (or all messages if detail logging is on).
        /// </summary>
        public void LogMessageCounts(bool logMessages)
        {
            // Calculate and log message counts
            int errorCount, warningCount, commentCount;
            GetMessageCounts(out errorCount, out warningCount, out commentCount);
            Log.WriteLine(string.Format("{0}{1:N0} messages ({2:N0} errors; {3:N0} warnings; {4:N0} comments)",
                (!string.IsNullOrEmpty(Name) ? Name + ": " : ""), _listedAnnotations.Count, errorCount, warningCount, commentCount));

            // Log errors and warnings if requested
            if (logMessages)
            {
                foreach (Annotation annotation in _listedAnnotations)
                {
                    // Log all messages if the LogLevel is Detailed, log Warnings if Normal, and Errors if Minimal
                    Message message = annotation as Message;
                    if (Log.LogLevel >= Log.Level.Detailed || (message != null
                        && ((Log.LogLevel >= Log.Level.Normal && message.Severity == MessageSeverity.Warning)
                        || (Log.LogLevel >= Log.Level.Minimal && message.Severity == MessageSeverity.Error))))
                        Log.WriteLine(annotation is Message ? annotation.GetDescription() : annotation.ToString());
                }
            }
        }

        /// <summary>
        /// Get the name of the save file.
        /// </summary>
        public static string GetSaveFileName(string filePath)
        {
            if (SaveChangesToSeparateFile)
                return Path.GetDirectoryName(filePath) + @"\" + Path.GetFileNameWithoutExtension(filePath) + ".Nova" + Path.GetExtension(filePath);
            return filePath;
        }

        /// <summary>
        /// Save the <see cref="CodeUnit"/> to the specified file name.
        /// </summary>
        public void SaveAs(string fileName)
        {
            RemoveAllMessages(MessageSource.Save);
            try
            {
                // Save as text, suppressing the implied leading newline, and adding one at the end
                using (CodeWriter writer = new CodeWriter(fileName, FileEncoding, FileHasUTF8BOM, FileUsingTabs, IsGenerated))
                {
                    AsText(writer, RenderFlags.SuppressNewLine);
                    writer.WriteLine();
                }
                _isNew = false;
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "writing", MessageSource.Save);
            }
        }

        /// <summary>
        /// Save the <see cref="CodeUnit"/>.
        /// </summary>
        public void Save()
        {
            // Skip saving generated (".g.cs") files
            if (!IsGenerated)
                SaveAs(GetSaveFileName(_fileName));
        }

        /// <summary>
        /// Update the LineNumber and ColumnNumber properties of all child <see cref="CodeObject"/>s of the <see cref="CodeUnit"/>.
        /// </summary>
        public void UpdateAllLineColInfo()
        {
            // Use the length calculating method to avoid the overhead of building a big string
            AsTextLength(RenderFlags.UpdateLineCol);
        }

        /// <summary>
        /// Get the indent level of this object.
        /// </summary>
        public override int GetIndentLevel()
        {
            // A code unit is never indented
            return 0;
        }

        /// <summary>
        /// Returns true if the specified child object is indented from the parent.
        /// </summary>
        protected override bool IsChildIndented(CodeObject obj)
        {
            // Children of a code unit are never indented
            return false;
        }

        /// <summary>
        /// Determine if the specified compiler directive symbol exists.
        /// </summary>
        public bool IsCompilerDirectiveSymbolDefined(string name)
        {
            return (_compilerDirectiveSymbols.Contains(name) || name == "USING_NOVA" || name == "USING_NOVA_2");
        }

        /// <summary>
        /// Define the specified compiler directive symbol.
        /// </summary>
        public void DefineCompilerDirectiveSymbol(string name)
        {
            _compilerDirectiveSymbols.Add(name);
        }

        /// <summary>
        /// Undefine the specified compiler directive symbol.
        /// </summary>
        public void UndefineCompilerDirectiveSymbol(string name)
        {
            if (_compilerDirectiveSymbols.Contains(name))
                _compilerDirectiveSymbols.Remove(name);
        }

        protected override void NotifyListedAnnotationAdded(Annotation annotation)
        {
            _listedAnnotations.Add(annotation);
            if (_parent != null)
                Project.AnnotationAdded(annotation, this, annotation.Parent is CodeUnit);
        }

        protected override void NotifyListedAnnotationRemoved(Annotation annotation)
        {
            _listedAnnotations.Remove(annotation);
            if (_parent != null)
                Project.AnnotationRemoved(annotation);
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity, string toolTip)
        {
            string prefix = (severity == MessageSeverity.Error ? "ERROR: " : (severity == MessageSeverity.Warning ? "Warning: " : ""));
            Log.WriteLine(prefix + "File '" + _name + "': " + message, toolTip != null ? toolTip.TrimEnd() : null);
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public void LogMessage(string message, MessageSeverity severity)
        {
            LogMessage(message, severity, null);
        }

        /// <summary>
        /// Log the specified exception and message.
        /// </summary>
        public string LogException(Exception ex, string message)
        {
            return Log.Exception(ex, message + " file '" + _name + "'");
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
        /// Log the specified exception and message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachException(Exception ex, string message, MessageSource source)
        {
            message = LogException(ex, message);
            AttachMessage(message, MessageSeverity.Error, source);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
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
        /// Compare one <see cref="CodeUnit"/> to another.
        /// </summary>
        public int CompareTo(object obj2)
        {
            // Sort by directory first, with special logic so that parent directories
            // come after their children, then sort by file name within the directories.
            string obj2Path = ((CodeUnit)obj2).FileName;
            string directory1 = Path.GetDirectoryName(_fileName);
            string directory2 = Path.GetDirectoryName(obj2Path);
            int diff;
            if (directory1 == directory2)
                diff = 0;
            else if (directory1 == null || (directory2 != null && directory1.StartsWith(directory2)))
                diff = -1;
            else if (directory2 == null || directory2.StartsWith(directory1))
                diff = 1;
            else
                diff = directory1.CompareTo(directory2);
            if (diff == 0)
            {
                string fileName1 = Path.GetFileName(_fileName);
                string fileName2 = Path.GetFileName(obj2Path);
                if (fileName1 == null)
                {
                    if (fileName2 != null)
                        diff = -1;
                }
                else
                    diff = fileName1.CompareTo(fileName2);
            }
            return diff;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse the <see cref="CodeUnit"/> from its file.
        /// </summary>
        public void Parse(ParseFlags flags)
        {
            // Abort if it's not C#
            if (!IsCSharp) return;

            // Check that the file exists (to avoid an exception)
            if (IsFile && !File.Exists(_fileName))
            {
                // Only record the error if there isn't a similar message already
                if (Annotations == null || !Enumerable.Any(Annotations, delegate(Annotation annotation) { return annotation.Text.Contains("doesn't exist") || annotation.Text.Contains("is missing"); }))
                    LogAndAttachMessage("File '" + _fileName + "' doesn't exist!", MessageSeverity.Error, MessageSource.Parse);
                return;
            }

            // If we're loading a generated file, record it as such so we can disable saving it
            if (_fileName.EndsWith(Project.XamlCSharpGeneratedExtension) || _fileName.EndsWith(Project.DesignerCSharpGeneratedExtension))
                IsGenerated = true;

            // Get any compiler directive symbols from the project (copy them, because
            // they can be both defined and un-defined at the file level).
            _compilerDirectiveSymbols.Clear();
            if (_parent != null && Project.CurrentConfiguration != null)
                _compilerDirectiveSymbols = new HashSet<string>(Project.CurrentConfiguration.Constants);

            try
            {
                // Create a parser instance and parse the file
                using (Parser parser = new Parser(this, flags, IsGenerated))
                {
                    // Parse the body until we hit EOF, and add types to the namespace
                    new Block(out _body, parser, this, false, null);
                    _totalLines = parser.LineNumber;
                    _SLOC = parser.SLOC;
                    FileUsingTabs = (AutoDetectTabs ? parser.UsingMoreTabsThanSpaces() : UseTabs);
                }

                // Also check for other types of generated files, to disable saving them
                if (Body != null && Body.Count > 0)
                {
                    CodeObject firstCodeObject = Body[0];
                    if (firstCodeObject is CommentBase)
                        CheckIfGenerated((CommentBase)firstCodeObject);
                    else if (firstCodeObject.Annotations != null)
                    {
                        foreach (Annotation annotation in firstCodeObject.Annotations)
                        {
                            if (annotation is CommentBase)
                                CheckIfGenerated((CommentBase)annotation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing", MessageSource.Parse);
            }
            finally
            {
                // Make certain the body is set to IsFirstOnLine
                if (_body != null)
                    _body.IsFirstOnLine = true;
            }
        }

        /// <summary>
        /// Parse the <see cref="CodeUnit"/> from its file.
        /// </summary>
        public void Parse()
        {
            Parse(ParseFlags.None);
        }

        private void CheckIfGenerated(CommentBase commentBase)
        {
            string text = commentBase.Text;
            if (StringUtil.ContainsIgnoreCase(text, "<auto-generated>") || StringUtil.ContainsIgnoreCase(text, "do not edit"))
                IsGenerated = true;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references in the <see cref="CodeUnit"/>, handling any exceptions.
        /// </summary>
        public override void Resolve(ResolveFlags flags)
        {
            try
            {
                // Clear any existing error messages from a previous resolve pass
                if ((flags & (ResolveFlags.Phase2 | ResolveFlags.Phase3)) == 0)
                    RemoveAllMessages(MessageSource.Resolve);

                // Set the IsGenerated resolve flag if this is a generated file
                if (IsGenerated)
                    flags |= ResolveFlags.IsGenerated;

                // Don't list messages for resolve errors in workflow ".xoml.cs" files unless enabled
                if (_isWorkflowCodeBesideFile && !ListWorkflowFileErrors)
                    flags |= ResolveFlags.Quiet;

                // Do a 3-phase resolve if a specific phase wasn't specified by a higher level (the first phase stops at base lists
                // of type decls, the second at method/property bodies, and the third does the bodies - this resolves all base classes
                // and signatures first in order to resolve all references in a single attempt).
                if ((flags & (ResolveFlags.Phase1 | ResolveFlags.Phase2 | ResolveFlags.Phase3)) == 0)
                {
                    base.Resolve(ResolveCategory.CodeObject, flags | ResolveFlags.Phase1);
                    base.Resolve(ResolveCategory.CodeObject, flags | ResolveFlags.Phase2);
                    base.Resolve(ResolveCategory.CodeObject, flags | ResolveFlags.Phase3);
                }
                else
                    base.Resolve(ResolveCategory.CodeObject, flags);
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "resolving", MessageSource.Resolve);
            }
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            base.ResolveRefUp(name, resolver);
            if (resolver.HasCompleteMatch) return;  // Abort if we found a match

            // Check for root-level namespaces here, including matching on a root-level namespace if we're looking
            // for the namespace-or-type of an Alias statement and haven't found one yet.
            if (resolver.ResolveCategory == ResolveCategory.RootNamespace
                || (resolver.ResolveCategory == ResolveCategory.NamespaceOrType && resolver.UnresolvedRef.Parent is Alias))
            {
                if (_parent != null)
                {
                    // Search for root-level namespaces
                    if (name == ExternAlias.GlobalName)
                        resolver.AddMatch(Project.GlobalNamespace);
                    else
                    {
                        foreach (Reference reference in Project.References)
                        {
                            RootNamespace aliasNamespace = reference.AliasNamespace;
                            if (aliasNamespace != null && aliasNamespace.Name == name)
                                resolver.AddMatch(aliasNamespace);
                        }
                    }
                }
            }
            else if (resolver.ResolveCategory == ResolveCategory.NamespaceAlias)
            {
                // Check for the global extern alias (others are checked in the NamespaceDecl base class)
                if (name == ExternAlias.GlobalName)
                    resolver.AddMatch(GlobalAlias);
            }
            else
            {
                // For other categories, we might have a code fragment with a CodeUnit parent, so resolve like we
                // would for any other BlockStatement, except we want to ignore any Alias or UsingDirective objects
                // because they will have already been checked by 'base.ResolveRefUp()' at the top of this method.
                foreach (CodeObject codeObject in Find(name))
                {
                    if (!(codeObject is Alias || codeObject is UsingDirective))
                        resolver.AddMatch(codeObject);
                }
            }
        }

        protected override void ResolveNamespaces()
        {
            // A CodeUnit is actually a NamespaceDecl with an Expression that evaluates to the
            // global namespace, but we don't need to resolve it, because it's always a NamespaceRef.
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return 1; }
            set { throw new Exception("Can't set NewLines on a CodeUnit (it's always 1)."); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the body of the <see cref="BlockStatement"/> should be formatted with braces.
        /// </summary>
        public override bool ShouldHaveBraces()
        {
            return false;
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> requires an empty statement if it has an empty block with no braces.
        /// </summary>
        public override bool RequiresEmptyStatement
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return false; }
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// True if the <see cref="CodeObject"/> is renderable.
        /// </summary>
        public override bool IsRenderable
        {
            get
            {
                // Don't render if not C#, or if there are any Load or Parse errors (other than lost comments)
                return (IsCSharp && (_annotations == null || !Enumerable.Any(_annotations, delegate(Annotation annotation)
                    {
                        return annotation is Message && ((Message)annotation).Severity == MessageSeverity.Error
                               && ((Message)annotation).Source == MessageSource.Load || (((Message)annotation).Source == MessageSource.Parse && !annotation.Text.StartsWith("Line#"));
                    })));
            }
        }

        protected internal override void UpdateLineCol(CodeWriter writer, RenderFlags flags)
        { }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            base.AsText(writer, flags | RenderFlags.UpdateLineCol);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.Description))
                writer.Write(Name);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags | RenderFlags.SuppressNewLine);
            writer.Flush();  // Make sure everything is flushed
        }

        #endregion
    }
}
