using System;
using System.Reflection;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="PropertyDecl"/> or <see cref="PropertyInfo"/>.
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
        /// True if the referenced property has internal access.
        /// </summary>
        public bool IsInternal
        {
            get
            {
                if (_reference is PropertyDeclBase)
                    return ((PropertyDeclBase)_reference).IsInternal;
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
                return ((PropertyInfo)_reference).CanRead;
            }
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
            return new UnresolvedRef(name, isFirstOnLine);
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
            return new UnresolvedRef(name, isFirstOnLine);
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
            return new UnresolvedRef(name, isFirstOnLine);
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
            return new UnresolvedRef(name, isFirstOnLine);
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
        /// Get the declaring type of the specified property object.
        /// </summary>
        /// <param name="propertyObj">The property object (a <see cref="PropertyDeclBase"/> or <see cref="PropertyInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object propertyObj)
        {
            TypeRefBase declaringTypeRef = null;
            if (propertyObj is PropertyDeclBase)
            {
                TypeDecl declaringTypeDecl = ((PropertyDeclBase)propertyObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else if (propertyObj is PropertyInfo)
                declaringTypeRef = TypeRef.Create(((PropertyInfo)propertyObj).DeclaringType);
            return declaringTypeRef;
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
                return (type != null ? type.SkipPrefixes() as TypeRefBase : null);
            }
            return TypeRef.Create(((PropertyInfo)reference).PropertyType);
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
    }
}