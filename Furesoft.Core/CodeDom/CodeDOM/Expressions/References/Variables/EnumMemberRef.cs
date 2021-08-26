// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Reflection;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="EnumMemberDecl"/> (a member of an <see cref="EnumDecl"/> type declaration).
    /// </summary>
    public class EnumMemberRef : VariableRef
    {
        /// <summary>
        /// Create an <see cref="EnumMemberRef"/>.
        /// </summary>
        public EnumMemberRef(EnumMemberDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="EnumMemberRef"/>.
        /// </summary>
        public EnumMemberRef(EnumMemberDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="EnumMemberRef"/>.
        /// </summary>
        public EnumMemberRef(FieldInfo fieldInfo, bool isFirstOnLine)
            : base(fieldInfo, isFirstOnLine)
        {
            if (!fieldInfo.IsLiteral || (fieldInfo.DeclaringType != null && !fieldInfo.DeclaringType.IsEnum))
                throw new Exception("A FieldInfo used to construct an EnumMemberRef must represent an enum member.");
        }

        /// <summary>
        /// Create an <see cref="EnumMemberRef"/>.
        /// </summary>
        public EnumMemberRef(FieldInfo fieldInfo)
            : this(fieldInfo, false)
        { }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsConst
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsStatic
        {
            get { return true; }
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified <see cref="EnumDecl"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(EnumDecl enumDecl, string name, bool isFirstOnLine)
        {
            if (enumDecl != null)
            {
                EnumMemberRef enumMemberRef = enumDecl.GetMember(name);
                if (enumMemberRef != null)
                {
                    enumMemberRef.IsFirstOnLine = isFirstOnLine;
                    return enumMemberRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified <see cref="EnumDecl"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(EnumDecl enumDecl, string name)
        {
            return Find(enumDecl, name, false);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified enum <see cref="Type"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string valueName, bool isFirstOnLine)
        {
            if (type != null)
            {
                FieldInfo fieldInfo = type.GetField(valueName);
                if (fieldInfo != null)
                    return new EnumMemberRef(fieldInfo, isFirstOnLine);
            }
            return new UnresolvedRef(valueName, isFirstOnLine);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified enum <see cref="Type"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string valueName)
        {
            return Find(type, valueName, false);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified <see cref="TypeRefBase"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine)
        {
            if (typeRefBase is TypeRef && ((TypeRef)typeRefBase).IsEnum)
            {
                if (typeRefBase.Reference is Type)
                    return Find(typeRefBase.Reference as Type, name, isFirstOnLine);
                if (typeRefBase.Reference is EnumDecl)
                    return Find(typeRefBase.Reference as EnumDecl, name, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified <see cref="TypeRefBase"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        /// <summary>
        /// Get the declaring type of the specified enum member object.
        /// </summary>
        /// <param name="enumMemberObj">The enum member object (a <see cref="EnumMemberDecl"/> or <see cref="FieldInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object enumMemberObj)
        {
            TypeRefBase declaringTypeRef = null;
            if (enumMemberObj is EnumMemberDecl)
            {
                TypeDecl declaringTypeDecl = ((EnumMemberDecl)enumMemberObj).ParentEnumDecl;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else if (enumMemberObj is FieldInfo)
                declaringTypeRef = TypeRef.Create(((FieldInfo)enumMemberObj).DeclaringType);
            return declaringTypeRef;
        }

        /// <summary>
        /// Get the declaring type of the referenced enum member.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            TypeRefBase declaringTypeRef = GetDeclaringType(_reference);

            // An enum member reference doesn't store any type arguments for a parent type instance, so any
            // type arguments in any generic declaring type or its parent types will always default to
            // the declared type arguments.  Convert them from OpenTypeParameterRefs to TypeParameterRefs
            // so that they don't show up as Red in the GUI.
            if (declaringTypeRef != null && declaringTypeRef.HasTypeArguments)
                declaringTypeRef.ConvertOpenTypeParameters();

            return declaringTypeRef;
        }
    }
}