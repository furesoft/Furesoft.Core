// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types
{
    /// <summary>
    /// Represents a reference to a <see cref="TypeParameter"/> (or <see cref="Type"/>) from <b>within</b>
    /// the generic type or method declaration that declares it.
    /// </summary>
    /// <remarks>
    /// In contrast, an <see cref="OpenTypeParameterRef"/> represents a reference to a <see cref="TypeParameter"/> (or
    /// <see cref="Type"/>) from <b>outside</b> the generic type or method declaration that declares it, and is a temporary
    /// placeholder that should be resolved to either a concrete type or a <see cref="TypeParameterRef"/>.
    /// Like a <see cref="TypeRef"/>, a <see cref="TypeParameterRef"/> can include array ranks.  It subclasses <see cref="TypeRef"/>
    /// in order to provide this functionality, although it doesn't support type arguments.
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
                                TypeRef typeRef = ((TypeConstraint)constraint).Type.SkipPrefixes() as TypeRef;
                                if (typeRef != null && typeRef.IsValueType)
                                    return true;
                            }
                        }
                    }
                }
                else //if (_reference is Type)
                {
                    GenericParameterAttributes attributes = ((Type)_reference).GenericParameterAttributes;
                    if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                        return false;
                    if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
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
                            constraintTypeRefs.Add(((TypeConstraint)constraint).Type.SkipPrefixes() as TypeRefBase);
                        }
                    }
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
    }
}
