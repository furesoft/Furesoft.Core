// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections;
using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that represent type declarations,
    /// which includes <see cref="TypeDecl"/>, <see cref="TypeParameter"/>, and <see cref="Alias"/>.
    /// </summary>
    public interface ITypeDecl : INamedCodeObject
    {
        /// <summary>
        /// True if the type is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// True if the type is a class.
        /// </summary>
        bool IsClass { get; }

        /// <summary>
        /// True if the type is a delegate type.
        /// </summary>
        bool IsDelegateType { get; }

        /// <summary>
        /// True if the type is an enum.
        /// </summary>
        bool IsEnum { get; }

        /// <summary>
        /// True if the type is a generic parameter.
        /// </summary>
        bool IsGenericParameter { get; }

        /// <summary>
        /// True if the type is a generic type (meaning that either it or an enclosing type has type parameters,
        /// and it's not an enum).
        /// </summary>
        bool IsGenericType { get; }

        /// <summary>
        /// True if the type is an interface.
        /// </summary>
        bool IsInterface { get; }

        /// <summary>
        /// True if the type is a nested type.
        /// </summary>
        bool IsNested { get; }

        /// <summary>
        /// True if the type is a nullable type.
        /// </summary>
        bool IsNullableType { get; }

        /// <summary>
        /// True if the type is a partial type.
        /// </summary>
        bool IsPartial { get; }

        /// <summary>
        /// True if the type is a struct.
        /// </summary>
        bool IsStruct { get; }

        /// <summary>
        /// True if the type is a value type.
        /// </summary>
        bool IsValueType { get; }

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        CodeObject Parent { get; }

        /// <summary>
        /// The number of <see cref="TypeParameter"/>s the type has.
        /// </summary>
        int TypeParameterCount { get; }

        /// <summary>
        /// Create an array reference to the type.
        /// </summary>
        TypeRef CreateArrayRef(bool isFirstOnLine, params int[] ranks);

        /// <summary>
        /// Create an array reference to the type.
        /// </summary>
        TypeRef CreateArrayRef(params int[] ranks);

        /// <summary>
        /// Create a nullable reference to the type.
        /// </summary>
        TypeRef CreateNullableRef(bool isFirstOnLine);

        /// <summary>
        /// Create a nullable reference to the type.
        /// </summary>
        TypeRef CreateNullableRef();

        /// <summary>
        /// Create a reference to the type.
        /// </summary>
        TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks);

        /// <summary>
        /// Create a reference to the type.
        /// </summary>
        TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments);

        /// <summary>
        /// Create a reference to the type.
        /// </summary>
        TypeRef CreateRef(ChildList<Expression> typeArguments, List<int> arrayRanks);

        /// <summary>
        /// Create a reference to the type.
        /// </summary>
        TypeRef CreateRef(ChildList<Expression> typeArguments);

        /// <summary>
        /// Returns the first attribute expression (<see cref="Call"/> or <see cref="ConstructorRef"/>) with the specified name on the <see cref="CodeObject"/>.
        /// </summary>
        Expression GetAttribute(string attributeName);

        /// <summary>
        /// Get the base type.
        /// </summary>
        TypeRef GetBaseType();

        /// <summary>
        /// Get the non-static constructor with the specified parameters.
        /// </summary>
        ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes);

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        NamedCodeObjectGroup GetConstructors(bool currentPartOnly);

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        NamedCodeObjectGroup GetConstructors();

        /// <summary>
        /// Get the delegate parameters of the type (if any).
        /// </summary>
        ICollection GetDelegateParameters();

        /// <summary>
        /// Get the delegate return type of the type (if any).
        /// </summary>
        TypeRefBase GetDelegateReturnType();

        /// <summary>
        /// Get the field with the specified name.
        /// </summary>
        FieldRef GetField(string name);

        /// <summary>
        /// Get the method with the specified name and parameter types.
        /// </summary>
        MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes);

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="searchBaseClasses">Pass <c>false</c> to NOT search base classes.</param>
        List<MethodRef> GetMethods(string name, bool searchBaseClasses);

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        List<MethodRef> GetMethods(string name);

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results);

        /// <summary>
        /// Get the <see cref="Namespace"/> for this <see cref="CodeObject"/>.
        /// </summary>
        Namespace GetNamespace();

        /// <summary>
        /// Get the nested type with the specified name.
        /// </summary>
        TypeRef GetNestedType(string name);

        /// <summary>
        /// Get the property with the specified name.
        /// </summary>
        PropertyRef GetProperty(string name);

        /// <summary>
        /// Determine if the type is assignable from the specified type.
        /// </summary>
        bool IsAssignableFrom(TypeRef typeRef);

        /// <summary>
        /// Determine if the type implements the specified interface type.
        /// </summary>
        bool IsImplementationOf(TypeRef interfaceTypeRef);

        /// <summary>
        /// Determine if the type is a subclass of the specified type.
        /// </summary>
        bool IsSubclassOf(TypeRef classTypeRef);
    }
}