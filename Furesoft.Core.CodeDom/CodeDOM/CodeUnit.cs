// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;

namespace Furesoft.Core.CodeDom.CodeDOM
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
    public class CodeUnit : BlockStatement, INamedCodeObject, IComparable
    {
        #region /* STATIC FIELDS */

        /// <summary>
        /// Determines if changes are saved to a separate ".Nova.cs" file instead of the original.
        /// </summary>
        public static bool SaveChangesToSeparateFile;

        /// <summary>
        /// The global namespace.
        /// </summary>
        protected static RootNamespace _globalNamespace = new RootNamespace(ExternAlias.GlobalName, null);  // Setup the 'global' namespace;

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
        /// Create a new <see cref="CodeUnit"/> with the specified file name (or text source).
        /// </summary>
        public CodeUnit(string fileName, string code)
            : base()
        {
            Name = Path.GetFileName(fileName);
            _isNew = true;
            _fileName = fileName;

            FileEncoding = Encoding.UTF8;  // Default to UTF8 encoding with a BOM
            FileHasUTF8BOM = true;
            _code = code;
        }

        /// <summary>
        /// Create a new <see cref="CodeUnit"/> with the specified file name.
        /// </summary>
        public CodeUnit(string fileName)
            : this(fileName, null)
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
        /// The global namespace.
        /// </summary>
        public static RootNamespace GlobalNamespace
        {
            get { return _globalNamespace; }
        }

        /// <summary>
        /// The name of the <see cref="CodeUnit"/>.  If associated with a file, this is the file name and extension.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _isWorkflowCodeBesideFile = _name.EndsWith(".xoml.cs");
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
        /// Load a <see cref="CodeUnit"/> directly from the specified source file.
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <param name="statusCallback">Status callback for monitoring progress.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
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

                if (statusCallback != null)
                    statusCallback(LoadStatus.ObjectCreated, null);

                // Create and parse the code unit, and log statistics
                codeUnit = new CodeUnit(fileName);
                codeUnit.ParseLog(loadOptions, statusCallback);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading file");
            }
            return codeUnit;
        }

        /// <summary>
        /// Load a <see cref="CodeUnit"/> directly from the specified source file.
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
        /// </remarks>
        public static CodeUnit Load(string fileName, LoadOptions loadOptions)
        {
            return Load(fileName, loadOptions, null);
        }

        /// <summary>
        /// Load a <see cref="CodeUnit"/> directly from the specified source file.
        /// </summary>
        /// <param name="fileName">The source (".cs") file.</param>
        /// <returns>The resulting CodeUnit object.</returns>
        /// <remarks>
        /// Loading a code unit directly goes through the following steps:
        ///   - Parse the code file.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
        /// </remarks>
        public static CodeUnit Load(string fileName)
        {
            return Load(fileName, LoadOptions.Complete, null);
        }

        /// <summary>
        /// Load a code unit directly from the specified source file.
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <param name="name">An optional name for the code fragment.</param>
        /// <param name="loadOptions">Determines various optional processing.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment, string name, LoadOptions loadOptions)
        {
            CodeUnit codeUnit = null;
            try
            {
                // Create and parse the code fragment, and log statistics
                codeUnit = new CodeUnit(name, codeFragment);
                codeUnit.ParseLog(loadOptions, null);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "loading fragment");
            }
            return codeUnit;
        }



        /// <summary>
        /// Load a code unit directly from the specified source file.
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <param name="name">An optional name for the code fragment.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment, string name)
        {
            return LoadFragment(codeFragment, name, LoadOptions.Complete);
        }

        /// <summary>
        /// Load a code unit directly from the specified source file.
        /// </summary>
        /// <param name="codeFragment">A fragment of code as a string.</param>
        /// <returns>The resulting <see cref="CodeUnit"/> object.</returns>
        /// <remarks>
        /// The code fragment is parsed.
        /// Note that any compiler directive symbols will be undefined.  To work around this issue, create
        /// a dummy parent project to hold one or more <see cref="CodeUnit"/>s before parsing.
        /// </remarks>
        public static CodeUnit LoadFragment(string codeFragment)
        {
            return LoadFragment(codeFragment, null, LoadOptions.Complete);
        }

        /// <summary>
        /// Parse the <see cref="CodeUnit"/> (or fragment), and log statistics if requested.
        /// </summary>
        public void ParseLog(LoadOptions loadOptions, Action<LoadStatus, CodeObject> statusCallback)
        {
            Stopwatch overallStopWatch = new Stopwatch();
            overallStopWatch.Start();
            GC.Collect();
            long startBytes = GC.GetTotalMemory(true);

            // Parse the code unit
            if (statusCallback != null)
                statusCallback(LoadStatus.Parsing, null);
            Unrecognized.Count = 0;
            Parse(loadOptions.HasFlag(LoadOptions.DoNotParseBodies) ? ParseFlags.SkipMethodBodies : ParseFlags.None);
            if (Unrecognized.Count > 0)
                Log.WriteLine("UNRECOGNIZED OBJECT COUNT: " + Unrecognized.Count);
            Log.WriteLine("Parsed " + (IsFile ? "file" : "fragment") + " '" + Name + "', total elapsed time: " + overallStopWatch.Elapsed.TotalSeconds.ToString("N3"));

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
                    if (Log.LogLevel >= Log.Level.Detailed|| (message != null
                        && ((Log.LogLevel >= Log.Level.Normal&& message.Severity == MessageSeverity.Warning)
                        || (Log.LogLevel >= Log.Level.Minimal&& message.Severity == MessageSeverity.Error))))
                        Log.WriteLine(annotation is Message ? annotation.GetDescription() : annotation.ToString());
                }
            }
        }

        /// <summary>
        /// Parse the specified name into a <see cref="NamespaceRef"/> or <see cref="TypeRef"/>, or a <see cref="Dot"/> or <see cref="Lookup"/> expression that evaluates to one.
        /// </summary>
        public Expression ParseName(string fullName)
        {
            return _globalNamespace.ParseName(fullName);
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
        }

        protected override void NotifyListedAnnotationRemoved(Annotation annotation)
        {
            _listedAnnotations.Remove(annotation);
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


            // Check that the file exists (to avoid an exception)
            if (IsFile && !File.Exists(_fileName))
            {
                // Only record the error if there isn't a similar message already
                if (Annotations == null || !Enumerable.Any(Annotations, delegate(Annotation annotation) { return annotation.Text.Contains("doesn't exist") || annotation.Text.Contains("is missing"); }))
                    LogAndAttachMessage("File '" + _fileName + "' doesn't exist!", MessageSeverity.Error, MessageSource.Parse);
                return;
            }

            // If we're loading a generated file, record it as such so we can disable saving it
            if (_fileName.EndsWith(".g.cs") || _fileName.EndsWith(".Designer.cs"))
                IsGenerated = true;

            _compilerDirectiveSymbols.Clear();

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
                return ((_annotations == null || !Enumerable.Any(_annotations, delegate(Annotation annotation)
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

    #region /* LOAD OPTIONS */

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
        /// or intercepted by <see cref="Log.SetLogWriteLineCallback"/>.  Regardless, <see cref="Message"/>s are always created and propagated up.
        /// </remarks>
        LogMessages = 0x08,

        /// <summary>
        /// Do not parse method bodies (useful if you only need types and member signatures, and not code in methods).
        /// </summary>
        DoNotParseBodies = 0x10,

        /// <summary>
        /// Perform complete processing.
        /// </summary>
        Complete = ParseSources
    }

    #endregion

    #region /* LOAD STATUS */

    /// <summary>
    /// Used for status callbacks during the load process to monitor progress and update any UI.
    /// </summary>
    public enum LoadStatus
    {
        /// <summary>Starting to load a <see cref="CodeUnit"/>.</summary>
        Loading,

        /// <summary>Created a new <see cref="CodeUnit"/> object.</summary>
        ObjectCreated,

        /// <summary>A listed <see cref="Annotation"/> (such as an error or warning <see cref="Message"/>) was added to an object.</summary>
        ObjectAnnotated,

        /// <summary>Starting parsing <see cref="CodeUnit"/>.</summary>
        Parsing,

        /// <summary>Starting logging of message counts, and messages (if requested).</summary>
        LoggingResults
    }

    #endregion
}
