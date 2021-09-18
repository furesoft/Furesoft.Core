// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

namespace Nova.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that represent namespace declarations
    /// (<see cref="Namespace"/> and <see cref="Alias"/>).
    /// </summary>
    public interface INamespace : INamedCodeObject
    {
        /// <summary>
        /// The dictionary of child types and namespaces in the <see cref="Namespace"/>.
        /// </summary>
        NamespaceTypeDictionary Children { get; }

        /// <summary>
        /// The full name of the <see cref="Namespace"/>, including any parent namespaces.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// True if this <see cref="Namespace"/> has <see cref="NamespaceDecl"/> declarations in the current project, otherwise
        /// false (meaning items in the namespace exist only in imported assemblies and projects).
        /// </summary>
        bool HasDeclarationsInProject { get; }

        /// <summary>
        /// Determines if this <see cref="Namespace"/> is the project-global namespace.
        /// </summary>
        bool IsGlobal { get; }

        /// <summary>
        /// Determines if this <see cref="Namespace"/> is root-level (global or extern alias).
        /// </summary>
        bool IsRootLevel { get; }

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        CodeObject Parent { get; }

        /// <summary>
        /// Add a child <see cref="Namespace"/> to the <see cref="Namespace"/>.
        /// </summary>
        void Add(Namespace @namespace);

        /// <summary>
        /// Add a <see cref="TypeDecl"/> to the <see cref="Namespace"/>.
        /// </summary>
        void Add(TypeDecl typeDecl);

        /// <summary>
        /// Add a <see cref="Type"/> to the <see cref="Namespace"/>.
        /// </summary>
        void Add(Type type);

        /// <summary>
        /// Find a child <see cref="Namespace"/>, <see cref="Type"/>, or <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        object Find(string name);

        /// <summary>
        /// Find or create a child <see cref="Namespace"/>, including any missing parent namespaces.
        /// </summary>
        Namespace FindOrCreateChildNamespace(string namespaceName);

        /// <summary>
        /// Parse the specified name into a child <see cref="NamespaceRef"/> or <see cref="TypeRef"/> on the current namespace,
        /// or a <see cref="Dot"/> expression that evaluates to one.
        /// </summary>
        Expression ParseName(string name);

        /// <summary>
        /// Remove a child <see cref="Namespace"/> from the <see cref="Namespace"/>.
        /// </summary>
        void Remove(Namespace @namespace);

        /// <summary>
        /// Remove a <see cref="TypeDecl"/> from the <see cref="Namespace"/>.
        /// </summary>
        void Remove(TypeDecl typeDecl);

        /// <summary>
        /// Remove a <see cref="Type"/> from the <see cref="Namespace"/>.
        /// </summary>
        void Remove(Type type);

        /// <summary>
        /// Remove all items from the <see cref="Namespace"/>.
        /// </summary>
        void RemoveAll();
    }
}