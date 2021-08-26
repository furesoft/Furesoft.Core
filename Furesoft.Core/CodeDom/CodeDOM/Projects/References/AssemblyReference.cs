// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Xml;

using Furesoft.Core.CodeDom.Utilities;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a project reference to an external assembly.
    /// </summary>
    public class AssemblyReference : Reference
    {
        /// <summary>
        /// Types to be hidden from .NET 4.5 mscorlib 4.0 for older versions.
        /// </summary>
        protected static readonly HashSet<string> HideMscorlib45Types = new HashSet<string>
            {
                // Filter types that moved from System.Core 3.5 to mscorlib 4.0 for .NET 4.5 (using NEW mscorlib 4.0!)
                "System.Runtime.CompilerServices.ExtensionAttribute",
                // Filter types that are new in mscorlib 4.0 for .NET 4.5 (using NEW mscorlib 4.0!)
                "System.Progress`1", "System.IProgress`1"
            };

        /// <summary>
        /// Types to be hidden from .NET mscorlib 4.0 for older versions.
        /// </summary>
        protected static readonly HashSet<string> HideMscorlib4Types = new HashSet<string>
            {
                // The Action delegates with 0, 2, 3, 4 type parameters were moved from System.Core 3.5 to mscorlib for 4.0
                "System.Action", "System.Action`2", "System.Action`3", "System.Action`4",
                // The Func delegates with 1 to 5 type parameters were moved from System.Core 3.5 to mscorlib for 4.0
                "System.Func`1", "System.Func`2", "System.Func`3", "System.Func`4", "System.Func`5",
                // Other types moved from System.Core 3.5 to mscorlib 4.0
                "System.TimeZoneInfo",
                // Hide various types that are new in mscorlib 4.0
                "System.Tuple",

                // Filter types that moved from System.Core 3.5 to mscorlib 4.0 for .NET 4.5 (using NEW mscorlib 4.0!)
                "System.Runtime.CompilerServices.ExtensionAttribute",
                // Filter types that are new in mscorlib 4.0 for .NET 4.5 (using NEW mscorlib 4.0!)
                "System.Progress`1", "System.IProgress`1"
            };

        /// <summary>
        /// A dictionary of all framework assembly names across all platforms and versions.
        /// </summary>
        protected static HashSet<string> AllFrameworkAssemblies = new HashSet<string>
            {
                "agcore",
                "accessibility",
                "coreclr",
                "custommarshalers",
                "isymwrapper",
                "microsoft.build",
                "microsoft.build.conversion.v4.0",
                "microsoft.build.engine",
                "microsoft.build.framework",
                "microsoft.build.framework.v3.5",
                "microsoft.build.tasks",
                "microsoft.build.tasks.v3.5",
                "microsoft.build.tasks.v4.0",
                "microsoft.build.utilities",
                "microsoft.build.utilities.v4.0",
                "microsoft.build.visualjsharp",
                "microsoft.compactframework.build.tasks",
                "microsoft.csharp",
                "microsoft.data.entity.build.tasks",
                "microsoft.internal.tasks.dataflow",
                "microsoft.jscript",
                "microsoft.transactions.bridge",
                "microsoft.transactions.bridge.dtc",
                "microsoft.visualbasic.activities.compiler",
                "microsoft.visualbasic.compatibility.data",
                "microsoft.visualbasic.compatibility",
                "microsoft.visualbasic",
                "microsoft.visualbasic.vsa",
                "microsoft.visualc",
                "microsoft.visualc.stlclr",
                "microsoft.vsa",
                "microsoft.windows.applicationserver.applications",
                "mscorlib",
                "mscorrc",
                "npctrl",
                "npctrlui",
                "presentationbuildtasks",
                "presentationcore",
                "presentationframework.aero",
                "presentationframework.classic",
                "presentationframework",
                "presentationframework.luna",
                "presentationframework.royale",
                "presentationui",
                "reachframework",
                "silverlight.configurationui",
                "sysglobl",
                "system.activities.core.presentation",
                "system.activities",
                "system.activities.durableinstancing",
                "system.activities.presentation",
                "system.addin.contract",
                "system.addin",
                "system.componentmodel.composition",
                "system.componentmodel.dataannotations",
                "system.configuration",
                "system.configuration.install",
                "system.core",
                "system.data.datasetextensions",
                "system.data",
                "system.data.entity.design",
                "system.data.entity",
                "system.data.linq",
                "system.data.oracleclient",
                "system.data.services.client",
                "system.data.services.design",
                "system.data.services",
                "system.data.sqlxml",
                "system.deployment",
                "system.design",
                "system.device",
                "system.directoryservices.accountmanagement",
                "system.directoryservices",
                "system.directoryservices.protocols",
                "system",
                "system.drawing.design",
                "system.drawing",
                "system.dynamic",
                "system.enterpriseservices",
                "system.identitymodel",
                "system.identitymodel.selectors",
                "system.io.log",
                "system.management",
                "system.management.instrumentation",
                "system.messaging",
                "system.net",
                "system.numerics",
                "system.printing",
                "system.runtime.caching",
                "system.runtime.durableinstancing",
                "system.runtime.remoting",
                "system.runtime.serialization",
                "system.runtime.serialization.formatters.soap",
                "system.security",
                "system.servicemodel.activation",
                "system.servicemodel.activities",
                "system.servicemodel.channels",
                "system.servicemodel.discovery",
                "system.servicemodel",
                "system.servicemodel.routing",
                "system.servicemodel.web",
                "system.serviceprocess",
                "system.speech",
                "system.transactions",
                "system.web.abstractions",
                "system.web.applicationservices",
                "system.web.datavisualization.design",
                "system.web.datavisualization",
                "system.web",
                "system.web.dynamicdata.design",
                "system.web.dynamicdata",
                "system.web.entity.design",
                "system.web.entity",
                "system.web.extensions.design",
                "system.web.extensions",
                "system.web.mobile",
                "system.web.regularexpressions",
                "system.web.routing",
                "system.web.services",
                "system.windows",
                "system.windows.browser",
                "system.windows.forms.datavisualization.design",
                "system.windows.forms.datavisualization",
                "system.windows.forms",
                "system.windows.input.manipulations",
                "system.windows.presentation",
                "system.workflow.activities",
                "system.workflow.componentmodel",
                "system.workflow.runtime",
                "system.workflowservices",
                "system.xaml",
                "system.xml",
                //"system.xml.linq", // For Silverlight, this is an SDK assembly, not a framework assembly
                "uiautomationclient",
                "uiautomationclientsideproviders",
                "uiautomationprovider",
                "uiautomationtypes",
                "vjscor",
                "vjsharpcodeprovider",
                "vjslib",
                "vjslibcw",
                "vjssupuilib",
                "windowsbase",
                "windowsformsintegration",
                "xamlbuildtask"
            };

        /// <summary>
        /// Types to be hidden from Silverlight mscorlib 4.0 for older versions.
        /// </summary>
        protected static HashSet<string> HideSilverlightMscorlib4Types;

        protected bool? _embedInteropTypes;
        protected string _hintPath;
        protected bool _isSpecificVersion;
        protected bool? _private;
        protected string _requiredTargetFrameworkVersion;

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and other parameters.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The targeted framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        /// <param name="specificVersion">True if the specific version specified in the display name should be used.</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path to be found, which
        /// can be either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path.
        /// </summary>
        public AssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden, string hintPath, bool specificVersion)
            : base(name, isHidden)
        {
            _requiredTargetFrameworkVersion = requiredTargetFrameworkVersion;
            _isSpecificVersion = specificVersion;
            _hintPath = hintPath;
            Alias = alias;
        }

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and other parameters.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The targeted framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path to be found, which
        /// can be either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path.
        /// </summary>
        public AssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden, string hintPath)
            : this(name, alias, requiredTargetFrameworkVersion, isHidden, hintPath, false)
        { }

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and other parameters.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The targeted framework version for the assembly (only used for framework assemblies).</param>
        /// <param name="isHidden">True if the assembly reference should be hidden in the UI.</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path to be found, which
        /// can be either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path.
        /// </summary>
        public AssemblyReference(string name, string alias, string requiredTargetFrameworkVersion, bool isHidden)
            : this(name, alias, requiredTargetFrameworkVersion, isHidden, null, false)
        { }

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and other parameters.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="alias">The alias for the referenced assembly, if any.</param>
        /// <param name="requiredTargetFrameworkVersion">The targeted framework version for the assembly (only used for framework assemblies).</param>
        /// <summary>
        /// The optional parameters basically mirror settings used in the '.csproj' file.  The name is normally the file name of the
        /// assembly without the extension (short name), or can be a "display name" which includes a version number and other optional
        /// values.  If the assembly is not in the project's output directory or the GAC, it will require a hint-path to be found, which
        /// can be either relative or absolute, and should include the file name and extension to be compatible with the standard '.csproj'
        /// files.  You can also use a path on the name itself, but standard '.csproj' files use the hint-path.
        /// </summary>
        public AssemblyReference(string name, string alias, string requiredTargetFrameworkVersion)
            : this(name, alias, requiredTargetFrameworkVersion, false, null, false)
        { }

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and optional hintpath.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        /// <param name="hintPath">The full path, including the file name, of where the assembly is expected to be (not required if the assembly is in the GAC).</param>
        public AssemblyReference(string name, string hintPath)
            : this(name, null, null, false, hintPath)
        { }

        /// <summary>
        /// Create a new <see cref="AssemblyReference"/> with the specified name and optional hintpath.
        /// </summary>
        /// <param name="name">The short name or display name of the assembly.</param>
        public AssemblyReference(string name)
            : this(name, null, null, false, null)
        { }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "Assembly"; }
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
        /// The hint path for the referenced assembly, if any (null if none).
        /// </summary>
        public string HintPath
        {
            get { return _hintPath; }
            set { _hintPath = value; }
        }

        /// <summary>
        /// True if the specified version for the referenced assembly is the only one allowed.
        /// </summary>
        public bool IsSpecificVersion
        {
            get { return _isSpecificVersion; }
            set { _isSpecificVersion = value; }
        }

        /// <summary>
        /// True if the reference is private, null if not used.
        /// </summary>
        public bool? Private
        {
            get { return _private; }
            set { _private = value; }
        }

        /// <summary>
        /// The required target framework for the referenced assembly, if any (null if none).
        /// </summary>
        public string RequiredTargetFramework
        {
            get { return _requiredTargetFrameworkVersion; }
        }

        /// <summary>
        /// Parse from the specified <see cref="XmlReader"/>.
        /// </summary>
        public AssemblyReference(XmlReader xmlReader, Project project)
            : base(project)
        {
            try
            {
                if (xmlReader.MoveToAttribute("Include"))
                {
                    _name = xmlReader.Value;

                    // Default the specific version to true if the name has a version, otherwise false
                    _isSpecificVersion = AssemblyUtil.HasVersion(_name);

                    xmlReader.MoveToElement();
                    if (!xmlReader.IsEmptyElement)
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.IsStartElement())
                            {
                                if (xmlReader.Name == "SpecificVersion")
                                    _isSpecificVersion = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "RequiredTargetFramework")
                                    _requiredTargetFrameworkVersion = xmlReader.ReadString();
                                else if (xmlReader.Name == "HintPath")
                                    _hintPath = xmlReader.ReadString();
                                else if (xmlReader.Name == "Private")
                                    _private = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "EmbedInteropTypes")
                                    _embedInteropTypes = StringUtil.ParseBool(xmlReader.ReadString());
                                else if (xmlReader.Name == "Aliases")
                                    Alias = xmlReader.ReadString();
                            }
                            if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "Reference")
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
            xmlWriter.WriteStartElement("Reference");
            xmlWriter.WriteAttributeString("Include", _name);
            if (AssemblyUtil.IsDisplayName(_name) && !_isSpecificVersion)
                xmlWriter.WriteElementString("SpecificVersion", _isSpecificVersion.ToString());
            if (_requiredTargetFrameworkVersion != null)
                xmlWriter.WriteElementString("RequiredTargetFramework", _requiredTargetFrameworkVersion);
            if (_hintPath != null)
                xmlWriter.WriteElementString("HintPath", _hintPath);
            if (_private.HasValue)
                xmlWriter.WriteElementString("Private", _private.ToString());
            if (_embedInteropTypes.HasValue)
                xmlWriter.WriteElementString("EmbedInteropTypes", _embedInteropTypes.ToString());
            if (_alias != null)
                xmlWriter.WriteElementString("Aliases", _alias);
            xmlWriter.WriteEndElement();
        }
    }
}