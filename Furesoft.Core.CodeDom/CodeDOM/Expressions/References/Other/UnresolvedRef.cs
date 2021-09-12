// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a symbolic reference that hasn't been resolved to a direct reference (includes references that
    /// are unresolved because they are ambiguous, or because the type arguments or method arguments don't match
    /// in number or type).
    /// </summary>
    public class UnresolvedRef : TypeRefBase
    {
        #region /* FIELDS */

        protected ResolveCategory _resolveCategory;  // The targeted object category

        protected bool _resolving;           // Used to prevent infinite recursion
        protected MatchCandidates _matches;  // Objects with a matching name (either complete or partial matches)

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(string name, bool isFirstOnLine, ResolveCategory resolveCategory)
            : base(name, isFirstOnLine)
        {
            _resolveCategory = resolveCategory;
        }

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
        public UnresolvedRef(string name, ResolveCategory resolveCategory, int lineNumber, int column)
            : this(name, false, resolveCategory)
        {
            _lineNumber = lineNumber;
            _columnNumber = (ushort)column;
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(string name, ResolveCategory resolveCategory)
            : this(name, false, resolveCategory)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(SymbolicRef symbolicRef, ResolveCategory resolveCategory)
            : this(symbolicRef.Name, symbolicRef.IsFirstOnLine)
        {
            _resolveCategory = resolveCategory;
            Parent = symbolicRef.Parent;
            SetLineCol(symbolicRef);
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        public UnresolvedRef(TypeRefBase typeRefBase, ResolveCategory resolveCategory, bool copyTypeArguments)
            : this(typeRefBase.Name, typeRefBase.IsFirstOnLine)
        {
            _resolveCategory = resolveCategory;
            Parent = typeRefBase.Parent;
            SetLineCol(typeRefBase);
            if (copyTypeArguments)
                _typeArguments = typeRefBase.TypeArguments;
            _arrayRanks = typeRefBase.ArrayRanks;
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected internal UnresolvedRef(Token token, ResolveCategory resolveCategory)
            : this(token.NonVerbatimText, token.IsFirstOnLine, resolveCategory)
        {
            NewLines = token.NewLines;
            SetLineCol(token);
        }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected internal UnresolvedRef(Token token)
            : this(token, ResolveCategory.Unspecified)
        { }

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/>.
        /// </summary>
        protected internal UnresolvedRef(string name, Token token)
            : this(token)
        {
            // Initialize with token first, but then override to the specified name
            _reference = name;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="UnresolvedRef"/>.
        /// </summary>
        public override string Name
        {
            get { return (string)_reference; }
        }

        /// <summary>
        /// The descriptive category of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Category
        {
            get { return "unresolved"; }
        }

        /// <summary>
        /// The <see cref="ResolveCategory"/>.
        /// </summary>
        public ResolveCategory ResolveCategory
        {
            get { return _resolveCategory; }
        }

        /// <summary>
        /// True if there are some matches.
        /// </summary>
        public bool HasMatches
        {
            get { return (_matches != null && _matches.Count > 0); }
        }

        /// <summary>
        /// True if there is a complete match.
        /// </summary>
        public bool HasCompleteMatch
        {
            get { return (_matches != null && _matches.IsCompleteMatch); }
        }

        /// <summary>
        /// The collection of <see cref="MatchCandidate"/>s.
        /// </summary>
        public MatchCandidates Matches
        {
            get { return _matches; }
            set { _matches = value; }
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
        /// True if this <see cref="UnresolvedRef"/> represents a method group.
        /// </summary>
        public bool IsMethodGroup
        {
            get
            {
                // Method groups will have a category of Expression (because they have no parens)
                if (_matches != null && _matches.Count > 0 && _resolveCategory == ResolveCategory.Expression)
                    return Enumerable.Any(_matches, delegate(MatchCandidate match) { return match.Object is MethodDeclBase || match.Object is MethodDefinition || match.Object is MethodInfo; });
                return false;
            }
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

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Create an <see cref="UnresolvedRef"/> from the specified <see cref="Token"/>.
        /// </summary>
        public static UnresolvedRef Create(Token identifier)
        {
            return (identifier != null ? new UnresolvedRef(identifier) : null);
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Reset all resolution members.
        /// </summary>
        /// <param name="notify">Pass <c>false</c> to NOT send notifications.</param>
        public void ResetResolutionMembers(bool notify)
        {
            _matches = null;

            // Clear any existing error messages from a previous resolve pass
            RemoveAllMessages(MessageSource.Resolve, notify);
        }

        /// <summary>
        /// Reset all resolution members.
        /// </summary>
        public void ResetResolutionMembers()
        {
            ResetResolutionMembers(true);
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsPossibleDelegateType
        {
            get { return true; }
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
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            UnresolvedRef clone = (UnresolvedRef)base.Clone();
            clone.ResetResolutionMembers(false);
            return clone;
        }

        /// <summary>
        /// Dispose the <see cref="UnresolvedRef"/>.
        /// </summary>
        public override void Dispose()
        {
            // Clear any matches and remove all messages if disposed
            ResetResolutionMembers();
            _parent = null;
        }

        /// <summary>
        /// Get the full name of the object, including the namespace name.
        /// </summary>
        public override string GetFullName()
        {
            return Reference as string;
        }

        #endregion

        #region /* PARSING */

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

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve the unresolved reference, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_resolving || flags.HasFlag(ResolveFlags.Unresolve))
                return this;
            _resolving = true;

            SymbolicRef resultRef;
            try
            {
                // Set the resolve category, and reset all members related to the resolving process
                _resolveCategory = resolveCategory;
                ResetResolutionMembers();

                // Resolve any type arguments first
                ChildListHelpers.Resolve(TypeArguments, ResolveCategory.Type, flags);

                // Attempt to resolve this unresolved reference
                resultRef = new Resolver(this, resolveCategory, flags).Resolve();
            }
            finally
            {
                _resolving = false;
            }
            return resultRef;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            return true;
        }

        /// <summary>
        /// Conditionally add the candidate to the collection (depending upon what is already in it).
        /// </summary>
        /// <param name="candidate">The MatchCandidate object.</param>
        /// <returns>True if a complete match exists, otherwise false.</returns>
        public bool AddMatch(MatchCandidate candidate)
        {
            bool isMethodGroup;
            if (_matches != null)
            {
                // If we already have a "method group", then we must ignore any non-method candidates
                isMethodGroup = _matches.IsMethodGroup;
                if (isMethodGroup && !candidate.IsMethod)
                    return _matches.IsCompleteMatch;
            }
            else
            {
                // If the first match is a method, and we have an Expression category, then we have a "method group"
                isMethodGroup = (_resolveCategory == ResolveCategory.Expression && candidate.IsMethod);
            }

            // Process the results of the match
            if (candidate.IsCompleteMatch)
            {
                // The match was complete - purge any existing partial matches and save it
                if (_matches == null || !_matches.IsCompleteMatch)
                    _matches = new MatchCandidates(isMethodGroup, true, true);
                _matches.Add(candidate);
            }
            else if (candidate.IsCategoryMatch && (_matches == null || !_matches.IsCompleteMatch))
            {
                // The category matched, but not the type or method arguments or static mode, so this
                // is considered a partial match.  Purge any existing wrong-category matches and save it.
                if (_matches == null || !_matches.IsCategoryMatch)
                    _matches = new MatchCandidates(isMethodGroup, true, false);
                _matches.Add(candidate);
            }
            else if (_matches == null || (!_matches.IsCompleteMatch && !_matches.IsCategoryMatch))
            {
                // Only the name matched, but save it if we don't have anything better
                if (_matches == null)
                    _matches = new MatchCandidates(isMethodGroup, false, false);
                _matches.Add(candidate);
            }
            // Any matches which are inferior to existing matches are discarded

            return _matches.IsCompleteMatch;
        }

        /// <summary>
        /// Create a reference to the specified code object based upon its type, and copy all
        /// appropriate fields from the current UnresolvedRef object if so instructed.
        /// </summary>
        public SymbolicRef CreateRef(object obj, bool copyAll)
        {
            SymbolicRef symbolicRef = null;
            if (obj is INamedCodeObject)
            {
                if (obj is ITypeDecl)
                    symbolicRef = ((ITypeDecl)obj).CreateRef(IsFirstOnLine, TypeArguments, ArrayRanks);
                else if (obj is GenericMethodDecl)
                    symbolicRef = ((GenericMethodDecl)obj).CreateRef(IsFirstOnLine, TypeArguments);
                else if (obj is ConstructorDecl)
                    symbolicRef = ((ConstructorDecl)obj).CreateRef(IsFirstOnLine);
                else
                    symbolicRef = ((INamedCodeObject)obj).CreateRef(IsFirstOnLine);
            }
            else if (obj is MemberReference)
            {
                if (obj is TypeReference)  // TypeDefinition or GenericParameter
                    symbolicRef = TypeRef.Create((TypeReference)obj, IsFirstOnLine, TypeArguments, ArrayRanks);
                else if (obj is MethodDefinition)
                {
                    if (_resolveCategory == ResolveCategory.OperatorOverload)
                        symbolicRef = new OperatorRef((MethodDefinition)obj);
                    else
                        symbolicRef = MethodRef.Create((MethodDefinition)obj, IsFirstOnLine, TypeArguments);
                }
                else if (obj is PropertyDefinition)
                    symbolicRef = PropertyRef.Create((PropertyDefinition)obj, IsFirstOnLine);
                else if (obj is FieldDefinition)
                {
                    FieldDefinition fieldDefinition = (FieldDefinition)obj;
                    if (fieldDefinition.DeclaringType.IsEnum)
                        symbolicRef = new EnumMemberRef(fieldDefinition, IsFirstOnLine);
                    else
                        symbolicRef = new FieldRef(fieldDefinition, IsFirstOnLine);
                }
                else if (obj is EventDefinition)
                    symbolicRef = new EventRef((EventDefinition)obj, IsFirstOnLine);
            }
            else if (obj is ParameterDefinition)
                symbolicRef = new ParameterRef((ParameterDefinition)obj, IsFirstOnLine);
            else if (obj is MemberInfo)
            {
                if (obj is Type)
                    symbolicRef = TypeRef.Create((Type)obj, IsFirstOnLine, TypeArguments, ArrayRanks);
                else if (obj is MethodInfo)
                {
                    if (_resolveCategory == ResolveCategory.OperatorOverload)
                        symbolicRef = new OperatorRef((MethodInfo)obj);
                    else
                        symbolicRef = new MethodRef((MethodInfo)obj, IsFirstOnLine, TypeArguments);
                }
                else if (obj is ConstructorInfo)
                    symbolicRef = new ConstructorRef((ConstructorInfo)obj, IsFirstOnLine);
                else if (obj is PropertyInfo)
                    symbolicRef = PropertyRef.Create((PropertyInfo)obj, IsFirstOnLine);
                else if (obj is FieldInfo)
                {
                    FieldInfo fieldInfo = (FieldInfo)obj;
                    if (fieldInfo.DeclaringType != null && fieldInfo.DeclaringType.IsEnum)
                        symbolicRef = new EnumMemberRef(fieldInfo, IsFirstOnLine);
                    else
                        symbolicRef = new FieldRef(fieldInfo, IsFirstOnLine);
                }
                else if (obj is EventInfo)
                    symbolicRef = new EventRef((EventInfo)obj, IsFirstOnLine);
            }
            else if (obj is ParameterInfo)
                symbolicRef = new ParameterRef((ParameterInfo)obj, IsFirstOnLine);

            if (symbolicRef != null && copyAll)
            {
                // Copy the parent, formatting, and move any non-message annotations
                symbolicRef.Parent = _parent;
                symbolicRef.CopyFormatting(this);
                RemoveAllMessages();
                symbolicRef.Annotations = Annotations;
                Annotations = null;
                symbolicRef.SetLineCol(this);
            }
            return symbolicRef;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // If we found multiple complete matches, or one or more partial matches, try to determine if an
            // unambiguous common type exists that we can resolve to.  This is done to prevent "cascading" errors
            // where possible.  We do NOT determine a "minimum" common type here, but only an exact common type,
            // such as the common return type of several overloaded methods from which an exact match couldn't be
            // found.  If we can't evaluate to a precise type, we will evaluate to the UnresolvedRef itself, so
            // that "EvaluateType().FindTypeArgument(" can be used to evaluate common type arguments from the
            // return types of multiple matches.  We also don't want to be misleading by evaluating to a minimum
            // common base type - instead, EvaluateToMinimumType() must be explicitly called.
            if (_matches != null && _matches.IsCategoryMatch)
            {
                TypeRefBase typeRefBase = null;
                switch (_resolveCategory)
                {
                    case ResolveCategory.Type:
                        // Nothing to do here for types - if there's more than one, then they're not the same
                        break;
                    case ResolveCategory.Indexer:
                    case ResolveCategory.Method:
                        // Nothing to do for Indexers or Methods - just return the UnresolvedRef.  However, Call and
                        // Index can check for an UnresolvedRef and evaluate to any common return type among the matches.
                        break;
                    case ResolveCategory.Property:
                        // If there are one or more possible matches, evaluate to the return type if all are the same
                        // (very rare for properties, but could happen if two exist in error with the same name)
                        foreach (MatchCandidate candidate in _matches)
                        {
                            TypeRefBase currentRef = PropertyRef.GetPropertyType(candidate.Object);
                            if (currentRef == null)
                            {
                                typeRefBase = null;
                                break;
                            }
                            currentRef = currentRef.EvaluateTypeArgumentTypes(this);
                            if (typeRefBase == null)
                                typeRefBase = currentRef;
                            else if (!currentRef.IsSameRef(typeRefBase))
                            {
                                typeRefBase = null;
                                break;
                            }
                        }
                        break;
                    case ResolveCategory.Constructor:
                    case ResolveCategory.Attribute:
                        // If there are one or more possible matches, evaluate to the return type if all are the same
                        // (for these categories, a type effectively represents the "return type" of a constructor)
                        foreach (MatchCandidate candidate in _matches)
                        {
                            TypeRefBase currentRef = ConstructorRef.GetReturnType(this, candidate.Object);
                            if (currentRef == null)
                            {
                                typeRefBase = null;
                                break;
                            }
                            currentRef = currentRef.EvaluateTypeArgumentTypes(this);
                            if (typeRefBase == null)
                                typeRefBase = currentRef;
                            else if (!currentRef.IsSameRef(typeRefBase))
                            {
                                typeRefBase = null;
                                break;
                            }
                        }
                        break;
                    case ResolveCategory.Expression:
                        // If there are one or more possible matches, evaluate to the type if all are the same
                        // (all possible variable/parameter/field/property references might have the same type).
                        foreach (MatchCandidate candidate in _matches)
                        {
                            object obj = candidate.Object;
                            TypeRefBase currentRef = null;
                            if (obj is CodeObject)
                            {
                                if (obj is IVariableDecl)
                                {
                                    Expression type = ((IVariableDecl)obj).Type;
                                    if (type != null)
                                        currentRef = type.EvaluateType(withoutConstants);
                                }
                            }
                            else if (obj is IMemberDefinition)
                            {
                                TypeReference typeReference = null;
                                if (obj is PropertyDefinition)
                                    typeReference = ((PropertyDefinition)obj).PropertyType;
                                else if (obj is FieldDefinition)
                                    typeReference = ((FieldDefinition)obj).FieldType;
                                if (typeReference != null)
                                    currentRef = TypeRef.Create(typeReference);
                            }
                            else //if (obj is MemberInfo)
                            {
                                Type type = null;
                                if (obj is PropertyInfo)
                                    type = ((PropertyInfo)obj).PropertyType;
                                else if (obj is FieldInfo)
                                    type = ((FieldInfo)obj).FieldType;
                                if (type != null)
                                    currentRef = TypeRef.Create(type);
                            }
                            if (currentRef == null)
                            {
                                typeRefBase = null;
                                break;
                            }
                            currentRef = currentRef.EvaluateTypeArgumentTypes(this);
                            if (typeRefBase == null)
                                typeRefBase = currentRef;
                            else if (!currentRef.IsSameRef(typeRefBase))
                            {
                                typeRefBase = null;
                                break;
                            }
                        }
                        break;
                }
                if (typeRefBase != null)
                    return typeRefBase;
            }

            // By default, just evaluate to the UnresolvedRef itself
            return this;
        }

        /// <summary>
        /// Evaluate to the minimum common type of multiple possible matches.
        /// </summary>
        public TypeRefBase EvaluateToMinimumType()
        {
            // If we found multiple complete matches, or one or more partial matches, try to determine a minimum
            // common type, and evaluate to it.  This is done to prevent "cascading" errors whenever possible.
            // By default, even if there were no matches at all, evaluate to type 'object'.
            if (_matches != null && _matches.IsCategoryMatch)
            {
                TypeRefBase typeRefBase = null;
                switch (_resolveCategory)
                {
                    case ResolveCategory.Type:
                        // If there are one or more possible matches, evaluate to the minimum common type
                        foreach (MatchCandidate candidate in _matches)
                        {
                            TypeRefBase currentRef = (TypeRefBase)CreateRef(candidate.Object, false);
                            typeRefBase = (typeRefBase == null ? currentRef : TypeRef.GetCommonType(typeRefBase, currentRef));
                        }
                        break;
                    case ResolveCategory.Property:
                        // If there are one or more possible matches, evaluate to the minimum common return type
                        // (very rare for properties, but could happen if two exist in error with the same name)
                        foreach (MatchCandidate candidate in _matches)
                        {
                            TypeRefBase currentRef = PropertyRef.GetPropertyType(candidate.Object);
                            typeRefBase = (typeRefBase == null ? currentRef : TypeRef.GetCommonType(typeRefBase, currentRef));
                        }
                        break;
                    case ResolveCategory.Indexer:
                    case ResolveCategory.Method:
                        // Nothing to do for Indexers or Methods - just return the UnresolvedRef.  However, Call and
                        // Index can check for an UnresolvedRef and evaluate to any common return type among the matches.
                        break;
                    case ResolveCategory.Constructor:
                    case ResolveCategory.Attribute:
                        // If there are one or more possible matches, evaluate to the minimum common type
                        foreach (MatchCandidate candidate in _matches)
                        {
                            TypeRefBase currentRef = ConstructorRef.GetReturnType(this, candidate.Object);
                            typeRefBase = (typeRefBase == null ? currentRef : TypeRef.GetCommonType(typeRefBase, currentRef));
                        }
                        break;
                    case ResolveCategory.Expression:
                        // If there are one or more possible matches, evaluate to the minimum common type
                        foreach (MatchCandidate candidate in _matches)
                        {
                            object obj = candidate.Object;
                            TypeRefBase currentRef = null;
                            if (obj is CodeObject)
                            {
                                if (obj is IVariableDecl)
                                {
                                    Expression type = ((IVariableDecl)obj).Type;
                                    if (type != null)
                                        currentRef = type.EvaluateType();
                                }
                            }
                            else if (obj is IMemberDefinition)
                            {
                                TypeReference typeReference = null;
                                if (obj is PropertyDefinition)
                                    typeReference = ((PropertyDefinition)obj).PropertyType;
                                else if (obj is FieldDefinition)
                                    typeReference = ((FieldDefinition)obj).FieldType;
                                if (typeReference != null)
                                    currentRef = TypeRef.Create(typeReference);
                            }
                            else //if (obj is MemberInfo)
                            {
                                Type type = null;
                                if (obj is PropertyInfo)
                                    type = ((PropertyInfo)obj).PropertyType;
                                else if (obj is FieldInfo)
                                    type = ((FieldInfo)obj).FieldType;
                                if (type != null)
                                    currentRef = TypeRef.Create(type);
                            }

                            typeRefBase = (typeRefBase == null ? currentRef : TypeRef.GetCommonType(typeRefBase, currentRef));
                        }
                        break;
                }
                if (typeRefBase != null)
                    return typeRefBase;
            }

            // By default, just evaluate to type 'object'
            return TypeRef.ObjectRef;
        }

        /// <summary>
        /// Get the minimum common type for the specified delegate parameter, from multiple possible matches.
        /// </summary>
        public TypeRefBase GetDelegateParameterType(ArgumentsOperator argumentsOperator, int argumentIndex)
        {
            // If we found multiple complete matches, or one or more partial matches, try to determine a minimum
            // common type for the specified delegate parameter, and evaluate to it.  This is done to prevent
            // "cascading" errors whenever possible.
            TypeRefBase parameterTypeRef = null;
            if (_matches != null && _matches.IsCategoryMatch)
            {
                // Determine a "minimum" delegate parameter type from the possible matches
                foreach (MatchCandidate candidate in _matches)
                {
                    TypeRefBase currentTypeRef = argumentsOperator.GetDelegateParameterType(candidate.Object, argumentIndex);
                    parameterTypeRef = (parameterTypeRef == null ? currentTypeRef : TypeRef.GetCommonType(parameterTypeRef, currentTypeRef));
                }
            }
            return parameterTypeRef;
        }

        /// <summary>
        /// Resolve an existing method group using the specified delegate type.
        /// </summary>
        public UnresolvedRef ResolveMethodGroup(TypeRefBase delegateType)
        {
            // Make a copy of the current method group (don't Clone it, because we don't want to copy matches or messages),
            // setting the Parent to the delegate Type (yes, it's ugly, but it works).
            UnresolvedRef unresolvedRef = new UnresolvedRef(Name, ResolveCategory, LineNumber, ColumnNumber) { Parent = delegateType };
            if (HasArrayRanks)
                unresolvedRef.ArrayRanks = new List<int>(_arrayRanks);
            // The method overload logic (parameter matching and better-method determination) is handled by Resolver.AddMatch(),
            // so create a Resolver instance, and feed the existing method objects to it.
            Resolver resolver = new Resolver(unresolvedRef, ResolveCategory.Expression, ResolveFlags.Quiet);
            foreach (MatchCandidate match in Matches)
                resolver.AddMatch(match.Object);
            // Return the resulting new method group, which might now have a single method that is an exact match, or perhaps
            // still more than one method (with either exact or partial matches).
            return unresolvedRef;
        }

        /// <summary>
        /// Determine the return type of a method group, or null if it can't be determined.
        /// </summary>
        public TypeRefBase MethodGroupReturnType()
        {
            // Return either the type of a single match, or the common type of multiple matches
            TypeRefBase typeRefBase = null;
            if (_matches != null)
            {
                // We must handle indexers in addition to methods, since they also have parameters and can be overloaded
                Expression parentExpression = this;
                if (Parent is Index && Parent.HiddenRef == this)
                    parentExpression = ((Index)Parent).Expression;
                foreach (MatchCandidate candidate in _matches)
                {
                    object obj = candidate.Object;
                    TypeRefBase currentRef = null;
                    if (obj is MethodDeclBase)
                    {
                        Expression type = ((MethodDeclBase)obj).ReturnType;
                        if (type != null)
                            currentRef = type.EvaluateType();
                    }
                    else if (obj is IndexerDecl)
                    {
                        Expression type = ((IndexerDecl)obj).Type;
                        if (type != null)
                            currentRef = type.EvaluateType();
                    }
                    else if (obj is MethodDefinition)
                        currentRef = TypeRef.Create(((MethodDefinition)obj).ReturnType);
                    else if (obj is PropertyDefinition && ((PropertyDefinition)obj).HasParameters)
                        currentRef = TypeRef.Create(((PropertyDefinition)obj).PropertyType);
                    else if (obj is MethodInfo)
                        currentRef = TypeRef.Create(((MethodInfo)obj).ReturnType);
                    else if (obj is PropertyInfo && PropertyInfoUtil.IsIndexed((PropertyInfo)obj))
                        currentRef = TypeRef.Create(((PropertyInfo)obj).PropertyType);
                    if (currentRef == null)
                    {
                        typeRefBase = null;
                        break;
                    }
                    currentRef = currentRef.EvaluateTypeArgumentTypes(parentExpression);
                    if (typeRefBase == null)
                        typeRefBase = currentRef;
                    else if (!currentRef.IsSameRef(typeRefBase))
                    {
                        typeRefBase = null;
                        break;
                    }
                }
            }
            return typeRefBase;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            // It doesn't make any sense to search for a type parameter in an UnresolvedRef, since it has no
            // definition.  Instead, we must lookup by name and index using the method below.

            // However, we should still recursively search any nested type arguments, since they might be resolved
            return (HasTypeArguments ? FindNestedTypeArgument(typeParameterRef) : null);
        }

        /// <summary>
        /// Find a type argument for the specified type name and type argument index.
        /// </summary>
        public TypeRefBase FindTypeArgument(string name, int index)
        {
            if (HasTypeArguments)
            {
                if ((string)_reference == name && index < TypeArgumentCount)
                {
                    Expression typeArgument = TypeArguments[index];
                    if (typeArgument != null)
                        return typeArgument.EvaluateType();
                }
            }
            return null;
        }

        /// <summary>
        /// Determine if the UnresolvedRef is implicitly convertible to the specified TypeRefBase.
        /// </summary>
        /// <param name="toTypeRefBase">The TypeRef, MethodRef, or UnresolvedRef being checked.</param>
        /// <param name="standardConversionsOnly">True if only standard conversions should be allowed.</param>
        public override bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            // Allow unresolved symbolic references to match if identical
            if (IsSameRef(toTypeRefBase))
                return true;

            // Method Groups: A "method group" represents an overloaded method - multiple methods with the same
            // name, but different parameters.  They don't have a type, but they may be implicitly converted to
            // a delegate.  Method groups come into play only for method invocations, delegate initializations,
            // and method arguments when the parameter has a delegate type (including delegate creation expressions).
            // Method invocations/arguments includes Call, Index, and NewObject.

            // Method groups are represented by an UnresolvedRef with method objects that didn't match completely
            // (usually multiple, although a single one is possible).  Method invocations are resolved by matching
            // the parameter types, and delegate initializations are resolved by matching the type of the delegate,
            // thus avoiding the use of method groups in many cases.  However, when a method name is passed as an
            // argument, it usually can't be resolved immediately, because the method being called won't have been
            // resolved yet (arguments are resolved first so that the parameter matching can work).  Instead, when
            // the method is being resolved, it must detect the unresolved method group argument, and check if any
            // of the possible methods match.  If so, then that method is a match, and if a single matching method
            // is found, the method group argument can then be resolved.
            if (IsMethodGroup)
            {
                // Check if any of the methods in the group is a match
                foreach (MatchCandidate methodMatch in _matches)
                {
                    object methodObj = methodMatch.Object;
                    MethodRef methodRef = null;
                    if (methodObj is MethodDeclBase)
                    {
                        if (methodObj is GenericMethodDecl)
                            methodRef = ((GenericMethodDecl)methodObj).CreateRef(TypeArguments);
                        else
                            methodRef = (MethodRef)((MethodDeclBase)methodObj).CreateRef();
                    }
                    else if (methodObj is MethodDefinition)
                        methodRef = new MethodRef((MethodDefinition)methodObj, TypeArguments);
                    else if (methodObj is MethodInfo)
                        methodRef = new MethodRef((MethodInfo)methodObj, TypeArguments);
                    if (methodRef != null)
                    {
                        methodRef = (MethodRef)methodRef.EvaluateTypeArgumentTypes(_parent);
                        toTypeRefBase = toTypeRefBase.EvaluateTypeArgumentTypes(_parent);
                        // For implicit conversion to work when the method belongs to a generic type, we must set the
                        // parent, which will also be used to determine the originating child (this UnresolvedRef).
                        methodRef.Parent = _parent;
                        if (methodRef.IsImplicitlyConvertibleTo(toTypeRefBase))
                            return true;
                    }
                }
            }

            // If we can determine a minimum common type from all possible matches, then check if that
            // will work (so we can avoid the propagation of errors due to the unresolved type).
            TypeRefBase minimumType = EvaluateToMinimumType();
            if (!(minimumType is UnresolvedRef))
                return minimumType.IsImplicitlyConvertibleTo(toTypeRefBase);

            // Allow an unresolved type reference to be passed or assigned to an 'object' type
            if (toTypeRefBase.IsSameRef(TypeRef.ObjectRef))
                return true;

            return false;
        }

        /// <summary>
        /// Determine if any mismatch was solely due to unresolved or undetermined parameter or argument types.
        /// </summary>
        /// <remarks>
        /// This method is used to display an error as a warning if any mismatch was solely due to unresolved
        /// types - assuming that it might have been valid if all types had been resolved, and therefore assuming
        /// this unresolved reference is secondary to another one.  This does not take overload resolution into
        /// account, so it's actually possible the warning could become an error once the types are resolved, but
        /// this is still the desired behavior as it draws attention to the more important errors first.
        /// </remarks>
        public bool IsAnyMismatchDueToUnresolvedOnly()
        {
            // Check if any candidate failed only due to unresolved references (as far as we can tell)
            return (_matches != null && Enumerable.Any(_matches, delegate(MatchCandidate candidate) { return candidate.IsMismatchDueToUnresolvedOnly(); }));
        }

        /// <summary>
        /// Attach the specified text to the <see cref="UnresolvedRef"/> as a <see cref="Message"/>.
        /// </summary>
        public override void AttachMessage(string text, MessageSeverity messageType, MessageSource messageSource)
        {
            if (messageType != MessageSeverity.Unspecified)
                base.AttachMessage("'" + (string)_reference + "' - " + text, messageType, messageSource);
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write((string)_reference);
            AsTextTypeArguments(writer, _typeArguments, flags);
            AsTextArrayRanks(writer, flags);
        }

        #endregion
    }
}
