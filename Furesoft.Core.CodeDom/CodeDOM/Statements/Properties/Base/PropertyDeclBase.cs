// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Linq;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="PropertyDecl"/>, <see cref="IndexerDecl"/>, and <see cref="EventDecl"/>.
    /// </summary>
    public abstract class PropertyDeclBase : BlockStatement, IVariableDecl, IModifiers
    {
        protected Modifiers _modifiers;

        /// <summary>
        /// The name can be a string or an Expression (in which case it should be a Dot operator
        /// with a TypeRef to an Interface on the left and an interface member ref on the right), or if this
        /// is an IndexerDecl it can be a ThisRef or a Dot operator with a TypeRef to an Interface on the
        /// left and ThisRef on the right.
        /// </summary>
        protected object _name;

        /// <summary>
        /// The return type is an <see cref="Expression"/> that must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected Expression _type;

        protected PropertyDeclBase(string name, Expression type, Modifiers modifiers, CodeObject body)
            : base(body, true)
        {
            _name = name;
            Type = type;
            _modifiers = modifiers;
        }

        protected PropertyDeclBase(string name, Expression type, Modifiers modifiers)
            : this(name, type, modifiers, new Block())
        { }

        protected PropertyDeclBase(string name, Expression type)
            : this(name, type, Modifiers.None)
        { }

        protected PropertyDeclBase(Expression name, Expression type, Modifiers modifiers)
            : this(null, type, modifiers)
        {
            _name = name;
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// Get the declaring <see cref="TypeDecl"/>.
        /// </summary>
        public TypeDecl DeclaringType
        {
            get { return (_parent as TypeDecl); }
        }

        /// <summary>
        /// Get the explicit interface expression (if any).
        /// </summary>
        public Expression ExplicitInterfaceExpression
        {
            get { return _name as Expression; }
        }

        /// <summary>
        /// True if the property is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return _modifiers.HasFlag(Modifiers.Abstract); }
            set { _modifiers = (value ? _modifiers | Modifiers.Abstract : _modifiers & ~Modifiers.Abstract); }
        }

        /// <summary>
        /// True if this is an explicit interface implementation.
        /// </summary>
        public bool IsExplicitInterfaceImplementation
        {
            get { return _name is Dot; }
        }

        /// <summary>
        /// True if the property has internal access.
        /// </summary>
        public bool IsInternal
        {
            get { return _modifiers.HasFlag(Modifiers.Internal); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Internal : _modifiers & ~Modifiers.Internal); }
        }

        /// <summary>
        /// True if the method is an override.
        /// </summary>
        public bool IsOverride
        {
            get { return _modifiers.HasFlag(Modifiers.Override); }
            set { _modifiers = (value ? _modifiers | Modifiers.Override : _modifiers & ~Modifiers.Override); }
        }

        /// <summary>
        /// True if the property has private access.
        /// </summary>
        public bool IsPrivate
        {
            get { return _modifiers.HasFlag(Modifiers.Private); }
            // Force other flags off if setting to Private
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Public) | Modifiers.Private : _modifiers & ~Modifiers.Private); }
        }

        /// <summary>
        /// True if the property has protected access.
        /// </summary>
        public bool IsProtected
        {
            get { return _modifiers.HasFlag(Modifiers.Protected); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Protected : _modifiers & ~Modifiers.Protected); }
        }

        /// <summary>
        /// True if the property has public access.
        /// </summary>
        public bool IsPublic
        {
            get { return _modifiers.HasFlag(Modifiers.Public); }
            // Force other flags off if setting to Public
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Protected | Modifiers.Internal) | Modifiers.Public : _modifiers & ~Modifiers.Public); }
        }

        /// <summary>
        /// True if the property is readable.
        /// </summary>
        public abstract bool IsReadable { get; }

        /// <summary>
        /// True if the property is static.
        /// </summary>
        public bool IsStatic
        {
            get { return _modifiers.HasFlag(Modifiers.Static); }
            set { _modifiers = (value ? _modifiers | Modifiers.Static : _modifiers & ~Modifiers.Static); }
        }

        /// <summary>
        /// True if the property is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get { return _modifiers.HasFlag(Modifiers.Virtual); }
            set { _modifiers = (value ? _modifiers | Modifiers.Virtual : _modifiers & ~Modifiers.Virtual); }
        }

        /// <summary>
        /// True if the property is writable.
        /// </summary>
        public abstract bool IsWritable { get; }

        /// <summary>
        /// Optional <see cref="Modifiers"/> for the property.
        /// </summary>
        public Modifiers Modifiers
        {
            get { return _modifiers; }
            set { _modifiers = value; }
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (_name is string)
                    return (string)_name;
                // If it's an explicit interface implementation, use the full name
                if (_name is Expression)
                    return ((Expression)_name).AsString();
                return null;
            }
            set { _name = value; }
        }

        /// <summary>
        /// The type of the property.
        /// </summary>
        public Expression Type
        {
            get { return _type; }
            set { SetField(ref _type, value, true); }
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            PropertyDeclBase clone = (PropertyDeclBase)base.Clone();
            clone.CloneField(ref clone._type, _type);
            clone.CloneField(ref clone._name, _name);
            return clone;
        }

        /// <summary>
        /// Get the access rights of the property.
        /// </summary>
        public abstract void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal);

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public virtual string GetFullName(bool descriptive)
        {
            if (_parent is TypeDecl)
                return ((TypeDecl)_parent).GetFullName(descriptive) + "." + _name;
            return _name.ToString();
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        protected PropertyDeclBase(Parser parser, CodeObject parent, bool parse)
            : base(parser, parent)
        {
            IsFirstOnLine = true;  // Force all property, indexer, and event declarations to start on a new line

            if (parse)
            {
                // Parse the name
                if (parser.HasUnusedIdentifier)
                {
                    Token token = parser.RemoveLastUnusedToken();
                    _name = token.NonVerbatimText;
                    SetLineCol(token);
                }
                else
                {
                    // Support Dot expressions so we can handle explicit interface members
                    Expression expression = parser.RemoveLastUnusedExpression();
                    SetField(ref _name, expression, false);
                    Expression leftExpression = (expression is BinaryOperator ? ((BinaryOperator)expression).Left : expression);
                    if (leftExpression != null)
                        SetLineCol(leftExpression);
                }

                ParseTypeModifiersAnnotations(parser);     // Parse type and any modifiers and/or attributes
                new Block(out _body, parser, this, true);  // Parse the body
            }
        }

        public static void AddParsePoints()
        {
            // Property declarations are only valid with a TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            // Use a parse-priority of 200 (GenericMethodDecl uses 0, UnresolvedRef uses 100, BlockDecl uses 300, Initializer uses 400)
            Parser.AddParsePoint(Block.ParseTokenStart, 200, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="PropertyDecl"/> or <see cref="EventDecl"/>.
        /// </summary>
        public static PropertyDeclBase Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If our parent is a TypeDecl, verify that we have an unused Expression (it can be either an
            // identifier or a Dot operator for explicit interface implementations).  Otherwise, require a
            // possible type in addition to the Expression.
            // If it doesn't seem to match the proper pattern, abort so that other types can try parsing it.
            if ((parent is TypeDecl && parser.HasUnusedExpression) || parser.HasUnusedTypeRefAndExpression)
            {
                // If we have an unused 'event' modifier, it's an event, otherwise treat it as a property
                string eventModifier = ModifiersHelpers.AsString(Modifiers.Event).Trim();
                bool isEvent = (Enumerable.Any(parser.Unused, delegate (ParsedObject parsedObject) { return parsedObject is Token && ((Token)parsedObject).Text == eventModifier; }));
                return (isEvent ? (PropertyDeclBase)new EventDecl(parser, parent) : new PropertyDecl(parser, parent));
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

        protected void ParseTypeModifiersAnnotations(Parser parser)
        {
            ParseUnusedType(parser, ref _type);                 // Parse the type from the Unused list
            _modifiers = ModifiersHelpers.Parse(parser, this);  // Parse any modifiers in reverse from the Unused list
            ParseUnusedAnnotations(parser, this, false);        // Parse attributes and/or doc comments from the Unused list
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_type == null || (!_type.IsFirstOnLine && _type.IsSingleLine))
                    && (!(_name is Expression) || (!((Expression)_name).IsFirstOnLine && ((Expression)_name).IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_type != null)
                    {
                        _type.IsFirstOnLine = false;
                        _type.IsSingleLine = true;
                    }
                    if (_name is Expression)
                    {
                        ((Expression)_name).IsFirstOnLine = false;
                        ((Expression)_name).IsSingleLine = true;
                    }
                }
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
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a property declaration, unless it's formatted on a
            // single line and is preceeded by another single-line property declaration of the same type or a comment.
            if (IsSingleLine && ((previous.GetType() == GetType() && previous.IsSingleLine) || previous is Comment))
                return 1;
            return 2;
        }

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        public override void ReformatBlock()
        {
            base.ReformatBlock();

            // If the child accessors have no bodies (they are interface properties/indexers/events),
            // then format the entire statement as a single line.
            if (_body != null)
            {
                if (Enumerable.All(_body, delegate (CodeObject codeObject) { return codeObject is AccessorDecl && ((AccessorDecl)codeObject).Body == null; }))
                    IsSingleLine = true;
            }
        }

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (_type != null)
                _type.AsText(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            if (flags.HasFlag(RenderFlags.Description) && _parent is TypeDecl)
            {
                ((TypeDecl)_parent).AsTextName(writer, flags);
                Dot.AsTextDot(writer);
            }
            if (_name is string)
                writer.WriteIdentifier((string)_name, flags);
            else if (_name is Expression)
                ((Expression)_name).AsText(writer, passFlags & ~(RenderFlags.Description | RenderFlags.ShowParentTypes));
        }
    }
}