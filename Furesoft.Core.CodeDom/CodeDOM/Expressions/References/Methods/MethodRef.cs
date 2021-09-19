using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="MethodDeclBase"/> (common base of <see cref="MethodDecl"/>,
    /// <see cref="GenericMethodDecl"/>, <see cref="ConstructorDecl"/>, <see cref="DestructorDecl"/>),
    /// <see cref="AnonymousMethod"/>, or <see cref="MethodInfo"/>.
    /// </summary>
    /// <remarks>
    /// Instead of having a derived GenericMethodRef, this class supports type arguments directly.
    /// This is because <see cref="ConstructorRef"/> (which is derived from this class) also needs type argument
    /// support (even though a <see cref="ConstructorDecl"/> doesn't have type parameters, constructors for generic
    /// types still need type arguments when they're referenced).  Furthermore, <see cref="MethodRef"/> can be treated
    /// as a delegate type, such as being passed to a method parameter of delegate type, so it is derived from
    /// <see cref="TypeRefBase"/>, which provides the required type argument support (the array support isn't used).
    /// </remarks>
    public class MethodRef : TypeRefBase
    {
        /// <summary>
        /// True if the type arguments are inferred.
        /// </summary>
        public bool HasInferredTypeArguments;

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodDeclBase, bool isFirstOnLine, ChildList<Expression> typeArguments)
            : base(methodDeclBase, isFirstOnLine)
        {
            // If the method is generic, and no type arguments were specified, do NOT default any (they will be inferred).
            // Unlike nested types, methods of generic types are not considered generic if they don't have any local type
            // arguments, and generic methods never include type arguments of generic enclosing types (presumably, when
            // assigned to a delegate, any type arguments of generic enclosing types will be included in the object reference
            // associated with the delegate).
            // Constructors of generic types are also not generic methods, however the type arguments associated with their
            // declaring type must be supplied when they are invoked - these are defaulted in ConstructorRef if omitted.

            TypeArguments = typeArguments;
        }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodDeclBase, bool isFirstOnLine)
            : base(methodDeclBase, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodDeclBase)
            : base(methodDeclBase, false)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodDeclBase, ChildList<Expression> typeArguments)
            : this(methodDeclBase, false, typeArguments)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodBase, bool isFirstOnLine, params Expression[] typeArguments)
            : this(methodBase, isFirstOnLine, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodDeclBase"/>.
        /// </summary>
        public MethodRef(MethodDeclBase methodBase, params Expression[] typeArguments)
            : this(methodBase, false, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo, bool isFirstOnLine, ChildList<Expression> typeArguments)
            : base(methodInfo, isFirstOnLine)
        {
            // If the method is generic, and no type arguments were specified, do NOT default any (they will be inferred).
            // Unlike nested types, methods of generic types are not considered generic if they don't have any local type
            // arguments, and generic methods never include type arguments of generic enclosing types (presumably, when
            // assigned to a delegate, any type arguments of generic enclosing types will be included in the object reference
            // associated with the delegate).

            TypeArguments = typeArguments;
        }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo, bool isFirstOnLine)
            : base(methodInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo)
            : base(methodInfo, false)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo, ChildList<Expression> typeArguments)
            : this(methodInfo, false, typeArguments)
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo, bool isFirstOnLine, params Expression[] typeArguments)
            : this(methodInfo, isFirstOnLine, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="MethodRef"/> from a <see cref="MethodInfo"/>.
        /// </summary>
        public MethodRef(MethodInfo methodInfo, params Expression[] typeArguments)
            : this(methodInfo, false, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        protected MethodRef(AnonymousMethod anonymousMethod, bool isFirstOnLine)
            : base(anonymousMethod, isFirstOnLine)
        { }

        protected MethodRef(MethodBase methodBase, bool isFirstOnLine)
            : base(methodBase, isFirstOnLine)
        {
            // Constructors of generic types are not generic methods, however the type arguments associated with their
            // declaring type must be supplied when they are invoked - these are defaulted in ConstructorRef if omitted.
        }

        /// <summary>
        /// A <see cref="MethodRef"/> can't have array ranks, so this property always returns null, and throws
        /// an exception if set to a non-null value.
        /// </summary>
        public override List<int> ArrayRanks
        {
            set
            {
                if (value != null)
                    throw new Exception("Can't set array ranks on a MethodRef!");
            }
        }

        /// <summary>
        /// True if the referenced method has parameters.
        /// </summary>
        public bool HasParameters
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).HasParameters;
                if (_reference is MethodInfo)
                    return (((MethodInfo)_reference).GetParameters().Length > 0);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsAbstract;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsAbstract;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method is generic.
        /// </summary>
        public bool IsGenericMethod
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsGenericMethod;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsGenericMethod;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method has internal access.
        /// </summary>
        public override bool IsInternal
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsInternal;
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsInternal;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsAssembly;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method is an override.
        /// </summary>
        public bool IsOverride
        {
            get { return IsOverridden(_reference); }
        }

        /// <summary>
        /// True if the referenced method has private access.
        /// </summary>
        public override bool IsPrivate
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsPrivate;
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsPrivate;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsPrivate;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method has protected access.
        /// </summary>
        public override bool IsProtected
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsProtected;
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsProtected;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsFamily;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method has public access.
        /// </summary>
        public override bool IsPublic
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsPublic;
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsPublic;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsPublic;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method is static.
        /// </summary>
        public override bool IsStatic
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).IsStatic;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsStatic;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced method is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return (((MethodDeclBase)_reference).IsVirtual || ((MethodDeclBase)_reference).IsOverride);
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).IsVirtual;
                return false;
            }
        }

        /// <summary>
        /// The name of the <see cref="MethodRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is INamedCodeObject)
                    return ((INamedCodeObject)_reference).Name;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).Name;
                return null;
            }
        }

        /// <summary>
        /// The number of parameters the referenced method has.
        /// </summary>
        public int ParameterCount
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).ParameterCount;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).GetParameters().Length;
                return 0;
            }
        }

        /// <summary>
        /// Get the parameters of the referenced method object (as either a <see cref="ChildList{ParameterDecl}"/> or a <see cref="ParameterInfo"/>[].
        /// </summary>
        public ICollection Parameters
        {
            get
            {
                if (_reference is MethodDeclBase)
                    return ((MethodDeclBase)_reference).Parameters;
                if (_reference is MethodInfo)
                    return ((MethodInfo)_reference).GetParameters();
                return null;
            }
        }

        public static void AsTextMethodInfo(CodeWriter writer, MethodInfo methodInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
            {
                Attribute.AsTextAttributes(writer, methodInfo);
                Attribute.AsTextAttributes(writer, methodInfo.ReturnParameter, AttributeTarget.Return);
            }
            Modifiers modifiers = GetMethodModifiers(methodInfo);
            writer.Write(ModifiersHelpers.AsString(modifiers));
            bool isConversionOperator = (modifiers.HasFlag(Modifiers.Explicit) || modifiers.HasFlag(Modifiers.Implicit));
            if (!isConversionOperator)
            {
                Type returnType = methodInfo.ReturnType;
                AsTextType(writer, returnType, passFlags);
                writer.Write(" ");
            }
            Type declaringType = methodInfo.DeclaringType;
            AsTextType(writer, declaringType, passFlags);
            Dot.AsTextDot(writer);

            if (methodInfo.IsGenericMethod)
                AsTextGenericMember(writer, methodInfo.Name, methodInfo.GetGenericArguments(), passFlags);
            else if (methodInfo.Name.StartsWith(Operator.NamePrefix))
            {
                // Convert the internal name into the appropriate symbol
                writer.Write(OperatorDecl.ParseToken + " ");
                if (isConversionOperator)
                    AsTextType(writer, methodInfo.ReturnType, passFlags);
                else
                    writer.Write(OperatorDecl.GetOperatorSymbol(methodInfo.Name));
            }
            else
                writer.Write(methodInfo.Name);

            AsTextMethodParameters(writer, methodInfo, passFlags);

            // Render type constraints (if any)
            if (methodInfo.IsGenericMethod)
                AsTextConstraints(writer, methodInfo.GetGenericMethodDefinition().GetGenericArguments());
        }

        public static void AsTextMethodParameters(CodeWriter writer, MethodBase methodBase, RenderFlags flags)
        {
            writer.Write(MethodDeclBase.ParseTokenStart);
            ICollection parameters = GetMethodParameters(methodBase);
            if (parameters is ParameterInfo[])
                AsTextParameters(writer, (ParameterInfo[])parameters, flags);
            else
                writer.WriteList((ChildList<ParameterDecl>)parameters, flags, null);
            writer.Write(MethodDeclBase.ParseTokenEnd);
        }

        public static void AsTextParameters(CodeWriter writer, ParameterInfo[] parameters, RenderFlags flags)
        {
            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo parameterInfo = parameters[i];
                if (i > 0)
                    writer.Write(ParameterDecl.ParseTokenSeparator + " ");

                ParameterRef.AsTextParameterInfo(writer, parameterInfo, flags);
            }
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase, bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, isFirstOnLine, typeArguments) : new ConstructorRef((ConstructorInfo)methodBase, isFirstOnLine));
            return null;
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase, bool isFirstOnLine)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, isFirstOnLine) : new ConstructorRef((ConstructorInfo)methodBase, isFirstOnLine));
            return null;
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, false) : new ConstructorRef((ConstructorInfo)methodBase, false));
            return null;
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase, ChildList<Expression> typeArguments)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, false, typeArguments) : new ConstructorRef((ConstructorInfo)methodBase));
            return null;
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase, bool isFirstOnLine, params Expression[] typeArguments)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, isFirstOnLine, typeArguments) : new ConstructorRef((ConstructorInfo)methodBase, isFirstOnLine));
            return null;
        }

        /// <summary>
        /// Construct a <see cref="MethodRef"/> or <see cref="ConstructorRef"/> from a <see cref="MethodBase"/>.
        /// </summary>
        public static MethodRef Create(MethodBase methodBase, params Expression[] typeArguments)
        {
            if (methodBase != null)
                return (methodBase is MethodInfo ? new MethodRef((MethodInfo)methodBase, false, typeArguments) : new ConstructorRef((ConstructorInfo)methodBase));
            return null;
        }

        /// <summary>
        /// Find a method on the specified <see cref="TypeDecl"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, string name, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeDecl != null)
            {
                MethodRef methodRef = typeDecl.GetMethod(name, parameterTypes);
                if (methodRef != null)
                {
                    methodRef.IsFirstOnLine = isFirstOnLine;
                    return methodRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a method on the specified <see cref="TypeDecl"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, string name, params TypeRefBase[] parameterTypes)
        {
            return Find(typeDecl, name, false, parameterTypes);
        }

        /// <summary>
        /// Find a method on the specified type <see cref="Alias"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias typeAlias, string name, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeAlias != null)
            {
                MethodRef methodRef = typeAlias.GetMethod(name, parameterTypes);
                if (methodRef != null)
                {
                    methodRef.IsFirstOnLine = isFirstOnLine;
                    return methodRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a method on the specified type <see cref="Alias"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias typeAlias, string name, params TypeRefBase[] parameterTypes)
        {
            return Find(typeAlias, name, false, parameterTypes);
        }

        /// <summary>
        /// Find a method on the specified <see cref="Type"/> with the specified name and signature, using the specified <see cref="BindingFlags"/>.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, string name, BindingFlags bindingFlags, bool isFirstOnLine, params Type[] paramTypes)
        {
            if (type != null)
            {
                MethodInfo methodInfo = TypeUtil.GetMethod(type, name, bindingFlags, paramTypes);
                if (methodInfo != null)
                    return new MethodRef(methodInfo, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a method on the specified <see cref="Type"/> with the specified name and signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, string name, bool isFirstOnLine, params Type[] paramTypes)
        {
            return Find(type, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, isFirstOnLine, paramTypes);
        }

        /// <summary>
        /// Find a method on the specified <see cref="Type"/> with the specified name and signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, string name, params Type[] paramTypes)
        {
            return Find(type, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, false, paramTypes);
        }

        /// <summary>
        /// Find a method on the specified <see cref="TypeRefBase"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeRefBase typeRefBase, string name, BindingFlags bindingFlags, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            if (typeRefBase is TypeRef)
            {
                MethodRef methodRef = ((TypeRef)typeRefBase).GetMethod(name, bindingFlags, parameterTypes);
                if (methodRef != null)
                {
                    methodRef.IsFirstOnLine = isFirstOnLine;
                    return methodRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a method on the specified <see cref="TypeRefBase"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine, params TypeRefBase[] parameterTypes)
        {
            return Find(typeRefBase, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, isFirstOnLine, parameterTypes);
        }

        /// <summary>
        /// Find a method on the specified <see cref="TypeRefBase"/> with the specified signature.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/> to the method, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeRefBase typeRefBase, string name, params TypeRefBase[] parameterTypes)
        {
            return Find(typeRefBase, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, false, parameterTypes);
        }

        /// <summary>
        /// Get the declaring type of the specified method object.
        /// </summary>
        /// <param name="methodObj">The method object (a <see cref="MethodDeclBase"/> or <see cref="MethodBase"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object methodObj)
        {
            TypeRefBase declaringTypeRef = null;
            if (methodObj is MethodDeclBase)
            {
                TypeDecl declaringTypeDecl = ((MethodDeclBase)methodObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else if (methodObj is MethodBase)
                declaringTypeRef = TypeRef.Create(((MethodBase)methodObj).DeclaringType);
            return declaringTypeRef;
        }

        /// <summary>
        /// Determine the delegate type of the parameter of the specified method object with the specified argument index.
        /// </summary>
        /// <param name="obj">The code object (an IParameters CodeObject, MethodInfo, ConstructorInfo, or PropertyInfo for an indexer;
        /// or an IVariableDecl CodeObject, FieldInfo, PropertyInfo, or EventInfo that has a delegate type).</param>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <param name="parentExpression">The parent expression of the code object.</param>
        /// <returns>The TypeRefBase representing the delegate type of the parameter, otherwise null.</returns>
        public static TypeRefBase GetDelegateParameterType(object obj, int parameterIndex, Expression parentExpression)
        {
            // Get the parameter type
            TypeRefBase delegateType = GetParameterType(obj, parameterIndex, parentExpression);

            // Return null if the type isn't a delegate type
            if (delegateType != null && !delegateType.IsDelegateType)
                delegateType = null;

            return delegateType;
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="MethodBase"/>.
        /// </summary>
        public static Modifiers GetMethodModifiers(MethodBase methodBase)
        {
            Modifiers modifiers = 0;
            if (methodBase.IsPublic)
                modifiers |= Modifiers.Public;
            if (methodBase.IsFamily || methodBase.IsFamilyOrAssembly)
                modifiers |= Modifiers.Protected;
            if (methodBase.IsAssembly || methodBase.IsFamilyOrAssembly)
                modifiers |= Modifiers.Internal;
            if (methodBase.IsPrivate)
                modifiers |= Modifiers.Private;
            if (methodBase.IsAbstract)
                modifiers |= Modifiers.Abstract;
            if (methodBase.IsFinal)
                modifiers |= Modifiers.Sealed;
            if (methodBase.IsStatic)
                modifiers |= Modifiers.Static;
            if (methodBase.IsVirtual)
                modifiers |= Modifiers.Virtual;
            // If it's both 'virtual' and 'sealed', it's not relevant to external users, so
            // hide them both (otherwise, various BCL methods will show these attributes).
            if ((modifiers & (Modifiers.Virtual | Modifiers.Sealed)) == (Modifiers.Virtual | Modifiers.Sealed))
                modifiers &= ~(Modifiers.Virtual | Modifiers.Sealed);
            // 'override' would be nice instead of just 'virtual', but it's not really relevant to
            // external users, and we'd have to scan base classes to determine the difference.
            // 'new' isn't relevant to external users, so don't bother figuring it out (we could look
            // at IsHideBySig, but we'd have to further determine if it's the hide-er or the hide-e).
            // 'partial' and 'extern' aren't relevant to external users.
            if (methodBase.IsSpecialName)
            {
                if (methodBase.Name == "op_Explicit")
                    modifiers |= Modifiers.Explicit;
                else if (methodBase.Name == "op_Implicit")
                    modifiers |= Modifiers.Implicit;
            }
            return modifiers;
        }

        /// <summary>
        /// Get the parameters of a MethodBase, handling the VarArgs calling convention.
        /// </summary>
        public static ICollection GetMethodParameters(MethodBase methodBase)
        {
            return methodBase.GetParameters();
        }

        /// <summary>
        /// Retrieve the parameters (if any) of a code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be an <see cref="IParameters"/> CodeObject (MethodDeclBase, IndexerDecl, DelegateDecl, AnonymousMethod),
        /// MethodDefinition/MethodInfo, ConstructorInfo, or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <param name="parentExpression">The parent expression of the code object.</param>
        /// <returns>An <see cref="ICollection"/> of <see cref="ParameterDecl"/>s or <see cref="ParameterInfo"/>s, or null if none were found.</returns>
        public static ICollection GetParameters(object obj, Expression parentExpression)
        {
            // Handle method & indexer parameters
            ICollection parameters = null;
            if (obj is IParameters)
                parameters = ((IParameters)obj).Parameters;
            else if (obj is MethodBase)  // MethodInfo or ConstructorInfo
                return GetMethodParameters((MethodBase)obj);
            else if (obj is PropertyInfo && PropertyInfoUtil.IsIndexed((PropertyInfo)obj))
                return ((PropertyInfo)obj).GetIndexParameters();
            else if (obj is IVariableDecl)
            {
                // Handle variables that evaluate to a delegate type
                Expression type = ((IVariableDecl)obj).Type;
                if (type != null)
                {
                    // Evaluate if a TypeParameter
                    TypeRefBase typeRefBase = type.SkipPrefixes() as TypeRefBase;
                    if (typeRefBase != null)
                    {
                        if (typeRefBase.IsDelegateType)
                            parameters = typeRefBase.GetDelegateParameters();
                    }
                }
            }
            else //if (obj is MemberInfo)
            {
                // Handle external variables that evaluate to a delegate type
                Type delegateType = null;
                if (obj is FieldInfo)
                    delegateType = ((FieldInfo)obj).FieldType;
                else if (obj is PropertyInfo)
                    delegateType = ((PropertyInfo)obj).PropertyType;
                else if (obj is EventInfo)
                    delegateType = ((EventInfo)obj).EventHandlerType;
                if (delegateType != null)
                {
                    // Evaluate if a generic parameter
                    if (delegateType.IsGenericParameter)
                        parameters = TypeRef.Create(delegateType).GetDelegateParameters();
                    else
                        parameters = TypeUtil.GetDelegateParameters(delegateType);
                }
            }
            return parameters;
        }

        /// <summary>
        /// Retrieve the parameters (if any) of a code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be an <see cref="IParameters"/> CodeObject (MethodDeclBase, IndexerDecl, DelegateDecl, AnonymousMethod),
        /// MethodDefinition/MethodInfo, ConstructorInfo, or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <returns>An <see cref="ICollection"/> of <see cref="ParameterDecl"/>s or <see cref="ParameterInfo"/>s, or null if none were found.</returns>
        public static ICollection GetParameters(object obj)
        {
            return GetParameters(obj, null);
        }

        /// <summary>
        /// Retrieve the type of the parameter with the specified index from the specified code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be an <see cref="IParameters"/> CodeObject (MethodDeclBase, IndexerDecl, DelegateDecl, AnonymousMethod),
        /// MethodDefinition/MethodInfo, ConstructorInfo, or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <param name="parentExpression">The parent expression of the code object.</param>
        /// <returns>The TypeRefBase representing the type of the parameter, otherwise null.</returns>
        public static TypeRefBase GetParameterType(object obj, int parameterIndex, Expression parentExpression)
        {
            if (obj == null)
                return null;

            ICollection parameters = GetParameters(obj, parentExpression);
            int parameterCount = (parameters != null ? parameters.Count : 0);
            if (parameterCount == 0)
                return null;

            // Check for params parameter
            bool isParamsParameter = false;
            if (parameterIndex >= parameterCount - 1)
            {
                int paramsIndex = parameterCount - 1;
                isParamsParameter = ParameterRef.ParameterIsParams(parameters, paramsIndex);
                if (isParamsParameter)
                    parameterIndex = paramsIndex;
            }

            // Get the parameter type
            TypeRefBase parameterType = null;
            if (parameterIndex < parameterCount)
                parameterType = ParameterRef.GetParameterType(parameters, parameterIndex, parentExpression) as TypeRef;

            // Expand any params type - this routine wants the type of the parameter, so we shouldn't have to
            // worry about a parameter that is an array of delegates (params del[]), since that wouldn't be a delegate.
            if (isParamsParameter && parameterType != null)
                parameterType = parameterType.GetElementType() as TypeRef;

            return parameterType;
        }

        /// <summary>
        /// Retrieve the type of the parameter with the specified index from the specified code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be an <see cref="IParameters"/> CodeObject (MethodDeclBase, IndexerDecl, DelegateDecl, AnonymousMethod),
        /// MethodDefinition/MethodInfo, ConstructorInfo, or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <returns>The TypeRefBase representing the type of the parameter, otherwise null.</returns>
        public static TypeRefBase GetParameterType(object obj, int parameterIndex)
        {
            return GetParameterType(obj, parameterIndex, null);
        }

        /// <summary>
        /// Retrieve the return type of a code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be a MethodDeclBase, MethodDefinition/MethodInfo, or ConstructorInfo; an IndexerDecl,
        /// or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <param name="parentExpression">The parent expression of the code object.</param>
        /// <returns>The <see cref="TypeRefBase"/> of the return type, or null if none exists.</returns>
        public static TypeRefBase GetReturnType(object obj, Expression parentExpression)
        {
            // Handle method and indexer return types
            TypeRefBase returnTypeRef = null;
            if (obj is MethodDeclBase)
                returnTypeRef = ((MethodDeclBase)obj).ReturnType.SkipPrefixes() as TypeRefBase;
            else if (obj is IndexerDecl)
                returnTypeRef = ((IndexerDecl)obj).Type.SkipPrefixes() as TypeRefBase;
            else if (obj is MethodInfo)
                returnTypeRef = TypeRef.Create(((MethodInfo)obj).ReturnType);
            else if (obj is ConstructorInfo)
                returnTypeRef = TypeRef.Create(((ConstructorInfo)obj).DeclaringType);
            else if (obj is PropertyInfo && PropertyInfoUtil.IsIndexed((PropertyInfo)obj))
                returnTypeRef = TypeRef.Create(((PropertyInfo)obj).PropertyType);
            else if (obj is IVariableDecl)
            {
                // Handle variables that evaluate to a delegate type
                Expression type = ((IVariableDecl)obj).Type;
                if (type != null)
                {
                    // Evaluate if a TypeParameter
                    TypeRefBase typeRefBase = type.SkipPrefixes() as TypeRefBase;
                    if (typeRefBase != null)
                    {
                        if (typeRefBase.IsDelegateType)
                            returnTypeRef = typeRefBase.GetDelegateReturnType();
                    }
                }
            }
            else //if (obj is MemberInfo)
            {
                // Handle external variables that evaluate to a delegate type
                Type delegateType = null;
                if (obj is FieldInfo)
                    delegateType = ((FieldInfo)obj).FieldType;
                else if (obj is PropertyInfo)
                    delegateType = ((PropertyInfo)obj).PropertyType;
                else if (obj is EventInfo)
                    delegateType = ((EventInfo)obj).EventHandlerType;
                if (delegateType != null)
                {
                    // Evaluate if a generic parameter
                    if (delegateType.IsGenericParameter)
                        returnTypeRef = TypeRef.Create(delegateType).GetDelegateReturnType();
                    else
                        returnTypeRef = TypeRef.Create(TypeUtil.GetDelegateReturnType(delegateType));
                }
            }
            return returnTypeRef;
        }

        /// <summary>
        /// Retrieve the return type of a code object for a method, constructor, indexer, or delegate invocation.
        /// </summary>
        /// <param name="obj">The code object - can be a MethodDeclBase, MethodDefinition/MethodInfo, or ConstructorInfo; an IndexerDecl,
        /// or PropertyDefinition/PropertyInfo for an indexer; or an <see cref="IVariableDecl"/> CodeObject
        /// (PropertyDeclBase, VariableDecl), FieldDefinition/FieldInfo, PropertyDefinition/PropertyInfo, or EventDefinition/EventInfo that has a delegate type).</param>
        /// <returns>The <see cref="TypeRefBase"/> of the return type, or null if none exists.</returns>
        public static TypeRefBase GetReturnType(object obj)
        {
            return GetReturnType(obj, null);
        }

        /// <summary>
        /// Get any constraints for the specified type parameter on the specified method, or on the base virtual method if the method is an override.
        /// </summary>
        public static List<TypeParameterConstraint> GetTypeParameterConstraints(MethodInfo methodInfo, Type typeParameter)
        {
            // Override methods don't specify constraints - they inherit them from the base virtual method.
            // In order to handle invalid code, just look in the first occurrence of constraints, searching
            // any base method if the current one is an override.
            List<TypeParameterConstraint> constraints = TypeParameterConstraint.Create(typeParameter);
            if (constraints == null || constraints.Count == 0)
            {
                MethodInfo baseMethodInfo = MethodInfoUtil.FindBaseMethod(methodInfo);
                if (baseMethodInfo != null)
                {
                    // If the constraints are from a base method, we have to translate the type parameter
                    int index = MethodInfoUtil.FindTypeParameterIndex(methodInfo, typeParameter);
                    typeParameter = MethodInfoUtil.GetTypeParameter(baseMethodInfo, index);
                    constraints = GetTypeParameterConstraints(baseMethodInfo, typeParameter);
                }
            }
            return constraints;
        }

        /// <summary>
        /// True if the specified method object is an override.
        /// </summary>
        public static bool IsOverridden(object method)
        {
            if (method is MethodDeclBase)
                return ((MethodDeclBase)method).IsOverride;
            if (method is MethodInfo)
                return MethodInfoUtil.IsOverride((MethodInfo)method);
            return false;
        }

        /// <summary>
        /// Get a list of MethodRefs from the specified NamedCodeObjectGroup.
        /// </summary>
        public static List<MethodRef> MethodRefsFromGroup(NamedCodeObjectGroup namedCodeObjectGroup)
        {
            List<MethodRef> methods = null;
            if (namedCodeObjectGroup.Count > 0)
            {
                methods = new List<MethodRef>();
                foreach (object methodObj in namedCodeObjectGroup)
                {
                    MethodRef methodRef;
                    if (methodObj is MethodDeclBase)
                        methodRef = (MethodRef)((MethodDeclBase)methodObj).CreateRef();
                    else
                        methodRef = Create((MethodBase)methodObj);
                    methods.Add(methodRef);
                }
            }
            return methods;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // If we have no dot prefix, and the ShowParentTypes flag is set, then render all parent types
            // (this shouldn't occur in display of code, but only when displaying an evaluated type reference,
            // such as in a tooltip).  Generic methods won't include type arguments for enclosing types, so
            // we don't have to worry about them.
            if (!flags.HasFlag(RenderFlags.HasDotPrefix) && flags.HasFlag(RenderFlags.ShowParentTypes))
            {
                TypeRefBase typeRef = GetDeclaringType();
                if (typeRef != null)
                {
                    typeRef.AsText(writer, flags);
                    writer.Write(Dot.ParseToken);
                    flags |= RenderFlags.HasDotPrefix;
                }
            }
            else
                UpdateLineCol(writer, flags);

            if (_reference is MethodDeclBase)
                writer.WriteIdentifier(((MethodDeclBase)_reference).Name, flags);
            else if (_reference is MethodInfo)
                writer.WriteIdentifier(((MethodInfo)_reference).Name, flags);

            if (!HasInferredTypeArguments || flags.HasFlag(RenderFlags.Description))
                AsTextTypeArguments(writer, _typeArguments, flags);
        }

        /// <summary>
        /// Invalid for a <see cref="MethodRef"/> - throws an exception if called.
        /// </summary>
        public override List<int> CreateArrayRanks()
        {
            throw new Exception("Can't create array ranks on a MethodRef!");
        }

        /// <summary>
        /// Get the declaring type of the referenced method.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            TypeRefBase declaringTypeRef = GetDeclaringType(_reference);

            // A method reference doesn't store any type arguments for a parent type instance, so any
            // type arguments in any generic declaring type or its parent types will always default to
            // the declared type arguments.  Convert them from OpenTypeParameterRefs to TypeParameterRefs
            // so that they don't show up as Red in the GUI.
            if (declaringTypeRef != null && declaringTypeRef.HasTypeArguments)
                declaringTypeRef.ConvertOpenTypeParameters();

            return declaringTypeRef;
        }

        /// <summary>
        /// Get the delegate parameters if the expression evaluates to a delegate type.
        /// </summary>
        public override ICollection GetDelegateParameters()
        {
            return Parameters;
        }

        /// <summary>
        /// Get the delegate return type if the expression evaluates to a delegate type.
        /// </summary>
        public override TypeRefBase GetDelegateReturnType()
        {
            return GetReturnType();
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public override TypeRefBase GetElementType()
        {
            return null;
        }

        /// <summary>
        /// Get the full name of the object, including the namespace name.
        /// </summary>
        public override string GetFullName()
        {
            object reference = GetReferencedType();
            if (reference is MethodDeclBase)
                return ((MethodDeclBase)reference).GetFullName();
            if (reference is MethodBase)
                return MemberInfoUtil.GetFullName((MethodBase)reference);
            return null;
        }

        /// <summary>
        /// Find the parameter with the specified name.
        /// </summary>
        public ParameterRef GetParameter(string name)
        {
            object reference = GetReferencedType();
            if (reference is MethodDeclBase)
                return ((MethodDeclBase)reference).GetParameter(name);
            if (reference is MethodBase)
            {
                ParameterInfo parameterInfo = MethodInfoUtil.GetParameter((MethodBase)reference, name);
                if (parameterInfo != null)
                    return new ParameterRef(parameterInfo);
            }
            return null;
        }

        /// <summary>
        /// Get the type of the parameter with the specified index.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <param name="parentExpression">The parent expression for type evaluation purposes.</param>
        /// <returns>The TypeRefBase representing the type of the parameter, otherwise null.</returns>
        public TypeRefBase GetParameterType(int parameterIndex, Expression parentExpression)
        {
            return GetParameterType(Reference, parameterIndex, parentExpression);
        }

        /// <summary>
        /// Get the type of the parameter with the specified index.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter.</param>
        /// <returns>The TypeRefBase representing the type of the parameter, otherwise null.</returns>
        public TypeRefBase GetParameterType(int parameterIndex)
        {
            return GetParameterType(Reference, parameterIndex, null);
        }

        /// <summary>
        /// Get the return type of the method (never null - will be type 'void' instead).
        /// </summary>
        public virtual TypeRefBase GetReturnType()
        {
            TypeRefBase returnTypeRef = null;
            if (_reference is MethodDeclBase)
                returnTypeRef = ((MethodDeclBase)_reference).ReturnType.SkipPrefixes() as TypeRefBase;
            else if (_reference is MethodInfo)
            {
                MethodInfo methodInfo = (MethodInfo)_reference;
                Type returnType = methodInfo.ReturnType;
                returnTypeRef = TypeRef.Create(returnType);
            }

            return returnTypeRef;
        }

        /// <summary>
        /// Get the type parameter of the referenced method declaration with the specified index (returns null if not found).
        /// </summary>
        public TypeParameterRef GetTypeParameter(int index)
        {
            if (_reference is GenericMethodDecl)
            {
                TypeParameter typeParameter = ((GenericMethodDecl)_reference).GetTypeParameter(index);
                if (typeParameter != null)
                    return (TypeParameterRef)typeParameter.CreateRef();
            }
            else if (_reference is MethodInfo)
            {
                Type typeParameter = MethodInfoUtil.GetTypeParameter((MethodInfo)_reference, index);
                if (typeParameter != null)
                    return new TypeParameterRef(typeParameter);
            }
            return null;
        }

        /// <summary>
        /// Get any constraints for the specified type parameter on this method, or on the base virtual method if this method is an override.
        /// </summary>
        public List<TypeParameterConstraint> GetTypeParameterConstraints(TypeParameterRef typeParameterRef)
        {
            if (_reference is GenericMethodDecl)
                return ((GenericMethodDecl)_reference).GetTypeParameterConstraints(typeParameterRef.Reference as TypeParameter);
            if (_reference is MethodInfo)
                return GetTypeParameterConstraints((MethodInfo)_reference, typeParameterRef.Reference as Type);
            return null;
        }

        /// <summary>
        /// Always returns the current <see cref="MethodRef"/> object, because it doesn't make sense to add array ranks to a MethodRef.
        /// </summary>
        public override TypeRefBase MakeArrayRef(List<int> ranksToBeCopied)
        {
            return this;
        }

        /// <summary>
        /// Does nothing, because it makes no sense to parse array ranks on a <see cref="MethodRef"/>.
        /// </summary>
        public override void ParseArrayRanks(Parser parser)
        { }
    }
}