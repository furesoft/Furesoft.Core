// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents the declaration of an individual enum member.
    /// Can only be used as a child of <see cref="MultiEnumMemberDecl"/>, not as a stand-alone declaration.
    /// </summary>
    /// <remarks>
    /// The type of an EnumMemberDecl is always the type of its parent MultiEnumMemberDecl (which in
    /// turn is always the type of its parent <see cref="EnumDecl"/>).
    /// </remarks>
    public class EnumMemberDecl : VariableDecl
    {
        /// <summary>
        /// Create an enum member declaration.
        /// </summary>
        /// <param name="name">The name of the enum member.</param>
        /// <param name="initialization">The initialization expression for the enum member.</param>
        public EnumMemberDecl(string name, Expression initialization)
            : base(name, null, initialization)
        { }

        /// <summary>
        /// Create an enum member declaration.
        /// </summary>
        /// <param name="name">The name of the enum member.</param>
        public EnumMemberDecl(string name)
            : base(name, null, null)
        { }

        /// <summary>
        /// Parse an <see cref="EnumMemberDecl"/>.
        /// </summary>
        public EnumMemberDecl(Parser parser, CodeObject parent, bool unusedName)
            : base(parser, parent)
        {
            Token token;
            if (unusedName)
            {
                // Get the name from the Unused list
                token = parser.RemoveLastUnusedToken();
                _name = token.NonVerbatimText;
            }
            else
            {
                // Parse the name
                _name = parser.GetIdentifierText();
                token = parser.LastToken;
            }
            MoveLocationAndComment(token);

            ParseUnusedAnnotations(parser, this, true);  // Parse any annotations from the Unused list
            ParseInitialization(parser, parent);         // Parse the initialization (if any)

            // Move any EOL or Postfix annotations on the init expression to the parent
            if (_initialization != null)
                MoveEOLAndPostAnnotations(_initialization);
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "enum"; }
        }

        /// <summary>
        /// Determines if the code object has a terminator character.
        /// </summary>
        public override bool HasTerminator
        {
            // EnumMemberDecls don't have terminators, so disable use of this flag
            get { return false; }
            set { }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return false; }
        }

        /// <summary>
        /// True if this is a member of a bit-flag enum.
        /// </summary>
        public bool IsBitFlag
        {
            get
            {
                EnumDecl enumDecl = ParentEnumDecl;
                return (enumDecl != null && enumDecl.IsBitFlags);
            }
        }

        /// <summary>
        /// Always <c>true</c> for an enum member.
        /// </summary>
        public override bool IsConst
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return !HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// Always <c>true</c> for an enum member.
        /// </summary>
        public override bool IsStatic
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return base.NewLines; }
            set
            {
                // If we're changing to or from zero, also change any prefix attributes
                bool isFirstOnLine = (value != 0);
                if (_annotations != null && ((!isFirstOnLine && IsFirstOnLine) || (isFirstOnLine && !IsFirstOnLine)))
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (annotation is Attribute)
                            annotation.IsFirstOnLine = isFirstOnLine;
                    }
                }

                base.NewLines = value;
            }
        }

        /// <summary>
        /// The parent <see cref="EnumDecl"/>.
        /// </summary>
        public virtual EnumDecl ParentEnumDecl
        {
            get
            {
                // Our parent should be a MultiEnumMemberDecl, and our grandparent is the EnumDecl
                return (_parent is MultiEnumMemberDecl ? _parent.Parent as EnumDecl : null);
            }
        }

        /// <summary>
        /// The type of the parent <see cref="EnumDecl"/>.
        /// </summary>
        public override Expression Type
        {
            get { return (_parent is MultiEnumMemberDecl ? ((MultiEnumMemberDecl)_parent).Type : null); }
            set { throw new Exception("Can't change the Type of an EnumMemberDecl - it's always the parent EnumDecl."); }
        }

        public static void AddParsePoints()
        {
            // We detect enum member declarations by '=' or ',' at the top level of an EnumDecl block.
            // We parse backwards from the parse-point, and then parse forwards to complete the parsing.
            // This is consistent with how we parse LocalDecls, but it doesn't handle a single-name enum,
            // so we do a special check for that in EnumDecl/Parser.  Unlike LocalDecls and FieldDecls,
            // EnumMemberDecls can't exist independently, but only as part of a MultiEnumMemberDecl.
            // However, we parse them here to be consistent with how the others work, and to avoid the
            // issue of a parse constructor for MultiEnumMemberDecl having to call the EnumMemberDecl
            // parse constructor.

            // Use a parse-priority of 200 (FieldDecl uses 0, LocalDecl uses 100, Assignment uses 300)
            Parser.AddParsePoint(Assignment.ParseToken, 200, Parse, typeof(EnumDecl));

            // Use a parse-priority of 200 (FieldDecl uses 0, LocalDecl uses 100)
            Parser.AddParsePoint(Expression.ParseTokenSeparator, 200, Parse, typeof(EnumDecl));
        }

        /// <summary>
        /// Parse an <see cref="EnumMemberDecl"/>.
        /// </summary>
        public static EnumMemberDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Turn off special callback from EnumDecl used to parse single-identifier enums
            parser.SingleUnusedIdentifierParser = null;

            // Validate that we have an unused identifier token
            if (parser.HasUnusedIdentifier)
            {
                // Parse the first EnumMemberDecl
                EnumMemberDecl enumMemberDecl = new EnumMemberDecl(parser, parent, true);

                // Always create a MultiEnumMemberDecl for enums
                MultiEnumMemberDecl multiEnumMemberDecl = new MultiEnumMemberDecl(enumMemberDecl);
                multiEnumMemberDecl.SetLineCol(enumMemberDecl);

                // Handle additional EnumMemberDecls after any commas
                while (parser.TokenText == Expression.ParseTokenSeparator)
                {
                    Token lastTokenOfLastMember = parser.LastToken;
                    bool lastCommaFirstOnLine = parser.Token.IsFirstOnLine;
                    parser.NextToken();  // Move past ','

                    // Associate any EOL comment on the ',' to the last EnumMemberDecl
                    enumMemberDecl.MoveEOLComment(parser.LastToken, false, false);

                    // Parse any comments, doc comments, attributes, compiler directives
                    enumMemberDecl.ParseAnnotations(parser, parent, false, false);

                    // Abort if we had a trailing comma with nothing after it
                    if (parser.TokenText == Block.ParseTokenEnd)
                        break;

                    // Parse the next EnumMemberDecl
                    enumMemberDecl = new EnumMemberDecl(parser, null, false);
                    // Get any post comments after the last member and before the comma
                    enumMemberDecl.MoveComments(lastTokenOfLastMember);

                    // Force the EnumMemberDecl to first-on-line if the last comma was (handles special-case
                    // formatting where the commas preceed the list items instead of following them).
                    if (lastCommaFirstOnLine)
                        enumMemberDecl.IsFirstOnLine = true;

                    multiEnumMemberDecl.Add(enumMemberDecl);
                }

                // Parse any post compiler directives
                enumMemberDecl.ParseAnnotations(parser, parent, true, false);
                return multiEnumMemberDecl;
            }
            return null;
        }

        /// <summary>
        /// Parse an <see cref="EnumMemberDecl"/> starting from an unused name.
        /// </summary>
        public static EnumMemberDecl ParseEnd(Parser parser, CodeObject parent, ParseFlags flags)
        {
            if (parser.HasUnusedIdentifier)
                return Parse(parser, parent, flags);
            return null;
        }

        /// <summary>
        /// Create a reference to the <see cref="EnumMemberDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="EnumMemberRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new EnumMemberRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not another enum member declaration.
            if (HasFirstOnLineAnnotations || !(previous is EnumMemberDecl))
                return 2;
            return 1;
        }

        /// <summary>
        /// Get the full name of the <see cref="EnumMemberDecl"/>, including the namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            EnumDecl enumDecl = ParentEnumDecl;
            if (enumDecl != null)
                return enumDecl.GetFullName(descriptive) + "." + _name;
            return _name;
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool isDescription = flags.HasFlag(RenderFlags.Description);

            UpdateLineCol(writer, flags);
            if (isDescription)
            {
                EnumDecl parentEnumDecl = ParentEnumDecl;
                if (parentEnumDecl != null)
                {
                    parentEnumDecl.AsTextName(writer, flags);
                    Dot.AsTextDot(writer);
                }
            }
            writer.Write(_name);

            if (_initialization != null)
            {
                // Check for alignment of the initialization (ignore if empty or it doesn't fit the pattern)
                if (IsBitFlag && IsFirstOnLine)
                {
                    int padding = writer.GetColumnWidth(Parent, 0) - _name.Length;
                    if (padding > 0)
                        writer.Write(new string(' ', padding));
                }

                AsTextInitialization(writer, passFlags);
            }
        }
    }
}