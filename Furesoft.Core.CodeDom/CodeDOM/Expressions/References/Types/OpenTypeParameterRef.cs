// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using Mono.Cecil;

using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="TypeParameter"/> (or <see cref="GenericParameter"/>/<see cref="Type"/>) from <b>outside</b>
    /// the generic type or method declaration that declares it, and exists only temporarily until it is replaced by a concrete type or
    /// <see cref="TypeParameterRef"/> during the type argument evaluation process.
    /// </summary>
    /// <remarks>
    /// In contrast, a <see cref="TypeParameterRef"/> represents a reference to a <see cref="TypeParameter"/> (or <see cref="GenericParameter"/>
    /// /<see cref="Type"/>) from <b>within</b> the generic type or method declaration that declares it.
    /// Like a <see cref="TypeRef"/> and <see cref="TypeParameterRef"/>, an <see cref="OpenTypeParameterRef"/> can include array ranks,
    /// although it doesn't support type arguments.
    /// </remarks>
    public class OpenTypeParameterRef : TypeParameterRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine, List<int> arrayRanks)
            : base(declaration, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, List<int> arrayRanks)
            : base(declaration, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine, params int[] arrayRanks)
            : base(declaration, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, params int[] arrayRanks)
            : base(declaration, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine, List<int> arrayRanks)
            : base(genericParameter, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine)
            : base(genericParameter, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter)
            : base(genericParameter, false)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter, List<int> arrayRanks)
            : base(genericParameter, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine, params int[] arrayRanks)
            : base(genericParameter, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(GenericParameter genericParameter, params int[] arrayRanks)
            : base(genericParameter, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine, List<int> arrayRanks)
            : base(type, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine)
            : base(type, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type)
            : base(type, false)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, List<int> arrayRanks)
            : base(type, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine, params int[] arrayRanks)
            : base(type, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, params int[] arrayRanks)
            : base(type, false, arrayRanks)
        { }

        #endregion

        #region /* PROPERTIES */

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Convert into a <see cref="TypeParameterRef"/>.
        /// </summary>
        public TypeParameterRef ConvertToTypeParameterRef()
        {
            object reference = Reference;
            if (reference is TypeParameter)
                return new TypeParameterRef((TypeParameter)reference, IsFirstOnLine, ArrayRanks);
            if (reference is GenericParameter)
                return new TypeParameterRef((GenericParameter)reference, IsFirstOnLine, ArrayRanks);
            return new TypeParameterRef((Type)reference, IsFirstOnLine, ArrayRanks);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve child code objects that match the specified name.
        /// </summary>
        public override void ResolveRef(string name, Resolver resolver)
        {
            // An OpenTypeParameterRef can only have members if it's an array, or if the member is on the base 'object' type
            ResolveRef(HasArrayRanks ? ArrayRef : ObjectRef, name, resolver);
        }

        /// <summary>
        /// Resolve indexers.
        /// </summary>
        public override void ResolveIndexerRef(Resolver resolver)
        {
            // Don't attempt on an OpenTypeParameterRef
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            // Always treat open type parameters as having an unresolved type
            return true;
        }

        /// <summary>
        /// Evaluate the type of the OpenTypeParameterRef by searching for matching type arguments in the
        /// specified parent code object tree.
        /// </summary>
        public override TypeRefBase EvaluateTypeArgumentTypes(CodeObject parent, CodeObject originatingChild)
        {
            // Search for a matching type argument in the parent expression
            TypeRefBase typeRefBase = null;
            if (parent != null)
            {
                bool checkEnclosingTypesOnly = false;
                bool skipEnclosingTypes = false;
                if (originatingChild != null)
                {
                    // In order to prevent infinite recursion where FindTypeArgument() calls EvaluateType() which in turn
                    // eventually calls FindTypeArgument() again, we must check for some special cases and only check for
                    // any enclosing GenericMethodDecls/TypeDecls when they occur.  We could avoid this with a separate
                    // virtual method on the parent type, but this is more efficient than bubbling up the entire code tree.

                    // If the parent is a BinaryOperator, Index, or Call and we came from the "left" side, only check enclosing TypeDecls
                    if ((parent is BinaryOperator && ((BinaryOperator)parent).Left == originatingChild)
                        || (parent is Index && (((Index)parent).Expression == originatingChild || parent.HiddenRef == originatingChild))
                        || (parent is Call && ((Call)parent).Expression == originatingChild))
                        checkEnclosingTypesOnly = true;
                }
                if (!checkEnclosingTypesOnly)
                {
                    // Determine if we're inferring type arguments for a generic method invocation, meaning our call stack originated from
                    // MatchCandidate.GetParameterTypes().  We do a check here of 'parent == originatingChild' for final confirmation.
                    bool inferringTypeArguments = (parent is MethodRef && ((MethodRef)parent).HasInferredTypeArguments && parent == originatingChild);

                    // First, look for the type argument directly in the parent expression if it's a reference
                    if (parent is Expression)
                    {
                        typeRefBase = ((Expression)parent).FindTypeArgument(this, originatingChild);

                        // If we found the type argument, and we're inferring type arguments for a generic method invocation, then we
                        // just matched with one of the default inferred type arguments and we need to stop searching.  If there was no
                        // match, then it's not one of the inferred type arguments and we must continue searching.
                        if (typeRefBase != null && inferringTypeArguments)
                            skipEnclosingTypes = true;
                    }

                    CodeObject grandparent = parent.Parent;
                    if (grandparent is Dot)
                    {
                        // If we're on the right side of a Dot, then look at the left side.  If we're inferring type arguments and we
                        // got to this point, then we're on the right side of the Dot, even though the Right property won't be pointing
                        // to our parent since it's a temporary MethodRef generated in MatchCandidate.GetParameterTypes().
                        if (((Dot)grandparent).Right == parent || ((Dot)grandparent).Right == originatingChild || inferringTypeArguments)
                        {
                            // Only look at the left side if we didn't find anything above
                            if (typeRefBase == null)
                                typeRefBase = ((Dot)grandparent).Left.FindTypeArgument(this);

                            // If we're on the right side of a Dot, do NOT look at enclosing types
                            skipEnclosingTypes = true;
                        }
                        else
                        {
                            // If we're on the left side of a Dot, get the Dot's parent so we can continue searching below
                            parent = grandparent.Parent;
                        }
                    }
                }

                // If not found OR we found a TypeParameterRef, recursively look in the type parameters of any enclosing GenericMethodDecl
                // and the type parameters and/or any base type declaration of each enclosing type (a TypeParameterRef might be evaluated
                // into a concrete type or another TypeParameterRef).  Once we find a match, we can stop looking.
                if (!skipEnclosingTypes)
                {
                    TypeParameterRef targetTypeParameterRef;
                    if (typeRefBase is TypeParameterRef)
                    {
                        targetTypeParameterRef = (TypeParameterRef)typeRefBase;
                        typeRefBase = null;
                    }
                    else
                        targetTypeParameterRef = this;

                    // Remove any array ranks from the target type parameter ref
                    TypeParameterRef nonArrayTargetRef = targetTypeParameterRef;
                    while (nonArrayTargetRef.IsArray)
                        nonArrayTargetRef = (TypeParameterRef)nonArrayTargetRef.GetElementType();

                    while (typeRefBase == null && parent != null)
                    {
                        // Look at the type arguments of any parent GenericMethodDecl or TypeDecl
                        if (parent is ITypeParameters)
                        {
                            ChildList<TypeParameter> typeParameters = ((ITypeParameters)parent).TypeParameters;
                            if (typeParameters != null)
                            {
                                foreach (TypeParameter typeParameter in typeParameters)
                                {
                                    TypeRefBase typeParameterRef = (TypeRefBase)typeParameter.CreateRef();
                                    if (typeParameterRef.IsSameRef(nonArrayTargetRef))
                                    {
                                        typeRefBase = (TypeRefBase)typeParameter.CreateRef();
                                        break;
                                    }
                                }
                            }
                        }

                        // If we didn't find anything yet, look at any base types if the parent is a BaseListTypeDecl
                        if (typeRefBase == null && parent is BaseListTypeDecl)
                        {
                            // If we're coming from the base list of the parent type, skip it to avoid infinite recursion
                            List<Expression> baseList = ((BaseListTypeDecl)parent).GetAllBaseTypes();
                            if (baseList != null)
                            {
                                bool alreadyInBaseList = false;
                                if (originatingChild != null)
                                {
                                    foreach (Expression expression in baseList)
                                    {
                                        if (expression == originatingChild)
                                        {
                                            alreadyInBaseList = true;
                                            break;
                                        }
                                    }
                                }
                                if (!alreadyInBaseList)
                                    typeRefBase = ((BaseListTypeDecl)parent).FindTypeArgumentInBase(nonArrayTargetRef);
                            }
                        }

                        // Repeat if not found, finding any parent method first, then any parent types
                        if (typeRefBase == null)
                        {
                            originatingChild = parent;
                            parent = (!(parent is TypeDecl || parent is MethodDeclBase) ? (CodeObject)parent.FindParent<MethodDeclBase>() : parent.FindParent<BaseListTypeDecl>());
                        }
                    }

                    // If we had a previous TypeParameterRef match, and didn't find a further translation, replace it
                    if (typeRefBase == null && targetTypeParameterRef != this)
                        typeRefBase = targetTypeParameterRef;
                }

                // Convert the result to an array type if necessary
                if (typeRefBase != null && HasArrayRanks)
                    typeRefBase = typeRefBase.MakeArrayRef(ArrayRanks);
            }
            return (typeRefBase ?? this);
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            // Don't attempt on an OpenTypeParameterRef
            return null;
        }

        /// <summary>
        /// Determine if the OpenTypeParameterRef is implicitly convertible to the specified TypeRefBase.
        /// </summary>
        /// <param name="toTypeRefBase">The TypeRef, MethodRef, or UnresolvedRef being checked.</param>
        /// <param name="standardConversionsOnly">True if only standard conversions should be allowed.</param>
        public override bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            // Open type parameters only match if they are identical, or if there is an implicit conversion from 'object'
            return (IsSameRef(toTypeRefBase) || IsImplicitlyConvertible(ObjectRef, toTypeRefBase, false));
        }

        /// <summary>
        /// Determine if the reference is implicitly convertible *from* the specified reference.
        /// </summary>
        public override bool IsImplicitlyConvertibleFrom(TypeRefBase fromTypeRefBase)
        {
            // Open type parameters only match if they are identical, or if there is an implicit conversion to 'object'
            return (IsSameRef(fromTypeRefBase) || IsImplicitlyConvertible(fromTypeRefBase, ObjectRef, false));
        }

        #endregion
    }
}
