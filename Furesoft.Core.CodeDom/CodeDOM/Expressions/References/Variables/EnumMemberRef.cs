// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables
{
    /// <summary>
    /// Represents a reference to an <see cref="EnumMemberDecl"/> (a member of an <see cref="EnumDecl"/> type declaration).
    /// </summary>
    public class EnumMemberRef : VariableRef
    {
        #region /* CONSTRUCTORS */

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
        public EnumMemberRef(FieldDefinition fieldDefinition, bool isFirstOnLine)
            : base(fieldDefinition, isFirstOnLine)
        {
            if (!fieldDefinition.IsLiteral || !fieldDefinition.DeclaringType.IsEnum)
                throw new Exception("A FieldDefinition used to construct an EnumMemberRef must represent an enum member.");
        }

        /// <summary>
        /// Create an <see cref="EnumMemberRef"/>.
        /// </summary>
        public EnumMemberRef(FieldDefinition fieldDefinition)
            : this(fieldDefinition, false)
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

        #endregion

        #region /* PROPERTIES */

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

        #endregion

        #region /* STATIC METHODS */

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
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
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
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Expression);
        }

        /// <summary>
        /// Find the enum value with the specified name in the specified <see cref="TypeRefBase"/>.
        /// </summary>
        /// <returns>An <see cref="EnumMemberRef"/> to the enum value, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        #endregion

        #region /* METHODS */

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
            else if (enumMemberObj is FieldDefinition)
                declaringTypeRef = TypeRef.Create(((FieldDefinition)enumMemberObj).DeclaringType);
            else if (enumMemberObj is FieldInfo)
                declaringTypeRef = TypeRef.Create(((FieldInfo)enumMemberObj).DeclaringType);
            return declaringTypeRef;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Get the TypeRef of the enum, using any existing TypeRef on a Dot prefix if possible
            TypeRefBase typeRefBase = null;
            if (_parent is Dot)
            {
                typeRefBase = ((Dot)_parent).Left.EvaluateType(withoutConstants);

                // Convert any aliased enum to the actual enum type
                if (typeRefBase is AliasRef)
                    typeRefBase = ((AliasRef)typeRefBase).Type;
            }

            // Evaluate to a TypeRef that references both the enum type plus the underlying constant value.
            // We have to check for null constant values, because the enum initialization might be unresolved.
            object constantValue = null;
            if (_reference is EnumMemberDecl)
            {
                EnumMemberDecl subEnumDecl = (EnumMemberDecl)_reference;
                // If we had trouble getting the enum type, determine it from the enum member
                if (typeRefBase == null)
                    typeRefBase = subEnumDecl.EvaluateType(withoutConstants);
                if (typeRefBase is TypeRef && !withoutConstants)
                {
                    constantValue = subEnumDecl.GetValue();
                    if (constantValue != null)
                        return new TypeRef((TypeRef)typeRefBase, constantValue);
                }
                return typeRefBase;
            }

            if (_reference is FieldDefinition)
            {
                FieldDefinition fieldDefinition = (FieldDefinition)_reference;
                if (!(typeRefBase is TypeRef))
                    typeRefBase = TypeRef.Create(fieldDefinition.DeclaringType);
                if (!withoutConstants)
                {
                    try { constantValue = fieldDefinition.Constant; }
                    catch { }
                    if (constantValue != null && typeRefBase is TypeRef)
                        return new TypeRef((TypeRef)typeRefBase, constantValue);
                }
                return typeRefBase;
            }
            FieldInfo fieldInfo = (FieldInfo)_reference;
            if (!(typeRefBase is TypeRef))
                typeRefBase = TypeRef.Create(fieldInfo.DeclaringType);
            if (!withoutConstants)
            {
                try { constantValue = fieldInfo.GetRawConstantValue(); }
                catch { }
                if (constantValue != null)
                    return new TypeRef((TypeRef)typeRefBase, constantValue);
            }
            return typeRefBase;
        }

        #endregion
    }
}
