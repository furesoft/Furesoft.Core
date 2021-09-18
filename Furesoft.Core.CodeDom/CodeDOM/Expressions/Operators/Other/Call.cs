// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a call to a Method or a Delegate, including any arguments.
    /// </summary>
    /// <remarks>
    /// The Expression of the ArgumentsOperator base class should evaluate to a MethodRef or
    /// a TypeRef of delegate type (it might be a VariableRef of delegate type, or a Call that
    /// returns a delegate type).  The MethodRef can be a ConstructorRef if this is the
    /// base of a ConstructorInitializer, or if it's part of an Attribute (the main user of
    /// constructors is NewObject, but it derives directly from ArgumentsOperator without
    /// making use of this class).
    /// </remarks>
    public class Call : ArgumentsOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Call"/>.
        /// </summary>
        public Call(Expression expression, params Expression[] arguments)
            : base(expression, arguments)
        { }

        /// <summary>
        /// Create a <see cref="Call"/>.
        /// </summary>
        public Call(MethodDecl methodDecl, params Expression[] arguments)
            : this(methodDecl.CreateRef(), arguments)
        { }

        #endregion

        #region /* PROPERTIES */

        #endregion

        #region / * METHODS */

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            if (_expression != null)
            {
                TypeRefBase invokedRef = _expression.SkipPrefixes() as TypeRefBase;
                if (invokedRef != null)
                    return invokedRef.GetDelegateParameterType(argumentIndex);
            }
            return null;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of the parameters.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        /// <summary>
        /// The token used to parse the end of the parameters.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 200 (ConstructorDecl uses 0, MethodDecl uses 50, LambdaExpression uses 100, Cast uses 300, Expression parens uses 400)
            Parser.AddOperatorParsePoint(ParseTokenStart, 200, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="Call"/>.
        /// </summary>
        public static Call Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have an unused expression before proceeding
            if (parser.HasUnusedExpression)
                return new Call(parser, parent, false);
            return null;
        }

        protected Call(Parser parser, CodeObject parent, bool isInitializer)
            : base(parser, parent)
        {
            if (isInitializer) return;

            // Clear any newlines set by the current token in the base initializer, since it will be the open '(',
            // and we move any newlines on that character to the first parameter in ParseArguments below.
            NewLines = 0;

            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            // Get expression being called
            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);

            ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd);  // Parse arguments

            // Set the parent starting token to the beginning of the expression
            parser.ParentStartingToken = startingToken;
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            _expression.AsText(writer, flags);
            UpdateLineCol(writer, flags);
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseTokenStart);
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
            if (IsEndFirstOnLine)
                writer.WriteLine();
            writer.Write(ParseTokenEnd);
        }

        #endregion
    }
}
