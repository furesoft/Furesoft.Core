﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares a type that represents a reference to a method.
    /// </summary>
    /// <remarks>
    /// A delegate has no visible body, but one is generated by the compiler to hold the
    /// compiler-generated BeginInvoke and EndInvoke methods.
    /// </remarks>
    public class DelegateDecl : TypeDecl, IParameters
    {
        protected ChildList<ParameterDecl> _parameters;

        /// <summary>
        /// The return type is an <see cref="Expression"/> that must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected Expression _returnType;

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, Expression returnType, Modifiers modifiers)
            : base(name, modifiers, new Block { IsGenerated = true })
        {
            ReturnType = returnType;
            GenerateMethods();  // Generate invoke methods and constructor
        }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, Expression returnType)
            : this(name, returnType, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and parameters.
        /// </summary>
        public DelegateDecl(string name, Expression returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : this(name, returnType, modifiers)
        {
            CreateParameters().AddRange(parameters);
        }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and type parameters.
        /// </summary>
        public DelegateDecl(string name, Expression returnType, Modifiers modifiers, params TypeParameter[] typeParameters)
            : this(name, returnType, modifiers)
        {
            CreateTypeParameters().AddRange(typeParameters);
        }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, Type returnType, Modifiers modifiers)
            : this(name, TypeRef.Create(returnType), modifiers)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, Type returnType)
            : this(name, TypeRef.Create(returnType), Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and parameters.
        /// </summary>
        public DelegateDecl(string name, Type returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : this(name, TypeRef.Create(returnType), modifiers, parameters)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and type parameters.
        /// </summary>
        public DelegateDecl(string name, Type returnType, Modifiers modifiers, params TypeParameter[] typeParameters)
            : this(name, TypeRef.Create(returnType), modifiers, typeParameters)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, ITypeDecl returnType, Modifiers modifiers)
            : this(name, returnType.CreateRef(), modifiers)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name and return type.
        /// </summary>
        public DelegateDecl(string name, ITypeDecl returnType)
            : this(name, returnType.CreateRef(), Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and parameters.
        /// </summary>
        public DelegateDecl(string name, ITypeDecl returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : this(name, returnType.CreateRef(), modifiers, parameters)
        { }

        /// <summary>
        /// Create a <see cref="DelegateDecl"/> with the specified name, return type, modifiers, and type parameters.
        /// </summary>
        public DelegateDecl(string name, ITypeDecl returnType, Modifiers modifiers, params TypeParameter[] typeParameters)
            : this(name, returnType.CreateRef(), modifiers, typeParameters)
        { }

        /// <summary>
        /// True if there are any parameters.
        /// </summary>
        public bool HasParameters
        {
            get { return (_parameters != null && _parameters.Count > 0); }
        }

        /// <summary>
        /// True if the type is a delegate type.
        /// </summary>
        public override bool IsDelegateType
        {
            get { return true; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The number of parameters.
        /// </summary>
        public int ParameterCount
        {
            get { return (_parameters != null ? _parameters.Count : 0); }
        }

        /// <summary>
        /// The list of <see cref="ParameterDecl"/>s.
        /// </summary>
        public ChildList<ParameterDecl> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// The return type of the delegate (never null - will be type 'void' instead).
        /// </summary>
        public Expression ReturnType
        {
            get { return (_returnType ?? TypeRef.VoidRef); }
            set { SetField(ref _returnType, value, true); }
        }

        /// <summary>
        /// The name of the parameter of the constructor of the delegate that accepts a delegate type.
        /// </summary>
        public const string DelegateConstructorParameterName = "target";

        /// <summary>
        /// Add one or more <see cref="ParameterDecl"/>s.
        /// </summary>
        public void AddParameter(ParameterDecl parameterDecl)
        {
            CreateParameters().Add(parameterDecl);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DelegateDecl clone = (DelegateDecl)base.Clone();
            clone.CloneField(ref clone._returnType, _returnType);
            clone._typeParameters = ChildListHelpers.Clone(_typeParameters, clone);
            return clone;
        }

        /// <summary>
        /// Create the list of <see cref="ParameterDecl"/>s, or return the existing list.
        /// </summary>
        public ChildList<ParameterDecl> CreateParameters()
        {
            if (_parameters == null)
                _parameters = new ChildList<ParameterDecl>(this);
            return _parameters;
        }

        /// <summary>
        /// Create (or re-create) the compiler-generated invoke methods and constructor.
        /// This method should be called whenever the parameters of the delegate are set or changed.
        /// </summary>
        public void GenerateMethods()
        {
            // Remove any existing methods before generating them - since the Body is compiler-generated just
            // to hold these methods, we can just re-create it.
            Body = new Block { IsGenerated = true };

            // The Invoke method has the same parameters as the delegate, and the same return type
            MethodDecl invokeDecl = new MethodDecl("Invoke", (Expression)_returnType.Clone(), Modifiers.Public) { IsGenerated = true };
            invokeDecl.Parameters = (_parameters != null ? ChildListHelpers.Clone(_parameters, invokeDecl) : null);
            Add(invokeDecl);

            // The BeginInvoke method has the same parameters as the delegate, plus 2 extra ones, and a return type of IAsyncResult
            MethodDecl beginInvokeDecl = new MethodDecl("BeginInvoke", (TypeRef)TypeRef.IAsyncResultRef.Clone(), Modifiers.Public) { IsGenerated = true };
            ChildList<ParameterDecl> parameters = (_parameters != null ? ChildListHelpers.Clone(_parameters, beginInvokeDecl) : new ChildList<ParameterDecl>(beginInvokeDecl));
            parameters.Add(new ParameterDecl("callback", (TypeRef)TypeRef.AsyncCallbackRef.Clone()));
            parameters.Add(new ParameterDecl("object", (TypeRef)TypeRef.ObjectRef.Clone()));
            beginInvokeDecl.Parameters = parameters;
            Add(beginInvokeDecl);

            // The EndInvoke method has any 'ref' or 'out' parameters of the delegate, plus 1 extra one, and the same return type
            MethodDecl endInvokeDecl = new MethodDecl("EndInvoke", (Expression)_returnType.Clone(), Modifiers.Public) { IsGenerated = true };
            parameters = new ChildList<ParameterDecl>(endInvokeDecl);
            if (_parameters != null)
            {
                foreach (ParameterDecl parameterDecl in _parameters)
                {
                    if (parameterDecl.IsRef || parameterDecl.IsOut)
                        parameters.Add((ParameterDecl)parameterDecl.Clone());
                }
            }
            parameters.Add(new ParameterDecl("result", (TypeRef)TypeRef.IAsyncResultRef.Clone()));
            endInvokeDecl.Parameters = parameters;
            Add(endInvokeDecl);

            // Delegates have a constructor that takes an object and an IntPtr that is used internally by the compiler during
            // code generation.  We have to create a dummy constructor that will allow a MethodRef to be passed to it, in order
            // to make the C# syntax work when resolving.
            TypeRef delegateTypeRef = CreateRef();
            ConstructorDecl constructor = new ConstructorDecl(new[] { new ParameterDecl(DelegateConstructorParameterName, delegateTypeRef) }) { IsGenerated = true };
            Add(constructor);
        }

        /// <summary>
        /// Get the base type.
        /// </summary>
        public override TypeRef GetBaseType()
        {
            return TypeRef.MulticastDelegateRef;
        }

        /// <summary>
        /// Get the non-static constructor with the specified parameters.
        /// </summary>
        public override ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes)
        {
            ConstructorDecl found = GetMethod<ConstructorDecl>(Name, parameterTypes);
            if (found != null)
                return (ConstructorRef)found.CreateRef();
            return null;  // Don't look in base types for DelegateDecls
        }

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        public override NamedCodeObjectGroup GetConstructors(bool currentPartOnly)
        {
            // Find the constructor (ignore 'currentPartOnly', since delegates have no bodies)
            TypeRef delegateTypeRef = CreateRef();
            ConstructorRef constructorRef = GetConstructor(delegateTypeRef);
            return new NamedCodeObjectGroup(constructorRef != null ? constructorRef.Reference : null);
        }

        /// <summary>
        /// Get the parameters of this delegate type.
        /// </summary>
        public override ICollection GetDelegateParameters()
        {
            // Return an empty list instead of null if we have no parameters - this is used elsewhere
            // to distinguish between zero parameters and no parameters available.
            return (_parameters ?? new List<ParameterDecl>());
        }

        /// <summary>
        /// Get the return type of this delegate type.
        /// </summary>
        public override TypeRefBase GetDelegateReturnType()
        {
            return (_returnType != null ? _returnType.SkipPrefixes() as TypeRefBase : null);
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "delegate";

        /// <summary>
        /// The token used to parse the end of the parameters.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// The token used to parse the start of the parameters.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        protected DelegateDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);                                         // Get any comments before 'class'
            parser.NextToken();                                                     // Move past 'delegate'
            SetField(ref _returnType, Expression.Parse(parser, this, true), true);  // Parse the return type

            ParseNameTypeParameters(parser);  // Parse the name and any optional type parameters

            // Parse the parameter declarations
            bool isEndFirstOnLine;
            _parameters = ParameterDecl.ParseList(parser, this, ParseTokenStart, ParseTokenEnd, false, out isEndFirstOnLine);
            IsEndFirstOnLine = isEndFirstOnLine;

            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
            ParseConstraintClauses(parser);  // Parse any constraint clauses
            ParseTerminator(parser);
            GenerateMethods();  // Generate invoke methods and constructor
        }

        public static void AddParsePoints()
        {
            // Delegates are only valid with a Namespace or TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            // Use a parse-priority of 0 (AnonymousMethod uses 100)
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="DelegateDecl"/>.
        /// </summary>
        public static DelegateDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DelegateDecl(parser, parent);
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return true; }
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
            get
            {
                return (base.IsSingleLine && (_returnType == null || (!_returnType.IsFirstOnLine && _returnType.IsSingleLine))
                    && (_parameters == null || _parameters.Count == 0 || (!_parameters[0].IsFirstOnLine && _parameters.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_returnType != null)
                    {
                        _returnType.IsFirstOnLine = false;
                        _returnType.IsSingleLine = true;
                    }
                    if (_parameters != null && _parameters.Count > 0)
                    {
                        _parameters[0].IsFirstOnLine = false;
                        _parameters.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not another delegate declaration.
            if (HasFirstOnLineAnnotations || !(previous is DelegateDecl))
                return 2;
            return 1;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextInfixComments(writer, 0, flags);
            writer.WriteList(_parameters, passFlags, this);
            if (IsEndFirstOnLine)
                writer.WriteLine();
        }

        protected override void AsTextArgumentPrefix(CodeWriter writer, RenderFlags flags)
        { }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken);
            _returnType.AsText(writer, passFlags | RenderFlags.IsPrefix | RenderFlags.PrefixSpace);
            base.AsTextArgument(writer, flags);
        }
    }
}