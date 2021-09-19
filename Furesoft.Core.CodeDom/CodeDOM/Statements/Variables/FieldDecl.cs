// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a class member variable declaration.
    /// </summary>
    /// <remarks>
    /// A FieldDecl can have attributes, but if it's a child of a <see cref="MultiFieldDecl"/>, only
    /// the MultiFieldDecl can have attributes.
    /// A FieldDecl can be used to declare field-like events by using the 'event' modifier
    /// and giving the field a delegate type.
    /// </remarks>
    public class FieldDecl : VariableDecl, IModifiers
    {
        protected Modifiers _modifiers;

        /// <summary>
        /// Create a field declaration.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="type">The type of the field</param>
        /// <param name="initialization">The initialization expression for the field.</param>
        public FieldDecl(string name, Expression type, Expression initialization)
            : base(name, type, initialization)
        { }

        /// <summary>
        /// Create a field declaration.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="type">The type of the field</param>
        public FieldDecl(string name, Expression type)
            : base(name, type, null)
        { }

        /// <summary>
        /// Create a field declaration.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="type">The type of the field</param>
        /// <param name="modifiers">The modifiers for the field.</param>
        /// <param name="initialization">The initialization expression for the field.</param>
        public FieldDecl(string name, Expression type, Modifiers modifiers, Expression initialization)
            : base(name, type, initialization)
        {
            _modifiers = modifiers;
        }

        /// <summary>
        /// Create a field declaration.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="type">The type of the field</param>
        /// <param name="modifiers">The modifiers for the field.</param>
        public FieldDecl(string name, Expression type, Modifiers modifiers)
            : base(name, type, null)
        {
            _modifiers = modifiers;
        }

        protected FieldDecl(Parser parser, CodeObject parent, bool isMulti)
                    : base(parser, parent)
        {
            // Ignore for derived types (FixedSizeBufferDecl)
            if (GetType() != typeof(FieldDecl)) return;

            if (isMulti)
            {
                // Parse the name
                _name = parser.GetIdentifierText();
                MoveLocationAndComment(parser.LastToken);

                ParseInitialization(parser, parent);  // Parse the initialization (if any)
            }
            else
            {
                // Parse the name from the Unused list
                Token token = parser.RemoveLastUnusedToken();
                _name = token.NonVerbatimText;
                MoveLocationAndComment(token);

                ParseUnusedType(parser, ref _type);                 // Parse the type from the Unused list
                _modifiers = ModifiersHelpers.Parse(parser, this);  // Parse any modifiers in reverse from the Unused list
                ParseUnusedAnnotations(parser, this, false);        // Parse attributes and/or doc comments from the Unused list

                ParseInitialization(parser, parent);  // Parse the initialization (if any)
                if (parser.TokenText != Expression.ParseTokenSeparator)
                    ParseTerminator(parser);

                // Check for compiler directives, storing them as postfix annotations on the parent
                Block.ParseCompilerDirectives(parser, this, AnnotationFlags.IsPostfix, false);

                // Force field decls to always start on a new line
                IsFirstOnLine = true;
            }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return (IsConst ? "constant" : "field"); }
        }

        /// <summary>
        /// Get the declaring <see cref="TypeDecl"/>.
        /// </summary>
        public TypeDecl DeclaringType
        {
            get { return (_parent is MultiFieldDecl ? _parent.Parent as TypeDecl : _parent as TypeDecl); }
        }

        /// <summary>
        /// Determines if the code object has a terminator character.
        /// </summary>
        public override bool HasTerminator
        {
            // Ignore any terminator if we're part of a multi
            get { return (!(_parent is MultiFieldDecl) && base.HasTerminator); }
        }

        /// <summary>
        /// True if the field is const.
        /// </summary>
        public override bool IsConst
        {
            get { return _modifiers.HasFlag(Modifiers.Const); }
            set { _modifiers = (value ? _modifiers | Modifiers.Const : _modifiers & ~Modifiers.Const); }
        }

        /// <summary>
        /// True if the field is an event.
        /// </summary>
        public bool IsEvent
        {
            get { return _modifiers.HasFlag(Modifiers.Event); }
            set { _modifiers = (value ? _modifiers | Modifiers.Event : _modifiers & ~Modifiers.Event); }
        }

        /// <summary>
        /// True if the field has internal access.
        /// </summary>
        public bool IsInternal
        {
            get { return _modifiers.HasFlag(Modifiers.Internal); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Internal : _modifiers & ~Modifiers.Internal); }
        }

        /// <summary>
        /// True if the field has private access.
        /// </summary>
        public bool IsPrivate
        {
            get { return _modifiers.HasFlag(Modifiers.Private); }
            // Force other flags off if setting to Private
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Public) | Modifiers.Private : _modifiers & ~Modifiers.Private); }
        }

        /// <summary>
        /// True if the field has protected access.
        /// </summary>
        public bool IsProtected
        {
            get { return _modifiers.HasFlag(Modifiers.Protected); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Protected : _modifiers & ~Modifiers.Protected); }
        }

        /// <summary>
        /// True if the field has public access.
        /// </summary>
        public bool IsPublic
        {
            get { return _modifiers.HasFlag(Modifiers.Public); }
            // Force other flags off if setting to Public
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Protected | Modifiers.Internal) | Modifiers.Public : _modifiers & ~Modifiers.Public); }
        }

        /// <summary>
        /// True if the field is static.
        /// </summary>
        public override bool IsStatic
        {
            get { return (_modifiers.HasFlag(Modifiers.Static) || IsConst); }
            set { _modifiers = (value ? _modifiers | Modifiers.Static : _modifiers & ~Modifiers.Static); }
        }

        /// <summary>
        /// Optional <see cref="Modifiers"/> for the <see cref="FieldDecl"/>.
        /// </summary>
        public virtual Modifiers Modifiers
        {
            get { return _modifiers; }
            set
            {
                if (_parent is MultiFieldDecl)
                    throw new Exception("Can't directly change the Modifiers of a FieldDecl which is a member of a MultiFieldDecl.");
                _modifiers = value;
            }
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
        /// The type of the <see cref="FieldDecl"/>.
        /// </summary>
        public override Expression Type
        {
            set
            {
                if (_parent is MultiFieldDecl)
                    throw new Exception("Can't directly change the Type of a FieldDecl which is a member of a MultiFieldDecl.");
                SetField(ref _type, value, true);
            }
        }

        public static void AddParsePoints()
        {
            // NOTE: We detect field declarations by a ';', '=', or ',' - we parse backwards from the
            //       parse-point, and then (in the latter two cases) parse forwards to complete the parsing.

            // Use a parse-priority of 0 (LocalDecl uses 100)
            Parser.AddParsePoint(ParseTokenTerminator, Parse, typeof(TypeDecl));

            // Use a parse-priority of 0 (LocalDecl uses 100, MultiEnumMemberDecl uses 200, Assignment uses 300)
            Parser.AddParsePoint(Assignment.ParseToken, Parse, typeof(TypeDecl));

            // Use a parse-priority of 0 (LocalDecl uses 100, MultiEnumMemberDecl uses 200)
            Parser.AddParsePoint(Expression.ParseTokenSeparator, Parse, typeof(TypeDecl));
        }

        /// <summary>
        /// Parse a <see cref="FieldDecl"/>.
        /// </summary>
        public static FieldDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Validate that we have an unused identifier token preceeded by a type, and double-check the constraint that our
            // parent is a TypeDecl (necessary when constraints are relaxed for code embedded in doc comments).
            if (parser.HasUnusedTypeRefAndIdentifier && parent is TypeDecl)
            {
                FieldDecl fieldDecl = new FieldDecl(parser, parent, false);

                // Handle additional FieldDecls after any commas
                if (!fieldDecl.HasTerminator && parser.TokenText == Expression.ParseTokenSeparator)
                {
                    // If it's a multi, create one, and transfer any new lines and annotations
                    MultiFieldDecl multiFieldDecl = new MultiFieldDecl(fieldDecl) { NewLines = fieldDecl.NewLines, HasTerminator = false };
                    multiFieldDecl.SetLineCol(fieldDecl);
                    fieldDecl.NewLines = 0;
                    do
                    {
                        Token commaToken = parser.Token;
                        parser.NextToken();  // Move past ','

                        // Associate any EOL comment on the ',' to the last FieldDecl
                        fieldDecl.MoveEOLComment(commaToken, false, false);

                        fieldDecl = new FieldDecl(parser, null, true);

                        // Force the expression to first-on-line if the last comma was (handles special-case
                        // formatting where the commas preceed the list items instead of following them).
                        if (commaToken.IsFirstOnLine)
                            fieldDecl.IsFirstOnLine = true;

                        // Move any comments after the ',' to the current FieldDecl
                        fieldDecl.MoveComments(commaToken);

                        multiFieldDecl.Add(fieldDecl);
                    }
                    while (parser.TokenText == Expression.ParseTokenSeparator);
                    fieldDecl = multiFieldDecl;

                    multiFieldDecl.ParseTerminator(parser);
                }

                return fieldDecl;
            }
            return null;
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return true;
        }

        /// <summary>
        /// Create a reference to the <see cref="FieldDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="FieldRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new FieldRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not another field declaration.
            if (HasFirstOnLineAnnotations || !(previous is FieldDecl))
                return 2;
            return 1;
        }

        /// <summary>
        /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
        /// <param name="isPrivate">True if the access is private.</param>
        /// <param name="isProtected">True if the access is protected.</param>
        /// <param name="isInternal">True if the access is internal.</param>
        public void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            // The isTargetOfAssignment flag is needed only for properties/indexers/events, not fields
            isPrivate = IsPrivate;
            if (!isPrivate)
            {
                isProtected = IsProtected;
                isInternal = IsInternal;
            }
            else
                isProtected = isInternal = false;
        }

        /// <summary>
        /// Get the full name of the <see cref="FieldDecl"/>, including the namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            TypeDecl declaringType = DeclaringType;
            if (declaringType != null)
                return declaringType.GetFullName(descriptive) + "." + _name;
            return _name;
        }

        protected internal void SetTypeFromParentMulti(Expression type)
        {
            SetField(ref _type, type, true);
        }

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            // If in Description mode, use NoEOLComments to determine if we're being rendered as part of a MultiFieldDecl or not
            if (!(_parent is MultiFieldDecl) || (flags.HasFlag(RenderFlags.Description) && !flags.HasFlag(RenderFlags.NoEOLComments)))
                ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool isDescription = flags.HasFlag(RenderFlags.Description);

            // If in Description mode, use NoEOLComments to determine if we're being rendered as part of a MultiFieldDecl or not
            if (!(_parent is MultiFieldDecl) || (isDescription && !flags.HasFlag(RenderFlags.NoEOLComments)))
                AsTextType(writer, flags);

            UpdateLineCol(writer, flags);
            if (isDescription && !flags.HasFlag(RenderFlags.NoEOLComments))
            {
                CodeObject parent = (_parent is MultiFieldDecl ? _parent.Parent : _parent);
                if (parent is TypeDecl)
                {
                    ((TypeDecl)parent).AsTextName(writer, flags);
                    Dot.AsTextDot(writer);
                }
            }
            writer.WriteIdentifier(_name, flags);

            if (_initialization != null)
                AsTextInitialization(writer, passFlags);
        }
    }
}