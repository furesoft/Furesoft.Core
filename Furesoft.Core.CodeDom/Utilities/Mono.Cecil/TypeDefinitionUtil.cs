// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using Mono.Collections.Generic;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Nova.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="TypeDefinition"/>, and also it's base class <see cref="TypeReference"/>
    /// where appropriate.
    /// </summary>
    public static class TypeDefinitionUtil
    {
        /// <summary>
        /// The name of the flags attribute.
        /// </summary>
        public const string FlagsAttributeName = "FlagsAttribute";

        /// <summary>
        /// Special type used to match any generic parameter type in GetMethod() and GetConstructor().
        /// </summary>
        public static TypeDefinition T = new TypeDefinition("", "T", TypeAttributes.Class);

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public static TypeReference FindTypeArgument(TypeReference thisTypeReference, GenericParameter typeParameter)
        {
            if (thisTypeReference.IsGenericInstance)
            {
                GenericInstanceType genericInstanceType = (GenericInstanceType)thisTypeReference;
                if (genericInstanceType.HasGenericArguments)
                {
                    foreach (GenericParameter genericParameter in GetGenericTypeDefinition(thisTypeReference).GenericParameters)
                    {
                        if (genericParameter == typeParameter)
                            return genericInstanceType.GenericArguments[genericParameter.Position];
                    }
                }
            }
            else if (thisTypeReference.HasGenericParameters)
            {
                foreach (GenericParameter genericParameter in thisTypeReference.GenericParameters)
                {
                    if (genericParameter == typeParameter)
                        return genericParameter;
                }
            }

            // If we didn't find a match, search any base types
            return FindTypeArgumentInBase(thisTypeReference, typeParameter);
        }

        /// <summary>
        /// Find a type argument in a base class for the specified type parameter.
        /// </summary>
        public static TypeReference FindTypeArgumentInBase(TypeReference thisTypeReference, GenericParameter typeParameter)
        {
            if (!thisTypeReference.IsGenericParameter)
            {
                TypeReference found = null;
                TypeDefinition thisTypeDefinition = null;
                try { thisTypeDefinition = thisTypeReference.Resolve(); }
                catch { }
                if (thisTypeDefinition != null)
                {
                    TypeReference baseType = thisTypeDefinition.BaseType;
                    if (baseType != null && baseType.FullName != "System.Object")
                        found = FindTypeArgument(baseType, typeParameter);
                    if (found != null) return found;
                    foreach (var interfaceReference in thisTypeDefinition.Interfaces)
                    {
                        found = FindTypeArgument(interfaceReference.InterfaceType, typeParameter);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the index of the specified type parameter.
        /// </summary>
        public static int FindTypeParameterIndex(TypeReference thisTypeReference, GenericParameter typeParameter)
        {
            if (thisTypeReference != null && typeParameter != null)
            {
                if (thisTypeReference.HasGenericParameters)
                {
                    // If the TypeParameter is declared in a nested type that isn't 'thisTypeReference', move up through it's enclosing
                    // types looking for a matching type, and update the TypeParameter if we find one.
                    TypeReference declaringType = typeParameter.Owner as TypeReference;
                    while (declaringType != null && declaringType.HasGenericParameters)
                    {
                        if (declaringType == thisTypeReference || GetGenericTypeDefinition(declaringType) == GetGenericTypeDefinition(thisTypeReference))
                        {
                            if (declaringType != typeParameter.Owner)
                            {
                                // If we moved up to a enclosing type, update the TypeParameter to the one declared there if possible
                                Collection<GenericParameter> genericArguments = declaringType.GenericParameters;
                                if (typeParameter.Position < genericArguments.Count)
                                    typeParameter = genericArguments[typeParameter.Position];
                            }
                            break;
                        }
                        declaringType = declaringType.DeclaringType;
                    }

                    // If TypeParameter isn't found in 'thisTypeReference' and it's nested, move up through it's enclosing types, searching them also
                    TypeReference currentType = thisTypeReference;
                    do
                    {
                        // Search the generic type definition first, but if the type isn't the definition and we don't find a match in the
                        // definition, then also search the type itself.  This is necessary because if one generic type references another
                        // in it's definition, the referenced generic type will have type parameters of the "parent" generic type and we
                        // can match on those - avoiding extra work of searching until we find the parent generic type instance.  It's also
                        // important to NOT check that 'currentType' matches 'declaringType' here for this reason, AND we can't just look
                        // at the TypeParameter's position, but must scan all of the type arguments.
                        if (!IsGenericTypeDefinition(currentType))
                        {
                            foreach (GenericParameter genericParameter in GetGenericTypeDefinition(thisTypeReference).GenericParameters)
                            {
                                if (genericParameter == typeParameter)
                                    return genericParameter.Position;
                            }
                        }
                        // In this case, we can't use GenericParameterPosition, because we're not necessarily using the GenericTypeDefinition,
                        // so the position might refer to the position within the declaring type, which might not be the same as 'thisTypeDefinition'.
                        Collection<GenericParameter> genericArguments = thisTypeReference.GenericParameters;
                        for (int i = 0; i < genericArguments.Count; ++i)
                        {
                            if (genericArguments[i] == typeParameter)
                                return i;
                        }

                        currentType = currentType.DeclaringType;
                    }
                    while (currentType != null && currentType.HasGenericParameters);
                }
            }
            return -1;
        }

        /// <summary>
        /// Search for a constructor by name and parameter types.  Does 'loose' matching on generic parameter types.
        /// </summary>
        public static MethodDefinition GetConstructor(TypeDefinition thisTypeDefinition, params TypeReference[] parameterTypes)
        {
            return GetMethod(thisTypeDefinition, ".ctor", BindingFlags.Instance | BindingFlags.Public, parameterTypes);
        }

        /// <summary>
        /// Search for all methods with the specified name and flags.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetConstructors(TypeDefinition thisTypeDefinition, BindingFlags bindingFlags)
        {
            return Enumerable.Where(thisTypeDefinition.Methods, delegate (MethodDefinition method)
                {
                    return method.IsConstructor
                           && ((method.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) || (!method.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance)))
                           && ((method.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) || (!method.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic)));
                });
        }

        /// <summary>
        /// Get the parameters for a delegate type.
        /// </summary>
        public static Collection<ParameterDefinition> GetDelegateParameters(TypeReference thisTypeReference)
        {
            TypeDefinition typeDefinition = null;
            try { typeDefinition = thisTypeReference.Resolve(); }
            catch { }
            if (typeDefinition != null)
            {
                MethodDefinition methodDefinition = GetInvokeMethod(typeDefinition);
                if (methodDefinition != null)
                    return methodDefinition.Parameters;
            }
            return null;
        }

        /// <summary>
        /// Get the return type for a delegate type.
        /// </summary>
        public static TypeReference GetDelegateReturnType(TypeReference thisTypeReference)
        {
            TypeDefinition typeDefinition = null;
            try { typeDefinition = thisTypeReference.Resolve(); }
            catch { }
            if (typeDefinition != null)
            {
                MethodDefinition methodDefinition = GetInvokeMethod(typeDefinition);
                if (methodDefinition != null)
                    return methodDefinition.ReturnType;
            }
            return null;
        }

        /// <summary>
        /// Get the generic type definition if the type is a generic type instance (otherwise, just return the type).
        /// </summary>
        public static TypeReference GetGenericTypeDefinition(TypeReference thisTypeReference)
        {
            if (thisTypeReference.IsGenericInstance)
            {
                TypeDefinition thisTypeDefinition = null;
                try { thisTypeDefinition = thisTypeReference.Resolve(); }
                catch { }
                if (thisTypeDefinition != null)
                    return thisTypeDefinition;
            }
            return thisTypeReference;
        }

        /// <summary>
        /// Get the 'Invoke()' method for the specified <see cref="TypeDefinition"/>.
        /// </summary>
        public static MethodDefinition GetInvokeMethod(TypeDefinition typeDefinition)
        {
            return GetMethod(typeDefinition, "Invoke");
        }

        /// <summary>
        /// Get the number of local generic arguments for the type, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static int GetLocalGenericArgumentCount(TypeReference thisTypeReference)
        {
            int count = 0;
            if (thisTypeReference.IsGenericInstance)
            {
                GenericInstanceType genericInstanceType = (GenericInstanceType)thisTypeReference;
                count = genericInstanceType.GenericArguments.Count;
                if (count > 0 && genericInstanceType.IsNested)
                {
                    GenericInstanceType declaringType = genericInstanceType.DeclaringType as GenericInstanceType;
                    if (declaringType != null)
                        count -= declaringType.GenericArguments.Count;
                }
            }
            else if (thisTypeReference is TypeDefinition)
                count = GetLocalGenericArgumentCount((TypeDefinition)thisTypeReference);
            return count;
        }

        /// <summary>
        /// Get the number of local generic parameters for the type definition, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static int GetLocalGenericArgumentCount(TypeDefinition thisTypeDefinition)
        {
            int count = 0;
            if (thisTypeDefinition.HasGenericParameters)
            {
                count = thisTypeDefinition.GenericParameters.Count;
                if (count > 0 && thisTypeDefinition.IsNested)
                {
                    TypeDefinition declaringType = thisTypeDefinition.DeclaringType;
                    if (declaringType != null)
                        count -= declaringType.GenericParameters.Count;
                }
            }
            return count;
        }

        /// <summary>
        /// Get the local generic arguments for the type, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static IEnumerable<TypeReference> GetLocalGenericArguments(TypeReference thisTypeReference)
        {
            if (thisTypeReference.IsGenericInstance)
            {
                GenericInstanceType genericInstanceType = (GenericInstanceType)thisTypeReference;
                Collection<TypeReference> totalArguments = genericInstanceType.GenericArguments;
                int totalCount = totalArguments.Count;
                if (totalCount == 0 || !genericInstanceType.IsNested)
                    return totalArguments;

                int localCount = GetLocalGenericArgumentCount(genericInstanceType);
                if (localCount == totalCount)
                    return totalArguments;

                return Enumerable.Skip(totalArguments, totalCount - localCount);
            }
            if (thisTypeReference is TypeDefinition)
                return GetLocalGenericArguments((TypeDefinition)thisTypeReference);
            return null;
        }

        /// <summary>
        /// Get the local generic parameters for the type definition, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static IEnumerable<GenericParameter> GetLocalGenericArguments(TypeDefinition thisTypeDefinition)
        {
            if (thisTypeDefinition.HasGenericParameters)
            {
                Collection<GenericParameter> totalArguments = thisTypeDefinition.GenericParameters;
                int totalCount = totalArguments.Count;
                if (totalCount == 0 || !thisTypeDefinition.IsNested)
                    return totalArguments;

                int localCount = GetLocalGenericArgumentCount(thisTypeDefinition);
                if (localCount == totalCount)
                    return totalArguments;

                return Enumerable.Skip(totalArguments, totalCount - localCount);
            }
            return null;
        }

        /// <summary>
        /// Search for all members with the specified name.
        /// </summary>
        public static IEnumerable<IMemberDefinition> GetMembers(TypeDefinition thisTypeDefinition, string name)
        {
            // NOTE: The casts below are necessary for Nova 2.x since it doesn't support covariance for generic type arguments
            return Enumerable.Concat((IEnumerable<IMemberDefinition>)Enumerable.Where(thisTypeDefinition.Methods, delegate (MethodDefinition method) { return method.Name == name; }),
                Enumerable.Concat((IEnumerable<IMemberDefinition>)Enumerable.Where(thisTypeDefinition.Properties, delegate (PropertyDefinition property) { return property.Name == name; }),
                    Enumerable.Concat((IEnumerable<IMemberDefinition>)Enumerable.Where(thisTypeDefinition.Fields, delegate (FieldDefinition field) { return field.Name == name; }),
                        Enumerable.Concat((IEnumerable<IMemberDefinition>)Enumerable.Where(thisTypeDefinition.Events, delegate (EventDefinition @event) { return @event.Name == name; }),
                            (IEnumerable<IMemberDefinition>)Enumerable.Where(thisTypeDefinition.NestedTypes, delegate (TypeDefinition nestedType) { return nestedType.Name == name; })))));
        }

        /// <summary>
        /// Find the method with the specified name.
        /// </summary>
        public static MethodDefinition GetMethod(TypeDefinition thisTypeDefinition, string name)
        {
            return Enumerable.FirstOrDefault(thisTypeDefinition.Methods, delegate (MethodDefinition method) { return method.Name == name; });
        }

        /// <summary>
        /// Search for a method by name and parameter types.  Does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        public static MethodDefinition GetMethod(TypeDefinition thisTypeDefinition, string name, params TypeReference[] parameterTypes)
        {
            return GetMethod(thisTypeDefinition, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        /// <summary>
        /// Search for a method by name, parameter types, and binding flags.  Does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        public static MethodDefinition GetMethod(TypeDefinition thisTypeDefinition, string name, BindingFlags bindingFlags, params TypeReference[] parameterTypes)
        {
            MethodDefinition matchingMethod = null;

            // Check all methods with the specified name, including in base classes if BindingFlags.FlattenHierarchy is specified
            GetMethod(ref matchingMethod, thisTypeDefinition, name, bindingFlags, parameterTypes);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod == null && thisTypeDefinition.IsInterface)
            {
                foreach (var interfaceReference in thisTypeDefinition.Interfaces)
                {
                    TypeDefinition interfaceDefinition = null;
                    try { interfaceDefinition = interfaceReference.InterfaceType.Resolve(); }
                    catch { }
                    if (interfaceDefinition != null)
                        GetMethod(ref matchingMethod, interfaceDefinition, name, bindingFlags, parameterTypes);
                }
            }

            return matchingMethod;
        }

        /// <summary>
        /// Search for all methods with the specified name and flags.
        /// </summary>
        public static IEnumerable<MethodDefinition> GetMethods(TypeDefinition thisTypeDefinition, string name, BindingFlags bindingFlags)
        {
            return Enumerable.Where(thisTypeDefinition.Methods, delegate (MethodDefinition method)
                {
                    return method.Name == name
                           && ((method.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) || (!method.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance)))
                           && ((method.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) || (!method.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic)));
                });
        }

        /// <summary>
        /// Get the nested type with the specified name (returns null if none found).
        /// </summary>
        public static TypeDefinition GetNestedType(TypeDefinition thisTypeDefinition, string name)
        {
            return Enumerable.FirstOrDefault(thisTypeDefinition.NestedTypes, delegate (TypeDefinition typeDefinition) { return typeDefinition.Name == name; });
        }

        /// <summary>
        /// Search for a property by name.  Searches base interfaces.
        /// </summary>
        public static PropertyDefinition GetProperty(TypeDefinition thisTypeDefinition, string name)
        {
            PropertyDefinition matchingProperty = Enumerable.FirstOrDefault(thisTypeDefinition.Properties, delegate (PropertyDefinition property) { return property.Name == name; });

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingProperty == null && thisTypeDefinition.IsInterface)
            {
                foreach (var interfaceReference in thisTypeDefinition.Interfaces)
                {
                    TypeDefinition interfaceDefinition = null;
                    try { interfaceDefinition = interfaceReference.InterfaceType.Resolve(); }
                    catch { }
                    if (interfaceDefinition != null)
                    {
                        matchingProperty = GetProperty(interfaceDefinition, name);
                        if (matchingProperty != null)
                            break;
                    }
                }
            }

            return matchingProperty;
        }

        /// <summary>
        /// Get the underlying type of an enum.
        /// </summary>
        public static TypeReference GetUnderlyingTypeOfEnum(TypeDefinition thisTypeDefinition)
        {
            if (thisTypeDefinition.IsEnum)
            {
                foreach (FieldDefinition fieldDefinition in thisTypeDefinition.Fields)
                {
                    if (fieldDefinition.Name == "value__")
                        return fieldDefinition.FieldType;
                }
            }
            return null;
        }

        /// <summary>
        /// Determine if the type is assignable from the specified type.
        /// </summary>
        public static bool IsAssignableFrom(TypeDefinition thisTypeDefinition, TypeDefinition typeDefinition)
        {
            return (thisTypeDefinition == typeDefinition || (typeDefinition.IsClass && !typeDefinition.IsValueType && IsSubclassOf(typeDefinition, thisTypeDefinition))
                || (typeDefinition.IsInterface && IsImplementationOf(typeDefinition, thisTypeDefinition)));
        }

        /// <summary>
        /// Determine if the type is a bit-flags style enum.
        /// </summary>
        public static bool IsBitFlagsEnum(TypeDefinition thisTypeDefinition)
        {
            return ICustomAttributeProviderUtil.HasCustomAttribute(thisTypeDefinition, FlagsAttributeName);
        }

        /// <summary>
        /// Determine if the type is a delegate type.
        /// </summary>
        public static bool IsDelegateType(TypeDefinition thisTypeDefinition)
        {
            return (thisTypeDefinition.BaseType != null && thisTypeDefinition.BaseType.FullName == "System.MulticastDelegate");
        }

        /// <summary>
        /// Determine if the type is an enum.
        /// </summary>
        public static bool IsEnum(TypeReference thisTypeReference)
        {
            TypeDefinition typeDefinition = null;
            try { typeDefinition = thisTypeReference.Resolve(); }
            catch { }
            return (typeDefinition != null && typeDefinition.IsEnum);
        }

        /// <summary>
        /// Determine if the type is a generic type definition (as opposed to an instance).
        /// </summary>
        public static bool IsGenericTypeDefinition(TypeReference thisTypeReference)
        {
            return (thisTypeReference.HasGenericParameters && !thisTypeReference.IsGenericInstance);
        }

        /// <summary>
        /// Determine if the type implements the specified interface.
        /// </summary>
        public static bool IsImplementationOf(TypeDefinition thisTypeDefinition, TypeDefinition interfaceTypeDefinition)
        {
            foreach (var interfaceReference in thisTypeDefinition.Interfaces)
            {
                if (interfaceReference.InterfaceType == interfaceTypeDefinition)
                    return true;
                TypeDefinition interfaceDefinition = null;
                try { interfaceDefinition = interfaceReference.InterfaceType.Resolve(); }
                catch { }
                if (interfaceDefinition != null && IsImplementationOf(interfaceDefinition, interfaceTypeDefinition))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if the type is internal, otherwise false.
        /// </summary>
        public static bool IsInternal(TypeDefinition thisTypeDefinition)
        {
            return (thisTypeDefinition.IsNested ? thisTypeDefinition.IsNestedAssembly : thisTypeDefinition.IsNotPublic);
        }

        /// <summary>
        /// Determine if the type is a nullable type.
        /// </summary>
        public static bool IsNullableType(TypeReference thisTypeReference)
        {
            return (thisTypeReference.HasGenericParameters && thisTypeReference.FullName == "System.Nullable`1" && !thisTypeReference.IsDefinition);
        }

        /// <summary>
        /// Return true if the type is private, otherwise false.
        /// </summary>
        public static bool IsPrivate(TypeDefinition thisTypeDefinition)
        {
            return (thisTypeDefinition.IsNested && thisTypeDefinition.IsNestedPrivate);
        }

        /// <summary>
        /// Return true if the type is protected, otherwise false.
        /// </summary>
        public static bool IsProtected(TypeDefinition thisTypeDefinition)
        {
            return (thisTypeDefinition.IsNested && thisTypeDefinition.IsNestedFamily);
        }

        /// <summary>
        /// Return true if the type is static, otherwise false.
        /// </summary>
        public static bool IsStatic(TypeDefinition thisTypeDefinition)
        {
            return (thisTypeDefinition.IsAbstract && thisTypeDefinition.IsSealed);
        }

        /// <summary>
        /// Determine if the type is a subclass of the specified type.
        /// </summary>
        public static bool IsSubclassOf(TypeDefinition thisTypeDefinition, TypeDefinition typeDefinition)
        {
            if (thisTypeDefinition.BaseType != null)
            {
                TypeDefinition baseTypeDefinition = null;
                try { baseTypeDefinition = thisTypeDefinition.BaseType.Resolve(); }
                catch { }
                if (baseTypeDefinition != null)
                    return (baseTypeDefinition == typeDefinition || IsSubclassOf(baseTypeDefinition, typeDefinition));
            }
            return false;
        }

        /// <summary>
        /// Determine if the type is a user-defined class (excludes 'object' and 'string').
        /// </summary>
        public static bool IsUserClass(TypeDefinition thisTypeDefinition)
        {
            // Exclude built-in types that are classes (object and string)
            return (thisTypeDefinition.IsClass && !thisTypeDefinition.IsValueType
                && thisTypeDefinition.FullName != "System.Object" && thisTypeDefinition.FullName != "System.String");
        }

        /// <summary>
        /// Determine if the type is a user-defined struct (excludes primitive types including 'void' and 'decimal', and enums).
        /// </summary>
        public static bool IsUserStruct(TypeDefinition thisTypeDefinition)
        {
            // Exclude primitive types, enums, and built-in types that are value types but aren't primitive (void and decimal)
            return (thisTypeDefinition.IsValueType && !thisTypeDefinition.IsPrimitive && !thisTypeDefinition.IsEnum
                && thisTypeDefinition.FullName != "System.Void" && thisTypeDefinition.FullName != "System.Decimal");
        }

        /// <summary>
        /// Returns the name without any trailing '`' and type parameter count.
        /// </summary>
        public static string NonGenericName(TypeReference thisTypeReference)
        {
            // The names of generic types have a trailing "`" followed by the number of type arguments that they
            // have (generic methods do NOT).  However, the type argument count only includes those arguments at
            // the current level, and not any arguments of enclosing types, even though calling GetGenericArguments()
            // will return all type arguments from all levels.  If a nested type has one or more generic types
            // enclosing it, then it's considered a generic type even if it doesn't have any type parameters of
            // its own (IsGenericType will be true, and GetGenericArguments() will return the type arguments of
            // its enclosing generic types), however its name won't have the "`" suffix.
            string name = thisTypeReference.Name;
            int index = name.LastIndexOf('`');
            return (index >= 0 ? name.Substring(0, index) : name);
        }

        private static void GetMethod(ref MethodDefinition matchingMethod, TypeDefinition typeDefinition, string name, BindingFlags bindingFlags, params TypeReference[] parameterTypes)
        {
            while (true)
            {
                // Check all methods with the specified name, including in base classes if BindingFlags.FlattenHierarchy is specified
                IEnumerable<MethodDefinition> methodDefinitions = GetMethods(typeDefinition, name, bindingFlags);
                foreach (MethodDefinition methodDefinition in methodDefinitions)
                {
                    // Check that the parameter counts and types match, with 'loose' matching on generic parameters
                    Collection<ParameterDefinition> parameterInfos = methodDefinition.Parameters;
                    if (parameterInfos.Count == parameterTypes.Length)
                    {
                        int i = 0;
                        for (; i < parameterInfos.Count; ++i)
                        {
                            if (!IsSimilarType(parameterInfos[i].ParameterType, parameterTypes[i]))
                                break;
                        }
                        if (i == parameterInfos.Count)
                        {
                            if (matchingMethod == null)
                            {
                                matchingMethod = methodDefinition;
                                return;
                            }
                        }
                    }
                }
                if (bindingFlags.HasFlag(BindingFlags.FlattenHierarchy))
                {
                    TypeReference baseType = typeDefinition.BaseType;
                    if (baseType != null)
                    {
                        try { typeDefinition = baseType.Resolve(); }
                        catch { typeDefinition = null; }
                        if (typeDefinition != null)
                            continue;
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Determines if the two types are either identical, or are both generic parameters or generic types
        /// with generic parameters in the same locations (generic parameters match any other generic parameter,
        /// but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(TypeReference thisTypeReference, TypeReference typeReference)
        {
            // Ignore any 'ref' types
            if (thisTypeReference.IsByReference)
                thisTypeReference = ((ByReferenceType)thisTypeReference).ElementType;
            if (typeReference.IsByReference)
                typeReference = ((ByReferenceType)typeReference).ElementType;

            // Handle array types
            if (thisTypeReference.IsArray && typeReference.IsArray)
                return IsSimilarType(((ArrayType)thisTypeReference).ElementType, ((ArrayType)typeReference).ElementType);

            // If the types are identical, or the names/namespaces match (they could be defined in separate assemblies), or
            // if they're both generic parameters or the special 'T' type, treat as a match.
            if (thisTypeReference == typeReference || (thisTypeReference.FullName == typeReference.FullName)
                || ((thisTypeReference.IsGenericParameter || thisTypeReference == T || thisTypeReference.FullName == "Nova.Utilities.TypeUtil/T")
                    && (typeReference.IsGenericParameter || typeReference == T || typeReference.FullName == "Nova.Utilities.TypeUtil/T")))
                return true;

            // Handle any generic arguments
            if (thisTypeReference.HasGenericParameters && typeReference.HasGenericParameters)
            {
                Collection<GenericParameter> thisArguments = thisTypeReference.GenericParameters;
                Collection<GenericParameter> arguments = typeReference.GenericParameters;
                if (thisArguments.Count == arguments.Count)
                {
                    for (int i = 0; i < thisArguments.Count; ++i)
                    {
                        if (!IsSimilarType(thisArguments[i], arguments[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}