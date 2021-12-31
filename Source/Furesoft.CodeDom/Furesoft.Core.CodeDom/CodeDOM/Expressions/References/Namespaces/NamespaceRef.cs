// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces
{
    /// <summary>
    /// Represents a reference to a <see cref="Namespace"/>.
    /// </summary>
    public class NamespaceRef : SymbolicRef
    {
        /// <summary>
        /// Create a <see cref="NamespaceRef"/> from a <see cref="Namespace"/>.
        /// </summary>
        public NamespaceRef(Namespace @namespace, bool isFirstOnLine)
            : base(@namespace, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="NamespaceRef"/> from a <see cref="Namespace"/>.
        /// </summary>
        public NamespaceRef(Namespace @namespace)
            : base(@namespace, false)
        { }

        /// <summary>
        /// The children of the referenced <see cref="Namespace"/>.
        /// </summary>
        public NamespaceTypeDictionary Children
        {
            get { return ((Namespace)_reference).Children; }
        }

        /// <summary>
        /// The full name (including parent <see cref="Namespace"/>s) of the referenced <see cref="Namespace"/>.
        /// </summary>
        public string FullName
        {
            get { return ((Namespace)_reference).FullName; }
        }

        /// <summary>
        /// True if the referenced <see cref="Namespace"/> has <see cref="NamespaceDecl"/> declarations in the current
        /// project, otherwise false (meaning items in the namespace exist only in imported assemblies and projects).
        /// </summary>
        public bool HasDeclarationsInProject
        {
            get { return ((Namespace)_reference).HasDeclarationsInProject; }
        }

        /// <summary>
        /// Determines if the referenced <see cref="Namespace"/> is the project-global namespace.
        /// </summary>
        public bool IsGlobal
        {
            get { return ((Namespace)_reference).IsGlobal; }
        }

        /// <summary>
        /// Determines if the referenced <see cref="Namespace"/> is root-level (global or extern alias).
        /// </summary>
        public bool IsRootLevel
        {
            get { return ((Namespace)_reference).IsRootLevel; }
        }

        /// <summary>
        /// The referenced <see cref="Namespace"/>.
        /// </summary>
        public Namespace Namespace
        {
            get { return (Namespace)_reference; }
        }

        /// <summary>
        /// Find a child namespace in the specified <see cref="Namespace"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/> to the namespace, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(INamespace @namespace, string name, bool isFirstOnLine)
        {
            return Find(@namespace.Find(name), name, isFirstOnLine);
        }

        /// <summary>
        /// Find a child namespace in the specified <see cref="Namespace"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/> to the namespace, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(INamespace @namespace, string name)
        {
            return Find(@namespace.Find(name), name, false);
        }

        /// <summary>
        /// Find a child namespace in the specified <see cref="NamespaceRef"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/> to the namespace, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(SymbolicRef symbolicRef, string name, bool isFirstOnLine)
        {
            if (symbolicRef is NamespaceRef)
                Find(((NamespaceRef)symbolicRef).Namespace.Find(name), name, isFirstOnLine);
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a child namespace in the specified <see cref="NamespaceRef"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/> to the namespace, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(SymbolicRef symbolicRef, string name)
        {
            return Find(symbolicRef, name, false);
        }

        /// <summary>
        /// Add a child <see cref="Namespace"/> to the namespace.
        /// </summary>
        public void Add(Namespace @namespace)
        {
            ((Namespace)_reference).Add(@namespace);
        }

        /// <summary>
        /// Add a <see cref="TypeDecl"/> to the namespace.
        /// </summary>
        public void Add(TypeDecl typeDecl)
        {
            ((Namespace)_reference).Add(typeDecl);
        }

        /// <summary>
        /// Add a <see cref="Type"/> to the namespace.
        /// </summary>
        public void Add(Type type)
        {
            ((Namespace)_reference).Add(type);
        }

        /// <summary>
        /// Find a child <see cref="Namespace"/>, <see cref="TypeDecl"/>, or <see cref="Type"/> with
        /// the specified name.
        /// </summary>
        /// <returns>The child object if found, otherwise null.</returns>
        public object Find(string name)
        {
            return ((Namespace)_reference).Find(name);
        }

        /// <summary>
        /// Find or create a child namespace, including any missing parent namespaces.
        /// </summary>
        public Namespace FindOrCreateChildNamespace(string namespaceName)
        {
            return ((Namespace)_reference).FindOrCreateChildNamespace(namespaceName);
        }

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public override bool IsSameRef(SymbolicRef symbolicRef)
        {
            NamespaceRef namespaceRef = (symbolicRef is AliasRef ? ((AliasRef)symbolicRef).Namespace : symbolicRef as NamespaceRef);
            return (namespaceRef != null && Reference == namespaceRef.Reference);
        }

        /// <summary>
        /// Parse the specified name into a <see cref="NamespaceRef"/> or <see cref="TypeRef"/>, or a <see cref="Dot"/> expression that evaluates to one.
        /// </summary>
        public Expression ParseName(string name)
        {
            return ((Namespace)_reference).ParseName(name);
        }

        /// <summary>
        /// Remove a child <see cref="Namespace"/> from the namespace.
        /// </summary>
        public void Remove(Namespace @namespace)
        {
            ((Namespace)_reference).Remove(@namespace);
        }

        /// <summary>
        /// Remove a <see cref="TypeDecl"/> from the namespace.
        /// </summary>
        public void Remove(TypeDecl typeDecl)
        {
            ((Namespace)_reference).Remove(typeDecl);
        }

        /// <summary>
        /// Remove a <see cref="Type"/> from the namespace.
        /// </summary>
        public void Remove(Type type)
        {
            ((Namespace)_reference).Remove(type);
        }

        /// <summary>
        /// Remove all items from the namespace.
        /// </summary>
        public virtual void RemoveAll()
        {
            ((Namespace)_reference).RemoveAll();
        }

        /// <summary>
        /// Format the <see cref="NamespaceRef"/> as a string.
        /// </summary>
        public override string ToString()
        {
            // Use full name in debugger
            return GetType().Name + ": " + Namespace.FullName;
        }

        private static SymbolicRef Find(object obj, string name, bool isFirstOnLine)
        {
            if (obj is Namespace)
                return new NamespaceRef((Namespace)obj, isFirstOnLine);
            return new UnresolvedRef(name, isFirstOnLine);
        }
    }
}
