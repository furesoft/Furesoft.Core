// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nova.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// The name of the flags attribute.
        /// </summary>
        public const string FlagsAttributeName = "FlagsAttribute";

        /// <summary>
        /// Change the specified object to the specified type, forcing larger integers into smaller ones without exceptions.
        /// </summary>
        public static object ChangeType(object obj, Type toType)
        {
            if (obj == null || toType == null)
                return null;

            object newObj = null;
            try
            {
                // Tragically, the Convert.ChangeType() method can't be used to explicitly convert from larger to smaller
                // types (it will throw an exception and fail).  Therefore, we do explicit conversions here for any that
                // might otherwise throw exceptions, and use Convert.ChangeType() for all others.  Very ugly, but it works.
                var toTypeCode = Type.GetTypeCode(toType);
                var fromTypeCode = Type.GetTypeCode(obj.GetType());
                switch (toTypeCode)
                {
                    case TypeCode.Byte:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Byte:
                                newObj = obj;
                                break;

                            case TypeCode.SByte:
                                newObj = (byte)(sbyte)obj;
                                break;

                            case TypeCode.Int16:
                                newObj = (byte)(short)obj;
                                break;

                            case TypeCode.UInt16:
                                newObj = (byte)(ushort)obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (byte)(int)obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = (byte)(uint)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (byte)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (byte)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (byte)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (byte)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (byte)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.SByte:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Byte:
                                newObj = (sbyte)(byte)obj;
                                break;

                            case TypeCode.SByte:
                                newObj = obj;
                                break;

                            case TypeCode.Int16:
                                newObj = (sbyte)(short)obj;
                                break;

                            case TypeCode.UInt16:
                                newObj = (sbyte)(ushort)obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (sbyte)(int)obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = (sbyte)(uint)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (sbyte)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (sbyte)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (sbyte)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (sbyte)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (sbyte)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.Int16:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Int16:
                                newObj = obj;
                                break;

                            case TypeCode.UInt16:
                                newObj = (short)(ushort)obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (short)(int)obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = (short)(uint)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (short)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (short)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (short)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (short)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (short)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.UInt16:
                        switch (fromTypeCode)
                        {
                            case TypeCode.SByte:
                                newObj = (ushort)(sbyte)obj;
                                break;

                            case TypeCode.Int16:
                                newObj = (ushort)(short)obj;
                                break;

                            case TypeCode.UInt16:
                                newObj = obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (ushort)(int)obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = (ushort)(uint)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (ushort)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (ushort)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (ushort)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (ushort)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (ushort)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.Int32:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Int32:
                                newObj = obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = (int)(uint)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (int)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (int)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (int)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (int)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (int)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.UInt32:
                        switch (fromTypeCode)
                        {
                            case TypeCode.SByte:
                                newObj = (uint)(sbyte)obj;
                                break;

                            case TypeCode.Int16:
                                newObj = (uint)(short)obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (uint)(int)obj;
                                break;

                            case TypeCode.UInt32:
                                newObj = obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (uint)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (uint)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (uint)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (uint)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (uint)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.Int64:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Int64:
                                newObj = obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = (long)(ulong)obj;
                                break;

                            case TypeCode.Single:
                                newObj = (long)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (long)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (long)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.UInt64:
                        switch (fromTypeCode)
                        {
                            case TypeCode.SByte:
                                newObj = (ulong)(sbyte)obj;
                                break;

                            case TypeCode.Int16:
                                newObj = (ulong)(short)obj;
                                break;

                            case TypeCode.Int32:
                                newObj = (ulong)(int)obj;
                                break;

                            case TypeCode.Int64:
                                newObj = (ulong)(long)obj;
                                break;

                            case TypeCode.UInt64:
                                newObj = obj;
                                break;

                            case TypeCode.Single:
                                newObj = (ulong)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (ulong)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = (ulong)(decimal)obj;
                                break;
                        }
                        break;

                    case TypeCode.Single:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Char:
                                newObj = (float)(char)obj;
                                break;

                            case TypeCode.Single:
                                newObj = obj;
                                break;

                            case TypeCode.Double:
                                newObj = (float)(double)obj;
                                break;
                        }
                        break;

                    case TypeCode.Double:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Char:
                                newObj = (double)(char)obj;
                                break;

                            case TypeCode.Double:
                                newObj = obj;
                                break;
                        }
                        break;

                    case TypeCode.Decimal:
                        switch (fromTypeCode)
                        {
                            case TypeCode.Single:
                                newObj = (decimal)(float)obj;
                                break;

                            case TypeCode.Double:
                                newObj = (decimal)(double)obj;
                                break;

                            case TypeCode.Decimal:
                                newObj = obj;
                                break;
                        }
                        break;
                }
                if (newObj == null)
                {
                    //Type fromType = obj.GetType();
                    if (toType == typeof(IntPtr))
                    {
                        if (fromTypeCode == TypeCode.Int32)
                            newObj = (IntPtr)(int)obj;
                        else if (fromTypeCode == TypeCode.Int64)
                            newObj = (IntPtr)(long)obj;
                        //else if (fromType == typeof(void*))
                        //    newObj = (IntPtr)(void*)obj;
                    }
                    else if (toType == typeof(UIntPtr))
                    {
                        if (fromTypeCode == TypeCode.UInt32)
                            newObj = (UIntPtr)(uint)obj;
                        else if (fromTypeCode == TypeCode.UInt64)
                            newObj = (UIntPtr)(ulong)obj;
                        //else if (fromType == typeof(void*))
                        //    newObj = (UIntPtr)(void*)obj;
                    }
                    else if (IsNullableType(toType))
                    {
                        // If the object type differs from the nullable's type element type, convert it
                        var elementType = toType.GetGenericArguments()[0];
                        if (fromTypeCode != Type.GetTypeCode(elementType))
                            obj = ChangeType(obj, elementType);
                        // It's unfortunately not possible to return a nullable type as an object, because
                        // nullable types can't be boxed - they are implicitly converted to either null or their
                        // wrapped type.  This means that a cast to an 'int?' will evaluate as an 'int' type.
                        // Fixing this for Nova's purposes would require a custom Nullable type.
                        //newObj = Activator.CreateInstance(toType, obj);
                        newObj = obj;
                    }
                    else if (toType.IsPointer)
                        newObj = obj;
                    else
                        newObj = Convert.ChangeType(obj, toType);
                }
            }
            catch { }
            return newObj;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public static Type FindTypeArgument(Type thisType, Type typeParameter)
        {
            if (thisType.IsGenericType)
            {
                foreach (var genericParameter in thisType.GetGenericTypeDefinition().GetGenericArguments())
                {
                    if (genericParameter == typeParameter)
                        return thisType.GetGenericArguments()[genericParameter.GenericParameterPosition];
                }
            }

            // If we didn't find a match, search any base types
            return FindTypeArgumentInBase(thisType, typeParameter);
        }

        /// <summary>
        /// Find a type argument in a base class for the specified type parameter.
        /// </summary>
        public static Type FindTypeArgumentInBase(Type thisType, Type typeParameter)
        {
            Type found = null;
            var baseType = thisType.BaseType;
            if (baseType != null && baseType != typeof(object))
                found = FindTypeArgument(baseType, typeParameter);
            if (found != null) return found;
            var interfaces = thisType.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                found = FindTypeArgument(@interface, typeParameter);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Find the index of the specified type parameter.
        /// </summary>
        public static int FindTypeParameterIndex(Type thisType, Type typeParameter)
        {
            if (thisType != null && thisType.IsGenericType && typeParameter != null)
            {
                // If the TypeParameter is declared in a nested type that isn't 'thisType', move up through it's enclosing
                // types looking for a matching type, and update the TypeParameter if we find one.
                var declaringType = typeParameter.DeclaringType;
                while (declaringType != null && declaringType.IsGenericType)
                {
                    if (declaringType == thisType || declaringType.GetGenericTypeDefinition() == thisType.GetGenericTypeDefinition())
                    {
                        if (declaringType != typeParameter.DeclaringType)
                        {
                            // If we moved up to a enclosing type, update the TypeParameter to the one declared there if possible
                            var genericArguments = declaringType.GetGenericArguments();
                            if (typeParameter.GenericParameterPosition < genericArguments.Length)
                                typeParameter = genericArguments[typeParameter.GenericParameterPosition];
                        }
                        break;
                    }
                    declaringType = declaringType.DeclaringType;
                }

                // If TypeParameter isn't found in 'thisType' and it's nested, move up through it's enclosing types, searching them also
                var currentType = thisType;
                do
                {
                    // Search the generic type definition first, but if the type isn't the definition and we don't find a match in the
                    // definition, then also search the type itself.  This is necessary because if one generic type references another
                    // in it's definition, the referenced generic type will have type parameters of the "parent" generic type and we
                    // can match on those - avoiding extra work of searching until we find the parent generic type instance.  It's also
                    // important to NOT check that 'currentType' matches 'declaringType' here for this reason, AND we can't just look
                    // at the TypeParameter's position, but must scan all of the type arguments.
                    if (!currentType.IsGenericTypeDefinition)
                    {
                        foreach (var genericParameter in thisType.GetGenericTypeDefinition().GetGenericArguments())
                        {
                            if (genericParameter == typeParameter)
                                return genericParameter.GenericParameterPosition;
                        }
                    }
                    // In this case, we can't use GenericParameterPosition, because we're not necessarily using the GenericTypeDefinition,
                    // so the position might refer to the position within the declaring type, which might not be the same as 'thisType'.
                    var genericArguments = thisType.GetGenericArguments();
                    for (var i = 0; i < genericArguments.Length; ++i)
                    {
                        if (genericArguments[i] == typeParameter)
                            return i;
                    }

                    currentType = currentType.DeclaringType;
                }
                while (currentType != null && currentType.IsGenericType);
            }
            return -1;
        }

        /// <summary>
        /// Get the parameters for a delegate type.
        /// </summary>
        public static ParameterInfo[] GetDelegateParameters(Type thisType)
        {
            var methodInfo = GetInvokeMethod(thisType);
            return (methodInfo != null ? methodInfo.GetParameters() : null);
        }

        /// <summary>
        /// Get the return type for a delegate type.
        /// </summary>
        public static Type GetDelegateReturnType(Type thisType)
        {
            var methodInfo = GetInvokeMethod(thisType);
            return (methodInfo != null ? methodInfo.ReturnType : null);
        }

        /// <summary>
        /// Get the 'Invoke()' method for the specified <see cref="Type"/>.
        /// </summary>
        public static MethodInfo GetInvokeMethod(Type type)
        {
            // As a workaround for a possible fix in one of the TypeRef constructors that we haven't activated
            // because it breaks other things, make certain that we're using the generic type definition, so
            // that any type parameters in the method signature reflect the ones from the definition as opposed
            // to those from a parent generic type or method.
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();
            return type.GetMethod("Invoke");
        }

        /// <summary>
        /// Get the number of local generic arguments for the type, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static int GetLocalGenericArgumentCount(Type thisType)
        {
            var count = thisType.GetGenericArguments().Length;
            if (count > 0 && thisType.IsNested)
            {
                var declaringType = thisType.DeclaringType;
                if (declaringType != null)
                    count -= declaringType.GetGenericArguments().Length;
            }
            return count;
        }

        /// <summary>
        /// Get the local generic arguments for the type, NOT including arguments from any enclosing generic types.
        /// </summary>
        public static Type[] GetLocalGenericArguments(Type thisType)
        {
            var totalArguments = thisType.GetGenericArguments();
            var totalCount = totalArguments.Length;
            if (totalCount == 0 || !thisType.IsNested)
                return totalArguments;

            var localCount = GetLocalGenericArgumentCount(thisType);
            if (localCount == totalCount)
                return totalArguments;

            var localArguments = new Type[localCount];
            if (localCount > 0)
                Array.Copy(totalArguments, totalCount - localCount, localArguments, 0, localCount);
            return localArguments;
        }

        /// <summary>
        /// Search for a method by name and parameter types.  Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethod(Type thisType, string name, params Type[] parameterTypes)
        {
            return GetMethod(thisType, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        /// <summary>
        /// Search for a method by name, parameter types, and binding flags.  Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethod(Type thisType, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            MethodInfo matchingMethod = null;

            // Check all methods with the specified name, including in base classes
            GetMethod(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod == null && thisType.IsInterface)
            {
                foreach (var interfaceType in thisType.GetInterfaces())
                    GetMethod(ref matchingMethod, interfaceType, name, bindingFlags, parameterTypes);
            }

            return matchingMethod;
        }

        /// <summary>
        /// Search for a property by name.  Unlike GetProperty(), searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static PropertyInfo GetProperty(Type thisType, string name)
        {
            var matchingProperty = thisType.GetProperty(name);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingProperty == null && thisType.IsInterface)
            {
                foreach (var interfaceType in thisType.GetInterfaces())
                {
                    matchingProperty = interfaceType.GetProperty(name);
                    if (matchingProperty != null)
                        break;
                }
            }

            return matchingProperty;
        }

        /// <summary>
        /// Determine if this type has the same type arguments as the specified type.
        /// </summary>
        public static bool HasSameTypeArguments(Type thisType, Type type)
        {
            var thisArguments = thisType.GetGenericArguments();
            var arguments = type.GetGenericArguments();
            if (thisArguments.Length != arguments.Length)
                return false;
            for (var i = 0; i < thisArguments.Length; ++i)
            {
                if (thisArguments[i] != arguments[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determine if the type is a bit-flags style enum.
        /// </summary>
        public static bool IsBitFlagsEnum(Type thisType)
        {
            return MemberInfoUtil.HasCustomAttribute(thisType, FlagsAttributeName);
        }

        /// <summary>
        /// Determine if the type is a delegate type.
        /// </summary>
        public static bool IsDelegateType(Type thisType)
        {
            return typeof(Delegate).IsAssignableFrom(thisType);
        }

        /// <summary>
        /// Determine if the type implements the specified interface.  If a generic interface is specified, and
        /// it's not the generic type definition of the interface, then the type arguments are also compared.
        /// </summary>
        public static bool IsImplementationOf(Type thisType, Type interfaceType)
        {
            if (interfaceType.IsGenericType)
            {
                if (interfaceType.IsGenericTypeDefinition)
                {
                    // If the specified interface is a generic type definition, then match on generic type definitions
                    foreach (var @interface in thisType.GetInterfaces())
                    {
                        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == interfaceType)
                            return true;
                    }
                }
                else
                {
                    // If the specified interface isn't a generic type definition, then also match on any type arguments
                    foreach (var @interface in thisType.GetInterfaces())
                    {
                        if (@interface.IsGenericType && @interface == interfaceType && HasSameTypeArguments(@interface, interfaceType))
                            return true;
                    }
                }
            }
            else
            {
                // Handle non-generic interfaces
                foreach (var @interface in thisType.GetInterfaces())
                {
                    if (@interface == interfaceType)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return true if the type is internal, otherwise false.
        /// </summary>
        public static bool IsInternal(Type thisType)
        {
            return (thisType.IsNested ? thisType.IsNestedAssembly : thisType.IsNotPublic);
        }

        /// <summary>
        /// Determine if the type is a nullable type.
        /// </summary>
        public static bool IsNullableType(Type thisType)
        {
            return (thisType.IsGenericType && thisType.GetGenericTypeDefinition() == typeof(Nullable<>) && !thisType.IsGenericTypeDefinition);
        }

        /// <summary>
        /// Return true if the type is private, otherwise false.
        /// </summary>
        public static bool IsPrivate(Type thisType)
        {
            return (thisType.IsNested && thisType.IsNestedPrivate);
        }

        /// <summary>
        /// Return true if the type is protected, otherwise false.
        /// </summary>
        public static bool IsProtected(Type thisType)
        {
            return (thisType.IsNested && thisType.IsNestedFamily);
        }

        /// <summary>
        /// Return true if the type is static, otherwise false.
        /// </summary>
        public static bool IsStatic(Type thisType)
        {
            return (thisType.IsAbstract && thisType.IsSealed);
        }

        /// <summary>
        /// Determine if the type is a user-defined class (excludes 'object' and 'string').
        /// </summary>
        public static bool IsUserClass(Type thisType)
        {
            // Exclude built-in types that are classes (object and string)
            return (thisType.IsClass && thisType != typeof(object) && thisType != typeof(string));
        }

        /// <summary>
        /// Determine if the type is a user-defined struct (excludes primitive types including 'void' and 'decimal', and enums).
        /// </summary>
        public static bool IsUserStruct(Type thisType)
        {
            // Exclude primitive types, enums, and built-in types that are value types but aren't primitive (void and decimal)
            return (thisType.IsValueType && !thisType.IsPrimitive && !thisType.IsEnum && thisType != typeof(void) && thisType != typeof(decimal));
        }

        /// <summary>
        /// Make an array type.
        /// </summary>
        public static Type MakeArrayType(Type thisType, List<int> arrayRanks)
        {
            var type = thisType;
            foreach (var dim in arrayRanks)
                type = type.MakeArrayType(dim);
            return type;
        }

        /// <summary>
        /// Returns the name without any trailing '`' and type parameter count.
        /// </summary>
        public static string NonGenericName(Type thisType)
        {
            // The names of generic types have a trailing "`" followed by the number of type arguments that they
            // have (generic methods do NOT).  However, the type argument count only includes those arguments at
            // the current level, and not any arguments of enclosing types, even though calling GetGenericArguments()
            // will return all type arguments from all levels.  If a nested type has one or more generic types
            // enclosing it, then it's considered a generic type even if it doesn't have any type parameters of
            // its own (IsGenericType will be true, and GetGenericArguments() will return the type arguments of
            // its enclosing generic types), however its name won't have the "`" suffix.
            var name = thisType.Name;
            var index = name.LastIndexOf('`');
            return (index >= 0 ? name.Substring(0, index) : name);
        }

        private static void GetMethod(ref MethodInfo matchingMethod, Type type, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            // Check all methods with the specified name, including in base classes if BindingFlags.FlattenHierarchy is specified
            foreach (MethodInfo methodInfo in type.GetMember(name, MemberTypes.Method, bindingFlags))
            {
                // Check that the parameter counts and types match, with 'loose' matching on generic parameters
                var parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length == parameterTypes.Length)
                {
                    var i = 0;
                    for (; i < parameterInfos.Length; ++i)
                    {
                        if (!IsSimilarType(parameterInfos[i].ParameterType, parameterTypes[i]))
                            break;
                    }
                    if (i == parameterInfos.Length)
                    {
                        if (matchingMethod == null)
                        {
                            matchingMethod = methodInfo;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the two types are either identical, or are both generic parameters or generic types
        /// with generic parameters in the same locations (generic parameters match any other generic parameter,
        /// but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(Type thisType, Type type)
        {
            // Ignore any 'ref' types
            if (thisType.IsByRef)
                thisType = thisType.GetElementType();
            if (type.IsByRef)
                type = type.GetElementType();

            // Handle array types
            if (thisType.IsArray && type.IsArray)
                return IsSimilarType(thisType.GetElementType(), type.GetElementType());

            // If the types are identical, or the names/namespaces match (they could be defined in separate assemblies), or
            // if they're both generic parameters or the special 'T' type, treat as a match.
            if (thisType == type || (thisType.Name == type.Name && thisType.Namespace == type.Namespace)
                || ((thisType.IsGenericParameter || thisType == typeof(T)) && (type.IsGenericParameter || type == typeof(T))))
                return true;

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                var thisArguments = thisType.GetGenericArguments();
                var arguments = type.GetGenericArguments();
                if (thisArguments.Length == arguments.Length)
                {
                    for (var i = 0; i < thisArguments.Length; ++i)
                    {
                        if (!IsSimilarType(thisArguments[i], arguments[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        public class T
        {
        }
    }
}