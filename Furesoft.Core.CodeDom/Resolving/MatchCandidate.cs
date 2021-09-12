// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Collections.Generic;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.AnonymousMethods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Loops;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Resolving;
using Furesoft.Core.CodeDom.Utilities.Mono.Cecil;
using Furesoft.Core.CodeDom.Utilities.Reflection;

namespace Furesoft.Core.CodeDom.Resolving
{
    /// <summary>
    /// Represents a possible match for an <see cref="UnresolvedRef"/>.
    /// </summary>
    public class MatchCandidate
    {
        #region /* FIELDS */

        /// <summary>
        /// The candidate <see cref="CodeObject"/> or <see cref="MemberInfo"/>.
        /// </summary>
        public readonly object Object;

        /// <summary>
        /// True if the <see cref="ResolveCategory"/> matches.
        /// </summary>
        public bool IsCategoryMatch;

        /// <summary>
        /// True if the type arguments match.
        /// </summary>
        public bool IsTypeArgumentsMatch = true;

        /// <summary>
        /// True if type inference failed.
        /// </summary>
        public bool IsTypeInferenceFailure;

        /// <summary>
        /// True if the method arguments match.
        /// </summary>
        public bool IsMethodArgumentsMatch = true;

        /// <summary>
        /// The index of the first non-matching method argument.
        /// </summary>
        public int MethodArgumentMismatchIndex = -1;

        /// <summary>
        /// True if the method arguments only failed to match because of <see cref="UnresolvedRef"/>s.
        /// </summary>
        public bool ArgumentsMismatchDueToUnresolvedOnly;

        /// <summary>
        /// True if the static modifiers match.
        /// </summary>
        public bool IsStaticModeMatch = true;

        /// <summary>
        /// True if the candidate is accessible.
        /// </summary>
        public bool IsAccessible = true;

        /// <summary>
        /// The associated <see cref="ResolveFlags"/>.
        /// </summary>
        public ResolveFlags ResolveFlags;

        /// <summary>
        /// Any inferred type arguments.
        /// </summary>
        public TypeRefBase[] InferredTypeArguments;

        /// <summary>
        /// An array of flags indicating if each inferred type argument has been fixed.
        /// </summary>
        public bool[] IsTypeArgumentFixed;

        /// <summary>
        /// True if the reference is inside a <see cref="DocCode"/> documentation comment.
        /// </summary>
        public bool IsDocCodeReference;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Construct a new MatchCandidate object.
        /// </summary>
        public MatchCandidate(object obj, UnresolvedRef unresolvedRef, ResolveFlags resolveFlags)
        {
            Object = obj;
            ResolveFlags = resolveFlags;

            if (!unresolvedRef.HasTypeArguments)
            {
                // If the reference doesn't have type arguments, but the matched object is a generic method,
                // then setup an inferred type argument list with OpenTypeParameterRef references to the declared
                // type parameters, which will then be replaced with actual types as they can be inferred.
                // If we're dealing with an explicit interface implementation, then use resolved TypeParameterRefs
                // instead of OpenTypeParameterRefs to the type parameters in the explicit interface method declaration.
                bool isExplicitInterfaceImplementation = unresolvedRef.IsExplicitInterfaceImplementation;
                if (obj is GenericMethodDecl)
                {
                    if (isExplicitInterfaceImplementation && unresolvedRef.Parent.Parent is GenericMethodDecl)
                        CopyTypeParameters((GenericMethodDecl)unresolvedRef.Parent.Parent, true);
                    else
                        CopyTypeParameters((GenericMethodDecl)obj, isExplicitInterfaceImplementation);
                }
                else if (obj is MethodDefinition && ((MethodDefinition)obj).HasGenericParameters)
                {
                    if (isExplicitInterfaceImplementation && unresolvedRef.Parent.Parent is GenericMethodDecl)
                        CopyTypeParameters((GenericMethodDecl)unresolvedRef.Parent.Parent, true);
                    else
                    {
                        Collection<GenericParameter> typeParameters = ((MethodDefinition)obj).GenericParameters;
                        InferredTypeArguments = new TypeRefBase[typeParameters.Count];
                        for (int i = 0; i < typeParameters.Count; ++i)
                            InferredTypeArguments[i] = (isExplicitInterfaceImplementation ? new TypeParameterRef(typeParameters[i]) : new OpenTypeParameterRef(typeParameters[i]));
                    }
                }
                else if (obj is MethodInfo && ((MethodInfo)obj).IsGenericMethod)
                {
                    if (isExplicitInterfaceImplementation && unresolvedRef.Parent.Parent is GenericMethodDecl)
                        CopyTypeParameters((GenericMethodDecl)unresolvedRef.Parent.Parent, true);
                    else
                    {
                        Type[] typeParameters = ((MethodInfo)obj).GetGenericArguments();
                        InferredTypeArguments = new TypeRefBase[typeParameters.Length];
                        for (int i = 0; i < typeParameters.Length; ++i)
                            InferredTypeArguments[i] = (isExplicitInterfaceImplementation ? new TypeParameterRef(typeParameters[i]) : new OpenTypeParameterRef(typeParameters[i]));
                    }
                }
                if (InferredTypeArguments != null)
                {
                    IsTypeArgumentFixed = new bool[InferredTypeArguments.Length];

                    // Mark the inferred types as already fixed if we're dealing with an explicit interface implementation
                    if (isExplicitInterfaceImplementation)
                        FixAllTypeArguments();
                }
            }
            else
            {
                // If the reference does have type arguments, and it's in a DocCodeRefBase, then "fake" inferred type arguments using the type
                // parameters from the declaration, if they match (in count and name).
                if (resolveFlags.HasFlag(ResolveFlags.InDocCodeRef) && unresolvedRef.IsDocCodeReference)
                {
                    IsDocCodeReference = true;
                    ChildList<Expression> typeArguments = unresolvedRef.TypeArguments;
                    if (obj is ITypeParameters)
                    {
                        ChildList<TypeParameter> typeParameters = ((ITypeParameters)obj).TypeParameters;
                        if (typeParameters != null && typeParameters.Count == typeArguments.Count && typeArguments[0] != null)
                        {
                            InferredTypeArguments = new TypeRefBase[typeParameters.Count];
                            for (int i = 0; i < typeParameters.Count; ++i)
                            {
                                Expression typeArgument = typeArguments[i];
                                if (typeArgument != null)
                                {
                                    InferredTypeArguments[i] = (typeArgument is UnresolvedRef && ((UnresolvedRef)typeArgument).Name == typeParameters[i].Name
                                        ? new TypeParameterRef(typeParameters[i]) : (TypeRefBase)typeArgument.Clone());
                                }
                            }
                        }
                    }
                    else if (obj is MethodDefinition)
                    {
                        if (((MethodDefinition)obj).HasGenericParameters)
                            CopyTypeParameters(((MethodDefinition)obj).GenericParameters, typeArguments);
                    }
                    else if (obj is TypeDefinition)
                    {
                        if (((TypeDefinition)obj).HasGenericParameters)
                            CopyTypeParameters(((TypeDefinition)obj).GenericParameters, typeArguments);
                    }
                    else if (obj is MethodInfo)
                    {
                        if (((MethodInfo)obj).IsGenericMethod)
                            CopyTypeParameters(((MethodInfo)obj).GetGenericArguments(), typeArguments);
                    }
                    else if (obj is Type)
                    {
                        if (((Type)obj).IsGenericType)
                            CopyTypeParameters(((Type)obj).GetGenericArguments(), typeArguments);
                    }
                    if (InferredTypeArguments != null)
                    {
                        IsTypeArgumentFixed = new bool[InferredTypeArguments.Length];
                        FixAllTypeArguments();
                    }
                }
            }
        }

        private void CopyTypeParameters(ITypeParameters iTypeParameters, bool isExplicitInterfaceImplementation)
        {
            ChildList<TypeParameter> typeParameters = iTypeParameters.TypeParameters;
            InferredTypeArguments = new TypeRefBase[typeParameters.Count];
            for (int i = 0; i < typeParameters.Count; ++i)
                InferredTypeArguments[i] = (isExplicitInterfaceImplementation ? new TypeParameterRef(typeParameters[i]) : new OpenTypeParameterRef(typeParameters[i]));
        }

        private void CopyTypeParameters(Collection<GenericParameter> typeParameters, ChildList<Expression> typeArguments)
        {
            if (typeParameters.Count == typeArguments.Count && typeArguments[0] != null)
            {
                InferredTypeArguments = new TypeRefBase[typeParameters.Count];
                for (int i = 0; i < typeParameters.Count; ++i)
                {
                    TypeRefBase typeArgument = typeArguments[i].EvaluateType();
                    if (typeArgument != null)
                    {
                        TypeRefBase inferredTypeArgument;
                        if (typeArgument is UnresolvedRef && typeArgument.Name == typeParameters[i].Name)
                        {
                            inferredTypeArgument = new TypeParameterRef(typeParameters[i]);
                            inferredTypeArgument.SetLineCol(typeArgument);
                        }
                        else
                            inferredTypeArgument = (TypeRefBase)typeArgument.Clone();
                        InferredTypeArguments[i] = inferredTypeArgument;
                    }
                }
            }
        }

        private void CopyTypeParameters(Type[] typeParameters, ChildList<Expression> typeArguments)
        {
            if (typeParameters.Length == typeArguments.Count && typeArguments[0] != null)
            {
                InferredTypeArguments = new TypeRefBase[typeParameters.Length];
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    TypeRefBase typeArgument = typeArguments[i].EvaluateType();
                    if (typeArgument != null)
                    {
                        TypeRefBase inferredTypeArgument;
                        if (typeArgument is UnresolvedRef && typeArgument.Name == typeParameters[i].Name)
                        {
                            inferredTypeArgument = new TypeParameterRef(typeParameters[i]);
                            inferredTypeArgument.SetLineCol(typeArgument);
                        }
                        else
                            inferredTypeArgument = (TypeRefBase)typeArgument.Clone();
                        InferredTypeArguments[i] = inferredTypeArgument;
                    }
                }
            }
        }

        private void FixAllTypeArguments()
        {
            for (int i = 0; i < IsTypeArgumentFixed.Length; ++i)
                IsTypeArgumentFixed[i] = true;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if the object is a method (does NOT include constructors or destructors).
        /// </summary>
        public bool IsMethod
        {
            get { return (Object is MethodDecl || (Object is MethodDefinition && !((MethodDefinition)Object).IsConstructor) || Object is MethodInfo); }
        }

        /// <summary>
        /// True if the candidate is a complete match.
        /// </summary>
        public bool IsCompleteMatch
        {
            get { return (IsCategoryMatch && IsTypeArgumentsMatch && !IsTypeInferenceFailure && IsMethodArgumentsMatch && IsStaticModeMatch && IsAccessible); }
        }

        #endregion

        #region /* METHODS */

        #region /* TYPE INFERENCE */

        /// <summary>
        /// Perform Type Inference for omitted type arguments on a generic method invocation.
        /// </summary>
        public bool InferTypeArguments(ICollection parameters, List<Expression> arguments, UnresolvedRef unresolvedRef)
        {
            // If we got this far, we know we have at least one parameter (which is necessary in order to infer type arguments).

            // The spec says "If the supplied number of arguments is different than the number of parameters for the method,
            // inference immediately fails.", but this is WRONG - we need to handle 'params' parameters, where the number of
            // arguments is either greater than the number of parameters or one less than the number of parameters!
            int argumentCount = arguments.Count;
            if (argumentCount > parameters.Count)
            {
                // If the last parameter is a 'params', then allow more arguments than parameters, but truncate the count so
                // that the extra arguments are ignored.  In this case, the last argument must match the array element type.
                if (ParameterRef.ParameterIsParams(parameters, parameters.Count - 1) && argumentCount > parameters.Count)
                    argumentCount = parameters.Count;
                else
                    return false;
            }
            else if (argumentCount < parameters.Count)
            {
                // Allow fewer arguments than parameters if the excess parameter is a 'params'.
                if (!ParameterRef.ParameterIsParams(parameters, argumentCount))
                    return false;
            }

            // If no unfixed type parameters exist then type inference succeeds (this can occur for explicit interface implementations,
            // which start out with inferred types that are already fixed).
            if (Enumerable.All(IsTypeArgumentFixed, delegate(bool isFixed) { return isFixed; }))
                return true;

            // Determine each parameter type P and any ref/out/params state, including evaluation of any type arguments
            TypeRefBase[] Parray = new TypeRefBase[argumentCount];
            bool[] PisRefOrOut = new bool[argumentCount];
            bool[] PisParams = new bool[argumentCount];
            GetParameterTypes(parameters, argumentCount, unresolvedRef, Parray, PisRefOrOut, PisParams);

            // If a 'params' parameter is being used in expanded form, convert it to a non-array non-params.
            // We know it has to be expanded form if arguments were truncated above, or if the type of the
            // argument isn't an array or it IS an array but it has fewer array ranks than the argument.
            if (PisParams[argumentCount - 1])
            {
                bool isExpanded = (argumentCount < arguments.Count);
                if (!isExpanded)
                {
                    TypeRefBase lastArgumentType = arguments[argumentCount - 1].EvaluateType();
                    if (lastArgumentType != null && (!lastArgumentType.IsArray || lastArgumentType.ArrayRanks.Count < Parray[argumentCount - 1].ArrayRanks.Count))
                        isExpanded = true;
                }
                if (isExpanded)
                {
                    Parray[argumentCount - 1] = Parray[argumentCount - 1].GetElementType();
                    PisParams[argumentCount - 1] = false;
                }
            }

            // Keep original type arguments around
            TypeRefBase[] originalTypeArguments = (TypeRefBase[])InferredTypeArguments.Clone();

            // For each of the method argument expressions E:
            for (int i = 0; i < argumentCount; ++i)
            {
                // If E is a null literal, an anonymous function, or a method group, nothing is inferred from the argument
                Expression E = arguments[i];
                if ((E is Literal && ((Literal)E).IsNull) || E is AnonymousMethod || (E is UnresolvedRef && ((UnresolvedRef)E).IsMethodGroup))
                    continue;

                // Evaluate the type of the argument expression as A, and get the parameter type as P
                TypeRefBase A = E.EvaluateType();
                TypeRefBase P = Parray[i];

                // The following steps are repeated as long as one is true:
                while (true)
                {
                    // - If P is an array type and A is an array type with the same rank, then replace A and P, respectively, with the element types of A and P.
                    // - If P is a type constructed from IEnumerable<T>, ICollection<T>, or IList<T> (all in the System.Collections.Generic namespace)
                    //   and A is a single-dimensional array type, then replace A and P, respectively, with the element types of A and P.
                    if (A.IsArray)
                    {
                        if (P.IsArray && P.ArrayRanks[0] == A.ArrayRanks[0])
                        {
                            P = P.GetElementType();
                            A = A.GetElementType();
                            continue;
                        }
                        if (A.ArrayRanks.Count == 1 && A.ArrayRanks[0] == 1 && P.HasTypeArguments
                            && (P.IsSameGenericType(TypeRef.IEnumerable1Ref) || P.IsSameGenericType(TypeRef.ICollection1Ref) || P.IsSameGenericType(TypeRef.IList1Ref)))
                        {
                            P = P.TypeArguments[0].EvaluateType();
                            A = A.GetElementType();
                            continue;
                        }
                    }
                    break;
                }

                // - If P is an array type (meaning that the previous step failed to relate A and P), then type inference fails for the generic method.
                if (P.IsArray)
                    return false;

                // - If P is a method type parameter, then type inference succeeds for this argument, and A is the type inferred for that type parameter.
                bool argumentSuccess = false;
                if (!InferTypeArgument(P, A, originalTypeArguments, ref argumentSuccess))
                    return false;
                if (argumentSuccess)
                    continue;

                // - Otherwise, P must be a constructed type. If, for each method type parameter MX that occurs in P, exactly one type TX can be determined
                // such that replacing each MX with each TX produces a type to which A is convertible by a standard implicit conversion, then inferencing
                // succeeds for this argument, and each TX is the type inferred for each MX. Method type parameter constraints, if any, are ignored for the
                // purpose of type inference. If, for a given MX, no TX exists or more than one TX exists, then type inference fails for the generic method
                // (a situation where more than one TX exists can only occur if P is a generic interface type and A implements multiple constructed versions
                // of that interface).
                if (P.HasTypeArguments)
                {
                    // Make a copy of P with the new TX type arguments so we can see if an implicit conversion is possible.
                    TypeRefBase P2 = (TypeRefBase)P.Clone();
                    bool foundTX = false;

                    // It's not obvious how to go about determining the TX types - so, we'll look for various specific cases.
                    // If A has the same number or more type arguments, try direct substitution (handles IEnumerable<T> <- List<Type>).
                    if (A.TypeArgumentCount >= P.TypeArgumentCount)
                    {
                        for (int j = 0; j < P2.TypeArguments.Count; ++j)
                            P2.TypeArguments[j] = (Expression)A.TypeArguments[j].Clone();
                        foundTX = true;
                    }
                    else if (P.TypeArgumentCount == 1)
                    {
                        // If P has a single type argument, and A is enumerable, try its element type (handles IEnumerable<T> <- MyCollection).
                        // Use the argument expression (E) so that any type parameters can be evaluated.
                        TypeRefBase elementTypeRef = ForEach.GetCollectionExpressionElementType(E);
                        if (elementTypeRef != null)
                        {
                            P2.TypeArguments[0] = elementTypeRef;
                            foundTX = true;
                        }
                    }

                    // If A is now implicitly convertible to P2, make an inference for each type argument
                    if (foundTX && A.IsImplicitlyConvertibleTo(P2, true))
                    {
                        for (int j = 0; j < P.TypeArgumentCount; ++j)
                        {
                            bool success = false;
                            if (!InferTypeArgument(P.TypeArguments[j].EvaluateType(), P2.TypeArguments[j].EvaluateType(), originalTypeArguments, ref success))
                                return false;
                        }
                    }
                }
            }

            // If no unfixed type parameters exist then type inference succeeds.
            return (Enumerable.All(IsTypeArgumentFixed, delegate(bool isFixed) { return isFixed; }));
        }

        protected bool InferTypeArgument(TypeRefBase P, TypeRefBase A, TypeRefBase[] originalTypeArguments, ref bool success)
        {
            for (int i = 0; i < InferredTypeArguments.Length; ++i)
            {
                // If P is an original type argument, infer A for the type argument, or if it's already been inferred,
                // type inference fails if A isn't implicitly convertiable to the previously inferred type.
                if (originalTypeArguments[i].IsSameRef(P))
                {
                    if (!IsTypeArgumentFixed[i])
                    {
                        InferredTypeArguments[i] = A;
                        IsTypeArgumentFixed[i] = success = true;
                        return true;
                    }
                    if (!A.IsImplicitlyConvertibleTo(InferredTypeArguments[i]))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the types of the specified collection of parameters, and also the ref/out and params status of each.
        /// </summary>
        protected void GetParameterTypes(ICollection parameters, int parameterCount, UnresolvedRef unresolvedRef, TypeRefBase[] T, bool[] TisRefOrOut, bool[] TisParams)
        {
            // To resolve any type arguments in the parameter types, a temporary MethodRef with the inferred type arguments
            // will be created to be used as the parent expression.  This method is only called during type inference, so we
            // know the UnresolvedRef represents a MethodRef with inferred type arguments.

            MethodRef parentMethodRef = (MethodRef)unresolvedRef.CreateRef(Object, false);
            parentMethodRef.Parent = unresolvedRef.Parent;
            CopyInferredTypeArguments(parentMethodRef);

            if (parameters is List<ParameterDecl>)
            {
                for (int i = 0; i < parameterCount; ++i)
                {
                    ParameterDecl parameterDecl = ((List<ParameterDecl>)parameters)[i];
                    TisParams[i] = parameterDecl.IsParams;
                    TisRefOrOut[i] = (parameterDecl.IsRef || parameterDecl.IsOut);
                    Expression typeExpression = parameterDecl.Type;
                    if (typeExpression != null)
                    {
                        TypeRefBase typeRefBase = typeExpression.EvaluateType();
                        if (typeRefBase != null)
                            T[i] = typeRefBase.EvaluateTypeArgumentTypes(parentMethodRef, parentMethodRef);
                    }
                }
            }
            else if (parameters is Collection<ParameterDefinition>)
            {
                for (int i = 0; i < parameterCount; ++i)
                {
                    ParameterDefinition parameterReference = ((Collection<ParameterDefinition>)parameters)[i];
                    TypeReference parameterType = parameterReference.ParameterType;
                    TisParams[i] = ParameterDefinitionUtil.IsParams(parameterReference);
                    TisRefOrOut[i] = parameterType.IsByReference;
                    // Dereference (remove the trailing '&') if it's a reference type
                    if (TisRefOrOut[i])
                        parameterType = ((ByReferenceType)parameterType).ElementType;
                    TypeRefBase typeRef = TypeRef.Create(parameterType);
                    T[i] = typeRef.EvaluateTypeArgumentTypes(parentMethodRef, parentMethodRef);
                }
            }
            else //if (parameters is ParameterInfo[])
            {
                for (int i = 0; i < parameterCount; ++i)
                {
                    ParameterInfo parameterInfo = ((ParameterInfo[])parameters)[i];
                    Type parameterType = parameterInfo.ParameterType;
                    TisParams[i] = ParameterInfoUtil.IsParams(parameterInfo);
                    TisRefOrOut[i] = parameterType.IsByRef;
                    // Dereference (remove the trailing '&') if it's a reference type
                    if (TisRefOrOut[i])
                        parameterType = parameterType.GetElementType();
                    TypeRef typeRef = TypeRef.Create(parameterType);
                    T[i] = typeRef.EvaluateTypeArgumentTypes(parentMethodRef, parentMethodRef);
                }
            }
        }

        #endregion

        /// <summary>
        /// Copy the inferred type arguments to the specified MethodRef or TypeRef (pseudo-inferred type arguments
        /// are used for references to type definitions in doc comments).
        /// </summary>
        public void CopyInferredTypeArguments(TypeRefBase typeRefBase)
        {
            // We must *clone* the type references while copying them, since we're adding them to the code object
            // tree, setting their parent references - we must not modify the existing type references.
            ChildList<Expression> typeArguments = new ChildList<Expression>(InferredTypeArguments.Length, typeRefBase);
            foreach (TypeRefBase inferredTypeRefBase in InferredTypeArguments)
                typeArguments.Add((TypeRefBase)inferredTypeRefBase.Clone());
            typeRefBase.TypeArguments = typeArguments;
            if (typeRefBase is MethodRef && !IsDocCodeReference)
                ((MethodRef)typeRefBase).HasInferredTypeArguments = true;
        }

        /// <summary>
        /// Determine if a mismatch is due to unresolved references only (as far as we can tell).
        /// </summary>
        public bool IsMismatchDueToUnresolvedOnly()
        {
            return (ArgumentsMismatchDueToUnresolvedOnly && IsTypeArgumentsMatch && !IsTypeInferenceFailure && IsStaticModeMatch && IsAccessible);
        }

        /// <summary>
        /// Determine if a mismatch is due to inaccessibility only.
        /// </summary>
        public bool IsMismatchDueToAccessibilityOnly()
        {
            return (!IsAccessible && IsCategoryMatch && IsMethodArgumentsMatch && IsTypeArgumentsMatch && !IsTypeInferenceFailure && IsStaticModeMatch);
        }

        /// <summary>
        /// Get a description of why the candidate doesn't match.
        /// </summary>
        public string GetMismatchDescription()
        {
            string result;
            bool isPlural = false;
            if (!IsCategoryMatch)
                result = "the category";
            else
            {
                result = "";
                if (!IsTypeArgumentsMatch)
                {
                    result += "the type arguments";
                    isPlural = true;
                }
                if (!IsMethodArgumentsMatch)
                {
                    result += (result.Length > 0 ? " and " : "") + "the method arguments";
                    isPlural = true;
                }
                if (!IsStaticModeMatch)
                    result += (result.Length > 0 ? " and " : "") + "the static mode";
            }
            if (!string.IsNullOrEmpty(result))
                result += (isPlural ? " don't" : " doesn't") + " match";
            if (IsTypeInferenceFailure)
                result += (result.Length > 0 ? " and " : "") + "type inference failed";
            if (!IsAccessible)
                result += (result.Length > 0 ? " and " : "") + "it isn't accessible";
            return result;
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// Render as a string (mainly for display in the debugger).
        /// </summary>
        public override string ToString()
        {
            return (IsCompleteMatch ? "Complete: " : "Partial: ") + Object;
        }

        #endregion
    }
}
