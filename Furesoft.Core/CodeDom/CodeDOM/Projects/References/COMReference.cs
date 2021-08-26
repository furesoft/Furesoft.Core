// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Xml;

using Furesoft.Core.CodeDom.Utilities;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a project reference to a COM object.
    /// </summary>
    public class COMReference : Reference
    {
        protected bool? _embedInteropTypes;
        protected Guid _guid;
        protected bool? _isolated;
        protected int? _lcid;
        protected string _versionMajor;
        protected string _versionMinor;
        protected string _wrapperTool;

        /// <summary>
        /// Create a new <see cref="COMReference"/> with the specified name and other parameters.
        /// </summary>
        public COMReference(string name, Guid guid, string versionMajor, string versionMinor)
            : base(name, false)
        {
            _guid = guid;
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "COM Object"; }
        }

        /// <summary>
        /// True if interop types should be embedded, null if not used.
        /// </summary>
        public bool? EmbedInteropTypes
        {
            get { return _embedInteropTypes; }
            set { _embedInteropTypes = value; }
        }

        /// <summary>
        /// The unique identifier of the referenced COM object.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
        }

        /// <summary>
        /// True if isolated, null if not used.
        /// </summary>
        public bool? Isolated
        {
            get { return _isolated; }
            set { _isolated = value; }
        }

        /// <summary>
        /// The LC ID number, or null if not used.
        /// </summary>
        public int? Lcid
        {
            get { return _lcid; }
            set { _lcid = value; }
        }

        /// <summary>
        /// The major version numbr of the referenced COM object.
        /// </summary>
        public string VersionMajor
        {
            get { return _versionMajor; }
        }

        /// <summary>
        /// The minor version numbr of the referenced COM object.
        /// </summary>
        public string VersionMinor
        {
            get { return _versionMinor; }
        }

        /// <summary>
        /// The name of an associated wrapper tool.
        /// </summary>
        public string WrapperTool
        {
            get { return _wrapperTool; }
            set { _wrapperTool = value; }
        }

        /// <summary>
        /// Parse from the specified <see cref="XmlReader"/>.
        /// </summary>
        public COMReference(XmlReader xmlReader, Project project)
            : base(project)
        {
            try
            {
                if (xmlReader.MoveToAttribute("Include"))
                {
                    _name = xmlReader.Value;
                    xmlReader.MoveToElement();
                    if (!xmlReader.IsEmptyElement)
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.IsStartElement())
                            {
                                if (xmlReader.Name == "Guid")
                                    _guid = Guid.Parse(xmlReader.ReadString());
                                else if (xmlReader.Name == "VersionMajor")
                                    _versionMajor = xmlReader.ReadString();
                                else if (xmlReader.Name == "VersionMinor")
                                    _versionMinor = xmlReader.ReadString();
                                else if (xmlReader.Name == "Lcid")
                                    _lcid = StringUtil.ParseInt(xmlReader.ReadString());
                                else if (xmlReader.Name == "WrapperTool")
                                    _wrapperTool = StringUtil.EmptyAsNull(xmlReader.ReadString());
                                else if (xmlReader.Name == "Isolated")
                                    _isolated = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "EmbedInteropTypes")
                                    _embedInteropTypes = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "Aliases")
                                    Alias = xmlReader.ReadString();
                            }
                            if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "COMReference")
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

        /// <summary>
        /// Write to the specified <see cref="XmlWriter"/>.
        /// </summary>
        public override void AsText(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("COMReference");
            xmlWriter.WriteAttributeString("Include", _name);
            xmlWriter.WriteElementString("Guid", _guid.ToString("B").ToUpper());
            if (_versionMajor != null)
                xmlWriter.WriteElementString("VersionMajor", _versionMajor);
            if (_versionMinor != null)
                xmlWriter.WriteElementString("VersionMinor", _versionMinor);
            if (_lcid != null)
                xmlWriter.WriteElementString("Lcid", _lcid.GetValueOrDefault().ToString());
            if (_wrapperTool != null)
                xmlWriter.WriteElementString("WrapperTool", _wrapperTool);
            if (_isolated != null)
                xmlWriter.WriteElementString("Isolated", _isolated.GetValueOrDefault().ToString());
            if (_embedInteropTypes != null)
                xmlWriter.WriteElementString("EmbedInteropTypes", _embedInteropTypes.GetValueOrDefault().ToString());
            if (_alias != null)
                xmlWriter.WriteElementString("Aliases", _alias);
            xmlWriter.WriteEndElement();
        }
    }
}