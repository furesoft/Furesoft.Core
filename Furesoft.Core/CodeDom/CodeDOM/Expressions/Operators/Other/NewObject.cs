// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// The NewObject operator is used to create new object instances of class, struct, or delegates.
    /// </summary>
    /// <remarks>
    /// Possible formats:
    ///
    /// New Object:
    ///    new non-array-type(args)           // Type with constructor call (args is optional)
    /// With initializers (3.0):
    ///    new non-array-type(args) { init }  // Type/constructor call with object or collection initializer (args and init are optional)
    ///    new non-array-type { init }        // Type with default constructor with object or collection initializer (init is optional)
    ///
    /// </remarks>
    public class NewObject : NewOperator
    {
        // A NewObject has both a TypeRef and an implied, hidden ConstructorRef.  The syntax "new List<int>()" is actually
        // short-hand for "new List<int>.List()".  We store both references, display the TypeRef name, show the 'new' keyword
        // in the error or warning color if the hidden ConstructorRef has an error or warning, and display the hidden ConstructorRef
        // in the tooltip.  This is necessary to handle type aliases, because the TypeRef and ConstructorRef will have different
        // names in this case, and the aliased type can have type arguments while the alias itself can't.
        protected SymbolicRef _constructorRef;

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="expression">An expression evaluating to the TypeRef of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(Expression expression, params Expression[] parameters)
            : base(expression, parameters)
        { }

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="constructorDecl">The ConstructorDecl of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(ConstructorDecl constructorDecl, params Expression[] parameters)
            : base(constructorDecl.DeclaringType.CreateRef(), parameters)
        {
            SetField(ref _constructorRef, constructorDecl.CreateRef(), false);
        }

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="typeDecl">The TypeDecl of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(TypeDecl typeDecl, params Expression[] parameters)
            : base(typeDecl.CreateRef(), parameters)
        { }

        /// <summary>
        /// The hidden <see cref="ConstructorRef"/> (or <see cref="UnresolvedRef"/>) that represents the constructor being called.
        /// Will be null if the constructor is implied, such as for the default constructor of a value type.
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return _constructorRef; }
        }

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            if (_constructorRef is ConstructorRef)
            {
                TypeRefBase parameterTypeRef = MethodRef.GetParameterType(_constructorRef.Reference, argumentIndex, _expression);
                if (parameterTypeRef != null)
                    return parameterTypeRef;
            }
            // By default, assume we're indexing an array type
            return TypeRef.IntRef;
        }

        /// <summary>
        /// The token used to parse the end of the arguments.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// The token used to parse the start of the arguments.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        /// <summary>
        /// Parse a <see cref="NewObject"/>.
        /// </summary>
        public NewObject(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Save the starting token for later
            Token startingToken = parser.Token;

            parser.NextToken();  // Move past 'new'

            // Parse the expression representing the TypeRef being instantiated
            SetField(ref _expression, Parse(parser, this, false, ParseTokenStart + Initializer.ParseTokenStart), false);

            // Parse the arguments (if any) of the implied ConstructorRef being called
            if (parser.TokenText == ParseTokenStart)
                ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd);

            // Set the parent starting token for use by the Initializer to determine proper indentation
            parser.ParentStartingToken = startingToken;

            // Parse any object or collection initializer
            ParseInitializer(parser, this);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Suppress empty parens *if* we have an initializer
            base.AsTextExpression(writer, flags | (_initializer != null ? RenderFlags.NoParensIfEmpty : 0));
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
            if (IsEndFirstOnLine)
                writer.WriteLine();
            writer.Write(ParseTokenEnd);
        }

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken);
            if (_expression != null)
                _expression.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseTokenStart);
        }
    }
}