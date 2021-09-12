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

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Handles references to generic and/or array types, and is the common base class of <see cref="TypeRef"/>,
    /// <see cref="MethodRef"/> (because it can be treated as a delegate type, and can have generic parameters),
    /// and <see cref="UnresolvedRef"/> (because unresolved symbols can have generic parameters and array ranks).
    /// </summary>
    public abstract class TypeRefBase : SymbolicRef
    {
        /// <summary>
        /// The alternate token used to parse the end of type arguments inside documentation comments.
        /// </summary>
        public const string ParseTokenAltArgumentEnd = "}";

        /// <summary>
        /// The alternate token used to parse the start of type arguments inside documentation comments.
        /// </summary>
        public const string ParseTokenAltArgumentStart = "{";

        /// <summary>
        /// The token used to parse the end of type arguments.
        /// </summary>
        public const string ParseTokenArgumentEnd = ">";

        /// <summary>
        /// The token used to parse the start of type arguments.
        /// </summary>
        public const string ParseTokenArgumentStart = "<";

        /// <summary>
        /// The token used to parse the end of array ranks.
        /// </summary>
        public const string ParseTokenArrayEnd = "]";

        /// <summary>
        /// The token used to parse the start of array ranks.
        /// </summary>
        public const string ParseTokenArrayStart = "[";

        /// <summary>
        /// The token used to parse nullable types.
        /// </summary>
        public const string ParseTokenNullable = "?";

        /// <summary>
        /// A map of built-in type names to keywords (the "System" namespace prefix is NOT included on the names).
        /// </summary>
        public static readonly Dictionary<string, string> TypeNameToKeywordMap = new Dictionary<string, string>(16)
            {
                { "Object",  "object"  },
                { "Void",    "void"    },
                { "SByte",   "sbyte"   },
                { "Byte",    "byte"    },
                { "Int16",   "short"   },
                { "UInt16",  "ushort"  },
                { "Int32",   "int"     },
                { "UInt32",  "uint"    },
                { "Int64",   "long"    },
                { "UInt64",  "ulong"   },
                { "Char",    "char"    },
                { "Boolean", "bool"    },
                { "String",  "string"  },
                { "Single",  "float"   },
                { "Double",  "double"  },
                { "Decimal", "decimal" }
            };

        protected static readonly HashSet<string> TypeArgumentTerminators = new HashSet<string>
            {
                "(", ")", "]", "}", ":", ";", ",", ".", "?", "==", "!=", "|", "^"
            };

        /// <summary>
        /// Optional array ranks (for array types only).
        /// </summary>
        protected List<int> _arrayRanks;

        /// <summary>
        /// Optional type arguments (for generic types only).
        /// </summary>
        protected ChildList<Expression> _typeArguments;

        protected TypeRefBase(string name, bool isFirstOnLine)
            : base(name, isFirstOnLine)
        { }

        protected TypeRefBase(ITypeDecl typeDecl, bool isFirstOnLine)
            : base(typeDecl, isFirstOnLine)
        { }

        protected TypeRefBase(TypeDefinition memberInfo, bool isFirstOnLine)
            : base(memberInfo, isFirstOnLine)
        { }

        protected TypeRefBase(GenericParameter genericParameter, bool isFirstOnLine)
            : base(genericParameter, isFirstOnLine)
        { }

        protected TypeRefBase(Type memberInfo, bool isFirstOnLine)
            : base(memberInfo, isFirstOnLine)
        { }

        protected TypeRefBase(MethodDeclBase methodDeclBase, bool isFirstOnLine)
            : base(methodDeclBase, isFirstOnLine)
        { }

        protected TypeRefBase(AnonymousMethod anonymousMethod, bool isFirstOnLine)
            : base(anonymousMethod, isFirstOnLine)
        { }

        protected TypeRefBase(MethodDefinition methodDefinition, bool isFirstOnLine)
            : base(methodDefinition, isFirstOnLine)
        { }

        protected TypeRefBase(MethodBase methodBase, bool isFirstOnLine)
            : base(methodBase, isFirstOnLine)
        { }

        protected TypeRefBase(object obj)
            : base(obj)
        { }

        // Alternate type argument delimiters allowed for code embedded inside documentation comments.
        // The normal delimiters are also allowed in doc comments, although they shouldn't show up
        // usually, since they cause errors with parsing the XML properly - but they could be used
        // programmatically in certain situations.  Both styles are thus supported inside doc comments,
        // but the open and close delimiters must match for each pair.
        protected TypeRefBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// The array ranks of the type reference (if any).
        /// </summary>
        public virtual List<int> ArrayRanks
        {
            get { return _arrayRanks; }
            set { _arrayRanks = value; }
        }

        /// <summary>
        /// True if the type reference has any array ranks.
        /// </summary>
        public bool HasArrayRanks
        {
            get { return (ArrayRanks != null && ArrayRanks.Count > 0); }
        }

        /// <summary>
        /// True if there are any type arguments.
        /// </summary>
        public bool HasTypeArguments
        {
            get { return (TypeArguments != null && TypeArguments.Count > 0); }
        }

        /// <summary>
        /// True if the type reference is an array.
        /// </summary>
        public bool IsArray
        {
            get { return (ArrayRanks != null && ArrayRanks.Count > 0); }
        }

        /// <summary>
        /// True if the referenced type is a built-in type (has a keyword).
        /// </summary>
        public virtual bool IsBuiltInType
        {
            get { return false; }
        }

        /// <summary>
        /// True if the referenced type is a delegate type.
        /// </summary>
        public override bool IsDelegateType
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if the TypeRefBase is a DocCodeRefBase reference to a code object, otherwise false.
        /// </summary>
        public bool IsDocCodeReference
        {
            get
            {
                CodeObject lastChild = this;
                CodeObject parent = Parent;
                while (parent is Dot)
                {
                    lastChild = parent;
                    parent = parent.Parent;
                }
                return (parent is DocCodeRefBase || (parent is Call && parent.Parent is DocCodeRefBase && ((Call)parent).Expression == lastChild));
            }
        }

        /// <summary>
        /// True if the referenced type is an interface.
        /// </summary>
        public virtual bool IsInterface
        {
            get { return false; }
        }

        /// <summary>
        /// True if the referenced type or method has internal access.
        /// </summary>
        public virtual bool IsInternal { get { return false; } }

        /// <summary>
        /// True if the referenced type is a nullable type.
        /// </summary>
        public virtual bool IsNullableType
        {
            get { return false; }
        }

        /// <summary>
        /// True if the referenced type or method has private access.
        /// </summary>
        public virtual bool IsPrivate { get { return false; } }

        /// <summary>
        /// True if the referenced type or method has protected access.
        /// </summary>
        public virtual bool IsProtected { get { return false; } }

        /// <summary>
        /// True if the referenced type or method has public access.
        /// </summary>
        public virtual bool IsPublic { get { return false; } }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_typeArguments == null || _typeArguments.Count == 0
              || ((_typeArguments[0] == null || !_typeArguments[0].IsFirstOnLine) && _typeArguments.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value && _typeArguments != null && _typeArguments.Count > 0)
                {
                    _typeArguments[0].IsFirstOnLine = false;
                    _typeArguments.IsSingleLine = true;
                }
            }
        }

        /// <summary>
        /// True if the referenced type or method is static.
        /// </summary>
        public virtual bool IsStatic { get { return false; } }

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        public override CodeObject Parent
        {
            set
            {
                base.Parent = value;

                // Workaround until the CodeObject.Parent setter has been fixed to recursively remove/add
                // listed annotations for all objects in the child tree.
                if (HasTypeArguments)
                {
                    foreach (Expression expression in TypeArguments)
                    {
                        if (expression != null && expression.Annotations != null)
                        {
                            foreach (Annotation annotation in expression.Annotations)
                            {
                                if (annotation.IsListed)
                                    NotifyListedAnnotationAdded(annotation);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The number of type arguments.
        /// </summary>
        public int TypeArgumentCount
        {
            get { return (TypeArguments != null ? TypeArguments.Count : 0); }
        }

        /// <summary>
        /// The type argument <see cref="Expression"/>s of the reference (if any).
        /// </summary>
        public virtual ChildList<Expression> TypeArguments
        {
            get { return _typeArguments; }
            set { SetField(ref _typeArguments, value); }
        }

        /// <summary>
        /// Format an array rank as a string.
        /// </summary>
        public static string ArrayRankToString(int rank)
        {
            return ParseTokenArrayStart + new string(',', rank - 1) + ParseTokenArrayEnd;
        }

        /// <summary>
        /// Render type constraints as text.
        /// </summary>
        public static void AsTextConstraints(CodeWriter writer, IEnumerable<GenericParameter> genericParameters)
        {
            foreach (GenericParameter genericParameter in genericParameters)
            {
                bool first = true;
                bool isValueType = false;
                Mono.Cecil.GenericParameterAttributes attributes = genericParameter.Attributes;

                if (attributes.HasFlag(Mono.Cecil.GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    AsTextConstraintPrefix(writer, ref first, genericParameter);
                    writer.Write(ClassConstraint.ParseToken);
                }
                if (attributes.HasFlag(Mono.Cecil.GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    AsTextConstraintPrefix(writer, ref first, genericParameter);
                    writer.Write(StructConstraint.ParseToken);
                    isValueType = true;
                }

                foreach (var constraint in genericParameter.Constraints)
                {
                    if (constraint.ConstraintType.FullName != "System.ValueType")  // Already handled above
                    {
                        AsTextConstraintPrefix(writer, ref first, genericParameter);
                        AsTextTypeReference(writer, constraint.ConstraintType, RenderFlags.None);
                    }
                }

                if (attributes.HasFlag(Mono.Cecil.GenericParameterAttributes.DefaultConstructorConstraint) && !isValueType)
                {
                    AsTextConstraintPrefix(writer, ref first, genericParameter);
                    writer.Write(NewConstraint.ParseToken);
                    writer.Write(ParseTokenStartGroup + ParseTokenEndGroup);
                }
            }
        }

        /// <summary>
        /// Render type constraints as text.
        /// </summary>
        public static void AsTextConstraints(CodeWriter writer, Type[] typeParameters)
        {
            foreach (Type typeParameter in typeParameters)
            {
                bool first = true;
                bool isValueType = false;
                System.Reflection.GenericParameterAttributes attributes = typeParameter.GenericParameterAttributes;

                if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    AsTextConstraintPrefix(writer, ref first, typeParameter);
                    writer.Write(ClassConstraint.ParseToken);
                }
                if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    AsTextConstraintPrefix(writer, ref first, typeParameter);
                    writer.Write(StructConstraint.ParseToken);
                    isValueType = true;
                }

                foreach (Type constraint in typeParameter.GetGenericParameterConstraints())
                {
                    if (constraint != typeof(ValueType))  // Already handled above
                    {
                        AsTextConstraintPrefix(writer, ref first, typeParameter);
                        AsTextType(writer, constraint, RenderFlags.None);
                    }
                }

                if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint) && !isValueType)
                {
                    AsTextConstraintPrefix(writer, ref first, typeParameter);
                    writer.Write(NewConstraint.ParseToken);
                    writer.Write(ParseTokenStartGroup + ParseTokenEndGroup);
                }
            }
        }

        /// <summary>
        /// Render a generic member as text.
        /// </summary>
        public static void AsTextGenericMember<T>(CodeWriter writer, string name, IEnumerable<T> args, RenderFlags flags) where T : TypeReference
        {
            writer.Write(name);
            if (!flags.HasFlag(RenderFlags.SuppressTypeArgs))
            {
                // Render the angle brackets as braces if we're inside a documentation comment
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentStart : ParseTokenArgumentStart);
                bool first = true;
                foreach (T typeArg in args)
                {
                    if (!first)
                        writer.Write(ParseTokenSeparator + " ");
                    AsTextTypeReference(writer, typeArg, RenderFlags.None);
                    first = false;
                }
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentEnd : ParseTokenArgumentEnd);
            }
        }

        /// <summary>
        /// Render a generic member as text.
        /// </summary>
        public static void AsTextGenericMember(CodeWriter writer, string name, Type[] args, RenderFlags flags)
        {
            writer.Write(name);
            if (!flags.HasFlag(RenderFlags.SuppressTypeArgs))
            {
                // Render the angle brackets as braces if we're inside a documentation comment
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentStart : ParseTokenArgumentStart);
                bool first = true;
                foreach (Type typeArg in args)
                {
                    if (!first)
                        writer.Write(ParseTokenSeparator + " ");
                    AsTextType(writer, typeArg, RenderFlags.None);
                    first = false;
                }
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentEnd : ParseTokenArgumentEnd);
            }
        }

        /// <summary>
        /// Render a <see cref="Type"/> as text.
        /// </summary>
        public static void AsTextType(CodeWriter writer, Type type, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            // Dereference (remove the trailing '&' or '*') if it's a reference type or pointer type
            bool isDelegate = false;
            if (type.IsByRef)
                type = type.GetElementType();

            bool isDescription = flags.HasFlag(RenderFlags.Description);
            if (isDescription)
            {
                string declType = null;
                if (type.IsClass)
                {
                    if (TypeUtil.IsDelegateType(type))
                    {
                        declType = DelegateDecl.ParseToken;
                        isDelegate = true;
                    }
                    else if (!type.IsGenericParameter)
                        declType = ClassDecl.ParseToken;
                }
                else if (type.IsInterface)
                    declType = InterfaceDecl.ParseToken;
                else if (type.IsEnum)
                    declType = EnumDecl.ParseToken;
                else if (type.IsValueType)
                    declType = StructDecl.ParseToken;

                MethodInfo delegateInvokeMethodInfo = (isDelegate ? TypeUtil.GetInvokeMethod(type) : null);
                if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                {
                    Attribute.AsTextAttributes(writer, type);
                    if (delegateInvokeMethodInfo != null)
                        Attribute.AsTextAttributes(writer, delegateInvokeMethodInfo.ReturnParameter, AttributeTarget.Return);
                }
                if (type.IsGenericParameter)
                {
                    writer.Write("(type parameter) ");
                    if (type.DeclaringMethod != null)
                        AsTextGenericMember(writer, type.DeclaringMethod.Name, type.DeclaringMethod.GetGenericArguments(), passFlags);
                    else if (type.DeclaringType != null)
                        AsTextType(writer, type.DeclaringType, passFlags);
                    writer.Write(Dot.ParseToken);
                }
                else
                    writer.Write(ModifiersHelpers.AsString(GetTypeModifiers(type)) + declType + " ");
                if (delegateInvokeMethodInfo != null)
                {
                    Type returnType = delegateInvokeMethodInfo.ReturnType;
                    AsTextType(writer, returnType, RenderFlags.None);
                    writer.Write(" ");
                }
            }

            bool hasDotPrefix = flags.HasFlag(RenderFlags.HasDotPrefix);

            // Go by local generic arguments instead of IsGenericType so that nested types with generic
            // enclosing types aren't treated as generic unless they have their own type arguments.
            string keyword;
            Type[] genericArguments = TypeUtil.GetLocalGenericArguments(type);
            if (genericArguments != null && genericArguments.Length > 0)
            {
                // Render "Nullable<Type>" as "Type?"
                if (TypeUtil.IsNullableType(type))
                {
                    Type typeParam = genericArguments[0];
                    if (!hasDotPrefix && TypeNameToKeywordMap.TryGetValue(typeParam.Name, out keyword) && typeParam.Namespace == "System")
                        writer.Write(keyword + ParseTokenNullable);
                    else
                    {
                        AsTextType(writer, typeParam, RenderFlags.None);
                        writer.Write(ParseTokenNullable);
                    }
                }
                else
                {
                    if (type.IsNested && !hasDotPrefix && flags.HasFlag(RenderFlags.ShowParentTypes))
                    {
                        AsTextType(writer, type.DeclaringType, passFlags);
                        writer.Write(Dot.ParseToken);
                    }
                    AsTextGenericMember(writer, TypeUtil.NonGenericName(type), genericArguments, flags);
                }
            }
            else if (type.IsGenericParameter)
                writer.WriteIdentifier(type.Name, flags);
            else if (type.IsArray)
            {
                AsTextType(writer, type.GetElementType(), RenderFlags.None);
                writer.Write(ArrayRankToString(type.GetArrayRank()));
            }
            else if (!hasDotPrefix && !isDescription && TypeNameToKeywordMap.TryGetValue(type.Name, out keyword) && type.Namespace == "System")
            {
                writer.Write(keyword);
            }
            else
            {
                if (type.IsNested && !hasDotPrefix && flags.HasFlag(RenderFlags.ShowParentTypes))
                {
                    AsTextType(writer, type.DeclaringType, passFlags);
                    writer.Write(Dot.ParseToken);
                }
                writer.WriteName(type.Name, flags, true);
            }

            if (isDescription)
            {
                if (isDelegate)
                {
                    MethodInfo delegateInvokeMethodInfo = TypeUtil.GetInvokeMethod(type);
                    if (delegateInvokeMethodInfo != null)
                        MethodRef.AsTextMethodParameters(writer, delegateInvokeMethodInfo, RenderFlags.None);
                }
                else
                {
                    bool hasBase = false;

                    // Render base type (if any)
                    if (!type.IsValueType)  // Don't show the implicit ValueType base for structs
                    {
                        Type baseType = type.IsEnum ? Enum.GetUnderlyingType(type) : type.BaseType;
                        if (baseType != null && baseType != typeof(object) && (!type.IsEnum || baseType != typeof(int)))
                        {
                            writer.Write(" " + BaseListTypeDecl.ParseToken + " ");
                            AsTextType(writer, baseType, RenderFlags.None);
                            hasBase = true;
                        }
                    }

                    // Render implemented interfaces (if any)
                    if (!type.IsEnum)  // Don't show the interfaces of the implicit Enum base for enums
                    {
                        Type[] interfaces = type.GetInterfaces();
                        if (interfaces.Length > 0)
                        {
                            // There's no way to tell which interfaces are implemented by the current type as
                            // opposed to one of it's base types.  We could subtract all interfaces from the base
                            // type, but that wouldn't be perfect since the same interface can be implemented at
                            // multiple levels.  Instead, display them all, since an external user really just
                            // cares that they're implemented by the type, and doesn't care if it's really by a
                            // base type.
                            foreach (Type @interface in interfaces)
                            {
                                writer.Write((hasBase ? ParseTokenSeparator : (" " + BaseListTypeDecl.ParseToken)) + " ");
                                AsTextType(writer, @interface, RenderFlags.None);
                                hasBase = true;
                            }
                        }
                    }

                    // Render any type constraints for local type arguments
                    if (type.IsGenericType)
                        AsTextConstraints(writer, TypeUtil.GetLocalGenericArguments(type.GetGenericTypeDefinition()));
                }
            }
        }

        /// <summary>
        /// Render a <see cref="TypeReference"/> as text.
        /// </summary>
        public static void AsTextTypeReference(CodeWriter writer, TypeReference typeReference, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            // Dereference (remove the trailing '&' or '*') if it's a reference type or pointer type
            bool isDelegate = false;
            if (typeReference.IsByReference)
                typeReference = ((ByReferenceType)typeReference).ElementType;

            TypeDefinition typeDefinition = null;
            try { typeDefinition = typeReference.Resolve(); }
            catch { }

            bool isDescription = flags.HasFlag(RenderFlags.Description);
            if (isDescription)
            {
                string declType = null;
                MethodDefinition delegateInvokeMethodInfo = null;
                if (typeDefinition != null)
                {
                    // For Mono, IsValueType and IsClass are also true for enums, so we have to check IsEnum first
                    if (typeDefinition.IsEnum)
                        declType = EnumDecl.ParseToken;
                    else if (typeDefinition.IsValueType)
                        declType = StructDecl.ParseToken;
                    else if (typeDefinition.IsClass)
                    {
                        if (TypeDefinitionUtil.IsDelegateType(typeDefinition))
                        {
                            declType = DelegateDecl.ParseToken;
                            isDelegate = true;
                        }
                        else
                            declType = ClassDecl.ParseToken;
                    }
                    else if (typeDefinition.IsInterface)
                        declType = InterfaceDecl.ParseToken;

                    delegateInvokeMethodInfo = (isDelegate ? TypeDefinitionUtil.GetInvokeMethod(typeDefinition) : null);
                    if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                    {
                        Attribute.AsTextAttributes(writer, typeDefinition);
                        if (delegateInvokeMethodInfo != null)
                            Attribute.AsTextAttributes(writer, delegateInvokeMethodInfo.MethodReturnType, AttributeTarget.Return);
                    }
                }
                if (typeReference.IsGenericParameter)
                {
                    writer.Write("(type parameter) ");
                    IGenericParameterProvider owner = ((GenericParameter)typeReference).Owner;
                    if (owner is TypeReference)
                        AsTextTypeReference(writer, (TypeReference)owner, passFlags);
                    else if (owner is MethodDefinition)
                        AsTextGenericMember(writer, ((MethodDefinition)owner).Name, (IEnumerable<TypeReference>)((MethodDefinition)owner).GenericParameters, passFlags);
                    writer.Write(Dot.ParseToken);
                }
                else
                {
                    if (typeDefinition != null)
                        writer.Write(ModifiersHelpers.AsString(GetTypeModifiers(typeDefinition)));
                    writer.Write(declType + " ");
                }
                if (delegateInvokeMethodInfo != null)
                {
                    TypeReference returnType = delegateInvokeMethodInfo.ReturnType;
                    AsTextTypeReference(writer, returnType, RenderFlags.None);
                    writer.Write(" ");
                }
            }

            bool hasDotPrefix = flags.HasFlag(RenderFlags.HasDotPrefix);

            // Go by local generic arguments instead of IsGenericType so that nested types with generic
            // enclosing types aren't treated as generic unless they have their own type arguments.
            string keyword;
            IEnumerable<TypeReference> genericArguments = TypeDefinitionUtil.GetLocalGenericArguments(typeReference);
            if (genericArguments != null)
            {
                // Render "Nullable<Type>" as "Type?"
                if (TypeDefinitionUtil.IsNullableType(typeReference))
                {
                    TypeReference typeParam = Enumerable.First(genericArguments);
                    if (!hasDotPrefix && TypeNameToKeywordMap.TryGetValue(typeParam.Name, out keyword) && typeParam.Namespace == "System")
                        writer.Write(keyword + ParseTokenNullable);
                    else
                    {
                        AsTextTypeReference(writer, typeParam, RenderFlags.None);
                        writer.Write(ParseTokenNullable);
                    }
                }
                else
                {
                    if (typeReference.IsNested && !hasDotPrefix && flags.HasFlag(RenderFlags.ShowParentTypes))
                    {
                        AsTextTypeReference(writer, typeReference.DeclaringType, passFlags);
                        writer.Write(Dot.ParseToken);
                    }
                    AsTextGenericMember(writer, TypeDefinitionUtil.NonGenericName(typeReference), genericArguments, flags);
                }
            }
            else if (typeReference.IsGenericParameter)
                writer.WriteIdentifier(typeReference.Name, flags);
            else if (typeReference.IsArray)
            {
                ArrayType arrayType = (ArrayType)typeReference;
                AsTextTypeReference(writer, arrayType.ElementType, RenderFlags.None);
                writer.Write(ArrayRankToString(arrayType.Rank));
            }
            else if (!hasDotPrefix && !isDescription && TypeNameToKeywordMap.TryGetValue(typeReference.Name, out keyword) && typeReference.Namespace == "System")
            {
                writer.Write(keyword);
            }
            else
            {
                if (typeReference.IsNested && !hasDotPrefix && flags.HasFlag(RenderFlags.ShowParentTypes))
                {
                    AsTextTypeReference(writer, typeReference.DeclaringType, passFlags);
                    writer.Write(Dot.ParseToken);
                }
                writer.WriteName(typeReference.Name, flags, true);
            }

            if (isDescription && typeDefinition != null)
            {
                if (isDelegate)
                    MethodRef.AsTextMethodParameters(writer, TypeDefinitionUtil.GetInvokeMethod(typeDefinition), RenderFlags.None);
                else
                {
                    bool hasBase = false;

                    // Render base type (if any)
                    if (!typeDefinition.IsValueType)  // Don't show the implicit ValueType base for structs
                    {
                        TypeReference baseType = (typeDefinition.IsEnum ? TypeDefinitionUtil.GetUnderlyingTypeOfEnum(typeDefinition) : typeDefinition.BaseType);
                        if (baseType != null && baseType.FullName != "System.Object" && !(typeDefinition.IsEnum && baseType.FullName == "System.Int32"))
                        {
                            writer.Write(" " + BaseListTypeDecl.ParseToken + " ");
                            AsTextTypeReference(writer, baseType, RenderFlags.None);
                            hasBase = true;
                        }
                    }

                    // Render implemented interfaces (if any)
                    if (!typeDefinition.IsEnum)  // Don't show the interfaces of the implicit Enum base for enums
                    {
                        Collection<InterfaceImplementation> interfaces = typeDefinition.Interfaces;
                        if (interfaces != null && interfaces.Count > 0)
                        {
                            // There's no way to tell which interfaces are implemented by the current type as
                            // opposed to one of it's base types.  We could subtract all interfaces from the base
                            // type, but that wouldn't be perfect since the same interface can be implemented at
                            // multiple levels.  Instead, display them all, since an external user really just
                            // cares that they're implemented by the type, and doesn't care if it's really by a
                            // base type.
                            foreach (var @interface in interfaces)
                            {
                                writer.Write((hasBase ? ParseTokenSeparator : (" " + BaseListTypeDecl.ParseToken)) + " ");
                                AsTextTypeReference(writer, @interface.InterfaceType, RenderFlags.None);
                                hasBase = true;
                            }
                        }
                    }

                    // Render any type constraints for local type arguments
                    if (typeDefinition.HasGenericParameters)
                        AsTextConstraints(writer, TypeDefinitionUtil.GetLocalGenericArguments(typeDefinition));
                }
            }
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="TypeDefinition"/>.
        /// </summary>
        public static Modifiers GetTypeModifiers(TypeDefinition typeDefinition)
        {
            Modifiers modifiers = 0;
            if (!typeDefinition.IsNested)
            {
                if (typeDefinition.IsPublic)
                    modifiers |= Modifiers.Public;
                else if (typeDefinition.IsNotPublic)
                    modifiers |= Modifiers.Internal;
            }
            else
            {
                if (typeDefinition.IsNestedPublic)
                    modifiers |= Modifiers.Public;
                if (typeDefinition.IsNestedFamily || typeDefinition.IsNestedFamilyOrAssembly)
                    modifiers |= Modifiers.Protected;
                if (typeDefinition.IsNestedAssembly || typeDefinition.IsNestedFamilyOrAssembly)
                    modifiers |= Modifiers.Internal;
                if (typeDefinition.IsNestedPrivate)
                    modifiers |= Modifiers.Private;
            }
            if (TypeDefinitionUtil.IsStatic(typeDefinition))
                modifiers |= Modifiers.Static;
            else
            {
                if (typeDefinition.IsAbstract)
                    modifiers |= Modifiers.Abstract;
                // Enums, Structs, and Delegates are implicitly 'sealed', so don't display it
                if (typeDefinition.IsSealed && !typeDefinition.IsEnum && !typeDefinition.IsValueType && !TypeDefinitionUtil.IsDelegateType(typeDefinition))
                    modifiers |= Modifiers.Sealed;
            }
            // 'new' isn't relevant to external users, so don't bother figuring it out (we could look
            // at IsHideBySig, but we'd have to further determine if it's the hide-er or the hide-e).
            // 'partial' isn't relevant to external users.
            return modifiers;
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="Type"/>.
        /// </summary>
        public static Modifiers GetTypeModifiers(Type type)
        {
            Modifiers modifiers = 0;
            if (!type.IsNested)
            {
                if (type.IsPublic)
                    modifiers |= Modifiers.Public;
                else if (type.IsNotPublic)
                    modifiers |= Modifiers.Internal;
            }
            else
            {
                if (type.IsNestedPublic)
                    modifiers |= Modifiers.Public;
                if (type.IsNestedFamily || type.IsNestedFamORAssem)
                    modifiers |= Modifiers.Protected;
                if (type.IsNestedAssembly || type.IsNestedFamORAssem)
                    modifiers |= Modifiers.Internal;
                if (type.IsNestedPrivate)
                    modifiers |= Modifiers.Private;
            }
            if (TypeUtil.IsStatic(type))
                modifiers |= Modifiers.Static;
            else
            {
                if (type.IsAbstract)
                    modifiers |= Modifiers.Abstract;
                // Enums, Structs, and Delegates are implicitly 'sealed', so don't display it
                if (type.IsSealed && !type.IsEnum && !type.IsValueType && !TypeUtil.IsDelegateType(type))
                    modifiers |= Modifiers.Sealed;
            }
            // 'new' isn't relevant to external users, so don't bother figuring it out (we could look
            // at IsHideBySig, but we'd have to further determine if it's the hide-er or the hide-e).
            // 'partial' isn't relevant to external users.
            return modifiers;
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Type"/> to a <see cref="TypeRefBase"/> (actually, a <see cref="TypeRef"/>).
        /// </summary>
        /// <remarks>This allows Types such as <c>typeof(int)</c> to be passed directly to any method
        /// expecting a <see cref="TypeRefBase"/> type without having to create a reference first.</remarks>
        /// <param name="type">The <see cref="Type"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeRef"/> to the specified <see cref="Type"/>.</returns>
        public static implicit operator TypeRefBase(Type type)
        {
            return TypeRef.Create(type);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="TypeDecl"/> to a <see cref="TypeRefBase"/> (actually, a <see cref="TypeRef"/>).
        /// </summary>
        /// <remarks>This allows type declarations to be passed directly to any method expecting a <see cref="TypeRefBase"/>
        /// type without having to create a reference first.</remarks>
        /// <param name="typeDecl">The <see cref="TypeDecl"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeRef"/> to the specified <see cref="TypeDecl"/>.</returns>
        public static implicit operator TypeRefBase(TypeDecl typeDecl)
        {
            return typeDecl.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="TypeParameter"/> to a <see cref="TypeRefBase"/> (actually, a <see cref="TypeParameterRef"/>).
        /// </summary>
        /// <remarks>This allows TypeParameters to be passed directly to any method expecting a <see cref="TypeRefBase"/> type
        /// without having to create a reference first.</remarks>
        /// <param name="typeParameter">The <see cref="TypeParameter"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeParameterRef"/> to the specified <see cref="TypeParameter"/>.</returns>
        public static implicit operator TypeRefBase(TypeParameter typeParameter)
        {
            return (TypeRefBase)typeParameter.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="MethodBase"/> to a <see cref="TypeRefBase"/> (actually, a <see cref="MethodRef"/> or <see cref="ConstructorRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="MethodBase"/>s (<see cref="MethodInfo"/>s or <see cref="ConstructorInfo"/>s) to be passed directly
        /// to any method expecting a <see cref="TypeRefBase"/> type without having to create a reference first.</remarks>
        /// <param name="methodBase">The <see cref="MethodBase"/> to be converted.</param>
        /// <returns>A generated <see cref="MethodRef"/> to the specified <see cref="MethodBase"/>.</returns>
        public static implicit operator TypeRefBase(MethodBase methodBase)
        {
            return MethodRef.Create(methodBase);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="MethodDeclBase"/> to a <see cref="TypeRefBase"/> (actually, a <see cref="MethodRef"/> or <see cref="ConstructorRef"/>).
        /// </summary>
        /// <remarks>This allows method declarations to be passed directly to any method expecting a <see cref="TypeRefBase"/>
        /// type without having to create a reference first.</remarks>
        /// <param name="methodDeclBase">The <see cref="MethodDeclBase"/> to be converted.</param>
        /// <returns>A generated <see cref="MethodRef"/> to the specified <see cref="MethodDeclBase"/>.</returns>
        public static implicit operator TypeRefBase(MethodDeclBase methodDeclBase)
        {
            return (MethodRef)methodDeclBase.CreateRef();
        }

        /// <summary>
        /// Parse a list of type arguments.
        /// </summary>
        public static ChildList<Expression> ParseTypeArgumentList(Parser parser, CodeObject parent)
        {
            ChildList<Expression> typeArguments = null;

            // Assume we're already at a '<' (or '{') when we start
            string argumentEnd = (parser.TokenText == ParseTokenAltArgumentStart ? ParseTokenAltArgumentEnd : ParseTokenArgumentEnd);

            // Parse the type argument list
            while (parser.Token != null && parser.TokenText != argumentEnd
                && (parser.TokenText != RightShift.ParseToken || parser.TokenText == ParseTokenAltArgumentEnd))
            {
                parser.NextToken();  // Move past start token or ','
                if (typeArguments == null)
                    typeArguments = new ChildList<Expression>(parent);
                typeArguments.Add(Parse(parser, parent, true, argumentEnd));
            }

            if (parser.Token != null)
            {
                // If we stopped at a '>>' token, transmute it into a '>'
                if (parser.TokenText == RightShift.ParseToken)
                {
                    parser.Token.Text = parser.TokenText = ParseTokenArgumentEnd;
                    ++parser.Token.ColumnNumber;
                }
                else
                    parser.NextToken();  // Move past end token
            }

            return typeArguments;
        }

        /// <summary>
        /// Peek ahead at the input tokens to determine if they look like valid array rank specifiers.
        /// </summary>
        public static bool PeekArrayRanks(Parser parser)
        {
            bool valid = true;
            while (true)
            {
                Token token = parser.PeekNextToken();
                if (token != null && token.Text == ParseTokenSeparator)
                {
                    // Handle any # of commas (w/o anything else)
                    do
                        token = parser.PeekNextToken();
                    while (token != null && token.Text == ParseTokenSeparator);
                }
                // Verify ending ']'
                if (token == null || token.Text != ParseTokenArrayEnd)
                    valid = false;
                if (parser.PeekNextTokenText() != ParseTokenArrayStart)
                    break;
            }
            return valid;
        }

        /// <summary>
        /// Peek ahead at the input tokens to determine if they look like a valid type.
        /// </summary>
        /// <remarks>
        /// A valid type is usually a single identifier, but it can optionally have one
        /// or more of the following:
        ///    - trailing type arguments (angle brackets, or braces if in a doc comment)
        ///    - a trailing '?' or '*'
        ///    - namespace prefix(es) (".") or a global namespace specifier ("::")
        ///    - parent type prefix(es) ("."), which can optionally have type parameters of their own
        /// </remarks>
        public static bool PeekType(Parser parser, Token token, bool nonArrayType, ParseFlags flags)
        {
            bool valid = false;

            // A type must start with an identifier
            if (token != null && token.IsIdentifier)
            {
                valid = true;

                // We have a valid type, but are we done yet?
                token = parser.PeekNextToken();
                if (token != null && token.IsSymbol)
                {
                    // A '<' means we need to check for type arguments
                    if (token.Text == ParseTokenArgumentStart)
                    {
                        valid = PeekTypeArguments(parser, ParseTokenArgumentEnd, flags);
                        token = parser.LastPeekedToken;
                    }
                    // A '{' in a doc comment means we need to check for alternate-form type arguments
                    if (parser.InDocComment && token.Text == ParseTokenAltArgumentStart)
                    {
                        valid = PeekTypeArguments(parser, ParseTokenAltArgumentEnd, flags);
                        token = parser.LastPeekedToken;
                    }
                    // A '?' means we have a valid nullable type
                    else if (token.Text == ParseTokenNullable)
                        token = parser.PeekNextToken();  // Move past '?'

                    // A '.' or '::' means we had a namespace or type prefix, so start all over
                    if (valid && (token.Text == Dot.ParseToken || token.Text == Lookup.ParseToken))
                        valid = PeekType(parser, parser.PeekNextToken(), nonArrayType, flags);
                    // We have a valid type - now check for any array ranks
                    else if (valid && !nonArrayType && token.Text == ParseTokenArrayStart)
                        valid = PeekArrayRanks(parser);
                }
            }
            return valid;
        }

        /// <summary>
        /// Peek ahead at the input tokens to determine if they look like a valid type argument list.
        /// </summary>
        public static bool PeekTypeArguments(Parser parser, string argumentEnd, ParseFlags flags)
        {
            // Turn off any Arguments flag for any nested generic arguments, as the Grammar Ambiguity logic only applies to
            // arguments to methods and not type arguments, so this is necessary to prevent parse errors.
            ParseFlags passFlags = flags & ~ParseFlags.Arguments;

            Token token;
            do
            {
                token = parser.PeekNextToken();
                if (token == null) return false;

                // Check for an attribute
                if (token.Text == Attribute.ParseTokenStart)
                {
                    // Verify that we have a matching ']' for the '['.  Parse in a very simplified manner for efficiency, just
                    // keeping track of nested '[]' pairs until we find the matching ']'.
                    int nestLevel = 0;
                    while (true)
                    {
                        token = parser.PeekNextToken();
                        if (token == null) return false;
                        if (token.IsSymbol)
                        {
                            // Keep track of nested brackets
                            string nextText = token.Text;
                            if (nextText == Attribute.ParseTokenStart)
                                ++nestLevel;
                            else if (nextText == Attribute.ParseTokenEnd)
                            {
                                if (nestLevel == 0)
                                {
                                    token = parser.PeekNextToken();
                                    break;
                                }
                                --nestLevel;
                            }
                        }
                    }
                }

                // Check for a valid identifier or type
                if (token.IsIdentifier)
                {
                    if (!PeekType(parser, token, false, passFlags)) return false;
                    token = parser.LastPeekedToken;
                    if (token == null) return false;
                }
            }
            while (token.Text == ParseTokenSeparator);  // Repeat if there's a comma

            // Check for ending '>' (or '}')
            if (token.Text == argumentEnd)
            {
                parser.PeekNextToken();

                // Grammar ambiguity:
                // Disallow type arguments if parsing an expression vs a namespace-or-type-name, and they aren't
                // followed by certain symbols.  We further restrict this special case to an expression being
                // passed to an ArgumentsOperator, since that's the situation where the commas can make it ambiguous.
                if (flags.HasFlag(ParseFlags.Arguments))
                    return TypeArgumentTerminators.Contains(parser.LastPeekedTokenText);
                return true;
            }

            // If we ended with a '>>' token, split it into two '>'
            // (this is OK to do, because we know we've got a valid set of type arguments at this point)
            if (token.Text == RightShift.ParseToken && argumentEnd == ParseTokenArgumentEnd)
            {
                token.Text = ParseTokenArgumentEnd;
                ++token.ColumnNumber;
                parser.AddPeekAheadToken(token);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Render array rank brackets (if any) as text.
        /// </summary>
        public void AsTextArrayRanks(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.SuppressBrackets))
            {
                // Use the array ranks if we have any
                if (_arrayRanks != null)
                {
                    foreach (int ranks in _arrayRanks)
                        writer.Write(ArrayRankToString(ranks));
                }
                //else if (TypeRefBase.Reference is TypeDefinition)  // A TypeDefinition won't have any ranks
                else if (_reference is Type)
                    AsTextArrayRank(writer, (Type)_reference);
            }
        }

        /// <summary>
        /// Render type arguments (if any) as text.
        /// </summary>
        public void AsTextTypeArguments(CodeWriter writer, IEnumerable<Expression> typeArguments, RenderFlags flags)
        {
            if (typeArguments != null)
            {
                // Don't pass Description, SuppressBrackets, or ShowParentTypes on to any type arguments, and don't display EOL comments if this is a Description
                RenderFlags passFlags = (flags & (RenderFlags.PassMask & ~(RenderFlags.Description | RenderFlags.SuppressBrackets | RenderFlags.ShowParentTypes)))
                    | (flags.HasFlag(RenderFlags.Description) ? RenderFlags.NoEOLComments : 0);

                // Render the angle brackets as braces if we're inside a documentation comment
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentStart : ParseTokenArgumentStart);
                writer.WriteList(typeArguments, passFlags, this);
                writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltArgumentEnd : ParseTokenArgumentEnd);
            }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            TypeRefBase clone = (TypeRefBase)base.Clone();
            if (_arrayRanks != null && _arrayRanks.Count > 0)
                clone._arrayRanks = new List<int>(_arrayRanks);
            clone._typeArguments = ChildListHelpers.Clone(_typeArguments, clone);
            return clone;
        }

        /// <summary>
        /// Convert all OpenTypeParameterRef type arguments to TypeParameterRefs.
        /// </summary>
        public void ConvertOpenTypeParameters()
        {
            int typeArgumentCount = TypeArgumentCount;
            for (int i = 0; i < typeArgumentCount; ++i)
            {
                Expression typeArgument = TypeArguments[i];
                if (typeArgument is OpenTypeParameterRef)
                    TypeArguments[i] = ((OpenTypeParameterRef)typeArgument).ConvertToTypeParameterRef();
                else if (typeArgument is TypeRef)
                    ((TypeRef)typeArgument).ConvertOpenTypeParameters();
            }
        }

        /// <summary>
        /// Create the child list of array ranks, or return the existing one.
        /// </summary>
        public virtual List<int> CreateArrayRanks()
        {
            if (_arrayRanks == null)
                _arrayRanks = new List<int>();
            return _arrayRanks;
        }

        /// <summary>
        /// Create the child list of type argument <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        public ChildList<Expression> CreateTypeArguments()
        {
            if (_typeArguments == null)
                _typeArguments = new ChildList<Expression>(this);
            return _typeArguments;
        }

        /// <summary>
        /// Evaluate the types of any type arguments of the TypeRef by searching for matching type
        /// arguments in the specified parent code object tree.
        /// </summary>
        public virtual TypeRefBase EvaluateTypeArgumentTypes(CodeObject parent, CodeObject originatingChild)
        {
            // Check that we have a parent expression, and also a TypeParameterRef somewhere in our
            // type arguments - otherwise there's nothing to do.  We do this check to avoid creating
            // a new reference below unless absolutely necessary.
            if (parent != null && HasTypeArguments)
            {
                ChildList<Expression> typeArguments = TypeArguments;
                if (HasTypeParameterArgument(typeArguments))
                {
                    // We must create a new reference, so we can evaluate the type arguments
                    TypeRefBase newTypeRef = (TypeRefBase)Clone();
                    for (int i = 0; i < typeArguments.Count; ++i)
                    {
                        TypeRefBase typeRef = typeArguments[i].EvaluateType();
                        newTypeRef.TypeArguments[i] = (typeRef != null ? typeRef.EvaluateTypeArgumentTypes(parent, originatingChild) : null);
                    }
                    return newTypeRef;
                }
            }
            return this;
        }

        /// <summary>
        /// Evaluate the types of any type arguments of the TypeRef by searching for matching type
        /// arguments in the specified parent code object tree.
        /// </summary>
        public TypeRefBase EvaluateTypeArgumentTypes(CodeObject parent)
        {
            return EvaluateTypeArgumentTypes(parent, null);
        }

        /// <summary>
        /// Find a type argument for the specified type parameter in the type arguments of the type.
        /// </summary>
        public TypeRefBase FindTypeArgumentInTypeArguments(TypeParameterRef typeParameterRef)
        {
            int index = -1;
            object reference = GetReferencedType();
            if (reference is TypeDecl)
                index = ((TypeDecl)reference).FindTypeParameterIndex(typeParameterRef.Reference as TypeParameter);
            else if (reference is TypeDefinition)
                index = TypeDefinitionUtil.FindTypeParameterIndex((TypeDefinition)reference, typeParameterRef.Reference as GenericParameter);
            else if (reference is Type)
                index = TypeUtil.FindTypeParameterIndex((Type)reference, typeParameterRef.Reference as Type);
            if (index >= 0 && index < TypeArgumentCount)
            {
                Expression typeArgument = TypeArguments[index];
                if (typeArgument != null)
                    return typeArgument.EvaluateType();
            }
            return null;
        }

        /// <summary>
        /// Get the value of any represented constant.  For enums, an EnumConstant object will be
        /// returned, which has both the Enum type and a constant value of its underlying type.
        /// </summary>
        public virtual object GetConstantValue()
        {
            return null;
        }

        /// <summary>
        /// Get the delegate parameters if the expression evaluates to a delegate type.
        /// </summary>
        public override ICollection GetDelegateParameters()
        {
            return null;
        }

        /// <summary>
        /// Get the type (or null if none) of the delegate parameter with the specified index.
        /// </summary>
        public TypeRefBase GetDelegateParameterType(int parameterIndex)
        {
            ICollection delegateParameters = GetDelegateParameters();
            if (delegateParameters != null && delegateParameters.Count > parameterIndex)
                return ParameterRef.GetParameterType(delegateParameters, parameterIndex, this);
            return null;
        }

        /// <summary>
        /// Get the delegate return type if the expression evaluates to a delegate type.
        /// </summary>
        public override TypeRefBase GetDelegateReturnType()
        {
            return null;
        }

        /// <summary>
        /// Get the element type of the type reference (if it's an array, otherwise null).
        /// </summary>
        public virtual TypeRefBase GetElementType()
        {
            // If we have array ranks, remove *one* of them to get the element type
            if (HasArrayRanks)
            {
                TypeRefBase elementType = (TypeRefBase)Clone();
                List<int> arrayRanks = elementType.ArrayRanks;
                elementType.ArrayRanks = (arrayRanks.Count == 1 ? null : arrayRanks.GetRange(1, arrayRanks.Count - 1));
                return elementType;
            }
            return null;
        }

        /// <summary>
        /// Get the full name of the object, including the namespace name.
        /// </summary>
        public abstract string GetFullName();

        /// <summary>
        /// Get the actual type reference (ITypeDecl or Type), retrieving them from any constant values if necessary.
        /// </summary>
        public virtual object GetReferencedType()
        {
            return Reference;
        }

        /// <summary>
        /// Get the actual type, excluding any constant values.
        /// </summary>
        public virtual TypeRefBase GetTypeWithoutConstant()
        {
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (ChildListHelpers.HasUnresolvedRef(_typeArguments))
                return true;
            return false;
        }

        /// <summary>
        /// Determine if an implicit identity conversion exists to the specified type.
        /// </summary>
        public virtual bool ImplicitIdentityConversionExists(TypeRefBase toTypeRefBase)
        {
            return IsSameRef(toTypeRefBase);
        }

        /// <summary>
        /// Determine if an implicit reference conversion exists to the specified type.
        /// </summary>
        public virtual bool ImplicitReferenceConversionExists(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            return false;
        }

        /// <summary>
        /// Determine if an implicit reference conversion exists to the specified type.
        /// </summary>
        public bool ImplicitReferenceConversionExists(TypeRefBase toTypeRefBase)
        {
            return ImplicitReferenceConversionExists(toTypeRefBase, false);
        }

        /// <summary>
        /// Determine if the reference is implicitly convertible *from* the specified reference.
        /// </summary>
        /// <param name="fromTypeRefBase">The TypeRef or UnresolvedRef being checked.</param>
        public virtual bool IsImplicitlyConvertibleFrom(TypeRefBase fromTypeRefBase)
        {
            return (fromTypeRefBase != null && fromTypeRefBase.IsImplicitlyConvertibleTo(this));
        }

        /// <summary>
        /// Determine if the TypeRefBase is implicitly convertible to the specified TypeRefBase.
        /// </summary>
        /// <param name="toTypeRefBase">The TypeRef, MethodRef, or UnresolvedRef being checked.</param>
        /// <param name="standardConversionsOnly">True if only standard conversions should be allowed.</param>
        public virtual bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            TypeRefBase typeRef = EvaluateType();
            return (typeRef != null && typeRef.IsImplicitlyConvertibleTo(toTypeRefBase));
        }

        /// <summary>
        /// Determine if the TypeRefBase is implicitly convertible to the specified TypeRefBase.
        /// </summary>
        /// <param name="toTypeRefBase">The TypeRef, MethodRef, or UnresolvedRef being checked.</param>
        public bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase)
        {
            return IsImplicitlyConvertibleTo(toTypeRefBase, false);
        }

        /// <summary>
        /// Determine if the specified TypeRefBase refers to the same generic type, regardless of actual type arguments.
        /// </summary>
        public virtual bool IsSameGenericType(TypeRefBase typeRefBase)
        {
            return false;
        }

        /// <summary>
        /// Make an array reference from the current type reference.
        /// </summary>
        public virtual TypeRefBase MakeArrayRef(List<int> ranksToBeCopied)
        {
            TypeRefBase newRef = (TypeRefBase)Clone();
            if (newRef.HasArrayRanks)
                newRef.ArrayRanks.AddRange(ranksToBeCopied);
            else
                newRef.ArrayRanks = new List<int>(ranksToBeCopied);
            return newRef;
        }

        /// <summary>
        /// Make a generic type reference from the current type reference.
        /// </summary>
        public virtual TypeRefBase MakeGenericRef(params Expression[] typeArguments)
        {
            TypeRefBase newRef = (TypeRefBase)Clone();
            newRef.TypeArguments = null;
            newRef.CreateTypeArguments().AddRange(typeArguments);
            return newRef;
        }

        /// <summary>
        /// Determine if the current type is the same as or occurs in the (potentially nested) type arguments of the specified type.
        /// </summary>
        public bool OccursIn(TypeRefBase typeRef)
        {
            if (IsSameRef(typeRef))
                return true;
            if (typeRef != null && typeRef.HasTypeArguments)
                return Enumerable.Any(typeRef.TypeArguments, delegate (Expression typeArgument) { return OccursIn(typeArgument.EvaluateType()); });
            return false;
        }

        /// <summary>
        /// Parse multiple array ranks.
        /// </summary>
        public virtual void ParseArrayRanks(Parser parser)
        {
            // Parse the ranks
            while (parser.TokenText == ParseTokenArrayStart)
            {
                int ranks = 1;
                parser.NextToken();  // Move past '['
                while (parser.TokenText == ParseTokenSeparator)
                {
                    parser.NextToken();  // Move past ','
                    ++ranks;
                }
                ParseExpectedToken(parser, ParseTokenArrayEnd);  // Move past ']'

                CreateArrayRanks().Add(ranks);
            }
        }

        /// <summary>
        /// Replace all occurrences of one type reference with another, including in (potentially nested) type arguments.
        /// </summary>
        public TypeRefBase ReplaceType(TypeRefBase oldTypeRef, TypeRefBase newTypeRef)
        {
            if (HasTypeArguments)
            {
                // We must create a new reference, so we can replace nested type parameters
                TypeRefBase clone = (TypeRefBase)Clone();
                ChildList<Expression> typeArguments = clone.TypeArguments;
                for (int i = 0; i < typeArguments.Count; ++i)
                    typeArguments[i] = typeArguments[i].EvaluateType().ReplaceType(oldTypeRef, newTypeRef);
                return clone;
            }
            return (IsSameRef(oldTypeRef) ? newTypeRef : this);
        }

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Unresolve instead of resolving if the flag is specified
            TypeRefBase typeRefBase = (flags.HasFlag(ResolveFlags.Unresolve) ? new UnresolvedRef(this, resolveCategory, true) : this);
            ChildListHelpers.Resolve(typeRefBase.TypeArguments, ResolveCategory.Type, flags);
            return typeRefBase;
        }

        protected static ChildList<Expression> DefaultTypeArguments(TypeDecl typeDecl, ChildList<Expression> typeArguments)
        {
            ChildList<TypeParameter> typeParameters = typeDecl.TypeParameters;
            int typeParameterCount = (typeParameters != null ? typeParameters.Count : 0);
            if (typeArguments == null)
            {
                // Use the declared type parameters if none were specified
                typeArguments = new ChildList<Expression>(typeParameterCount);
                if (typeParameters != null)
                {
                    foreach (TypeParameter typeParameter in typeParameters)
                        typeArguments.Add(typeParameter.CreateRef());
                }
            }
            if (typeDecl.IsNested && typeArguments.Count == typeParameterCount)
            {
                // Default to any type parameters from parent types if they were omitted
                TypeDecl parentDecl = typeDecl;
                do
                {
                    parentDecl = parentDecl.DeclaringType;
                    typeParameters = parentDecl.TypeParameters;
                    if (typeParameters != null)
                    {
                        for (int i = 0; i < typeParameters.Count; ++i)
                            typeArguments.Insert(i, typeParameters[i].CreateRef());
                    }
                }
                while (parentDecl.IsNested);
            }
            return typeArguments;
        }

        protected static ChildList<Expression> DefaultTypeArguments(TypeDefinition typeDefinition, ChildList<Expression> typeArguments)
        {
            Collection<GenericParameter> genericArguments = typeDefinition.GenericParameters;
            int count = genericArguments.Count;
            if (typeArguments == null)
            {
                // Use the declared type parameters if none were specified, including those of any enclosing types
                typeArguments = new ChildList<Expression>(count);
                foreach (GenericParameter genericArgument in genericArguments)
                    typeArguments.Add(TypeRef.Create(genericArgument));
            }
            else
            {
                // If only local arguments were specified, then prefix any enclosing type parameters
                int localCount = TypeDefinitionUtil.GetLocalGenericArgumentCount(typeDefinition);
                if (typeArguments.Count == localCount)
                {
                    for (int i = 0; i < count - localCount; ++i)
                        typeArguments.Insert(i, TypeRef.Create(genericArguments[i]));
                }
            }
            return typeArguments;
        }

        protected static ChildList<Expression> DefaultTypeArguments(GenericInstanceType genericInstanceType, ChildList<Expression> typeArguments)
        {
            Collection<TypeReference> genericArguments = genericInstanceType.GenericArguments;
            int count = genericArguments.Count;
            if (typeArguments == null)
            {
                // Use the declared type parameters if none were specified, including those of any enclosing types
                typeArguments = new ChildList<Expression>(count);
                foreach (TypeReference genericArgument in genericArguments)
                    typeArguments.Add(TypeRef.Create(genericArgument));
            }
            else
            {
                // If only local arguments were specified, then prefix any enclosing type parameters
                TypeDefinition typeDefinition = null;
                try { typeDefinition = genericInstanceType.Resolve(); }
                catch { }
                if (typeDefinition != null)
                {
                    int localCount = TypeDefinitionUtil.GetLocalGenericArgumentCount(typeDefinition);
                    if (typeArguments.Count == localCount)
                    {
                        for (int i = 0; i < count - localCount; ++i)
                            typeArguments.Insert(i, TypeRef.Create(genericArguments[i]));
                    }
                }
            }
            return typeArguments;
        }

        protected static ChildList<Expression> DefaultTypeArguments(Type type, ChildList<Expression> typeArguments)
        {
            Type[] genericArguments = type.GetGenericArguments();
            int count = genericArguments.Length;
            if (typeArguments == null)
            {
                // Use the declared type parameters if none were specified, including those of any enclosing types
                typeArguments = new ChildList<Expression>(count);
                foreach (Type genericArgument in genericArguments)
                    typeArguments.Add(TypeRef.Create(genericArgument));
            }
            else
            {
                // If only local arguments were specified, then prefix any enclosing type parameters
                int localCount = TypeUtil.GetLocalGenericArgumentCount(type);
                if (typeArguments.Count == localCount)
                {
                    for (int i = 0; i < count - localCount; ++i)
                        typeArguments.Insert(i, TypeRef.Create(genericArguments[i]));
                }
            }
            return typeArguments;
        }

        /// <summary>
        /// Recursively scan the type arguments to determine if we have a TypeParameterRef anywhere.
        /// </summary>
        protected static bool HasTypeParameterArgument(ChildList<Expression> typeArguments)
        {
            foreach (Expression expression in typeArguments)
            {
                if (expression != null)
                {
                    TypeRefBase typeRefBase = expression.EvaluateType();
                    if (typeRefBase is TypeParameterRef)
                        return true;
                    if (typeRefBase != null && typeRefBase.HasTypeArguments && HasTypeParameterArgument(typeRefBase.TypeArguments))
                        return true;
                }
            }
            return false;
        }

        protected TypeRefBase FindNestedTypeArgument(TypeParameterRef typeParameterRef)
        {
            foreach (Expression typeArgument in TypeArguments)
            {
                if (typeArgument != null)
                {
                    TypeRefBase typeArgumentRef = typeArgument.EvaluateType();
                    if (typeArgumentRef is TypeRef && typeArgumentRef.HasTypeArguments)
                    {
                        TypeRefBase typeRefBase = typeArgumentRef.FindTypeArgumentInTypeArguments(typeParameterRef);
                        if (typeRefBase != null)
                            return typeRefBase;
                    }
                }
            }
            return null;
        }

        private static void AsTextArrayRank(CodeWriter writer, Type type)
        {
            if (type.IsArray)
            {
                AsTextArrayRank(writer, type.GetElementType());
                writer.Write(ArrayRankToString(type.GetArrayRank()));
            }
        }

        private static void AsTextConstraintPrefix(CodeWriter writer, ref bool first, GenericParameter genericParameter)
        {
            if (first)
            {
                writer.Write(" " + ConstraintClause.ParseToken + " ");
                AsTextTypeReference(writer, genericParameter, RenderFlags.None);
                writer.Write(" " + ConstraintClause.ParseTokenSeparator + " ");
                first = false;
            }
            else
                writer.Write(ParseTokenSeparator + " ");
        }

        private static void AsTextConstraintPrefix(CodeWriter writer, ref bool first, Type typeParameter)
        {
            if (first)
            {
                writer.Write(" " + ConstraintClause.ParseToken + " ");
                AsTextType(writer, typeParameter, RenderFlags.None);
                writer.Write(" " + ConstraintClause.ParseTokenSeparator + " ");
                first = false;
            }
            else
                writer.Write(ParseTokenSeparator + " ");
        }

        /// <summary>
        /// Determines if one <see cref="TypeRefBase"/> is equivalent to another one, meaning they both refer
        /// to the same type.
        /// </summary>
        public class IsSameTypeRefComparer : IEqualityComparer<TypeRefBase>
        {
            /// <summary>
            /// Determines if one <see cref="TypeRefBase"/> is equivalent to another one.
            /// </summary>
            public bool Equals(TypeRefBase x, TypeRefBase y)  // For IEqualityComparer<TypeRefBase>
            {
                return x.IsSameRef(y);
            }

            /// <summary>
            /// Calculate the hash code for the specified <see cref="TypeRefBase"/>.
            /// </summary>
            public int GetHashCode(TypeRefBase obj)  // For IEqualityComparer<TypeRefBase>
            {
                return obj.GetIsSameRefHashCode();
            }
        }
    }
}