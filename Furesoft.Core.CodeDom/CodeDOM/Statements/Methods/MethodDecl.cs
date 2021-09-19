// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a named <see cref="Block"/> of code with optional parameters and an optional return value.
    /// Various optional modifiers can also be used (the default is 'private').
    /// </summary>
    public class MethodDecl : MethodDeclBase
    {
        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name, return type, and modifiers.
        /// </summary>
        public MethodDecl(string name, Expression returnType, Modifiers modifiers, CodeObject body, params ParameterDecl[] parameters)
            : base(name, returnType, modifiers, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name, return type, and modifiers.
        /// </summary>
        public MethodDecl(string name, Expression returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : base(name, returnType, modifiers, parameters)
        { }

        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name and return type.
        /// </summary>
        public MethodDecl(string name, Expression returnType, CodeObject body, params ParameterDecl[] parameters)
            : base(name, returnType, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name and return type.
        /// </summary>
        public MethodDecl(string name, Expression returnType, params ParameterDecl[] parameters)
            : base(name, returnType, parameters)
        { }

        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name and return type.
        /// </summary>
        public MethodDecl(Expression name, Expression returnType, CodeObject body, params ParameterDecl[] parameters)
            : base(name, returnType, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="MethodDecl"/> with the specified name and return type.
        /// </summary>
        public MethodDecl(Expression name, Expression returnType, params ParameterDecl[] parameters)
            : base(name, returnType, new Block(), parameters)
        { }

        protected MethodDecl(Parser parser, CodeObject parent, bool parse, ParseFlags flags)
                    : base(parser, parent)
        {
            if (parse)
            {
                ParseMethodNameAndType(parser, parent, true, false);
                ParseParameters(parser);
                ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
                ParseTerminatorOrBody(parser, flags);
            }
        }

        public static void AddParsePoints()
        {
            // Methods are only valid with a TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            // Use a parse-priority of 50 (ConstructorDecl uses 0, LambdaExpression uses 100, Call uses 200, Cast uses 300, Expression parens uses 400).
            Parser.AddParsePoint(ParseTokenStart, 50, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="MethodDecl"/>.
        /// </summary>
        public static MethodDeclBase Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If our parent is a TypeDecl, verify that we have an unused Expression (it can be either an
            // identifier or a Dot operator for explicit interface implementations).  Otherwise, require a
            // possible return type in addition to the Expression.
            // If it doesn't seem to match the proper pattern, abort so that other types can try parsing it.
            if ((parent is TypeDecl && parser.HasUnusedExpression) || parser.HasUnusedTypeRefAndExpression)
            {
                // If we have a Dot expression with an UnresolvedRef with type arguments on the right side,
                // then this is a special-case GenericMethodDecl and we need to turn parsing over to that
                // class (it's normal parsing of the type arguments fails to activate due to the Dot operator
                // activating expression parsing).
                Dot dot = parser.LastUnusedCodeObject as Dot;
                if (dot != null && dot.Right is UnresolvedRef && ((UnresolvedRef)dot.Right).HasTypeArguments)
                    return new GenericMethodDecl(parser, parent, true, flags);

                return new MethodDecl(parser, parent, true, flags);
            }
            return null;
        }

        /// <summary>
        /// Create a reference to the <see cref="MethodDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new MethodRef(this, isFirstOnLine);
        }
    }
}