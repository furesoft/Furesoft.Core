// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.IO;
using System.Xml;

using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a project reference to another project in the same solution.
    /// </summary>
    public class ProjectReference : Reference
    {
        #region /* FIELDS */

        protected string _projectFileName;
        protected Guid _guid;
        protected Guid? _package;

        protected Project _referencedProject;
        protected bool _treatAsAssemblyReference;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a new <see cref="ProjectReference"/> with the specified name, file name, and GUID.
        /// </summary>
        public ProjectReference(string name, string projectFileName, Guid projectGuid)
            : base(name, false)
        {
            _projectFileName = projectFileName;
            _guid = projectGuid;
        }

        /// <summary>
        /// Create a new <see cref="ProjectReference"/> with the specified name and file name.
        /// </summary>
        public ProjectReference(string name, string projectFileName)
            : this(name, projectFileName, new Guid())
        {
            _projectFileName = projectFileName;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The file name of the referenced <see cref="Project"/>.
        /// </summary>
        public string ProjectFileName
        {
            get { return _projectFileName; }
        }

        /// <summary>
        /// The unique identifier of the referenced <see cref="Project"/>.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
        }

        /// <summary>
        /// The associated package, if any.
        /// </summary>
        public Guid? Package
        {
            get { return _package; }
        }

        /// <summary>
        /// The referenced <see cref="Project"/>.
        /// </summary>
        public Project ReferencedProject
        {
            get { return _referencedProject; }
            set { _referencedProject = value; }
        }

        /// <summary>
        /// True if the project reference should be treated as an assembly reference.
        /// </summary>
        public override bool TreatAsAssemblyReference
        {
            get { return (_referencedProject != null && _treatAsAssemblyReference); }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "Project"; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Load the reference.
        /// </summary>
        public override void Load()
        {
            // If the project wasn't found in the solution, or the project type isn't supported (non-C# projects),
            // look for and load any last-built assembly if possible in order to resolve external type references.
            // Otherwise, we don't have to do anything - when symbols are being resolved, any referenced Projects
            // will be checked at that time.
            if (TreatAsAssemblyReference)
            {
                // Ideally, we'll at least parse the unsupported project enough to get the OutputPath and AssemblyName.
                // Otherwise, we have to make our best guess based solely upon the FileName.
                string outputPath = _referencedProject.GetFullOutputPath();
                string directory = Path.GetDirectoryName(_referencedProject.FileName);
                string assemblyPath = (!string.IsNullOrEmpty(outputPath) ? FileUtil.CombineAndNormalizePath(directory, outputPath)
                    : (directory + @"\Bin\" + (_parent != null ? ParentProject.ConfigurationName + @"\" : "")))
                    + (!string.IsNullOrEmpty(_referencedProject.AssemblyName) ? _referencedProject.AssemblyName
                    : Path.GetFileNameWithoutExtension(_referencedProject.FileName))
                    + (_referencedProject.OutputType == Project.OutputTypes.Library ? ".dll" : ".exe");

                string errorMessage = LoadAssembly(assemblyPath);
                if (errorMessage != null)
                    LogAndAttachMessage(errorMessage, MessageSeverity.Error, MessageSource.Load);
            }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse from the specified <see cref="XmlReader"/>.
        /// </summary>
        public ProjectReference(XmlReader xmlReader, Project project)
            : base(project)
        {
            try
            {
                if (xmlReader.MoveToAttribute("Include"))
                {
                    _projectFileName = xmlReader.Value;
                    xmlReader.MoveToElement();
                    if (!xmlReader.IsEmptyElement)
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.IsStartElement())
                            {
                                if (xmlReader.Name == "Project")
                                    _guid = Guid.Parse(xmlReader.ReadString());
                                else if (xmlReader.Name == "Name")
                                    _name = xmlReader.ReadString();
                                else if (xmlReader.Name == "Package")
                                    _package = Guid.Parse(xmlReader.ReadString());
                                else if (xmlReader.Name == "Aliases")
                                    Alias = xmlReader.ReadString();
                            }
                            if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "ProjectReference")
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndAttachException(ex, "parsing", MessageSource.Parse);
            }
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve the reference.
        /// </summary>
        public override void Resolve(ResolveFlags flags)
        {
            // Clear any existing error messages from a previous resolve pass
            RemoveAllMessages(MessageSource.Resolve);

            // Resolve the project reference
            if (_parent != null && ParentProject.Solution != null)
            {
                Solution solution = ParentProject.Solution;
                if (_name.StartsWith("{"))
                {
                    // If the project name is a string GUID, look up the project by GUID (this is used for website projects)
                    Guid projectGuid = Guid.Parse(_name);
                    _referencedProject = solution.FindProject(projectGuid);
                    if (_referencedProject != null)
                    {
                        // Change the project reference to a direct one instead of using the GUID
                        _name = _referencedProject.Name;
                        _projectFileName = FileUtil.MakeRelative(ParentProject.GetDirectory() + @"\", _referencedProject.FileName);
                    }
                }
                else if (_projectFileName != null)
                {
                    // Using the Name to search for a Project reference won't always work, because VS doesn't update reference names
                    // when projects are renamed.  So, resolve project references based upon the project path.  
                    string projectFullFileName = FileUtil.CombineAndNormalizePath(ParentProject.GetDirectory(), _projectFileName);
                    _referencedProject = solution.FindProjectByFileName(projectFullFileName);
                    if (_referencedProject == null)
                    {
                        string message;
                        MessageSeverity severity;
                        if (solution.IsNew && !ParentProject.IsNew)
                        {
                            message = "Referenced project '" + _name + "' treated as assembly reference since project was loaded directly";
                            severity = MessageSeverity.Information;
                        }
                        else
                        {
                            message = "Referenced project '" + _name + "' not found in current solution (using any assembly instead)";
                            severity = MessageSeverity.Warning;
                        }
                        ParentProject.LogMessage(message, severity);
                        AttachMessage(message, severity, MessageSource.Resolve);

                        // If a referenced project couldn't be found in the current solution, attempt to locate and parse the
                        // project file so that we can possibly load the last-built assembly in order to resolve references.
                        if (File.Exists(projectFullFileName))
                        {
                            _referencedProject = Project.Parse(projectFullFileName, ParentProject.Solution);
                            _treatAsAssemblyReference = true;
                        }
                    }
                }
                if (_referencedProject != null)
                {
                    // Treat unsupported projects as assembly references
                    if (_referencedProject.NotSupported)
                        _treatAsAssemblyReference = true;
                }
            }
            else
                AttachMessage("Can't resolve referenced project '" + _name + "' without a parent project and solution", MessageSeverity.Error, MessageSource.Resolve);
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// Write to the specified <see cref="XmlWriter"/>.
        /// </summary>
        public override void AsText(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("ProjectReference");
            xmlWriter.WriteAttributeString("Include", _projectFileName);
            xmlWriter.WriteElementString("Project", _guid.ToString("B").ToUpper());
            xmlWriter.WriteElementString("Name", _name);
            if (_package != null)
                xmlWriter.WriteElementString("Package", _package.Value.ToString("B").ToUpper());
            if (_alias != null)
                xmlWriter.WriteElementString("Aliases", _alias);
            xmlWriter.WriteEndElement();
        }

        #endregion
    }
}
