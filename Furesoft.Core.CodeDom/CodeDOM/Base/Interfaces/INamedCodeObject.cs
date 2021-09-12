// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Nova.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all <see cref="CodeObject"/>s that have a Name property, and can be referenced symbolically.
    /// </summary>
    /// <remarks>
    /// Implementers include:
    ///    <see cref="ITypeDecl"/> implementers (<see cref="TypeDecl"/> and subclasses [<see cref="ClassDecl"/>, <see cref="StructDecl"/>, <see cref="InterfaceDecl"/>, <see cref="EnumDecl"/>], <see cref="TypeParameter"/>, <see cref="Alias"/>),
    ///    <see cref="IVariableDecl"/> implementers (<see cref="VariableDecl"/> and subclasses [<see cref="ParameterDecl"/>, <see cref="LocalDecl"/>, <see cref="FieldDecl"/>, <see cref="EnumMemberDecl"/>],
    ///        <see cref="PropertyDeclBase"/> and subclasses [<see cref="PropertyDecl"/>, <see cref="IndexerDecl"/>, <see cref="EventDecl"/>]),
    ///    <see cref="MethodDeclBase"/> and subclasses (<see cref="MethodDecl"/>, <see cref="GenericMethodDecl"/>, <see cref="ConstructorDecl"/>, <see cref="DestructorDecl"/>)
    ///    <see cref="SwitchItem"/>, <see cref="Label"/>, <see cref="Solution"/>, <see cref="Project"/>, <see cref="CodeUnit"/>, <see cref="Namespace"/>, <see cref="ExternAlias"/>
    /// </remarks>
    public interface INamedCodeObject
    {
        /// <summary>
        /// The name of the <see cref="CodeObject"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The descriptive category of the <see cref="CodeObject"/>.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Create a reference to the <see cref="CodeObject"/>.
        /// </summary>
        SymbolicRef CreateRef(bool isFirstOnLine);

        /// <summary>
        /// Create a reference to the <see cref="CodeObject"/>.
        /// </summary>
        SymbolicRef CreateRef();

        /// <summary>
        /// Find the parent object of the specified type.
        /// </summary>
        T FindParent<T>() where T : CodeObject;

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        void AddToDictionary(NamedCodeObjectDictionary dictionary);

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        void RemoveFromDictionary(NamedCodeObjectDictionary dictionary);

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        string GetFullName(bool descriptive);

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        string GetFullName();
    }
}
