// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Defines an iteration variable and a collection (or array) plus a body (a statement or block) that is
    /// repeatedly executed for each variable in the collection.
    /// </summary>
    /// <remarks>
    /// The body is required.
    /// If the collection is null, nothing happens - unlike C#, which throws an exception.
    /// The type of each object in the collection must be convertible to the type of the iteration variable.
    /// The collection expression must evaluate to a type that implements IEnumerable, or a type that
    /// declares a GetEnumerator method, which in turn must return a type that either implements IEnumerable
    /// or declares all of the methods defined in IEnumerator.
    /// </remarks>
    public class ForEach : BlockStatement
    {
        #region /* FIELDS */

        protected LocalDecl _iteration;
        protected Expression _collection;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ForEach"/>.
        /// </summary>
        public ForEach(LocalDecl iteration, Expression collection, CodeObject body)
            : base(body, false)
        {
            Iteration = iteration;
            Collection = collection;
        }

        /// <summary>
        /// Create a <see cref="ForEach"/>.
        /// </summary>
        public ForEach(LocalDecl iteration, Expression collection)
            : base(null, false)
        {
            Iteration = iteration;
            Collection = collection;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The <see cref="LocalDecl"/> iteration variable.
        /// </summary>
        public LocalDecl Iteration
        {
            get { return _iteration; }
            set
            {
                if (value != null && value.Parent != null)
                    throw new Exception("The LocalDecl used for the iteration variable of a ForEach must be new, not one already owned by another Parent object.");
                SetField(ref _iteration, value, true);
            }
        }

        /// <summary>
        /// The collection being iterated over.
        /// </summary>
        public Expression Collection
        {
            get { return _collection; }
            set { SetField(ref _collection, value, true); }
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
        /// Determine the type of the elements in the collection.
        /// </summary>
        public TypeRefBase GetElementType()
        {
            return GetCollectionExpressionElementType(_collection);
        }

        /// <summary>
        /// Determine the element type of the specified collection expression.
        /// </summary>
        public static TypeRefBase GetCollectionExpressionElementType(Expression collectionExpression)
        {
            // Determine the element type of the specified collection expression
            TypeRefBase elementTypeRef = null;

            // Evaluate the type of the collection expression
            TypeRefBase collectionTypeRefBase = collectionExpression.EvaluateType();
            if (collectionTypeRefBase != null)
            {
                // For arrays, use the element type of the array
                if (collectionTypeRefBase.IsArray)
                    return collectionTypeRefBase.GetElementType();

                // For other types, first look for a GetEnumerator() method
                if (collectionTypeRefBase is TypeRef)
                {
                    TypeRef collectionTypeRef = (TypeRef)collectionTypeRefBase;
                    List<MethodRef> methods = collectionTypeRef.GetMethods("GetEnumerator");
                    if (methods != null && methods.Count == 1 && methods[0].IsPublic && !methods[0].IsStatic)
                    {
                        // If we found a single public non-static match, continue
                        MethodRef getEnumerator = methods[0];

                        // The return type of GetEnumerator() will usually be IEnumerator, IEnumerator<T>, or Enumerator<T>.
                        // However, the only actual requirement is that it be a class, struct, or interface type that has a property
                        // named Current (and also a method with the signature 'bool MoveNext()', but we don't care about that here).
                        TypeRef returnType = getEnumerator.GetReturnType() as TypeRef;
                        if (returnType != null && (returnType.IsUserClass || returnType.IsUserStruct || returnType.IsInterface))
                        {
                            // Look for the 'Current' property of the return type
                            PropertyRef current = returnType.GetProperty("Current");
                            if (current != null && current.IsPublic && !current.IsStatic && current.IsReadable)
                            {
                                // The return type of the property is the element type of the collection
                                elementTypeRef = current.GetPropertyType();
                                if (elementTypeRef != null)
                                    elementTypeRef = elementTypeRef.EvaluateTypeArgumentTypes(returnType);
                            }
                        }
                    }
                    else
                    {
                        // If we didn't find a matching GetEnumerator() method, check for an IEnumerable interface:

                        // If there is exactly one type T such that there is an implicit conversion to IEnumerable<T>,
                        // then the element type is T.
                        if (collectionTypeRef.IsSameGenericType(TypeRef.IEnumerable1Ref))
                            elementTypeRef = collectionTypeRef.TypeArguments[0].EvaluateType();
                        else
                        {
                            foreach (TypeRef interfaceTypeRef in collectionTypeRef.GetInterfaces(true))
                            {
                                if (interfaceTypeRef.IsSameGenericType(TypeRef.IEnumerable1Ref))
                                {
                                    elementTypeRef = interfaceTypeRef.TypeArguments[0].EvaluateType();
                                    break;
                                }
                            }
                        }

                        // Otherwise, if there is an implicit conversion to IEnumerable, then the element type is 'object'.
                        if (elementTypeRef == null && collectionTypeRef.IsImplicitlyConvertibleTo(TypeRef.IEnumerableRef))
                            return TypeRef.ObjectRef;
                    }
                    if (elementTypeRef != null)
                        elementTypeRef = elementTypeRef.EvaluateTypeArgumentTypes(collectionExpression);
                }
            }
            return elementTypeRef;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ForEach clone = (ForEach)base.Clone();
            clone.CloneField(ref clone._iteration, _iteration);
            clone.CloneField(ref clone._collection, _collection);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "foreach";

        /// <summary>
        /// The token used to parse the 'in' part.
        /// </summary>
        public const string ParseTokenIn = "in";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="ForEach"/>.
        /// </summary>
        public static ForEach Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ForEach(parser, parent);
        }

        protected ForEach(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'foreach'
            ParseExpectedToken(parser, Expression.ParseTokenStartGroup);  // Move past '('
            SetField(ref _iteration, LocalDecl.Parse(parser, this, false, false), false);
            ParseExpectedToken(parser, ParseTokenIn);
            SetField(ref _collection, Expression.Parse(parser, this, true, Expression.ParseTokenEndGroup), false);
            ParseExpectedToken(parser, Expression.ParseTokenEndGroup);  // Move past ')'

            new Block(out _body, parser, this, false);  // Parse the body
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Resolve the collection first, in case the iteration variable is of 'var' type
            _collection = (Expression)_collection.Resolve(ResolveCategory.Expression, flags);
            _iteration = (LocalDecl)_iteration.Resolve(ResolveCategory.CodeObject, flags);
            return base.Resolve(ResolveCategory.CodeObject, flags);
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            if (_body != null)
            {
                _body.ResolveRef(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            if (_iteration != null)
            {
                _iteration.ResolveRef(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            if (_parent != null)
                _parent.ResolveRefUp(name, resolver);
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_iteration != null && _iteration.HasUnresolvedRef())
                return true;
            if (_collection != null && _collection.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_iteration == null || (!_iteration.IsFirstOnLine && _iteration.IsSingleLine))
                    && (_collection == null || (!_collection.IsFirstOnLine && _collection.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_iteration != null)
                    {
                        _iteration.IsFirstOnLine = false;
                        _iteration.IsSingleLine = true;
                    }
                    if (_collection != null)
                    {
                        _collection.IsFirstOnLine = false;
                        _collection.IsSingleLine = true;
                    }
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_iteration != null)
                _iteration.AsText(writer, flags | RenderFlags.IsPrefix);
            writer.Write(ParseTokenIn);
            _collection.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        #endregion
    }
}
