// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Collections.Generic;

using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="ParameterDecl"/> or a <see cref="ParameterDefinition"/>/<see cref="ParameterInfo"/>.
    /// Similar to a <see cref="LocalRef"/>, but represents a parameter passed to the current method.
    /// </summary>
    /// <remarks>
    /// Although references to <see cref="ParameterDefinition"/>s/<see cref="ParameterInfo"/>s aren't common, they might occur in
    /// some special circumstances.
    /// </remarks>
    public class ParameterRef : VariableRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDecl parameterDecl, bool isFirstOnLine)
            : base(parameterDecl, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDecl parameterDecl)
            : base(parameterDecl, false)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDefinition parameterDefinition, bool isFirstOnLine)
            : base(parameterDefinition, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDefinition parameterDefinition)
            : base(parameterDefinition, false)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterInfo parameterInfo, bool isFirstOnLine)
            : base(parameterInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterInfo parameterInfo)
            : base(parameterInfo, false)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).Name;
                if (_reference is ParameterDefinition)
                    return ((ParameterDefinition)_reference).Name;
                return ((ParameterInfo)_reference).Name;
            }
        }

        /// <summary>
        /// True if the referenced parameter is a 'params' parameter.
        /// </summary>
        public bool IsParams
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsParams;
                if (_reference is ParameterDefinition)
                    return ParameterDefinitionUtil.IsParams((ParameterDefinition)_reference);
                return ParameterInfoUtil.IsParams((ParameterInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced parameter is a 'ref' parameter.
        /// </summary>
        public bool IsRef
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsRef;
                if (_reference is ParameterDefinition)
                    return ParameterDefinitionUtil.IsRef((ParameterDefinition)_reference);
                return ParameterInfoUtil.IsRef((ParameterInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced parameter is an 'out' parameter.
        /// </summary>
        public bool IsOut
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsOut;
                if (_reference is ParameterDefinition)
                    return ParameterDefinitionUtil.IsOut((ParameterDefinition)_reference);
                return ParameterInfoUtil.IsOut((ParameterInfo)_reference);
            }
        }

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Find the parameter on the specified <see cref="MethodDeclBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDeclBase methodDeclBase, string name, bool isFirstOnLine)
        {
            if (methodDeclBase != null)
            {
                ParameterRef parameterRef = methodDeclBase.GetParameter(name);
                if (parameterRef != null)
                {
                    parameterRef.IsFirstOnLine = isFirstOnLine;
                    return parameterRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="MethodDeclBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDeclBase methodDeclBase, string name)
        {
            return Find(methodDeclBase, name, false);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodDefinition"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDefinition methodDefinition, string name, bool isFirstOnLine)
        {
            if (methodDefinition != null)
            {
                ParameterDefinition parameterDefinition = MethodDefinitionUtil.GetParameter(methodDefinition, name);
                if (parameterDefinition != null)
                    return new ParameterRef(parameterDefinition, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodDefinition"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDefinition methodDefinition, string name)
        {
            return Find(methodDefinition, name, false);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodInfo"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodInfo methodInfo, string name, bool isFirstOnLine)
        {
            if (methodInfo != null)
            {
                ParameterInfo parameterInfo = MethodInfoUtil.GetParameter(methodInfo, name);
                if (parameterInfo != null)
                    return new ParameterRef(parameterInfo, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodInfo"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodInfo methodInfo, string name)
        {
            return Find(methodInfo, name, false);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine)
        {
            if (typeRefBase is MethodRef)
            {
                ParameterRef parameterRef = ((MethodRef)typeRefBase).GetParameter(name);
                if (parameterRef != null)
                {
                    parameterRef.IsFirstOnLine = isFirstOnLine;
                    return parameterRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        /// <summary>
        /// Get the <see cref="ParameterModifier"/> for the specified <see cref="ParameterDefinition"/>.
        /// </summary>
        public static ParameterModifier GetParameterModifier(ParameterDefinition parameterDefinition)
        {
            ParameterModifier modifier = ParameterModifier.None;
            if (ParameterDefinitionUtil.IsParams(parameterDefinition))
                modifier = ParameterModifier.Params;
            else if (ParameterDefinitionUtil.IsRef(parameterDefinition))
                modifier = ParameterModifier.Ref;
            else if (ParameterDefinitionUtil.IsOut(parameterDefinition))
                modifier = ParameterModifier.Out;
            return modifier;
        }

        /// <summary>
        /// Get the <see cref="ParameterModifier"/> for the specified <see cref="ParameterInfo"/>.
        /// </summary>
        public static ParameterModifier GetParameterModifier(ParameterInfo parameterInfo)
        {
            ParameterModifier modifier = ParameterModifier.None;
            if (ParameterInfoUtil.IsParams(parameterInfo))
                modifier = ParameterModifier.Params;
            else if (ParameterInfoUtil.IsRef(parameterInfo))
                modifier = ParameterModifier.Ref;
            else if (ParameterInfoUtil.IsOut(parameterInfo))
                modifier = ParameterModifier.Out;
            return modifier;
        }

        /// <summary>
        /// Determine if the parameter in the collection with the specified index is a 'params' parameter.
        /// </summary>
        public static bool ParameterIsParams(ICollection parameters, int index)
        {
            bool isParams;
            if (parameters is List<ParameterDecl>)
                isParams = (((List<ParameterDecl>)parameters)[index].IsParams);
            else if (parameters is Collection<ParameterDefinition>)
                isParams = ParameterDefinitionUtil.IsParams(((Collection<ParameterDefinition>)parameters)[index]);
            else //if (parameters is ParameterInfo[])
                isParams = ParameterInfoUtil.IsParams(((ParameterInfo[])parameters)[index]);
            return isParams;
        }

        /// <summary>
        /// Get the type of the parameter in the collection with the specified index, using the specified parent expression to evaluate any type argument types.
        /// </summary>
        public static TypeRefBase GetParameterType(ICollection parameters, int index, Expression parentExpression)
        {
            TypeRefBase parameterTypeRef;
            if (parameters is List<ParameterDecl>)
                parameterTypeRef = ((List<ParameterDecl>)parameters)[index].EvaluateType();
            else if (parameters is Collection<ParameterDefinition>)
                parameterTypeRef = TypeRef.Create(((Collection<ParameterDefinition>)parameters)[index].ParameterType);
            else //if (parameters is ParameterInfo[])
                parameterTypeRef = TypeRef.Create(((ParameterInfo[])parameters)[index].ParameterType);
            if (parameterTypeRef != null)
                parameterTypeRef = parameterTypeRef.EvaluateTypeArgumentTypes(parentExpression);
            return parameterTypeRef;
        }

        /// <summary>
        /// Get the type of the parameter in the collection with the specified index, also returning any 'ref' or 'out' status.
        /// </summary>
        public static TypeRefBase GetParameterType(ICollection parameters, int index, out bool isRef, out bool isOut, Expression parentExpression)
        {
            TypeRefBase parameterTypeRef;
            if (index >= parameters.Count)
            {
                parameterTypeRef = null;
                isRef = isOut = false;
            }
            else
            {
                // Get the reference to the type of the parameter
                if (parameters is List<ParameterDecl>)
                {
                    ParameterDecl parameterDecl = ((List<ParameterDecl>)parameters)[index];
                    parameterTypeRef = parameterDecl.EvaluateType();
                    isRef = parameterDecl.IsRef;
                    isOut = parameterDecl.IsOut;
                }
                else if (parameters is Collection<ParameterDefinition>)
                {
                    ParameterDefinition parameterDefinition = ((Collection<ParameterDefinition>)parameters)[index];
                    parameterTypeRef = TypeRef.Create(parameterDefinition.ParameterType);
                    isOut = ParameterDefinitionUtil.IsOut(parameterDefinition);
                    isRef = ParameterDefinitionUtil.IsRef(parameterDefinition);
                }
                else //if (parameters is ParameterInfo[])
                {
                    ParameterInfo parameterInfo = ((ParameterInfo[])parameters)[index];
                    parameterTypeRef = TypeRef.Create(parameterInfo.ParameterType);
                    isOut = ParameterInfoUtil.IsOut(parameterInfo);
                    isRef = ParameterInfoUtil.IsRef(parameterInfo);
                }
                if (parameterTypeRef != null)
                    parameterTypeRef = parameterTypeRef.EvaluateTypeArgumentTypes(parentExpression);
            }
            return parameterTypeRef;
        }

        /// <summary>
        /// Get the type of the parameter in the collection with the specified index, also returning any 'ref' or 'out' status.
        /// </summary>
        public static TypeRefBase GetParameterType(ICollection parameters, int index, out bool isRef, out bool isOut)
        {
            return GetParameterType(parameters, index, out isRef, out isOut, null);
        }

        #endregion

        #region /* METHODS */

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            TypeRefBase typeRefBase;
            if (_reference is ParameterDecl)
                typeRefBase = ((ParameterDecl)_reference).EvaluateType(withoutConstants);
            else if (_reference is ParameterDefinition)
            {
                ParameterDefinition parameterDefinition = (ParameterDefinition)_reference;
                TypeReference parameterType = parameterDefinition.ParameterType;
                typeRefBase = TypeRef.Create(parameterType);
            }
            else //if (_reference is ParameterInfo)
            {
                ParameterInfo parameterInfo = (ParameterInfo)_reference;
                Type parameterType = parameterInfo.ParameterType;
                typeRefBase = TypeRef.Create(parameterType);
            }

            // We shouldn't need to evaluate type arguments here.
            // If it turns out that we do, there's no need to check for a Dot parent, and we can pass
            // null for the parent expression, which will then only look for the parent TypeDecl.
            //if (typeRefBase != null)
            //    typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(null); //Parent);

            return typeRefBase;
        }

        /// <summary>
        /// Check if the provided argument expression-type is compatible with the type of the specified parameter.
        /// </summary>
        /// <param name="reference">The parent UnresolvedRef or MethodRef being evaluated.</param>
        /// <param name="obj">The CodeObject or MemberInfo having its parameter matched.</param>
        /// <param name="candidate">The associated MatchCandidate object (if any).</param>
        /// <param name="parameters">The entire collection of parameters for the current object.</param>
        /// <param name="parameterIndex">The index of the current parameter being checked (-1 if matching/excluding an empty parameter list).</param>
        /// <param name="argumentCount">The total number of arguments being matched to the parameters.</param>
        /// <param name="argumentTypeRef">The evaluated type of the current argument.</param>
        /// <param name="paramsTypeRef">The type of a 'params' parameter that is being expanded (if any).</param>
        /// <param name="expandParams">True if expanding 'params' parameters is allowed.</param>
        /// <param name="parentExpression">The parent expression (if any) used to track down generic type parameter types.</param>
        /// <param name="failedBecauseUnresolved">True if the match failed because of an unresolved or undetermined type reference, otherwise false.</param>
        /// <returns>True if the parameter matches, otherwise false.</returns>
        public static bool MatchParameter(TypeRefBase reference, object obj, MatchCandidate candidate,
            ICollection parameters, int parameterIndex, int argumentCount, TypeRefBase argumentTypeRef, ref TypeRefBase paramsTypeRef,
            bool expandParams, Expression parentExpression, out bool failedBecauseUnresolved)
        {
            // Abort if the argument type is undetermined
            if (argumentTypeRef == null)
            {
                failedBecauseUnresolved = true;
                return false;
            }

            bool matches = false;
            int parameterCount = (parameters != null ? parameters.Count : 0);
            failedBecauseUnresolved = false;

            if (paramsTypeRef != null)
            {
                // Special handling for 'params' mode
                matches = argumentTypeRef.IsImplicitlyConvertibleTo(paramsTypeRef);
                if (!matches)
                    failedBecauseUnresolved = (paramsTypeRef.HasUnresolvedRef() || argumentTypeRef.HasUnresolvedRef());
            }
            else if (parameterIndex < parameterCount)
            {
                // Determine the type of the specified parameter
                TypeRefBase parameterTypeRef;
                bool isRefOrOut;
                bool isParams;

                // Handle a list of ParameterDecls
                if (parameters is List<ParameterDecl>)
                {
                    // Get the reference to the type of the parameter
                    ParameterDecl parameterDecl = ((List<ParameterDecl>)parameters)[parameterIndex];
                    isParams = parameterDecl.IsParams;
                    isRefOrOut = (parameterDecl.IsRef || parameterDecl.IsOut);
                    TypeRefBase parameterTypeRefBase = parameterDecl.EvaluateType();

                    // Evaluate the type of the parameter, handling generic type parameters (including nested ones)
                    parameterTypeRef = EvaluateParameter(reference, obj, candidate, parameterTypeRefBase, parentExpression);
                }
                // Handle a list of ParameterDefinitions
                else if (parameters is Collection<ParameterDefinition>)
                {
                    // Get the type of the parameter
                    ParameterDefinition parameterDefinition = ((Collection<ParameterDefinition>)parameters)[parameterIndex];
                    isParams = ParameterDefinitionUtil.IsParams(parameterDefinition);
                    TypeReference parameterType = parameterDefinition.ParameterType;
                    isRefOrOut = parameterType.IsByReference;
                    // Dereference (remove the trailing '&') if it's a reference type
                    if (isRefOrOut)
                        parameterType = ((ByReferenceType)parameterType).ElementType;

                    // Evaluate the type of the parameter, handling generic type parameters (including nested ones).
                    // This was using a customized/optimized routine to handle a Type parameter instead of a TypeRef,
                    // and with 'obj' being a MemberDefinition and not a CodeObject.  However, it's possible with delegates
                    // to have the 'obj' be a CodeObject even though the parameter list is a Collection<ParameterDefinition>, so the
                    // routines have been merged and we must convert 'parameterType' to a TypeRef.
                    parameterTypeRef = EvaluateParameter(reference, obj, candidate, TypeRef.Create(parameterType), parentExpression);
                }
                // Handle a list of ParameterInfos
                else //if (parameters is ParameterInfo[])
                {
                    // Get the type of the parameter
                    ParameterInfo parameterInfo = ((ParameterInfo[])parameters)[parameterIndex];
                    isParams = ParameterInfoUtil.IsParams(parameterInfo);
                    Type parameterType = parameterInfo.ParameterType;
                    isRefOrOut = parameterType.IsByRef;
                    // Dereference (remove the trailing '&') if it's a reference type
                    if (isRefOrOut)
                        parameterType = parameterType.GetElementType();

                    // Evaluate the type of the parameter, handling generic type parameters (including nested ones).
                    // This was using a customized/optimized routine to handle a Type parameter instead of a TypeRef,
                    // and with 'obj' being a MemberInfo and not a CodeObject.  However, it's possible with delegates
                    // to have the 'obj' be a CodeObject even though the parameter list is a ParameterInfo[], so the
                    // routines have been merged and we must convert 'parameterType' to a TypeRef.
                    parameterTypeRef = EvaluateParameter(reference, obj, candidate, TypeRef.Create(parameterType), parentExpression);
                }

                // If we were able to determine the parameter type, check if the argument type is compatible
                if (parameterTypeRef != null)
                {
                    // For ref or out parameters, the types must match exactly
                    if (isRefOrOut)
                        matches = argumentTypeRef.IsSameRef(parameterTypeRef);
                    else
                    {
                        // If this is a 'params' parameter, and expansion is enabled, and we're on the last
                        // parameter (not already expanded and past it), and we have more arguments than
                        // parameters, then we *must* match the expanded type, and activate expanded mode.
                        // Otherwise, we'll try to match the 'params' array type first.
                        if (isParams && expandParams && (parameterIndex == parameterCount - 1) && argumentCount > parameterCount)
                        {
                            parameterTypeRef = parameterTypeRef.GetElementType();
                            paramsTypeRef = parameterTypeRef;
                        }

                        // Check if the types are compatible
                        matches = argumentTypeRef.IsImplicitlyConvertibleTo(parameterTypeRef);
                    }

                    // If we failed to match, and we were matching a 'params' parameter (see above), and we
                    // don't have more arguments than parameters (meaning we haven't entered expanded mode yet),
                    // then try to match the expanded type.
                    if (!matches && isParams && expandParams && argumentCount == parameterCount)
                    {
                        parameterTypeRef = parameterTypeRef.GetElementType();
                        matches = argumentTypeRef.IsImplicitlyConvertibleTo(parameterTypeRef);
                        // We don't have to turn expanded mode on in this case, because we know we don't have
                        // any extra arguments.
                    }
                }

                // If the match failed, consider the cause to be an unresolved reference if the argument is unresolved
                if (!matches)
                    failedBecauseUnresolved = argumentTypeRef.HasUnresolvedRef();
            }
            return matches;
        }

        protected static TypeRefBase EvaluateParameter(TypeRefBase reference, object obj, MatchCandidate candidate,
            TypeRefBase parameterRef, Expression parentExpression)
        {
            TypeRefBase parameterTypeRef = null;

            // Evaluate a type parameter
            if (parameterRef is TypeParameterRef)
            {
                TypeParameterRef typeParameterRef = (TypeParameterRef)parameterRef;

                // The 'reference' can be an UnresolvedRef if we're trying to match an 'obj' code object to it, or it
                // can be a MethodRef if 'obj' is a delegate type that we're checking if it's implicitly convertible to.

                // Look for any type arguments based upon the current object
                if (obj is CodeObject)
                {
                    if (obj is GenericMethodDecl)
                    {
                        if (reference is UnresolvedRef)
                        {
                            GenericMethodDecl genericMethodDecl = (GenericMethodDecl)obj;
                            int index = genericMethodDecl.FindTypeParameterIndex(typeParameterRef.Reference as TypeParameter);
                            if (reference.TypeArgumentCount == 0)
                            {
                                // Handle inferred type arguments
                                if (index >= 0)
                                    parameterTypeRef = candidate.InferredTypeArguments[index];
                            }
                            else
                            {
                                if (index >= 0)
                                    parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(genericMethodDecl.Name, index);
                            }
                            if (reference.TypeArguments != null && reference.TypeArguments.Count > 0)
                            {
                                if (index >= 0)
                                    parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(genericMethodDecl.Name, index);
                            }
                        }
                        else
                            parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                    }
                    else if (obj is ConstructorDecl)
                    {
                        if (reference is UnresolvedRef)
                        {
                            string name = null;
                            int index = -1;
                            ConstructorDecl constructorDecl = (ConstructorDecl)obj;
                            TypeDecl declaringType = constructorDecl.DeclaringType;
                            if (declaringType != null)
                            {
                                name = declaringType.Name;
                                index = declaringType.FindTypeParameterIndex(typeParameterRef.Reference as TypeParameter);
                            }
                            else
                            {
                                // If we don't have a parent, assume we're a generated constructor for
                                // a delegate (used for the obsolete explicit delegate creation syntax), and
                                // use the type of the parameter as our type.
                                TypeRef typeRef = constructorDecl.Parameters[0].EvaluateType() as TypeRef;
                                if (typeRef != null)
                                {
                                    Type type = typeRef.Reference as Type;
                                    if (type != null)
                                    {
                                        name = TypeUtil.NonGenericName(type);
                                        index = TypeUtil.FindTypeParameterIndex(type, typeParameterRef.Reference as Type);
                                    }
                                }
                            }
                            if (index >= 0)
                                parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(name, index);
                        }
                        else
                            parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                    }
                    else if (obj is IVariableDecl)  // Field, Property, or Event of delegate type
                    {
                        Expression type = ((IVariableDecl)obj).Type;
                        if (type != null)
                            parameterTypeRef = type.FindTypeArgument(typeParameterRef);
                    }
                }
                else if (obj is IMemberDefinition)
                {
                    TypeReference typeArgument = null;
                    GenericParameter typeParameter = typeParameterRef.Reference as GenericParameter;
                    if (obj is MethodDefinition)
                    {
                        MethodDefinition methodDefinition = (MethodDefinition)obj;
                        if (methodDefinition.IsConstructor)
                        {
                            if (reference is UnresolvedRef)
                            {
                                if (typeParameter != null)
                                {
                                    TypeDefinition declaringType = methodDefinition.DeclaringType;
                                    int index = TypeDefinitionUtil.FindTypeParameterIndex(declaringType, typeParameter);
                                    if (index >= 0)
                                        parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(TypeDefinitionUtil.NonGenericName(declaringType), index);
                                }
                            }
                            else
                                parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                        }
                        else
                        {
                            if (methodDefinition.HasGenericParameters)
                            {
                                if (reference is UnresolvedRef)
                                {
                                    if (typeParameter != null)
                                    {
                                        if (reference.TypeArgumentCount == 0)
                                        {
                                            // Handle inferred type arguments
                                            int index = MethodDefinitionUtil.FindTypeParameterIndex(methodDefinition, typeParameter);
                                            if (index >= 0)
                                                parameterTypeRef = candidate.InferredTypeArguments[index];
                                        }
                                        else
                                        {
                                            int index = MethodDefinitionUtil.FindTypeParameterIndex(methodDefinition, typeParameter);
                                            if (index >= 0)
                                                parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(methodDefinition.Name, index);
                                        }
                                    }
                                }
                                else
                                    parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                            }
                        }
                    }
                    else
                    {
                        if (!(reference is UnresolvedRef))
                            parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                        if (parameterTypeRef == null)
                        {
                            TypeReference genericType = null;
                            if (obj is FieldDefinition)
                                genericType = ((FieldDefinition)obj).FieldType;
                            else if (obj is PropertyDefinition)
                                genericType = ((PropertyDefinition)obj).PropertyType;
                            else if (obj is EventDefinition)
                                genericType = ((EventDefinition)obj).EventType;
                            if (genericType != null && typeParameter != null)
                                typeArgument = TypeDefinitionUtil.FindTypeArgument(genericType, typeParameter);
                        }
                    }
                    // Check if we found a non-generic type argument
                    if (typeArgument != null && !typeArgument.IsGenericParameter)
                        parameterTypeRef = TypeRef.Create(typeArgument);
                }
                else //if (obj is MemberInfo)
                {
                    Type typeArgument = null;
                    Type typeParameter = typeParameterRef.Reference as Type;
                    if (obj is MethodInfo)
                    {
                        MethodInfo methodInfo = (MethodInfo)obj;
                        if (methodInfo.IsGenericMethod)
                        {
                            if (reference is UnresolvedRef)
                            {
                                if (typeParameter != null)
                                {
                                    if (reference.TypeArgumentCount == 0)
                                    {
                                        // Handle inferred type arguments
                                        int index = MethodInfoUtil.FindTypeParameterIndex(methodInfo, typeParameter);
                                        if (index >= 0)
                                            parameterTypeRef = candidate.InferredTypeArguments[index];
                                    }
                                    else
                                    {
                                        if (methodInfo.IsGenericMethodDefinition)
                                        {
                                            int index = MethodInfoUtil.FindTypeParameterIndex(methodInfo, typeParameter);
                                            if (index >= 0)
                                                parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(methodInfo.Name, index);
                                        }
                                        else
                                            typeArgument = MethodInfoUtil.FindTypeArgument(methodInfo, typeParameter);
                                    }
                                }
                            }
                            else
                                parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                        }
                    }
                    else if (obj is ConstructorInfo)
                    {
                        if (reference is UnresolvedRef)
                        {
                            if (typeParameter != null)
                            {
                                Type declaringType = ((ConstructorInfo)obj).DeclaringType;
                                int index = TypeUtil.FindTypeParameterIndex(declaringType, typeParameter);
                                if (index >= 0)
                                    parameterTypeRef = ((UnresolvedRef)reference).FindTypeArgument(TypeUtil.NonGenericName(declaringType), index);
                            }
                        }
                        else
                            parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                    }
                    else
                    {
                        if (!(reference is UnresolvedRef))
                            parameterTypeRef = reference.FindTypeArgument(typeParameterRef);
                        if (parameterTypeRef == null)
                        {
                            Type genericType = null;
                            if (obj is FieldInfo)
                                genericType = ((FieldInfo)obj).FieldType;
                            else if (obj is PropertyInfo)
                                genericType = ((PropertyInfo)obj).PropertyType;
                            else if (obj is EventInfo)
                                genericType = ((EventInfo)obj).EventHandlerType;
                            if (genericType != null && typeParameter != null)
                                typeArgument = TypeUtil.FindTypeArgument(genericType, typeParameter);
                        }
                    }
                    // Check if we found a non-generic type argument
                    if (typeArgument != null && !typeArgument.IsGenericParameter)
                        parameterTypeRef = TypeRef.Create(typeArgument);
                }

                // If we didn't resolve it yet, try a couple more options
                if (parameterTypeRef == null)
                {
                    // Evaluate it using the parent expression if we have one
                    if (parentExpression != null)
                        parameterTypeRef = typeParameterRef.EvaluateTypeArgumentTypes(parentExpression, reference);
                    else
                    {
                        // Otherwise, look at the parent TypeDecl base classes, and finally just default to the type parameter itself
                        TypeDecl parentTypeDecl = reference.FindParent<TypeDecl>();
                        if (parentTypeDecl != null)
                        {
                            parameterTypeRef = parentTypeDecl.FindTypeArgumentInBase(typeParameterRef);

                            // Convert the result to an array type if necessary
                            if (parameterTypeRef != null && typeParameterRef.HasArrayRanks)
                                parameterTypeRef = parameterTypeRef.MakeArrayRef(typeParameterRef.ArrayRanks);
                        }
                        if (parameterTypeRef == null)
                            parameterTypeRef = typeParameterRef;
                    }
                }
                else
                {
                    // Convert the result to an array type if necessary
                    if (typeParameterRef.HasArrayRanks)
                        parameterTypeRef = parameterTypeRef.MakeArrayRef(typeParameterRef.ArrayRanks);
                }
            }
            else if (parameterRef != null && parameterRef.HasTypeArguments)
            {
                // If the type has type arguments, then recursively evaluate them
                parameterTypeRef = EvaluateTypeArguments(reference, obj, candidate, parameterRef, parentExpression);
            }
            else
            {
                // If there aren't any type arguments, just return the parameter type as-is
                parameterTypeRef = parameterRef;
            }

            return parameterTypeRef;
        }

        protected static TypeRefBase EvaluateTypeArguments(TypeRefBase reference, object obj, MatchCandidate candidate,
            TypeRefBase parameterTypeRef, Expression parentExpression)
        {
            // We must create a new reference, so we can evaluate nested type parameters (and also potentially
            // replace leading namespaces or types in type argument expressions).
            TypeRefBase newTypeRef = (TypeRefBase)parameterTypeRef.Clone();

            // Recursively evaluate the type arguments of the parameter type
            ChildList<Expression> parameterTypeArguments = newTypeRef.TypeArguments;
            for (int i = 0; i < parameterTypeArguments.Count; ++i)
            {
                Expression parameterTypeArgument = parameterTypeArguments[i];
                TypeRefBase parameterTypeArgumentRef = (parameterTypeArgument != null ? parameterTypeArgument.EvaluateType() : null);
                TypeRefBase newArgument = EvaluateParameter(reference, obj, candidate, parameterTypeArgumentRef, parentExpression);
                parameterTypeArguments[i] = (newArgument != null ? (TypeRefBase)newArgument.Clone() : null);
            }

            return newTypeRef;
        }

        #endregion

        #region /* RENDERING */

        public static void AsTextParameterDefinition(CodeWriter writer, ParameterDefinition parameterDefinition, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            Attribute.AsTextAttributes(writer, parameterDefinition);

            ParameterModifier modifier = GetParameterModifier(parameterDefinition);
            if (modifier != ParameterModifier.None)
                writer.Write(ParameterDecl.ParameterModifierToString(modifier) + " ");

            TypeReference parameterType = parameterDefinition.ParameterType;
            if (parameterType.IsByReference)
            {
                // Dereference (remove the trailing '&') if it's a reference type
                parameterType = ((ByReferenceType)parameterType).ElementType;
            }
            TypeRefBase.AsTextTypeReference(writer, parameterType, passFlags);
            writer.Write(" " + parameterDefinition.Name);

            // Display the default value if it has one
            if (parameterDefinition.HasDefault)
            {
                writer.Write(" " + Assignment.ParseToken);
                object defaultValue = parameterDefinition.Constant;
                new Literal(defaultValue).AsText(writer, flags | RenderFlags.PrefixSpace);
            }
        }

        public static void AsTextParameterInfo(CodeWriter writer, ParameterInfo parameterInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            Attribute.AsTextAttributes(writer, parameterInfo);

            ParameterModifier modifier = GetParameterModifier(parameterInfo);
            if (modifier != ParameterModifier.None)
                writer.Write(ParameterDecl.ParameterModifierToString(modifier) + " ");

            Type parameterType = parameterInfo.ParameterType;
            if (parameterType.IsByRef)
            {
                // Dereference (remove the trailing '&') if it's a reference type
                parameterType = parameterType.GetElementType();
            }
            TypeRefBase.AsTextType(writer, parameterType, passFlags);
            writer.Write(" " + parameterInfo.Name);
        }

        #endregion
    }
}
