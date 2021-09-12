﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;
using Furesoft.Core.CodeDom.Utilities.Mono.Cecil;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties
{
    /// <summary>
    /// Represents a reference to a <see cref="PropertyDecl"/> or <see cref="PropertyDefinition"/>/<see cref="PropertyInfo"/>.
    /// </summary>
    public class PropertyRef : VariableRef
    {
        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyDefinition propertyDefinition, bool isFirstOnLine)
            : base(propertyDefinition, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyDefinition propertyDefinition)
            : base(propertyDefinition, false)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyInfo propertyInfo, bool isFirstOnLine)
            : base(propertyInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        public PropertyRef(PropertyInfo propertyInfo)
            : base(propertyInfo, false)
        { }

        /// <summary>
        /// Create a <see cref="PropertyRef"/>.
        /// </summary>
        protected PropertyRef(PropertyDeclBase declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Construct a <see cref="PropertyRef"/> (or <see cref="IndexerRef"/>) from a <see cref="PropertyReference"/>.
        /// </summary>
        public static PropertyRef Create(PropertyReference propertyReference, bool isFirstOnLine)
        {
            PropertyDefinition propertyDefinition = propertyReference.Resolve();
            if (propertyDefinition != null)
            {
                if (propertyDefinition.HasParameters)
                    return new IndexerRef(propertyDefinition, isFirstOnLine);
                return new PropertyRef(propertyDefinition, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Construct a <see cref="PropertyRef"/> (or <see cref="IndexerRef"/>) from a <see cref="PropertyReference"/>.
        /// </summary>
        public static PropertyRef Create(PropertyReference propertyReference)
        {
            return Create(propertyReference, false);
        }

        /// <summary>
        /// Construct a <see cref="PropertyRef"/> (or <see cref="IndexerRef"/>) from a <see cref="PropertyInfo"/>.
        /// </summary>
        public static PropertyRef Create(PropertyInfo propertyInfo, bool isFirstOnLine)
        {
            if (propertyInfo != null)
            {
                if (PropertyInfoUtil.IsIndexed(propertyInfo))
                    return new IndexerRef(propertyInfo, isFirstOnLine);
                return new PropertyRef(propertyInfo, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Construct a <see cref="PropertyRef"/> (or <see cref="IndexerRef"/>) from a <see cref="PropertyInfo"/>.
        /// </summary>
        public static PropertyRef Create(PropertyInfo propertyInfo)
        {
            return Create(propertyInfo, false);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDecl typeDecl, string name, bool isFirstOnLine)
        {
            if (typeDecl != null)
            {
                PropertyRef propertyRef = typeDecl.GetProperty(name);
                if (propertyRef != null)
                {
                    propertyRef.IsFirstOnLine = isFirstOnLine;
                    return propertyRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Property);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDecl typeDecl, string name)
        {
            return Find(typeDecl, name, false);
        }

        /// <summary>
        /// Find a property on the specified type <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Alias typeAlias, string name, bool isFirstOnLine)
        {
            if (typeAlias != null)
            {
                PropertyRef propertyRef = typeAlias.GetProperty(name);
                if (propertyRef != null)
                {
                    propertyRef.IsFirstOnLine = isFirstOnLine;
                    return propertyRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Property);
        }

        /// <summary>
        /// Find a property on the specified type <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Alias typeAlias, string name)
        {
            return Find(typeAlias, name, false);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeDefinition"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDefinition typeDefinition, string name, bool isFirstOnLine)
        {
            if (typeDefinition != null)
            {
                PropertyDefinition propertyDefinition = Enumerable.FirstOrDefault(typeDefinition.Properties, delegate (PropertyDefinition property) { return property.Name == name; });
                if (propertyDefinition != null)
                    return new PropertyRef(propertyDefinition, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Property);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeDefinition"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeDefinition typeDefinition, string name)
        {
            return Find(typeDefinition, name, false);
        }

        /// <summary>
        /// Find a property on the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string name, bool isFirstOnLine)
        {
            if (type != null)
            {
                PropertyInfo propertyInfo = type.GetProperty(name);
                if (propertyInfo != null)
                    return new PropertyRef(propertyInfo, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Property);
        }

        /// <summary>
        /// Find a property on the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(Type type, string name)
        {
            return Find(type, name, false);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine)
        {
            if (typeRefBase is TypeRef)
            {
                PropertyRef propertyRef = ((TypeRef)typeRefBase).GetProperty(name);
                if (propertyRef != null)
                {
                    propertyRef.IsFirstOnLine = isFirstOnLine;
                    return propertyRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine, ResolveCategory.Property);
        }

        /// <summary>
        /// Find a property on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="PropertyRef"/> to the property, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="PropertyDefinition"/>.
        /// </summary>
        public static Modifiers GetPropertyModifiers(PropertyDefinition propertyDefinition)
        {
            Modifiers modifiers = 0;
            // A property doesn't actually have modifiers - get them from the getter/setter methods
            MethodDefinition getter = propertyDefinition.GetMethod;
            MethodDefinition setter = propertyDefinition.SetMethod;
            if (getter != null)
            {
                modifiers = MethodRef.GetMethodModifiers(getter);
                if (setter != null)
                {
                    // Combine the two sets of modifiers, removing any extraneous access modifiers
                    modifiers |= MethodRef.GetMethodModifiers(setter);
                    if (modifiers.HasFlag(Modifiers.Public))
                        modifiers &= ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Private);
                    else if (modifiers.HasFlag(Modifiers.Protected) || modifiers.HasFlag(Modifiers.Internal))
                        modifiers &= ~Modifiers.Private;
                }
            }
            else if (setter != null)
                modifiers = MethodRef.GetMethodModifiers(setter);
            return modifiers;
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="PropertyInfo"/>.
        /// </summary>
        public static Modifiers GetPropertyModifiers(PropertyInfo propertyInfo)
        {
            Modifiers modifiers = 0;
            // A property doesn't actually have modifiers - get them from the getter/setter methods
            MethodInfo getter = propertyInfo.GetGetMethod(true);
            MethodInfo setter = propertyInfo.GetSetMethod(true);
            if (getter != null)
            {
                modifiers = MethodRef.GetMethodModifiers(getter);
                if (setter != null)
                {
                    // Combine the two sets of modifiers, removing any extraneous access modifiers
                    modifiers |= MethodRef.GetMethodModifiers(setter);
                    if (modifiers.HasFlag(Modifiers.Public))
                        modifiers &= ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Private);
                    else if (modifiers.HasFlag(Modifiers.Protected) || modifiers.HasFlag(Modifiers.Internal))
                        modifiers &= ~Modifiers.Private;
                }
            }
            else if (setter != null)
                modifiers = MethodRef.GetMethodModifiers(setter);
            return modifiers;
        }

        /// <summary>
        /// Get the type of the specified property object.
        /// </summary>
        public static TypeRefBase GetPropertyType(object reference)
        {
            if (reference is PropertyDeclBase)
            {
                Expression type = ((PropertyDeclBase)reference).Type;
                return (type != null ? type.EvaluateType() : null);
            }
            if (reference is PropertyDefinition)
                return TypeRef.Create(((PropertyDefinition)reference).PropertyType);
            return TypeRef.Create(((PropertyInfo)reference).PropertyType);
        }

        /// <summary>
        /// True if the referenced property has internal access.
        /// </summary>
        public bool IsInternal
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsInternal;
                if (_reference is PropertyDefinition)
                    return PropertyDefinitionUtil.IsInternal((PropertyDefinition)_reference);
                return PropertyInfoUtil.IsInternal((PropertyInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced property has private access.
        /// </summary>
        public bool IsPrivate
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsPrivate;
                if (_reference is PropertyDefinition)
                    return PropertyDefinitionUtil.IsPrivate((PropertyDefinition)_reference);
                return PropertyInfoUtil.IsPrivate((PropertyInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced property has protected access.
        /// </summary>
        public bool IsProtected
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsProtected;
                if (_reference is PropertyDefinition)
                    return PropertyDefinitionUtil.IsProtected((PropertyDefinition)_reference);
                return PropertyInfoUtil.IsProtected((PropertyInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced property has public access.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsPublic;
                if (_reference is PropertyDefinition)
                    return PropertyDefinitionUtil.IsPublic((PropertyDefinition)_reference);
                return PropertyInfoUtil.IsPublic((PropertyInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced property is readable.
        /// </summary>
        public bool IsReadable
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsReadable;
                if (_reference is PropertyDefinition)
                    return (((PropertyDefinition)_reference).GetMethod != null);
                return ((PropertyInfo)_reference).CanRead;
            }
        }

        /// <summary>
        /// True if the referenced property is static.
        /// </summary>
        public override bool IsStatic
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsStatic;
                if (_reference is PropertyDefinition)
                    return PropertyDefinitionUtil.IsStatic((PropertyDefinition)_reference);
                return PropertyInfoUtil.IsStatic((PropertyInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced property is writable.
        /// </summary>
        public bool IsWritable
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsWritable;
                if (_reference is PropertyDefinition)
                    return (((PropertyDefinition)_reference).SetMethod != null);
                return ((PropertyInfo)_reference).CanRead;
            }
        }

        /// <summary>
        /// Get the declaring type of the specified property object.
        /// </summary>
        /// <param name="propertyObj">The property object (a <see cref="PropertyDeclBase"/> or <see cref="PropertyDefinition"/>/<see cref="PropertyInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object propertyObj)
        {
            TypeRefBase declaringTypeRef = null;
            if (propertyObj is PropertyDeclBase)
            {
                TypeDecl declaringTypeDecl = ((PropertyDeclBase)propertyObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else if (propertyObj is PropertyDefinition)
                declaringTypeRef = TypeRef.Create(((PropertyDefinition)propertyObj).DeclaringType);
            else if (propertyObj is PropertyInfo)
                declaringTypeRef = TypeRef.Create(((PropertyInfo)propertyObj).DeclaringType);
            return declaringTypeRef;
        }

        /// <summary>
        /// Get the declaring type of the referenced property.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            TypeRefBase declaringTypeRef = GetDeclaringType(_reference);

            // A property reference doesn't store any type arguments for a parent type instance, so any
            // type arguments in any generic declaring type or its parent types will always default to
            // the declared type arguments.  Convert them from OpenTypeParameterRefs to TypeParameterRefs
            // so that they don't show up as Red in the GUI.
            if (declaringTypeRef != null && declaringTypeRef.HasTypeArguments)
                declaringTypeRef.ConvertOpenTypeParameters();

            return declaringTypeRef;
        }

        /// <summary>
        /// Get the type of the referenced property.
        /// </summary>
        public TypeRefBase GetPropertyType()
        {
            return GetPropertyType(_reference);
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            TypeRefBase typeRefBase;
            if (_reference is PropertyDeclBase)
                typeRefBase = ((PropertyDeclBase)_reference).EvaluateType(withoutConstants);
            else if (_reference is PropertyDefinition)
            {
                PropertyDefinition propertyDefinition = (PropertyDefinition)_reference;
                TypeReference propertyType = propertyDefinition.PropertyType;
                typeRefBase = TypeRef.Create(propertyType);
            }
            else //if (_reference is PropertyInfo)
            {
                PropertyInfo propertyInfo = (PropertyInfo)_reference;
                Type propertyType = propertyInfo.PropertyType;
                typeRefBase = TypeRef.Create(propertyType);
            }

            // Evaluate any type arguments (this is necessary even for a PropertyInfo, because it's type might
            // be a generic type with a type argument that is specified in a base type list declaration).
            if (typeRefBase != null)
                typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(_parent, this);

            return typeRefBase;
        }

        public static void AsTextPropertyDefinition(CodeWriter writer, PropertyDefinition propertyDefinition, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                Attribute.AsTextAttributes(writer, propertyDefinition);
            writer.Write(ModifiersHelpers.AsString(GetPropertyModifiers(propertyDefinition)));
            TypeReference propertyType = propertyDefinition.PropertyType;
            TypeRefBase.AsTextTypeReference(writer, propertyType, passFlags);
            writer.Write(" ");
            TypeRefBase.AsTextTypeReference(writer, propertyDefinition.DeclaringType, passFlags);
            Dot.AsTextDot(writer);

            if (propertyDefinition.HasParameters)
            {
                // Display the actual name instead of 'this' - it will usually be 'Item', but not always,
                // plus it might have a prefix (if it's an explicit interface implementation).
                writer.Write(propertyDefinition.Name + IndexerDecl.ParseTokenStart);
                MethodRef.AsTextParameters(writer, propertyDefinition.Parameters, flags);
                writer.Write(IndexerDecl.ParseTokenEnd);
            }
            else
                writer.Write(propertyDefinition.Name);
        }

        public static void AsTextPropertyInfo(CodeWriter writer, PropertyInfo propertyInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                Attribute.AsTextAttributes(writer, propertyInfo);
            writer.Write(ModifiersHelpers.AsString(GetPropertyModifiers(propertyInfo)));
            Type propertyType = propertyInfo.PropertyType;
            TypeRefBase.AsTextType(writer, propertyType, passFlags);
            writer.Write(" ");
            TypeRefBase.AsTextType(writer, propertyInfo.DeclaringType, passFlags);
            Dot.AsTextDot(writer);

            if (PropertyInfoUtil.IsIndexed(propertyInfo))
            {
                // Display the actual name instead of 'this' - it will usually be 'Item', but not always,
                // plus it might have a prefix (if it's an explicit interface implementation).
                writer.Write(propertyInfo.Name + IndexerDecl.ParseTokenStart);
                MethodRef.AsTextParameters(writer, propertyInfo.GetIndexParameters(), flags);
                writer.Write(IndexerDecl.ParseTokenEnd);
            }
            else
                writer.Write(propertyInfo.Name);
        }
    }
}