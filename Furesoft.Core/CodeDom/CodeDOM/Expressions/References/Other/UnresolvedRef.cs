// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a symbolic reference that hasn't been resolved to a direct reference (includes references that
    /// are unresolved because they are ambiguous, or because the type arguments or method arguments don't match
    /// in number or type).
    /// </summary>
    public class UnresolvedRef : TypeRefBase
    {
        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(string name, bool isFirstOnLine)
            : base(name, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(string name)
            : base(name, false)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(string name, int lineNumber, int column)
            : this(name, false)
        {
            _lineNumber = lineNumber;
            _columnNumber = (ushort)column;
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(SymbolicRef symbolicRef)
            : this(symbolicRef.Name, symbolicRef.IsFirstOnLine)
        {
            Parent = symbolicRef.Parent;
            SetLineCol(symbolicRef);
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(TypeRefBase typeRefBase, bool copyTypeArguments)
            : this(typeRefBase.Name, typeRefBase.IsFirstOnLine)
        {
            Parent = typeRefBase.Parent;
            SetLineCol(typeRefBase);
            if (copyTypeArguments)
                _typeArguments = typeRefBase.TypeArguments;
            _arrayRanks = typeRefBase.ArrayRanks;
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected internal UnresolvedRef(Token token)
            : this(token.NonVerbatimText, token.IsFirstOnLine)
        {
            NewLines = token.NewLines;
            SetLineCol(token);
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected internal UnresolvedRef(string name, Token token)
            : this(token)
        {
            // Initialize with token first, but then override to the specified name
            _reference = name;
        }

        /// <summary>
        /// The descriptive category of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Category
        {
            get { return "unresolved"; }
        }

        /// <summary>
        /// Returns true if the UnresolvedRef represents an explicit interface implementation.
        /// </summary>
        public bool IsExplicitInterfaceImplementation
        {
            get
            {
                if (Parent is Dot)
                {
                    if (Parent.Parent is MethodDeclBase)
                        return (((MethodDeclBase)Parent.Parent).ExplicitInterfaceExpression == Parent);
                    if (Parent.Parent is PropertyDeclBase)
                        return (((PropertyDeclBase)Parent.Parent).ExplicitInterfaceExpression == Parent);
                }
                return false;
            }
        }

        /// <summary>
        /// True if this <see cref="UnresolvedRef"/> is the target of an assignment.
        /// </summary>
        public bool IsTargetOfAssignment
        {
            get
            {
                // Handle "expression.UnresolvedRef = ..."
                if (_parent is Dot && _parent.Parent is Assignment && ((Assignment)_parent.Parent).Left == _parent && ((Dot)_parent).Right == this)
                    return true;
                // Handle "expression[...] = ..." (where the hidden IndexerRef is the UnresolvedRef
                if (_parent is Index && _parent.Parent is Assignment && ((Assignment)_parent.Parent).Left == _parent && _parent.HiddenRef == this)
                    return true;
                // Handle "UnresolvedRef = ..."
                if (_parent is Assignment && ((Assignment)_parent).Left == this)
                {
                    // Don't treat as the target of an assignment in the special case of an "embedded collection" initialization.
                    // This is identified by the both the parent AND the right side of the assignment being an Initializer.  In such
                    // a case, the property being assigned should be a collection, and the getter will be used instead of the setter
                    // and the items in the Initializer will be added to the existing collection.
                    Assignment assignment = (Assignment)_parent;
                    return !(assignment.Parent is Initializer && assignment.Right is Initializer);
                }
                return false;
            }
        }

        /// <summary>
        /// The name of the <see cref="UnresolvedRef"/>.
        /// </summary>
        public override string Name
        {
            get { return (string)_reference; }
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/> from the specified <see cref="Token"/>.
        /// </summary>
        public static UnresolvedRef Create(Token identifier)
        {
            return (identifier != null ? new UnresolvedRef(identifier) : null);
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsPossibleDelegateType
        {
            get { return true; }
        }

        /// <summary>
        /// Dispose the <see cref="UnresolvedRef"/>.
        /// </summary>
        public override void Dispose()
        {
            _parent = null;
        }

        /// <summary>
        /// Get the full name of the object, including the namespace name.
        /// </summary>
        public override string GetFullName()
        {
            return Reference as string;
        }

        /// <summary>
        /// Calculate a hash code for the referenced object which is the same for all references where IsSameRef() is true.
        /// </summary>
        public override int GetIsSameRefHashCode()
        {
            // Make the hash codes as unique as possible while still ensuring that they are identical
            // for any objects for which IsSameRef() returns true.
            int hashCode = base.GetIsSameRefHashCode();
            if (_parent is Dot && ((Dot)_parent).Right == this && ((Dot)_parent).Left is SymbolicRef)
                hashCode ^= ((SymbolicRef)((Dot)_parent).Left).GetIsSameRefHashCode();
            return hashCode;
        }

        /// <summary>
        /// Determine if the specified TypeRefBase refers to the same generic type, regardless of actual type arguments.
        /// </summary>
        public override bool IsSameGenericType(TypeRefBase typeRefBase)
        {
            return (typeRefBase is UnresolvedRef && (_typeArguments != null ? _typeArguments.Count : 0) == typeRefBase.TypeArgumentCount && Name == typeRefBase.Name);
        }

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public override bool IsSameRef(SymbolicRef symbolicRef)
        {
            UnresolvedRef unresolvedRef = (symbolicRef is AliasRef ? ((AliasRef)symbolicRef).Alias.Expression.SkipPrefixes() : symbolicRef) as UnresolvedRef;
            if (unresolvedRef == null || (string)Reference != (string)unresolvedRef.Reference)
                return false;

            // The strings of the UnresolvedRefs match, but we have to also verify that any Dot prefixes
            // match - if either side has one, they must match, otherwise neither side can have one.
            Dot parentDot = _parent as Dot;
            Dot parentDot2 = symbolicRef.Parent as Dot;
            SymbolicRef dotPrefix = (parentDot != null && parentDot.Right == this ? parentDot.Left as SymbolicRef : null);
            SymbolicRef dotPrefix2 = (parentDot2 != null && parentDot2.Right == this ? parentDot2.Left as SymbolicRef : null);
            return (dotPrefix == null || dotPrefix2 == null || dotPrefix.IsSameRef(dotPrefix2));
        }

        /// <summary>
        /// Construct an unresolved reference to an array type, or generic type.
        /// </summary>
        protected UnresolvedRef(Parser parser, CodeObject parent, bool isArray, bool isGeneric)
            : base(parser, parent)
        {
            Token token = parser.RemoveLastUnusedToken();
            _reference = token.NonVerbatimText;  // Get the type name
            NewLines = token.NewLines;
            SetLineCol(token);

            if (isArray)
            {
                MoveCommentsAsPost(token);   // Get any comments after the identifier
                ParseArrayRanks(parser);     // Parse the array ranks
            }
            else if (isGeneric)
            {
                MoveCommentsAsPost(token);   // Get any comments after the identifier
                _typeArguments = ParseTypeArgumentList(parser, this);  // Parse the type arguments

                // Check for array ranks on the generic type
                if (parser.TokenText == ParseTokenArrayStart && PeekArrayRanks(parser))
                    ParseArrayRanks(parser);
            }
        }

        /// <summary>
        /// Parse type arguments using the alternate delimiters.
        /// </summary>
        public static Expression ParseAltTypeArguments(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we seem to match a type argument list pattern
            // (otherwise abort so that PropertyDeclBase will get a chance to parse it)
            // Only supported inside documentation comments - subroutines will look for the
            // appropriate delimiters according to the parser state.
            if (parser.InDocComment && parser.HasUnusedIdentifier && PeekTypeArguments(parser, ParseTokenAltArgumentEnd, flags))
                return new UnresolvedRef(parser, parent, false, true);
            return null;
        }

        /// <summary>
        /// Parse array ranks.
        /// </summary>
        public static Expression ParseArrayRanks(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we seem to match an array rank pattern
            // (otherwise abort so it can be parsed as an array index or Attribute)
            if (parser.HasUnusedIdentifier && PeekArrayRanks(parser))
                return new UnresolvedRef(parser, parent, true, false);
            return null;
        }

        /// <summary>
        /// Parse type arguments.
        /// </summary>
        public static Expression ParseTypeArguments(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we seem to match a type argument list pattern
            // (otherwise abort so that the LessThan operator will get a chance to parse it)
            if (parser.HasUnusedIdentifier && PeekTypeArguments(parser, ParseTokenArgumentEnd, flags))
                return new UnresolvedRef(parser, parent, false, true);
            return null;
        }

        internal static new void AddParsePoints()
        {
            // Parse generic type and method references and arrays here in UnresolvedRef, because they may
            // or may not parse as resolved.  Built-in types and '?' nullable types are parsed in TypeRef,
            // because they will parse as resolved.

            // Use a parse-priority of 100 (IndexerDecl uses 0, Index uses 200, Attribute uses 300)
            Parser.AddParsePoint(ParseTokenArrayStart, 100, ParseArrayRanks);

            // Use a parse-priority of 100 (GenericMethodDecl uses 0, LessThan uses 200)
            Parser.AddParsePoint(ParseTokenArgumentStart, 100, ParseTypeArguments);
            // Support alternate symbols for doc comments:
            // Use a parse-priority of 100 (GenericMethodDecl uses 0, PropertyDeclBase uses 200, BlockDecl uses 300, Initializer uses 400)
            Parser.AddParsePoint(ParseTokenAltArgumentStart, 100, ParseAltTypeArguments);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write((string)_reference);
            AsTextTypeArguments(writer, _typeArguments, flags);
            AsTextArrayRanks(writer, flags);
        }
    }
}