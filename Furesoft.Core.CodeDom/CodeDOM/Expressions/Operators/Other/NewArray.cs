// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The NewArray operator is used to create new array instances.
    /// </summary>
    /// <remarks>
    /// Possible formats:
    ///
    /// New Array:
    ///    new non-array-type[size,...]           // Create a new array type
    ///    new non-array-type[size,...][,...]...  // Create a new jagged array type
    /// With initializers (init list can be empty) - first format is preferred:
    ///    new array-type { init }                // Create array instance and initialize (init is optional), inferring the sizes
    ///    new non-array-type[size,...] { init }  // Create array and initialize, sizes must match init values
    ///    new non-array-type[size,...][,...]... { init }  // Create jagged array and initialize, sizes must match init values
    ///
    /// In the case of jagged arrays, the first set of array dimensions are arguments to the
    /// base ArgumentsOperator class, while the optional sets of brackets without dimensions
    /// (array ranks) to the right are actually a part of the Type on the left.  For example,
    /// "new int[10][]" represents a parameter of "10" and a TypeRef to an "int[]" type.
    /// The parsing and display of the type must be split up to allow the "[10]" in the middle.
    /// Also, the dimension sizes can be full expressions in addition to literals.
    ///
    /// </remarks>
    public class NewArray : NewOperator
    {
        /// <summary>
        /// The token used to parse the end of the array ranks.
        /// </summary>
        public const string ParseTokenEnd = TypeRefBase.ParseTokenArrayEnd;

        /// <summary>
        /// The token used to parse the start of the array ranks.
        /// </summary>
        public const string ParseTokenStart = TypeRefBase.ParseTokenArrayStart;

        /// <summary>
        /// Create a <see cref="NewArray"/>.
        /// </summary>
        /// <param name="type">An expression representing the TypeRef of the elements of the array.</param>
        /// <param name="parameters">The array size parameters (if any).</param>
        public NewArray(Expression type, params Expression[] parameters)
            : base(type, (parameters ?? new Expression[] { null }))  // Treat null as a single null parameter
        { }

        /// <summary>
        /// Parse a <see cref="NewArray"/>.
        /// </summary>
        public NewArray(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Save the starting token for later
            Token startingToken = parser.Token;

            parser.NextToken();  // Move past 'new'

            // Parse the expression representing the TypeRef, but NOT including any array brackets.  Any first set of
            // following array brackets is parsed below and stored as arguments on the base ArgumentsOperator.  Any
            // additional sets of brackets for nested arrays are then parsed after that and stored on the TypeRef or
            // UnresolvedRef.
            Expression expression = Parse(parser, this, false, ParseTokenStart + Initializer.ParseTokenStart, ParseFlags.NoArrays);
            SetField(ref _expression, expression, false);

            // Parse any parameters (dimension sizes) OR null parameters ([] or [,], etc)
            if (parser.TokenText == ParseTokenStart)
                ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd, true);

            // Parse any trailing empty dimensions (ranks)
            if (parser.TokenText == ParseTokenStart)
            {
                // Add to the existing TypeRef or UnresolvedRef
                Expression right = _expression.SkipPrefixes();
                if (right is TypeRefBase)
                    ((TypeRefBase)right).ParseArrayRanks(parser);
            }

            // Set the parent starting token for use by the Initializer to determine proper indentation
            parser.ParentStartingToken = startingToken;

            // Parse any array initializer
            ParseInitializer(parser, this);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Suppress empty brackets (they'll be included as a part of the type)
            base.AsTextExpression(writer, flags | RenderFlags.NoParensIfEmpty);
        }

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            return TypeRef.IntRef;
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
            if (IsEndFirstOnLine)
                writer.WriteLine();
            writer.Write(ParseTokenEnd);
        }

        protected override void AsTextInitializer(CodeWriter writer, RenderFlags flags)
        {
            // Render any omitted brackets here (after parameters and before any initializer),
            // skipping the first set, because they will be rendered as the arguments.
            if (_expression != null)
            {
                TypeRefBase typeRefBase = _expression.SkipPrefixes() as TypeRefBase;
                if (typeRefBase != null)
                    typeRefBase.AsTextArrayRanks(writer, flags);
            }

            base.AsTextInitializer(writer, flags);
        }

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken);

            // Omit any brackets (they will be rendered later)
            if (_expression != null)
                _expression.AsText(writer, flags | RenderFlags.SuppressBrackets | RenderFlags.PrefixSpace);
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseTokenStart);
        }
    }
}