// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="TypeParameter"/> (or <see cref="GenericParameter"/>/<see cref="Type"/>) from <b>within</b>
    /// the generic type or method declaration that declares it.
    /// </summary>
    /// <remarks>
    /// In contrast, an <see cref="OpenTypeParameterRef"/> represents a reference to a <see cref="TypeParameter"/> (or <see cref="GenericParameter"/>
    /// /<see cref="Type"/>) from <b>outside</b> the generic type or method declaration that declares it, and is a temporary placeholder that should
    /// be resolved to either a concrete type or a <see cref="TypeParameterRef"/>.
    /// Like a <see cref="TypeRef"/>, a <see cref="TypeParameterRef"/> can include array ranks.  It subclasses <see cref="TypeRef"/> in order to
    /// provide this functionality, although it doesn't support type arguments.
    /// </remarks>
    public class TypeParameterRef : TypeRef
    {
        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration, bool isFirstOnLine, List<int> arrayRanks)
            : base(declaration, isFirstOnLine, null, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration, List<int> arrayRanks)
            : base(declaration, false, null, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration, bool isFirstOnLine, params int[] arrayRanks)
            : base(declaration, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public TypeParameterRef(TypeParameter declaration, params int[] arrayRanks)
            : base(declaration, false, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine, List<int> arrayRanks)
            : base(genericParameter, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine)
            : base(genericParameter, isFirstOnLine)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter)
            : base(genericParameter, false)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter, List<int> arrayRanks)
            : base(genericParameter, false, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter, bool isFirstOnLine, params int[] arrayRanks)
            : base(genericParameter, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="GenericParameter"/>.
        /// </summary>
        public TypeParameterRef(GenericParameter genericParameter, params int[] arrayRanks)
            : base(genericParameter, false, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type, bool isFirstOnLine, List<int> arrayRanks)
            : base(type, isFirstOnLine, null, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type, bool isFirstOnLine)
            : base(type, isFirstOnLine)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type)
            : base(type, false)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type, List<int> arrayRanks)
            : base(type, false, null, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type, bool isFirstOnLine, params int[] arrayRanks)
            : base(type, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Construct a <see cref="TypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public TypeParameterRef(Type type, params int[] arrayRanks)
            : base(type, false, arrayRanks)
        { }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public override bool IsDelegateType
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsGenericParameter
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsPossibleDelegateType
        {
            get { return true; }
        }

        /// <summary>
        /// True if the referenced type is a value type.
        /// </summary>
        public override bool IsValueType
        {
            get
            {
                // Determine value vs reference type based upon any constraints
                if (_reference is TypeParameter)
                {
                    List<TypeParameterConstraint> constraints = ((TypeParameter)_reference).GetConstraints();
                    if (constraints != null && constraints.Count > 0)
                    {
                        foreach (TypeParameterConstraint constraint in constraints)
                        {
                            if (constraint is ClassConstraint)
                                return false;
                            if (constraint is StructConstraint)
                                return true;
                            if (constraint is TypeConstraint)
                            {
                                TypeRef typeRef = ((TypeConstraint)constraint).EvaluateType() as TypeRef;
                                if (typeRef != null && typeRef.IsValueType)
                                    return true;
                            }
                        }
                    }
                }
                else if (_reference is GenericParameter)
                {
                    GenericParameterAttributes attributes = ((GenericParameter)_reference).Attributes;
                    if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                        return false;
                    if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                        return true;
                    var constraintTypes = ((GenericParameter)_reference).Constraints.Select(_ => _.ConstraintType).ToList();
                    if (constraintTypes != null && constraintTypes.Count > 0)
                        return Enumerable.Any(constraintTypes, delegate (TypeReference constraintType) { return constraintType.IsValueType; });
                }
                else //if (_reference is Type)
                {
                    System.Reflection.GenericParameterAttributes attributes = ((Type)_reference).GenericParameterAttributes;
                    if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                        return false;
                    if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                        return true;
                    Type[] constraints = ((Type)_reference).GetGenericParameterConstraints();
                    if (constraints.Length > 0)
                        return Enumerable.Any(constraints, delegate (Type constraintType) { return constraintType.IsValueType; });
                }
                return false;
            }
        }

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is TypeParameter)
                    return ((TypeParameter)_reference).Name;
                if (_reference is GenericParameter)
                    return ((GenericParameter)_reference).Name;
                return ((Type)_reference).Name;
            }
        }

        /// <summary>
        /// The type argument <see cref="Expression"/>s of the reference (if any).
        /// </summary>
        public override ChildList<Expression> TypeArguments
        {
            get { return null; }
            set
            {
                if (value != null)
                    throw new Exception("Can't set type arguments on a TypeParameterRef!");
            }
        }

        /// <summary>
        /// Evaluate the type of the <see cref="TypeParameter"/> by searching for matching type arguments in the
        /// specified parent code object tree.
        /// </summary>
        public override TypeRefBase EvaluateTypeArgumentTypes(CodeObject parent, CodeObject originatingChild)
        {
            TypeRefBase result;

            // If this TypeParameterRef is part of an explicit inteface implementation, leave it alone so that it won't
            // be converted to an OpenTypeParameterRef and left as one.
            if (parent is Dot && ((Dot)parent).Right == originatingChild && parent.Parent is GenericMethodDecl && ((GenericMethodDecl)parent.Parent).ExplicitInterfaceExpression == parent)
                result = this;
            else
            {
                // In most cases, when a TypeParameterRef is evaluated, we have to convert it back to an OpenTypeParameterRef and re-evaluate it,
                // because its context might have changed from inside a generic type definition to a concrete instance of the generic type (for
                // example, the type of a field or return type of a method changes when it's referenced in the context of an instance of the type).
                // If the reference is a Type, then it's an external TypeParameter and can't be in scope, so it must always be evaluated.
                object reference = Reference;
                if (reference is TypeParameter)
                    result = new OpenTypeParameterRef((TypeParameter)reference, IsFirstOnLine, ArrayRanks).EvaluateTypeArgumentTypes(parent, originatingChild);
                else if (reference is GenericParameter)
                    result = new OpenTypeParameterRef((GenericParameter)reference, IsFirstOnLine, ArrayRanks).EvaluateTypeArgumentTypes(parent, originatingChild);
                else //if (reference is Type)
                    result = new OpenTypeParameterRef((Type)reference, IsFirstOnLine, ArrayRanks).EvaluateTypeArgumentTypes(parent, originatingChild);
            }
            return result;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            // Look for the type argument in any type constraints
            if (_reference is TypeParameter)
                return ((TypeParameter)_reference).FindTypeArgument(typeParameterRef);

            if (_reference is GenericParameter)
            {
                TypeReference typeReference = null;
                GenericParameter genericParameter = typeParameterRef.Reference as GenericParameter;
                if (genericParameter != null)
                {
                    foreach (var constraintType in ((GenericParameter)_reference).Constraints)
                    {
                        typeReference = TypeDefinitionUtil.FindTypeArgument(constraintType.ConstraintType, genericParameter);
                        if (typeReference != null)
                            break;
                    }
                }
                return Create(typeReference);
            }

            Type type = null;
            Type typeParameter = typeParameterRef.Reference as Type;
            if (typeParameter != null)
            {
                foreach (Type constraintType in ((Type)_reference).GetGenericParameterConstraints())
                {
                    type = TypeUtil.FindTypeArgument(constraintType, typeParameter);
                    if (type != null)
                        break;
                }
            }
            return Create(type);
        }

        /// <summary>
        /// Get the declaring generic type (returns null if the parent is a method).
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            return GetDeclaringTypeOrMethod() as TypeRef;
        }

        /// <summary>
        /// Get the declaring generic type or method.
        /// </summary>
        public TypeRefBase GetDeclaringTypeOrMethod()
        {
            object reference = GetReferencedType();
            if (reference is TypeParameter)
            {
                CodeObject parent = ((TypeParameter)reference).Parent;
                if (parent is GenericMethodDecl)
                {
                    GenericMethodDecl methodDecl = (GenericMethodDecl)parent;
                    return methodDecl.CreateRef(ChildList<Expression>.CreateListOfNulls(methodDecl.TypeParameterCount));
                }
                if (parent is ITypeDecl)
                {
                    ITypeDecl typeDecl = (ITypeDecl)parent;
                    return typeDecl.CreateRef(ChildList<Expression>.CreateListOfNulls(typeDecl.TypeParameterCount));
                }
            }
            else if (reference is GenericParameter)
            {
                GenericParameter genericParameter = (GenericParameter)reference;
                IGenericParameterProvider owner = genericParameter.Owner;
                if (owner is MethodReference)
                    return MethodRef.Create((MethodReference)owner);
                return Create((TypeReference)owner);
            }
            else if (reference is Type)
            {
                Type typeParameter = (Type)reference;
                if (typeParameter.DeclaringMethod != null)
                    return MethodRef.Create(typeParameter.DeclaringMethod);
                Type declaringType = ((Type)reference).DeclaringType;
                return (declaringType != null ? Create(declaringType) : null);
            }
            return null;
        }

        /// <summary>
        /// Get the name of the declaring generic type or method.
        /// </summary>
        public string GetDeclaringTypeOrMethodName()
        {
            return GetDeclaringTypeOrMethodName(GetReferencedType());
        }

        /// <summary>
        /// Calculate a hash code for the referenced object which is the same for all references where IsSameRef() is true.
        /// </summary>
        public override int GetIsSameRefHashCode()
        {
            // Make the hash codes as unique as possible while still ensuring that they are identical
            // for any objects for which IsSameRef() returns true.
            int hashCode = Name.GetHashCode();
            string namespaceName = NamespaceName;
            if (namespaceName != null) hashCode ^= namespaceName.GetHashCode();
            if (_arrayRanks != null)
            {
                foreach (int rank in _arrayRanks)
                    hashCode = (hashCode << 1) ^ rank;
            }
            return hashCode;
        }

        /// <summary>
        /// Get the actual type reference (TypeParameter or Type).
        /// </summary>
        /// <returns>The TypeParameter or Type.</returns>
        public override object GetReferencedType()
        {
            return Reference;
        }

        /// <summary>
        /// Get the type constraints (if any) for this type parameter.
        /// </summary>
        public List<TypeRefBase> GetTypeConstraints()
        {
            List<TypeRefBase> constraintTypeRefs = null;
            if (_reference is TypeParameter)
            {
                List<TypeParameterConstraint> constraints = ((TypeParameter)_reference).GetConstraints();
                if (constraints != null)
                {
                    foreach (TypeParameterConstraint constraint in constraints)
                    {
                        if (constraint is TypeConstraint)
                        {
                            if (constraintTypeRefs == null)
                                constraintTypeRefs = new List<TypeRefBase>();
                            constraintTypeRefs.Add(((TypeConstraint)constraint).EvaluateType());
                        }
                    }
                }
            }
            else if (_reference is GenericParameter)
            {
                var constraintTypes = ((GenericParameter)_reference).Constraints;
                if (constraintTypes != null && constraintTypes.Count > 0)
                {
                    constraintTypeRefs = new List<TypeRefBase>();
                    foreach (var typeConstraint in constraintTypes)
                        constraintTypeRefs.Add(Create(typeConstraint.ConstraintType));
                }
            }
            else //if (_reference is Type)
            {
                Type[] constraintTypes = ((Type)_reference).GetGenericParameterConstraints();
                if (constraintTypes.Length > 0)
                {
                    constraintTypeRefs = new List<TypeRefBase>();
                    foreach (Type typeConstraint in constraintTypes)
                        constraintTypeRefs.Add(Create(typeConstraint));
                }
            }
            return constraintTypeRefs;
        }

        /// <summary>
        /// Determine if the reference is implicitly convertible *from* the specified reference.
        /// </summary>
        public override bool IsImplicitlyConvertibleFrom(TypeRefBase fromTypeRefBase)
        {
            return IsImplicitlyConvertible(fromTypeRefBase, true);
        }

        /// <summary>
        /// Determine if the <see cref="TypeParameterRef"/> is implicitly convertible to the specified <see cref="TypeRefBase"/>.
        /// </summary>
        /// <param name="toTypeRefBase">The <see cref="TypeRef"/>, <see cref="MethodRef"/>, or <see cref="UnresolvedRef"/> being checked.</param>
        /// <param name="standardConversionsOnly">True if only standard conversions should be allowed.</param>
        public override bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            // Implicit conversions involving type parameters
            // The following implicit conversions exist for a given type parameter T:
            // - From T to its effective base class C, from T to any base class of C, and from T to any interface implemented by C.
            //   At run-time, if T is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is
            //   executed as an implicit reference conversion or identity conversion.
            // - From T to an interface type I in T’s effective interface set and from T to any base interface of I. At run-time,
            //   if T is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is executed as
            //   an implicit reference conversion or identity conversion.
            // - From T to a type parameter U, provided T depends on U. At run-time, if U is a value type, then T and U are
            //   necessarily the same type and no conversion is performed. Otherwise, if T is a value type, the conversion is executed
            //   as a boxing conversion. Otherwise, the conversion is executed as an implicit reference conversion or identity conversion.
            // - From the null literal to T, provided T is known to be a reference type. (See Literal.IsImplicitlyConvertibleTo())
            // If T is known to be a reference type, the conversions above are all classified as implicit reference
            // conversions. If T is not known to be a reference type, the conversions described in the first two bullets
            // above are classified as boxing conversions.

            return IsImplicitlyConvertible(toTypeRefBase, false);
        }

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public override bool IsSameRef(SymbolicRef symbolicRef)
        {
            if (!(symbolicRef is TypeParameterRef))
                return false;

            TypeParameterRef typeParameterRef = (TypeParameterRef)symbolicRef;
            object reference = GetReferencedType();
            object typeParameterRefReference = typeParameterRef.GetReferencedType();

            if (reference != typeParameterRefReference)
            {
                // We also have to consider the references the same if the Name and NamespaceNames match.
                // This can occur when types are present both in an assembly reference, and also as CodeDOM
                // objects (either in the current project, or a referenced project).  This can occur if one
                // or both types are in a project with assembly references instead of project references to
                // a type that is defined in the current solution, which isn't uncommon - it will occur in
                // "master" solutions that include projects that are also used in other solutions, and also
                // if a project in the solution uses a non-supported language and so is referenced by its
                // assembly.
                if (Name != typeParameterRef.Name || NamespaceName != typeParameterRef.NamespaceName)
                    return false;

                // With type parameters, the declaring (parent) type names must also match for the type
                // parameters to be considered the same.
                string declaringTypeName = GetDeclaringTypeOrMethodName(reference);
                string typeParameterRefDeclaringTypeName = typeParameterRef.GetDeclaringTypeOrMethodName();
                if (declaringTypeName == null || typeParameterRefDeclaringTypeName == null
                    || declaringTypeName != typeParameterRefDeclaringTypeName)
                    return false;
            }

            return HasSameArrayRanks(typeParameterRef);
        }

        /// <summary>
        /// Get the name of the declaring generic type or method for the specified type parameter
        /// (<see cref="TypeParameter"/> or <see cref="Type"/>).
        /// </summary>
        protected static string GetDeclaringTypeOrMethodName(object reference)
        {
            if (reference is TypeParameter)
            {
                CodeObject parent = ((TypeParameter)reference).Parent;
                if (parent is INamedCodeObject)
                    return ((INamedCodeObject)parent).Name;
            }
            else if (reference is GenericParameter)
            {
                GenericParameter genericParameter = (GenericParameter)reference;
                IGenericParameterProvider owner = genericParameter.Owner;
                if (owner is MethodReference)
                    return ((MethodReference)owner).Name;
                return ((TypeReference)owner).Name;
            }
            else if (reference is Type)
            {
                Type typeParameter = (Type)reference;
                if (typeParameter.DeclaringMethod != null)
                    return typeParameter.DeclaringMethod.Name;
                if (typeParameter.DeclaringType != null)
                    return typeParameter.DeclaringType.Name;
            }
            return null;
        }

        /// <summary>
        /// Determine if the reference is implicitly convertible to the specified reference.
        /// </summary>
        protected bool IsImplicitlyConvertible(TypeRefBase toTypeRefBase, bool reverse)
        {
            // Implicit identity conversion
            if (IsSameRef(toTypeRefBase))
                return true;

            // Check for implicit conversion to certain constraints
            if (_reference is TypeParameter)
            {
                List<TypeParameterConstraint> constraints = ((TypeParameter)_reference).GetConstraints();
                if (constraints != null && constraints.Count > 0)
                {
                    foreach (TypeParameterConstraint constraint in constraints)
                    {
                        TypeRefBase fromTypeRefBase = null;
                        if (constraint is TypeConstraint)
                            fromTypeRefBase = ((TypeConstraint)constraint).EvaluateType();
                        else if (constraint is ClassConstraint)
                            fromTypeRefBase = ObjectRef;
                        else if (constraint is StructConstraint)
                            fromTypeRefBase = ValueTypeRef;
                        if (fromTypeRefBase != null && IsImplicitlyConvertible(fromTypeRefBase, toTypeRefBase, reverse))
                            return true;
                    }
                }
            }
            else if (_reference is GenericParameter)
            {
                if (toTypeRefBase is UnresolvedRef)
                    return false;

                var constraints = ((GenericParameter)_reference).Constraints.Select(_ => _.ConstraintType).ToList();
                if (constraints != null && constraints.Count > 0)
                {
                    if (Enumerable.Any(constraints, delegate (TypeReference typeReference) { return IsImplicitlyConvertible(Create(typeReference), toTypeRefBase, reverse); }))
                        return true;

                    TypeRef fromTypeRef = null;
                    GenericParameterAttributes attributes = ((GenericParameter)_reference).Attributes;
                    if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                        fromTypeRef = ObjectRef;
                    else if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                        fromTypeRef = ValueTypeRef;
                    if (fromTypeRef != null && IsImplicitlyConvertible(fromTypeRef, toTypeRefBase, reverse))
                        return true;
                }
            }
            else //if (_reference is Type)
            {
                if (toTypeRefBase is UnresolvedRef)
                    return false;

                Type[] constraints = ((Type)_reference).GetGenericParameterConstraints();
                if (constraints.Length > 0)
                {
                    if (Enumerable.Any(constraints, delegate (Type type) { return IsImplicitlyConvertible(Create(type), toTypeRefBase, reverse); }))
                        return true;

                    TypeRef fromTypeRef = null;
                    System.Reflection.GenericParameterAttributes attributes = ((Type)_reference).GenericParameterAttributes;
                    if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                        fromTypeRef = ObjectRef;
                    else if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                        fromTypeRef = ValueTypeRef;
                    if (fromTypeRef != null && IsImplicitlyConvertible(fromTypeRef, toTypeRefBase, reverse))
                        return true;
                }
            }

            // Also check for conversion from 'object'
            return IsImplicitlyConvertible(ObjectRef, toTypeRefBase, reverse);
        }

        /// <summary>
        /// Determine if the specified 'from' reference is implicitly convertible to the specified 'to' reference.
        /// </summary>
        protected bool IsImplicitlyConvertible(TypeRefBase fromTypeRefBase, TypeRefBase toTypeRefBase, bool reverse)
        {
            if (fromTypeRefBase != null)
            {
                // First, add back any lost array ranks to 'fromTypeRefBase' (done here so that all callers
                // of this method don't have to do it).
                if (IsArray)
                    fromTypeRefBase = fromTypeRefBase.MakeArrayRef(ArrayRanks);

                if (reverse)
                {
                    if (fromTypeRefBase.IsImplicitlyConvertibleFrom(toTypeRefBase))
                        return true;
                }
                else
                {
                    if (fromTypeRefBase.IsImplicitlyConvertibleTo(toTypeRefBase))
                        return true;
                }
            }
            return false;
        }
    }
}