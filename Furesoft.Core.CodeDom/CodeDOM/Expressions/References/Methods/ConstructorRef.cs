using System;
using System.Reflection;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Rendering;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods
{
    /// <summary>
    /// Represents a reference to a <see cref="ConstructorDecl"/> or <see cref="ConstructorInfo"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="ConstructorRef"/> never has type arguments.  When using <see cref="NewObject"/> to create an
    /// instance of a generic type, <see cref="NewObject"/> has a <see cref="TypeRef"/> which has the type arguments
    /// plus a hidden <see cref="ConstructorRef"/> which doesn't need or have any type arguments.
    /// </remarks>
    public class ConstructorRef : MethodRef
    {
        /// <summary>
        /// Create a <see cref="ConstructorRef"/> from a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorRef(ConstructorDecl constructorDecl, bool isFirstOnLine)
            : base(constructorDecl, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorRef"/> from a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorRef(ConstructorDecl constructorDecl)
            : base(constructorDecl, false)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorRef"/> from a <see cref="ConstructorInfo"/>.
        /// </summary>
        public ConstructorRef(ConstructorInfo constructorInfo, bool isFirstOnLine)
            : base(constructorInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorRef"/> from a <see cref="ConstructorInfo"/>.
        /// </summary>
        public ConstructorRef(ConstructorInfo constructorInfo)
            : base(constructorInfo, false)
        { }

        /// <summary>
        /// The name of the <see cref="ConstructorRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is INamedCodeObject)
                    return ((INamedCodeObject)_reference).Name;
                if (_reference is ConstructorInfo)
                {
                    Type type = ((ConstructorInfo)_reference).DeclaringType;
                    if (type != null)
                        return (type.IsGenericType ? TypeUtil.NonGenericName(type) : type.Name);
                }
                return null;
            }
        }

        /// <summary>
        /// Always <c>null</c>.
        /// </summary>
        public override ChildList<Expression> TypeArguments
        {
            get { return null; }
            set
            {
                if (value != null)
                    throw new Exception("A ConstructorRef can't have type arguments!");
            }
        }

        public static void AsTextConstructorInfo(CodeWriter writer, ConstructorInfo constructorInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                Attribute.AsTextAttributes(writer, constructorInfo);
            writer.Write(ModifiersHelpers.AsString(GetMethodModifiers(constructorInfo)));
            Type declaringType = constructorInfo.DeclaringType;
            AsTextType(writer, declaringType, passFlags);
            Dot.AsTextDot(writer);
            if (declaringType != null)
                writer.WriteName(declaringType.IsGenericType ? TypeUtil.NonGenericName(declaringType) : declaringType.Name, passFlags);
            AsTextMethodParameters(writer, constructorInfo, flags);
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="TypeDecl"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeDecl != null)
            {
                ConstructorRef constructorRef = typeDecl.GetConstructor(parameterTypes);
                if (constructorRef != null)
                {
                    constructorRef.IsFirstOnLine = isFirstOnLine;
                    return constructorRef;
                }
                return new UnresolvedRef(typeDecl.Name, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="TypeDecl"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, params TypeRefBase[] parameterTypes)
        {
            return Find(typeDecl, false, parameterTypes);
        }

        /// <summary>
        /// Find the constructor of the specified type <see cref="Alias"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias typeAlias, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeAlias != null)
            {
                ConstructorRef constructorRef = typeAlias.GetConstructor(parameterTypes);
                if (constructorRef != null)
                {
                    constructorRef.IsFirstOnLine = isFirstOnLine;
                    return constructorRef;
                }
                return new UnresolvedRef(typeAlias.Name, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Find the constructor of the specified type <see cref="Alias"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias typeAlias, params TypeRefBase[] parameterTypes)
        {
            return Find(typeAlias, false, parameterTypes);
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="Type"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, bool isFirstOnLine, params Type[] parameterTypes)
        {
            if (type != null)
            {
                ConstructorInfo constructorInfo = type.GetConstructor(parameterTypes);
                if (constructorInfo != null)
                    return new ConstructorRef(constructorInfo, isFirstOnLine);
                return new UnresolvedRef(type.Name, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="Type"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, params Type[] parameterTypes)
        {
            return Find(type, false, parameterTypes);
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="TypeRefBase"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeRefBase typeRefBase, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeRefBase is TypeRef)
            {
                ConstructorRef constructorRef = ((TypeRef)typeRefBase).GetConstructor(parameterTypes);
                if (constructorRef != null)
                {
                    constructorRef.IsFirstOnLine = isFirstOnLine;
                    return constructorRef;
                }
                return new UnresolvedRef(typeRefBase.Name, isFirstOnLine);
            }
            return null;
        }

        /// <summary>
        /// Find the constructor of the specified <see cref="TypeRefBase"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="ConstructorRef"/> to the constructor, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeRefBase typeRefBase, params TypeRefBase[] parameterTypes)
        {
            return Find(typeRefBase, false, parameterTypes);
        }

        /// <summary>
        /// Get the return type of the constructor (never null).
        /// </summary>
        public static TypeRefBase GetReturnType(TypeRefBase thisRef, object reference)
        {
            // The 'return type' of a constructor is its declaring type, along with any type parameters
            TypeRefBase typeRefBase;
            if (reference is ConstructorDecl)
            {
                ConstructorDecl constructorDecl = (ConstructorDecl)reference;
                CodeObject parent = constructorDecl.Parent;
                if (parent == null)
                {
                    // If we don't have a parent, assume we're a generated constructor for
                    // a delegate (used for the obsolete explicit delegate creation syntax), and
                    // use the type of the parameter as our type.
                    // Clone the type so we can evaluate any type arguments it has later without consequences.
                    typeRefBase = constructorDecl.Parameters[0].Type.SkipPrefixes() as TypeRefBase;
                    typeRefBase = (typeRefBase != null ? (TypeRefBase)typeRefBase.Clone() : TypeRef.VoidRef);
                }
                else
                    typeRefBase = (TypeRef)parent.CreateRef();
            }
            else //if (reference is ConstructorInfo)
                typeRefBase = TypeRef.Create(((ConstructorInfo)reference).DeclaringType);

            return typeRefBase;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Display constructors as their declaring type name, including any type arguments (this
            // is the easy way to display them with the proper type arguments and any enclosing types,
            // if appropriate).
            UpdateLineCol(writer, flags);
            TypeRefBase typeRef = GetDeclaringType();
            if (typeRef != null)
                typeRef.AsText(writer, flags & ~RenderFlags.UpdateLineCol);
            else if (_reference is ConstructorDecl)
            {
                // If we failed to get the declaring type, and we have an "orphaned" ConstructorDecl,
                // go ahead and display it's name.
                writer.WriteName(((ConstructorDecl)_reference).Name, flags);
            }
        }

        /// <summary>
        /// Get the declaring type of the referenced constructor.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            // Do a special check for a generated constructor for an external delegate type (see TypeRef.GetConstructors())
            ConstructorDecl constructorDecl = _reference as ConstructorDecl;
            if (constructorDecl != null && constructorDecl.Parent == null && constructorDecl.IsGenerated)
            {
                ChildList<ParameterDecl> parameters = constructorDecl.Parameters;
                if (parameters.Count == 1)
                {
                    ParameterDecl parameterDecl = parameters[0];
                    if (parameterDecl.Name == DelegateDecl.DelegateConstructorParameterName)
                        return parameterDecl.Type as TypeRef;
                }
            }
            return base.GetDeclaringType();
        }

        /// <summary>
        /// Get the return type of the constructor (never null).
        /// </summary>
        public override TypeRefBase GetReturnType()
        {
            return GetReturnType(this, _reference);
        }
    }
}
