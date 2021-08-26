// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a local variable declaration.
    /// </summary>
    public class LocalDecl : VariableDecl
    {
        #region /* FIELDS */

        protected Modifiers _modifiers;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a local constant instance.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <param name="type">The type of the constant.</param>
        /// <param name="modifiers">Must be <c>Modifiers.Const</c>.</param>
        /// <param name="initialization">The initialization expression for the constant.</param>
        public LocalDecl(string name, Expression type, Modifiers modifiers, Expression initialization)
            : base(name, type, initialization)
        {
            _modifiers = modifiers;
        }

        /// <summary>
        /// Create a local variable declaration.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable</param>
        /// <param name="initialization">The initialization expression for the variable (optional).</param>
        public LocalDecl(string name, Expression type, Expression initialization)
            : base(name, type, initialization)
        { }

        /// <summary>
        /// Create a local variable declaration.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable</param>
        public LocalDecl(string name, Expression type)
            : base(name, type, null)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return (IsConst ? "local constant" : "local variable"); }
        }

        /// <summary>
        /// Optional <see cref="Modifiers"/>.
        /// </summary>
        public virtual Modifiers Modifiers
        {
            get { return _modifiers; }
            set
            {
                if (_parent is MultiLocalDecl)
                    throw new Exception("Can't directly change the Modifiers of a LocalDecl which is a member of a MultiLocalDecl.");
                _modifiers = value;
            }
        }

        /// <summary>
        /// The type of the <see cref="LocalDecl"/>.
        /// </summary>
        public override Expression Type
        {
            set
            {
                if (_parent is MultiLocalDecl)
                    throw new Exception("Can't directly change the Type of a LocalDecl which is a member of a MultiLocalDecl.");
                SetField(ref _type, value, true);
            }
        }

        /// <summary>
        /// True if the local variable is const.
        /// </summary>
        public override bool IsConst
        {
            get { return _modifiers.HasFlag(Modifiers.Const); }
            set { _modifiers = (value ? _modifiers | Modifiers.Const : _modifiers & ~Modifiers.Const); }
        }

        /// <summary>
        /// Always <c>false</c> for a local variable.
        /// </summary>
        public override bool IsStatic
        {
            get { return false; }
            set { }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create a reference to the <see cref="LocalDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="LocalRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new LocalRef(this, isFirstOnLine);
        }

        protected internal void SetTypeFromParentMulti(Expression type)
        {
            SetField(ref _type, type, true);
        }

        #endregion

        #region /* PARSING */

        internal static void AddParsePoints()
        {
            // We detect local variable declarations by a ';', '=', or ',' - we parse backwards from the
            // parse-point, and then (in the latter two cases) parse forwards to complete the parsing.

            // Use a parse-priority of 100 (FieldDecl uses 0)
            Parser.AddParsePoint(ParseTokenTerminator, 100, Parse, typeof(IBlock));

            // Use a parse-priority of 100 (FieldDecl uses 0, MultiEnumMemberDecl uses 200, Assignment uses 300)
            Parser.AddParsePoint(Assignment.ParseToken, 100, Parse, typeof(IBlock));

            // Use a parse-priority of 100 (FieldDecl uses 0, MultiEnumMemberDecl uses 200)
            Parser.AddParsePoint(Expression.ParseTokenSeparator, 100, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="LocalDecl"/>.
        /// </summary>
        public static LocalDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Validate that we have an unused identifier token preceeded by a type
            if (parser.HasUnusedTypeRefAndIdentifier)
                return Parse(parser, parent, true, true);
            return null;
        }

        /// <summary>
        /// Parse a <see cref="LocalDecl"/>.
        /// </summary>
        public static LocalDecl Parse(Parser parser, CodeObject parent, bool standAlone, bool allowInitAndMulti)
        {
            // Parse the first LocalDecl
            LocalDecl localDecl = new LocalDecl(parser, parent, standAlone, allowInitAndMulti, true, false);

            // Handle additional LocalDecls after any commas
            if (!localDecl.HasTerminator && allowInitAndMulti && parser.TokenText == Expression.ParseTokenSeparator)
            {
                // If it's a multi, create one, and transfer the IsFirstOnLine setting
                MultiLocalDecl multiLocalDecl = new MultiLocalDecl(localDecl) { NewLines = localDecl.NewLines, HasTerminator = false };
                multiLocalDecl.SetLineCol(localDecl);
                localDecl.NewLines = 0;
                do
                {
                    Token commaToken = parser.Token;
                    parser.NextToken();  // Move past ','

                    // Associate any EOL comment on the ',' to the last LocalDecl
                    localDecl.MoveEOLComment(commaToken, false, false);

                    localDecl = new LocalDecl(parser, null, false, true, false, true);

                    // Force the expression to first-on-line if the last comma was (handles special-case
                    // formatting where the commas preceed the list items instead of following them).
                    if (commaToken.IsFirstOnLine)
                        localDecl.IsFirstOnLine = true;

                    // Move any comments after the ',' to the current LocalDecl
                    localDecl.MoveComments(commaToken);

                    multiLocalDecl.Add(localDecl);
                }
                while (parser.TokenText == Expression.ParseTokenSeparator);
                localDecl = multiLocalDecl;

                if (standAlone)
                    multiLocalDecl.ParseTerminator(parser);
            }

            return localDecl;
        }

        /// <summary>
        /// Parse a <see cref="LocalDecl"/>.
        /// </summary>
        public LocalDecl(Parser parser, CodeObject parent, bool standAlone, bool allowInit, bool hasType, bool isMulti)
            : base(parser, parent)
        {
            if (isMulti)
            {
                ParseName(parser, parent);  // Parse the name
                if (allowInit)
                    ParseInitialization(parser, parent);  // Parse the initialization (if any)
            }
            else
            {
                if (standAlone)
                {
                    // Parse the name from the Unused list
                    Token token = parser.RemoveLastUnusedToken();
                    _name = token.NonVerbatimText;
                    MoveLocationAndComment(token);

                    ParseUnusedType(parser, ref _type);  // Parse the type from the Unused list

                    // Parse any modifiers in reverse from the Unused list.
                    // NOTE: Only 'const' is valid for LocalDecls.
                    _modifiers = ModifiersHelpers.Parse(parser, this);

                    if (allowInit)
                        ParseInitialization(parser, parent);  // Parse the initialization (if any)
                }
                else
                {
                    if (hasType)
                        ParseType(parser);  // Parse the type

                    ParseName(parser, parent);  // Parse the name
                    if (allowInit)
                        ParseInitialization(parser, parent);  // Parse the initialization (if any)
                }

                if (standAlone && parser.TokenText != Expression.ParseTokenSeparator)
                    ParseTerminator(parser);
            }
        }

        protected void ParseName(Parser parser, CodeObject parent)
        {
            // Parse the name
            _name = parser.GetIdentifierText();
            if (_name != null)
                MoveLocationAndComment(parser.LastToken);
        }

        /// <summary>
        /// Peek ahead at the input tokens to determine if they look like a valid LocalDecl.
        /// </summary>
        public static bool PeekLocalDecl(Parser parser)
        {
            bool valid = false;

            // Validate that we have what appears to be a valid Type followed by an identifier
            if (TypeRefBase.PeekType(parser, parser.Token, false, ParseFlags.Type))
            {
                Token next = parser.LastPeekedToken;
                if (next != null && next.IsIdentifier)
                {
                    // Also validate that it's followed by one of ";=),"
                    if (";=),".Contains(parser.PeekNextTokenText()))
                        valid = true;
                }
            }

            return valid;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object has a terminator character.
        /// </summary>
        public override bool HasTerminator
        {
            // Ignore any terminator if we're part of a multi
            get { return (!(_parent is MultiLocalDecl) && base.HasTerminator); }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            // If in Description mode, use NoEOLComments to determine if we're being rendered as part of a MultiLocalDecl or not
            if (!(_parent is MultiLocalDecl) || (flags.HasFlag(RenderFlags.Description) && !flags.HasFlag(RenderFlags.NoEOLComments)))
                ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool isDescription = flags.HasFlag(RenderFlags.Description);

            // If in Description mode, use NoEOLComments to determine if we're being rendered as part of a MultiLocalDecl or not
            if (!(_parent is MultiLocalDecl) || (isDescription && !flags.HasFlag(RenderFlags.NoEOLComments)))
                AsTextType(writer, flags);

            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);

            if (_initialization != null)
                AsTextInitialization(writer, passFlags);
        }

        #endregion
    }
}
