// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Xml;

using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="AssemblyReference"/>, <see cref="ProjectReference"/>, and <see cref="COMReference"/>.
    /// </summary>
    public abstract class Reference : CodeObject, IComparable
    {
        #region /* FIELDS */

        /// <summary>
        /// The name of the reference.
        /// </summary>
        protected string _name;

        /// <summary>
        /// True if the reference is hidden in the UI.
        /// </summary>
        protected bool _isHidden;

        /// <summary>
        /// The alias for the reference, if any (null if none).
        /// </summary>
        protected string _alias;

        /// <summary>
        /// The namespace associated with any alias.
        /// </summary>
        protected RootNamespace _aliasNamespace;

        /// <summary>
        /// The loaded assembly associated with this reference.
        /// </summary>
        protected LoadedAssembly _loadedAssembly;

        /// <summary>
        /// The version requested when loaded (actual may differ).
        /// </summary>
        public string RequestedVersion;

        /// <summary>
        /// The location requested when loaded (actual may differ).
        /// </summary>
        public string RequestedLocation;

        #endregion

        #region /* CONSTRUCTORS */

        protected Reference(string name, bool isHidden)
        {
            _name = name;
            _isHidden = isHidden;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the reference.
        /// </summary>
        public string Name
        {
            // Unescape the name on a get because some symbols (such as parens) can be encoded by Visual Studio.
            // Always retain the original unmodifed string in the field, because escaping it will encode characters
            // that aren't normally encoded.
            get { return Uri.UnescapeDataString(_name); }
            set { _name = value; }
        }

        /// <summary>
        /// The short name of the reference.
        /// </summary>
        public string ShortName
        {
            get
            {
                if (_name != null)
                {
                    int delimiter = _name.IndexOf(',');
                    if (delimiter > 0)
                        return _name.Substring(0, delimiter);
                }
                return _name;
            }
        }

        /// <summary>
        /// True if the reference is hidden in the UI.
        /// </summary>
        public bool IsHidden
        {
            get { return _isHidden; }
            set { _isHidden = value; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public abstract string Category
        {
            get;
        }

        /// <summary>
        /// The alias for the referenced assembly, if any (null if none).
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;

                // Setup a namespace for the alias
                _aliasNamespace = (!string.IsNullOrEmpty(_alias) ? new RootNamespace(_alias, this) : null);
            }
        }

        /// <summary>
        /// The RootNamespace for the alias, if any (null if none).
        /// </summary>
        public RootNamespace AliasNamespace
        {
            get { return _aliasNamespace; }
        }

        /// <summary>
        /// The loaded assembly associated with this reference.
        /// </summary>
        public LoadedAssembly LoadedAssembly
        {
            get { return _loadedAssembly; }
        }

        /// <summary>
        /// The parent <see cref="Project"/>.
        /// </summary>
        public Project ParentProject
        {
            get { return _parent as Project; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Compare one reference to another.
        /// </summary>
        public int CompareTo(object obj2)
        {
            // Sort by name
            return (_name == null ? -1 : _name.CompareTo(((Reference)obj2).Name));
        }

        /// <summary>
        /// Log the specified text message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachMessage(string message, MessageSeverity severity, MessageSource source, string tooltip)
        {
            ParentProject.LogMessage("Referenced " + Category + " '" + Name + "': " + message, severity, tooltip);
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
        /// Log the specified exception and message and also attach it as an annotation.
        /// </summary>
        public void LogAndAttachException(Exception ex, string message, MessageSource source)
        {
            message = ParentProject.LogException(ex, message + " referenced " + Category + " '" + Name + "' in");
            AttachMessage(message, MessageSeverity.Error, source);
        }

        /// <summary>
        /// True if the project reference should be treated as an assembly reference.
        /// </summary>
        public virtual bool TreatAsAssemblyReference
        {
            get { return false; }
        }

        /// <summary>
        /// Load the referenced assembly.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Load an assembly.
        /// </summary>
        public string LoadAssembly(string assemblyName, string hintPath)
        {
            // Do nothing if errors occurred while resolving the reference or we don't have a parent project
            string errorMessage = null;
            RequestedVersion = null;
            RequestedLocation = null;
            Project parentProject = ParentProject;
            if (parentProject != null && (!HasErrors || TreatAsAssemblyReference))
            {
                try
                {
                    hintPath = StringUtil.ExpandEnvironmentMacros(hintPath);
                    _loadedAssembly = parentProject.LoadAssembly(assemblyName, hintPath, out errorMessage, this);
                }
                catch (Exception ex)
                {
                    errorMessage = "EXCEPTION loading referenced assembly '" + Name + "': " + ex.Message;
                }
            }
            return errorMessage;
        }

        /// <summary>
        /// Load an assembly.
        /// </summary>
        public string LoadAssembly(string assemblyName)
        {
            return LoadAssembly(assemblyName, null);
        }

        /// <summary>
        /// Load all accessible types from the referenced assembly.
        /// </summary>
        public virtual int LoadTypes()
        {
            return LoadTypes(null);
        }

        /// <summary>
        /// Load all accessible types from the referenced assembly, hiding the specified types.
        /// </summary>
        protected int LoadTypes(HashSet<string> hideTypes)
        {
            int typeCount = 0;
            Project parentProject = ParentProject;
            if (_loadedAssembly != null && parentProject != null)
            {
                try
                {
                    // Load all of the types in the referenced assembly
                    string errorMessage;
                    typeCount = parentProject.LoadTypes(_loadedAssembly, _aliasNamespace ?? parentProject.GlobalNamespace, out errorMessage, hideTypes);
                    if (errorMessage != null)
                        LogAndAttachMessage(errorMessage, MessageSeverity.Error, MessageSource.Load);
                }
                catch (Exception ex)
                {
                    LogAndAttachException(ex, "loading types from", MessageSource.Load);
                }
            }
            return typeCount;
        }

        /// <summary>
        /// Unload the reference (discard any <see cref="LoadedAssembly"/> object).
        /// </summary>
        public void Unload()
        {
            _loadedAssembly = null;
        }

        protected override void NotifyListedAnnotationAdded(Annotation annotation)
        {
            if (_parent is Project)
                ((Project)_parent).AnnotationAdded(annotation, null, true);
        }

        #endregion

        #region /* PARSING */

        protected Reference(Project project)
        {
            _parent = project;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve the reference.
        /// </summary>
        public override void Resolve(ResolveFlags flags)
        {
            // Nothing to resolve by default
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// True if the <see cref="CodeObject"/> is renderable.
        /// </summary>
        public override bool IsRenderable
        {
            get { return false; }
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_name);
        }

        /// <summary>
        /// Write to the specified <see cref="XmlWriter"/>.
        /// </summary>
        public abstract void AsText(XmlWriter xmlWriter);

        #endregion
    }
}
