// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="NewObject"/> and <see cref="NewArray"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Expression"/> of the <see cref="ArgumentsOperator"/> base class should evaluate to a <see cref="TypeRef"/>.
    /// For <see cref="NewObject"/>, an additional hidden <see cref="ConstructorRef"/> exists.
    /// </remarks>
    public abstract class NewOperator : ArgumentsOperator
    {
        #region /* FIELDS */

        /// <summary>
        /// Optional array initializer.
        /// </summary>
        protected Initializer _initializer;

        #endregion

        #region /* CONSTRUCTORS */

        protected NewOperator(Expression expression, params Expression[] parameters)
            : base(expression, parameters)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Optional array initializer.
        /// </summary>
        public Initializer Initializer
        {
            get { return _initializer; }
            set { SetField(ref _initializer, value, true); }
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            NewOperator clone = (NewOperator)base.Clone();
            clone.CloneField(ref clone._initializer, _initializer);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "new";

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
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="NewObject"/> or <see cref="NewArray"/> operator.
        /// </summary>
        public static NewOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Abort if our parent is a TypeDecl (the 'new' is probably part of a method declaration)
            if (parent is TypeDecl)
                return null;

            NewOperator result = null;

            // Peek ahead to see if we have a valid non-array type
            TypeRefBase.PeekType(parser, parser.PeekNextToken(), true, flags | ParseFlags.Type);
            Token token = parser.LastPeekedToken;
            if (token != null)
            {
                // If we found a '[', assume NewArray
                if (token.Text == NewArray.ParseTokenStart)
                    result = new NewArray(parser, parent);
                // If we found '(' or '{', assume NewObject
                else if (token.Text == ParameterDecl.ParseTokenStart || token.Text == Initializer.ParseTokenStart)
                    result = new NewObject(parser, parent);
            }

            // Last chance - invalid code might still parse better as a NewObject, so assume that's
            // what it is if our parent is a VariableDecl.
            if (result == null && parent is VariableDecl)
                result = new NewObject(parser, parent);

            // If we didn't create an object, return null (the 'new' is probably part of a method declaration)
            return result;
        }

        protected NewOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseInitializer(Parser parser, CodeObject parent)
        {
            if (parser.TokenText == Initializer.ParseTokenStart)
                SetField(ref _initializer, new Initializer(parser, parent), false);
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // The initializer is resolved in NewObject/NewArray, because for NewObject it should be resolved after
            // everything else, and for NewArray it should be resolved first.
            base.Resolve(ResolveCategory.Type, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_initializer != null && _initializer.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_initializer == null || (!_initializer.IsFirstOnLine && _initializer.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_initializer != null)
                {
                    _initializer.IsFirstOnLine = !value;
                    _initializer.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextInitializer(CodeWriter writer, RenderFlags flags)
        {
            if (_initializer != null)
            {
                // Make the indent level for the initializer relative to the parent (unless disabled)
                if (!_initializer.HasNoIndentation)
                    writer.BeginIndentOnNewLineRelativeToParentOffset(this, true);
                _initializer.AsText(writer, flags | RenderFlags.PrefixSpace);
                if (!_initializer.HasNoIndentation)
                    writer.EndIndentation(this);
            }
        }

        #endregion
    }
}
