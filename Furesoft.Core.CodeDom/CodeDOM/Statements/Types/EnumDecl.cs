// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

using Nova.Parsing;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares an enumerated type, and includes a name and a body with a list of identifiers
    /// (with optional assigned values).  It can also optionally have modifiers and/or a base type.
    /// </summary>
    /// <remarks>
    /// Non-nested enums can be only public or internal, and default to internal.
    /// Nested enums can be any of the 5 access types, and default to private.
    /// Other valid modifiers include: new
    /// Allowable base types are: byte, sbyte, short, ushort, int (default), uint, long, ulong
    /// Enums contain zero or more identifiers as members, which are assigned constant
    /// values which default to 0 and auto-increment.  Alternatively, constant expressions
    /// may be used to manually assign values.
    /// </remarks>
    public class EnumDecl : BaseListTypeDecl
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="EnumDecl"/> with the specified name.
        /// </summary>
        public EnumDecl(string name, Modifiers modifiers)
            : base(name, modifiers)
        { }

        /// <summary>
        /// Create an <see cref="EnumDecl"/> with the specified name.
        /// </summary>
        public EnumDecl(string name)
            : base(name, Modifiers.None)
        { }

        /// <summary>
        /// Create an <see cref="EnumDecl"/> with the specified name, modifiers, and base type.
        /// </summary>
        public EnumDecl(string name, Modifiers modifiers, Expression baseType)
            : base(name, modifiers, baseType)
        { }

        /// <summary>
        /// Create an <see cref="EnumDecl"/> with the specified name and base type.
        /// </summary>
        public EnumDecl(string name, Expression baseType)
            : base(name, Modifiers.None, baseType)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Always <c>true</c> for an enum.
        /// </summary>
        public override bool IsStatic
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>true</c> for an enum.
        /// </summary>
        public override bool IsEnum
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>false</c> for an enum.
        /// </summary>
        public override bool IsGenericType
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>true</c> for an enum.
        /// </summary>
        public override bool IsValueType
        {
            get { return true; }
        }

        /// <summary>
        /// True if this is a bit-flag type enum, otherwise false.
        /// </summary>
        public bool IsBitFlags
        {
            get { return HasAttribute(TypeUtil.FlagsAttributeName); }
            set
            {
                bool isBitFlags = IsBitFlags;
                if (value)
                {
                    // Add the Flags attribute if it doesn't already exist
                    if (!isBitFlags)
                        AttachAnnotation(new Attribute((TypeRef)TypeRef.FlagsAttributeRef.Clone()));
                }
                else
                {
                    // Remove any existing Flags attribute
                    if (isBitFlags)
                        RemoveAttribute(TypeUtil.FlagsAttributeName);
                }
            }
        }

        /// <summary>
        /// The underlying type of the enum (will never be null - defaults to 'int').
        /// </summary>
        public Expression UnderlyingType
        {
            get
            {
                // Any non-default underlying (storage) type is actually stored as the base type, because
                // the syntax appears that way, but the real base type is always System.Enum.  Also, the
                // underlying type must be a primitive integral type, not a user-defined type.  If no
                // storage type is specified, the default is 'int'.
                Expression type = null;
                if (HasBaseTypes)
                    type = BaseTypes[0];
                return (type ?? TypeRef.IntRef);
            }
            set
            {
                // Clear the base type if 'int', otherwise create it or update any existing one
                if (value == null || value.EvaluateType().IsSameRef(TypeRef.IntRef))
                    _baseTypes = null;
                else
                    _baseTypes = new ChildList<Expression>(this) { value };
            }
        }

        /// <summary>
        /// The child MultiEnumMemberDecl object that in turn holds all of the EnumMemberDecls.
        /// </summary>
        public MultiEnumMemberDecl MultiEnumMemberDecl
        {
            get
            {
                // Return the first existing MultiEnumMemberDecl (should be only one)
                MultiEnumMemberDecl valueDecl = _body.FindFirst<MultiEnumMemberDecl>();
                if (valueDecl != null)
                    return valueDecl;

                // If none was found, create one now
                valueDecl = new MultiEnumMemberDecl();
                _body.Add(valueDecl);
                return valueDecl;
            }
        }

        /// <summary>
        /// The EnumMemberDecl grandchildren of the EnumDecl (from the MultiEnumMemberDecl child object).
        /// </summary>
        public ChildList<EnumMemberDecl> MemberDecls
        {
            get { return MultiEnumMemberDecl.MemberDecls; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add an <see cref="EnumMemberDecl"/>.
        /// </summary>
        public void Add(EnumMemberDecl enumMemberDecl)
        {
            MultiEnumMemberDecl.Add(enumMemberDecl);
        }

        /// <summary>
        /// Add an <see cref="EnumMemberDecl"/> with the specified name.
        /// </summary>
        public void Add(string name, Expression initialization)
        {
            MultiEnumMemberDecl.Add(name, initialization);
        }

        /// <summary>
        /// Add an <see cref="EnumMemberDecl"/> with the specified name.
        /// </summary>
        public void Add(string name)
        {
            MultiEnumMemberDecl.Add(name, null);
        }

        /// <summary>
        /// Add multiple <see cref="EnumMemberDecl"/>s.
        /// </summary>
        public void Add(params EnumMemberDecl[] enumMemberDecls)
        {
            MultiEnumMemberDecl.AddRange(enumMemberDecls);
        }

        /// <summary>
        /// Add a collection of <see cref="EnumMemberDecl"/>s.
        /// </summary>
        public void AddRange(IEnumerable<EnumMemberDecl> collection)
        {
            MultiEnumMemberDecl.AddRange(collection);
        }

        /// <summary>
        /// Get the base type.
        /// </summary>
        public override TypeRef GetBaseType()
        {
            // The base type is *always* Enum, NOT the underlying type
            return TypeRef.EnumRef;
        }

        /// <summary>
        /// Get the enum member with the specified name.
        /// </summary>
        public EnumMemberRef GetMember(string name)
        {
            return MultiEnumMemberDecl.GetMember(name);
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "enum";

        internal static void AddParsePoints()
        {
            // Enums are only valid with a Namespace or TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse an <see cref="EnumDecl"/>.
        /// </summary>
        public static EnumDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new EnumDecl(parser, parent);
        }

        protected EnumDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);        // Get any comments before 'enum'
            parser.NextToken();                    // Move past 'enum'
            ParseNameTypeParameters(parser);       // Parse the name.  Type parameters are also handled, although illegal.
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
            ParseBaseTypeList(parser);             // Parse the optional base-type list

            // If we don't have a base-type list, move any trailing compiler directives to the Postfix position
            if (_baseTypes == null || _baseTypes.Count == 0)
                MoveAnnotations(AnnotationFlags.IsInfix1, AnnotationFlags.IsPostfix);

            // We have to do a special callback check for the body to handle a single identifier in the Block
            // with no '=' or ',' for EnumMemberDecl to parse on.
            parser.SingleUnusedIdentifierParser = EnumMemberDecl.Parse;
            new Block(out _body, parser, this, true);  // Parse the body
            parser.SingleUnusedIdentifierParser = null;

            // Eat any trailing terminator (they are allowed but not required on non-delegate type declarations)
            if (parser.TokenText == ParseTokenTerminator)
                parser.NextToken();

            // Force to a multi-line body if the MultiEnumMemberDecl child is multi-line
            if (_body != null && _body.Count > 0 && _body[0].IsFirstOnLine)
                _body.IsFirstOnLine = true;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before an enum declaration
            return 2;
        }

        #endregion
    }
}
