// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous
{
    /// <summary>
    /// Represents an alias to a <see cref="Namespace"/> or a type (<see cref="TypeDecl"/>, or <see cref="Type"/>).
    /// </summary>
    /// <remarks>
    /// An alias can only be defined at the <see cref="CodeUnit"/> or <see cref="NamespaceDecl"/> level, and it can only
    /// be used within that scope.  The syntax is "using aliasname = expression;" where the expression evaluates to either
    /// a <see cref="NamespaceRef"/> or a <see cref="TypeRef"/>.
    /// </remarks>
    public class Alias : Statement, INamedCodeObject, ITypeDecl, INamespace
    {
        /// <summary>
        /// The expression should be a <see cref="NamespaceRef"/>, <see cref="TypeRef"/>, or a <see cref="Dot"/> operator
        /// whose right-most operand evaluates to a <see cref="NamespaceRef"/> or <see cref="TypeRef"/>.  It can also be
        /// an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected Expression _expression;

        protected string _name;

        /// <summary>
        /// Create an <see cref="Alias"/> with the specified name to the specified <see cref="Expression"/>.
        /// </summary>
        public Alias(string name, Expression expression)
        {
            _name = name;
            Expression = expression;
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get
            {
                Expression expression = _expression.SkipPrefixes();
                string category;
                if (expression is NamespaceRef)
                    category = "namespace";
                else if (expression is TypeRef)
                    category = "type";
                else
                    category = "unknown";
                return category + " alias";
            }
        }

        /// <summary>
        /// The aliased <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// True if this is a namespace alias.
        /// </summary>
        public bool IsNamespace
        {
            get { return _expression.SkipPrefixes() is NamespaceRef; }
        }

        /// <summary>
        /// True if this is a type alias.
        /// </summary>
        public bool IsType
        {
            get { return _expression.SkipPrefixes() is TypeRef; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The name of the <see cref="Alias"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The namespace that this alias refers to (null if not a namespace alias).
        /// </summary>
        public NamespaceRef Namespace
        {
            get { return _expression.SkipPrefixes() as NamespaceRef; }
        }

        /// <summary>
        /// The type that this alias refers to (null if not a type alias).
        /// </summary>
        public TypeRef Type
        {
            get { return _expression.SkipPrefixes() as TypeRef; }
        }

        /// <summary>
        /// True if the aliased type is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsAbstract);
            }
        }

        /// <summary>
        /// True if the aliased type is a class.
        /// </summary>
        public bool IsClass
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsClass);
            }
        }

        /// <summary>
        /// True if the aliased type is a delegate type.
        /// </summary>
        public bool IsDelegateType
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsDelegateType);
            }
        }

        /// <summary>
        /// True if the aliased type is an enum.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsEnum);
            }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsGenericParameter
        {
            get { return false; }
        }

        /// <summary>
        /// True if the aliased type is a generic type.
        /// </summary>
        public bool IsGenericType
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsGenericType);
            }
        }

        /// <summary>
        /// True if the aliased type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsInterface);
            }
        }

        /// <summary>
        /// True if the aliased type is a nested type.
        /// </summary>
        public bool IsNested
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsNested);
            }
        }

        /// <summary>
        /// True if the aliased type is a nullable type.
        /// </summary>
        public bool IsNullableType
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsNullableType);
            }
        }

        /// <summary>
        /// True if the aliased type is a partial type.
        /// </summary>
        public bool IsPartial
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsPartial);
            }
        }

        /// <summary>
        /// True if the aliased type is a struct.
        /// </summary>
        public bool IsStruct
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsUserStruct);
            }
        }

        /// <summary>
        /// True if the aliased type is a value type.
        /// </summary>
        public bool IsValueType
        {
            get
            {
                TypeRef typeRef = Type;
                return (typeRef != null && typeRef.IsValueType);
            }
        }

        /// <summary>
        /// Get the number of <see cref="TypeParameter"/>s in the aliased type (if any).
        /// </summary>
        public int TypeParameterCount
        {
            get
            {
                TypeRef typeRefBase = Type;
                return (typeRefBase != null ? typeRefBase.TypeArgumentCount : 0);
            }
        }

        /// <summary>
        /// The dictionary of child objects in the referenced <see cref="Namespace"/>.
        /// </summary>
        public NamespaceTypeDictionary Children
        {
            get
            {
                NamespaceRef namespaceRef = Namespace;
                return (namespaceRef != null ? namespaceRef.Children : null);
            }
        }

        /// <summary>
        /// The full name of the referenced <see cref="Namespace"/>, including any parent namespaces.
        /// </summary>
        public string FullName
        {
            get
            {
                NamespaceRef namespaceRef = Namespace;
                return (namespaceRef != null ? namespaceRef.FullName : null);
            }
        }

        /// <summary>
        /// True if the referenced <see cref="Namespace"/> has <see cref="NamespaceDecl"/> declarations in the current
        /// project, otherwise false (meaning items in the namespace exist only in imported assemblies and projects).
        /// </summary>
        public bool HasDeclarationsInProject
        {
            get
            {
                NamespaceRef namespaceRef = Namespace;
                return (namespaceRef != null && namespaceRef.HasDeclarationsInProject);
            }
        }

        /// <summary>
        /// Determines if the referenced <see cref="Namespace"/> is the project-global namespace.
        /// </summary>
        public bool IsGlobal
        {
            get
            {
                NamespaceRef namespaceRef = Namespace;
                return (namespaceRef != null && namespaceRef.IsGlobal);
            }
        }

        /// <summary>
        /// Determines if the referenced <see cref="Namespace"/> is root-level (global or extern alias).
        /// </summary>
        public bool IsRootLevel
        {
            get
            {
                NamespaceRef namespaceRef = Namespace;
                return (namespaceRef != null && namespaceRef.IsRootLevel);
            }
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Alias clone = (Alias)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        /// <summary>
        /// Create an array reference to this <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateArrayRef(bool isFirstOnLine, params int[] ranks)
        {
            return new AliasRef(this, isFirstOnLine, ranks);
        }

        /// <summary>
        /// Create an array reference to this <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateArrayRef(params int[] ranks)
        {
            return new AliasRef(this, false, ranks);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="Alias"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef(bool isFirstOnLine)
        {
            return TypeRef.CreateNullable(CreateRef(), isFirstOnLine);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="Alias"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef()
        {
            return TypeRef.CreateNullable(CreateRef());
        }

        /// <summary>
        /// Create a reference to the <see cref="Alias"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new AliasRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            // Ignore any type arguments - an AliasRef can't have any
            return new AliasRef(this, isFirstOnLine, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            // Ignore any type arguments - an AliasRef can't have any
            return new AliasRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            // Ignore any type arguments - an AliasRef can't have any
            return new AliasRef(this, false, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="Alias"/>.
        /// </summary>
        /// <returns>An <see cref="AliasRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments)
        {
            // Ignore any type arguments - an AliasRef can't have any
            return new AliasRef(this, false);
        }

        /// <summary>
        /// Get the base type of the aliased type.
        /// </summary>
        public TypeRef GetBaseType()
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetBaseType() as TypeRef : null);
        }

        /// <summary>
        /// Get the non-static constructor of the aliased type with the specified parameters.
        /// </summary>
        public ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetConstructor(parameterTypes) : null);
        }

        /// <summary>
        /// Get all non-static constructors of the aliased type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors(bool currentPartOnly)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetConstructors(currentPartOnly) : null);
        }

        /// <summary>
        /// Get all non-static constructors of the aliased type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors()
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetConstructors(false) : null);
        }

        /// <summary>
        /// Get the delegate parameters (if any) of the aliased type.
        /// </summary>
        public ICollection GetDelegateParameters()
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetDelegateParameters() : null);
        }

        /// <summary>
        /// Get the delegate return type (if any) of the aliased type.
        /// </summary>
        public TypeRefBase GetDelegateReturnType()
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetDelegateReturnType() : null);
        }

        /// <summary>
        /// Get the field of the aliased type with the specified name.
        /// </summary>
        public FieldRef GetField(string name)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetField(name) : null);
        }

        /// <summary>
        /// Get the method of the aliased type with the specified name and parameter types.
        /// </summary>
        public MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetMethod(name, parameterTypes) : null);
        }

        /// <summary>
        /// Get all methods of the aliased type with the specified name.
        /// </summary>
        public void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        {
            TypeRef typeRef = Type;
            if (typeRef != null)
                typeRef.GetMethods(name, searchBaseClasses, results);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="searchBaseClasses">Pass <c>false</c> to NOT search base classes.</param>
        public List<MethodRef> GetMethods(string name, bool searchBaseClasses)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetMethods(name, searchBaseClasses) : null);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        public List<MethodRef> GetMethods(string name)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetMethods(name, false) : null);
        }

        /// <summary>
        /// Get the nested type of the aliased type with the specified name.
        /// </summary>
        public TypeRef GetNestedType(string name)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetNestedType(name) : null);
        }

        /// <summary>
        /// Get the property of the aliased type with the specified name.
        /// </summary>
        public PropertyRef GetProperty(string name)
        {
            TypeRef typeRef = Type;
            return (typeRef != null ? typeRef.GetProperty(name) : null);
        }

        /// <summary>
        /// Determine if the aliased type is assignable from the specified type.
        /// </summary>
        public bool IsAssignableFrom(TypeRef fromTypeRef)
        {
            TypeRef typeRef = Type;
            return (typeRef != null && typeRef.IsAssignableFrom(fromTypeRef));
        }

        /// <summary>
        /// Determine if the aliased type implements the specified interface type.
        /// </summary>
        public bool IsImplementationOf(TypeRef interfaceTypeRef)
        {
            TypeRef typeRef = Type;
            return (typeRef != null && typeRef.IsImplementationOf(interfaceTypeRef));
        }

        /// <summary>
        /// Determine if the aliased type is a subclass of the specified type.
        /// </summary>
        public bool IsSubclassOf(TypeRef classTypeRef)
        {
            TypeRef typeRef = Type;
            return (typeRef != null && typeRef.IsSubclassOf(classTypeRef));
        }

        /// <summary>
        /// Add a child <see cref="Namespace"/> to the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Add(Namespace @namespace)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Add(@namespace);
        }

        /// <summary>
        /// Add a <see cref="TypeDecl"/> to the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Add(TypeDecl typeDecl)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Add(typeDecl);
        }

        /// <summary>
        /// Add a <see cref="Type"/> to the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Add(Type type)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Add(type);
        }

        /// <summary>
        /// Find or create a child <see cref="Namespace"/>, including any missing parent namespaces.
        /// </summary>
        public Namespace FindOrCreateChildNamespace(string namespaceName)
        {
            NamespaceRef namespaceRef = Namespace;
            return (namespaceRef != null ? namespaceRef.FindOrCreateChildNamespace(namespaceName) : null);
        }

        /// <summary>
        /// Parse the specified name into a child <see cref="NamespaceRef"/> or <see cref="TypeRef"/> on the referenced namespace,
        /// or a <see cref="Dot"/> expression that evaluates to one.
        /// </summary>
        public Expression ParseName(string name)
        {
            NamespaceRef namespaceRef = Namespace;
            return (namespaceRef != null ? namespaceRef.ParseName(name) : null);
        }

        /// <summary>
        /// Remove a child <see cref="Namespace"/> from the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Remove(Namespace @namespace)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Remove(@namespace);
        }

        /// <summary>
        /// Remove a <see cref="TypeDecl"/> from the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Remove(TypeDecl typeDecl)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Remove(typeDecl);
        }

        /// <summary>
        /// Remove a <see cref="Type"/> from the referenced <see cref="Namespace"/>.
        /// </summary>
        public void Remove(Type type)
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.Remove(type);
        }

        /// <summary>
        /// Remove all items from the referenced <see cref="Namespace"/>.
        /// </summary>
        public void RemoveAll()
        {
            NamespaceRef namespaceRef = Namespace;
            if (namespaceRef != null)
                namespaceRef.RemoveAll();
        }

        /// <summary>
        /// Find a child <see cref="Namespace"/>, <see cref="TypeDecl"/>, or <see cref="Type"/> with
        /// the specified name.
        /// </summary>
        /// <returns>The child object if found, otherwise null.</returns>
        public object Find(string name)
        {
            if (IsNamespace)
                return Namespace.Find(name);
            if (IsType)
                return Type.GetNestedType(name).Reference;
            return null;
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
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "using";

        /// <summary>
        /// The token used to parse between the alias name and expression.
        /// </summary>
        public const string ParseTokenSeparator = "=";

        protected Alias(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();                  // Move past 'using'
            _name = parser.GetIdentifierText();  // Parse the name
            parser.NextToken();                  // Move past '='
            SetField(ref _expression, Expression.Parse(parser, this, true), false);
            ParseTerminator(parser);
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 0 (UsingDirective uses 100, Using uses 200)
            Parser.AddParsePoint(ParseToken, Parse, typeof(NamespaceDecl));
        }

        /// <summary>
        /// Parse an <see cref="Alias"/>.
        /// </summary>
        public static Alias Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Continue only if it looks like an alias assignment instead of a UsingDirective
            if (parser.PeekNextTokenText() == ParseTokenSeparator || parser.PeekNextTokenText() == ParseTokenSeparator)
                return new Alias(parser, parent);
            return null;
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _expression != null)
                {
                    _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = true;
                }
            }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations
            if (HasFirstOnLineAnnotations)
                return 2;

            // Default to a preceeding blank line if the previous object was another alias directive
            // with a different root namespace, otherwise use a single newline.
            if (previous is Alias)
            {
                SymbolicRef symbolicRef = ((Alias)previous).Expression.FirstPrefix() as SymbolicRef;
                if (symbolicRef != null && !symbolicRef.IsSameRef(Expression.FirstPrefix() as SymbolicRef))
                    return 2;
                return 1;
            }

            // Default to no preceeding blank line if the previous object was a using directive with
            // the same root namespace, otherwise use a preceeding blank line.
            if (previous is UsingDirective)
            {
                SymbolicRef symbolicRef = ((UsingDirective)previous).Namespace.FirstPrefix() as SymbolicRef;
                if (symbolicRef != null && symbolicRef.IsSameRef(Expression.FirstPrefix() as SymbolicRef))
                    return 1;
                return 2;
            }

            // Default to a preceeding blank line if the object is a different type than the previous one
            if (previous.GetType() != GetType())
                return 2;
            return 1;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.WriteIdentifier(_name, flags);
            writer.Write(" " + ParseTokenSeparator);
            if (_expression != null)
                _expression.AsText(writer, flags | RenderFlags.PrefixSpace);
        }
    }
}
