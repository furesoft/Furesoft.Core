// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Reflection;

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="FieldDecl"/> or a <see cref="FieldInfo"/>.
    /// </summary>
    public class FieldRef : VariableRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="FieldRef"/>.
        /// </summary>
        public FieldRef(FieldDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="FieldRef"/>.
        /// </summary>
        public FieldRef(FieldDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create a <see cref="FieldRef"/>.
        /// </summary>
        public FieldRef(FieldInfo fieldInfo, bool isFirstOnLine)
            : base(fieldInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="FieldRef"/>.
        /// </summary>
        public FieldRef(FieldInfo fieldInfo)
            : base(fieldInfo, false)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if the referenced field is const.
        /// </summary>
        public override bool IsConst
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsConst;
                return ((FieldInfo)_reference).IsLiteral;
            }
        }

        /// <summary>
        /// True if the referenced field is static.
        /// </summary>
        public override bool IsStatic
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsStatic;
                return ((FieldInfo)_reference).IsStatic;
            }
        }

        /// <summary>
        /// True if the referenced field has public access.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsPublic;
                return ((FieldInfo)_reference).IsPublic;
            }
        }

        /// <summary>
        /// True if the referenced field has private access.
        /// </summary>
        public bool IsPrivate
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsPrivate;
                return ((FieldInfo)_reference).IsPrivate;
            }
        }

        /// <summary>
        /// True if the referenced field has protected access.
        /// </summary>
        public bool IsProtected
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsProtected;
                return ((FieldInfo)_reference).IsFamily;
            }
        }

        /// <summary>
        /// True if the referenced field has internal access.
        /// </summary>
        public bool IsInternal
        {
            get
            {
                if (_reference is FieldDecl)
                    return ((FieldDecl)_reference).IsInternal;
                return ((FieldInfo)_reference).IsAssembly;
            }
        }

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Find a field on the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDecl typeDecl, string name, bool isFirstOnLine)
        {
            if (typeDecl != null)
            {
                FieldRef fieldRef = typeDecl.GetField(name);
                if (fieldRef != null)
                {
                    fieldRef.IsFirstOnLine = isFirstOnLine;
                    return fieldRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a field on the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDecl typeDecl, string name)
        {
            return Find(typeDecl, name, false);
        }

        /// <summary>
        /// Find a field on the specified type <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Alias typeAlias, string name, bool isFirstOnLine)
        {
            if (typeAlias != null)
            {
                FieldRef fieldRef = typeAlias.GetField(name);
                if (fieldRef != null)
                {
                    fieldRef.IsFirstOnLine = isFirstOnLine;
                    return fieldRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a field on the specified type <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Alias typeAlias, string name)
        {
            return Find(typeAlias, name, false);
        }

        /// <summary>
        /// Find the field in the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string name, bool isFirstOnLine)
        {
            if (type != null)
            {
                FieldInfo fieldInfo = type.GetField(name);
                if (fieldInfo != null)
                    return new FieldRef(fieldInfo, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the field in the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string name)
        {
            return Find(type, name, false);
        }

        /// <summary>
        /// Find a field on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine)
        {
            if (typeRefBase is TypeRef)
            {
                FieldRef fieldRef = ((TypeRef)typeRefBase).GetField(name);
                if (fieldRef != null)
                {
                    fieldRef.IsFirstOnLine = isFirstOnLine;
                    return fieldRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a field on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="FieldRef"/> to the field, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="FieldInfo"/>.
        /// </summary>
        public static Modifiers GetFieldModifiers(FieldInfo fieldInfo)
        {
            Modifiers modifiers = 0;
            if (fieldInfo.IsPublic)
                modifiers |= Modifiers.Public;
            if (fieldInfo.IsFamily || fieldInfo.IsFamilyOrAssembly)
                modifiers |= Modifiers.Protected;
            if (fieldInfo.IsAssembly || fieldInfo.IsFamilyOrAssembly)
                modifiers |= Modifiers.Internal;
            if (fieldInfo.IsPrivate)
                modifiers |= Modifiers.Private;
            if (fieldInfo.IsLiteral)
                modifiers |= Modifiers.Const;
            else if (fieldInfo.IsStatic)  // Ignore static if const
                modifiers |= Modifiers.Static;
            if (fieldInfo.IsInitOnly)
                modifiers |= Modifiers.ReadOnly;
            // 'new' isn't relevant to external users, so don't bother figuring it out (we could look
            // at IsHideBySig, but we'd have to further determine if it's the hide-er or the hide-e).
            // 'volatile' would be nice to know, but there isn't any attribute or any other way to figure it out.
            return modifiers;
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the declaring type of the referenced field.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            TypeRefBase declaringTypeRef = GetDeclaringType(_reference);

            // A field reference doesn't store any type arguments for a parent type instance, so any
            // type arguments in any generic declaring type or its parent types will always default to
            // the declared type arguments.  Convert them from OpenTypeParameterRefs to TypeParameterRefs
            // so that they don't show up as Red in the GUI.
            if (declaringTypeRef != null && declaringTypeRef.HasTypeArguments)
                declaringTypeRef.ConvertOpenTypeParameters();

            return declaringTypeRef;
        }

        /// <summary>
        /// Get the declaring type of the specified field object.
        /// </summary>
        /// <param name="fieldObj">The field object (a <see cref="FieldDecl"/> or <see cref="FieldInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object fieldObj)
        {
            TypeRefBase declaringTypeRef;
            if (fieldObj is FieldDecl)
            {
                TypeDecl declaringTypeDecl = ((FieldDecl)fieldObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else //if (fieldObj is FieldInfo)
                declaringTypeRef = TypeRef.Create(((FieldInfo)fieldObj).DeclaringType);
            return declaringTypeRef;
        }

        #endregion

        #region /* RENDERING */

        public static void AsTextFieldInfo(CodeWriter writer, FieldInfo fieldInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            // Skip all details for enum members, including the declaring type since it will always be on the left of the dot
            if (!(fieldInfo.IsLiteral && fieldInfo.FieldType.IsEnum))
            {
                if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                    Attribute.AsTextAttributes(writer, fieldInfo);
                writer.Write(ModifiersHelpers.AsString(GetFieldModifiers(fieldInfo)));
                Type fieldType = fieldInfo.FieldType;
                TypeRefBase.AsTextType(writer, fieldType, passFlags);
                writer.Write(" ");
                TypeRefBase.AsTextType(writer, fieldInfo.DeclaringType, passFlags);
                Dot.AsTextDot(writer);
            }
            writer.Write(fieldInfo.Name);
        }

        #endregion
    }
}
