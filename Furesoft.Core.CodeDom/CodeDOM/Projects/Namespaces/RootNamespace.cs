// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Projects.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Projects.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Projects;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;

namespace Furesoft.Core.CodeDom.CodeDOM.Projects.Namespaces
{
    /// <summary>
    /// Represents the top-level namespace (either a project's global namespace, or an extern alias namespace).
    /// </summary>
    /// <remarks>
    /// A <see cref="RootNamespace"/> maintains a dictionary of child namespaces indexed by full name for
    /// better performance when searching for namespaces.
    /// The Parent of a <see cref="Namespace"/> instance should only be another Namespace, but the Parent of a RootNamespace
    /// can be either a <see cref="Project"/> (global namespace) or a <see cref="Reference"/> (extern alias namespace).
    /// </remarks>
    public class RootNamespace : Namespace
    {
        #region /* FIELDS */

        /// <summary>
        /// All loaded namespaces under the root namespace, indexed by full name (the contents of namespaces
        /// may vary depending upon referenced assemblies, so each project must maintain its own namespaces).
        /// </summary>
        protected Dictionary<string, Namespace> _namespaces = new Dictionary<string, Namespace>();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="RootNamespace"/>.
        /// </summary>
        public RootNamespace(string name, CodeObject parent)
            : base(name, parent)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Determines if this <see cref="Namespace"/> is root-level (global or extern alias).
        /// </summary>
        public override bool IsRootLevel
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if this <see cref="Namespace"/> is the project-global namespace.
        /// </summary>
        public override bool IsGlobal
        {
            get { return (_name == ExternAlias.GlobalName); }
        }

        /// <summary>
        /// The parent Project if this RootNamespace is a project-global namespace (otherwise null).
        /// </summary>
        public Project ParentProject
        {
            get { return (_parent as Project); }
        }

        /// <summary>
        /// The parent <see cref="Reference"/> if this RootNamespace is an extern alias for an external reference (otherwise null).
        /// </summary>
        public Reference ParentReference
        {
            get { return (_parent as Reference); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Remove all items from the namespace.
        /// </summary>
        public override void RemoveAll()
        {
            lock (this)
            {
                base.RemoveAll();
                _namespaces.Clear();
            }
        }

        protected override void NamespaceCreated(Namespace @namespace)
        {
            // Maintain dictionary of all namespaces by full name
            lock (this)
                _namespaces.Add(@namespace.FullName, @namespace);
        }

        /// <summary>
        /// Find or create a child <see cref="Namespace"/>, including any missing parent namespaces.
        /// </summary>
        public override Namespace FindOrCreateChildNamespace(string namespaceName)
        {
            // If the namespace already exists, return it, otherwise delegate to the base
            // class to create it along with any parent namespaces.
            Namespace @namespace;
            lock (this)
            {
                if (!_namespaces.TryGetValue(namespaceName, out @namespace))
                    @namespace = base.FindOrCreateChildNamespace(namespaceName);
            }
            return @namespace;
        }

        /// <summary>
        /// Find the <see cref="Namespace"/> with the fully-specified name in the global namespace.
        /// </summary>
        public Namespace FindNamespace(string namespaceFullName)
        {
            Namespace @namespace;
            lock (this)
                _namespaces.TryGetValue(namespaceFullName, out @namespace);
            return @namespace;
        }

        /// <summary>
        /// Update the FullName (called when the Name or Parent is changed).
        /// </summary>
        protected override void UpdateFullName()
        {
            _fullName = _name;
        }

        /// <summary>
        /// Parse the specified name into a child <see cref="NamespaceRef"/> or <see cref="TypeRef"/> on the current root
        /// namespace, or a <see cref="Lookup"/> or <see cref="Dot"/> expression that evaluates to one.
        /// </summary>
        public override Expression ParseName(string name)
        {
            if (!IsGlobal)
            {
                string prefix = RemovePrefix(ref name);
                Expression expression = new Lookup(this, base.ParseName(prefix));
                return ParseName(expression, name);
            }
            return base.ParseName(name);
        }

        #endregion
    }
}
