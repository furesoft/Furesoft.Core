// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an index into an array or type with an indexer.
    /// Array indexes must be integral.  Indexers may index with any type.
    /// </summary>
    /// <remarks>
    /// The Expression of the ArgumentsOperator base class should evaluate to a VariableRef of an array
    /// type or a type with an indexer, or an expression that evaluates to an object of an array type
    /// or a type with an indexer.
    /// </remarks>
    public class Index : ArgumentsOperator
    {
        #region /* FIELDS */

        // If the expression resolves to a type with an indexer, there is an implied ".Item" that
        // is omitted by the language for brevity - this hidden reference (IndexerRef) is stored here.
        protected SymbolicRef _indexerRef;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Index"/> operator.
        /// </summary>
        public Index(Expression expression, params Expression[] arguments)
            : base(expression, arguments)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// A hidden <see cref="IndexerRef"/> to an indexer declaration (if any).
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return _indexerRef; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            if (_indexerRef is IndexerRef)
            {
                TypeRefBase parameterTypeRef = MethodRef.GetParameterType(_indexerRef.Reference, argumentIndex, _expression);
                if (parameterTypeRef != null)
                    return parameterTypeRef;
            }
            // By default, assume we're indexing an array type
            return TypeRef.IntRef;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Index clone = (Index)base.Clone();
            clone.CloneField(ref clone._indexerRef, _indexerRef);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of the index operator.
        /// </summary>
        public const string ParseTokenStart = TypeRefBase.ParseTokenArrayStart;

        /// <summary>
        /// The token used to parse the end of the index operator.
        /// </summary>
        public const string ParseTokenEnd = TypeRefBase.ParseTokenArrayEnd;

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
            // Use a parse-priority of 200 (IndexerDecl uses 0, UnresolvedRef uses 100, Attribute uses 300)
            Parser.AddOperatorParsePoint(ParseTokenStart, 200, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Index"/> operator.
        /// </summary>
        public static Index Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have an unused expression before proceeding
            if (parser.HasUnusedExpression)
                return new Index(parser, parent);
            return null;
        }

        protected Index(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Clear any newlines set by the current token in the base initializer, since it will be the open '[',
            // and we move any newlines on that character to the first parameter in ParseArguments below.
            NewLines = 0;

            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);
            ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd);

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
