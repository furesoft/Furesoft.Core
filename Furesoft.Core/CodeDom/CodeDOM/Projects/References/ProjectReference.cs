// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Xml;

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
        public bool TreatAsAssemblyReference
        {
            get { return _treatAsAssemblyReference; }
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
