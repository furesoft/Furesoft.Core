// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections;
using System.Collections.Generic;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all user-defined type declarations (<see cref="ClassDecl"/>, <see cref="StructDecl"/>,
    /// <see cref="InterfaceDecl"/>, <see cref="EnumDecl"/>, and <see cref="DelegateDecl"/>).
    /// </summary>
    public abstract class TypeDecl : BlockStatement, ITypeDecl, ITypeParameters, IModifiers
    {
        #region /* FIELDS */

        protected Modifiers _modifiers;
        protected string _name;
        protected ChildList<TypeParameter> _typeParameters;        // Not used for EnumDecls
        protected ChildList<ConstraintClause> _constraintClauses;  // Not used for EnumDecls

        #endregion

        #region /* CONSTRUCTORS */

        protected TypeDecl(string name, Modifiers modifiers, CodeObject body)
            : base(body, false)
        {
            _name = name;
            _modifiers = modifiers;
        }

        protected TypeDecl(string name, Modifiers modifiers)
        {
            _name = name;
            _modifiers = modifiers;
        }

        protected TypeDecl(string name)
        {
            _name = name;
        }

        protected TypeDecl(string name, Modifiers modifiers, params TypeParameter[] typeParameters)
            : this(name, modifiers)
        {
            CreateTypeParameters().AddRange(typeParameters);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="TypeDecl"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "type"; }
        }

        /// <summary>
        /// Optional <see cref="Modifiers"/> for the type.
        /// </summary>
        public Modifiers Modifiers
        {
            get { return _modifiers; }
            set { _modifiers = value; }
        }

        /// <summary>
        /// True if the type is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return _modifiers.HasFlag(Modifiers.Abstract); }
            set { _modifiers = (value ? _modifiers | Modifiers.Abstract : _modifiers & ~Modifiers.Abstract); }
        }

        /// <summary>
        /// True if the type is a partial type.
        /// </summary>
        public bool IsPartial
        {
            get { return _modifiers.HasFlag(Modifiers.Partial); }
            set { _modifiers = (value ? _modifiers | Modifiers.Partial : _modifiers & ~Modifiers.Partial); }
        }

        /// <summary>
        /// True if the type has public access.
        /// </summary>
        public bool IsPublic
        {
            get { return _modifiers.HasFlag(Modifiers.Public); }
            // Force other flags off if setting to Public
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Protected | Modifiers.Internal) | Modifiers.Public : _modifiers & ~Modifiers.Public); }
        }

        /// <summary>
        /// True if the type has private access.
        /// </summary>
        public bool IsPrivate
        {
            // Should only be true for nested types, and defaults to true if nested and nothing else is set
            get { return (_modifiers.HasFlag(Modifiers.Private) || (IsNested && (_modifiers & (Modifiers.Protected | Modifiers.Internal | Modifiers.Public)) == 0)); }
            // Force other flags off if setting to Private
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Public) | Modifiers.Private : _modifiers & ~Modifiers.Private); }
        }

        /// <summary>
        /// True if the type has protected access.
        /// </summary>
        public bool IsProtected
        {
            // Should only be true for nested types
            get { return _modifiers.HasFlag(Modifiers.Protected); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Protected : _modifiers & ~Modifiers.Protected); }
        }

        /// <summary>
        /// True if the type has internal access.
        /// </summary>
        public bool IsInternal
        {
            // Defaults to true if nothing else is set
            get { return (_modifiers.HasFlag(Modifiers.Internal) || ((_modifiers & (Modifiers.Private | Modifiers.Protected | Modifiers.Public)) == 0)); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Internal : _modifiers & ~Modifiers.Internal); }
        }

        /// <summary>
        /// True if the type is a nested type.
        /// </summary>
        public bool IsNested
        {
            get { return (_parent is TypeDecl); }
        }

        /// <summary>
        /// True if the type is a nullable type.
        /// </summary>
        public bool IsNullableType
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is static.
        /// </summary>
        public virtual bool IsStatic
        {
            get { return _modifiers.HasFlag(Modifiers.Static); }
            set { _modifiers = (value ? _modifiers | Modifiers.Static : _modifiers & ~Modifiers.Static); }
        }

        /// <summary>
        /// A collection of optional <see cref="TypeParameter"/>s (for generic types).
        /// </summary>
        public ChildList<TypeParameter> TypeParameters
        {
            get { return _typeParameters; }
        }

        /// <summary>
        /// True if the type has <see cref="TypeParameter"/>s.
        /// </summary>
        public bool HasTypeParameters
        {
            get { return (_typeParameters != null && _typeParameters.Count > 0); }
        }

        /// <summary>
        /// The number of <see cref="TypeParameter"/>s the type has.
        /// </summary>
        public int TypeParameterCount
        {
            get { return (_typeParameters != null ? _typeParameters.Count : 0); }
        }

        /// <summary>
        /// A collection of optional <see cref="ConstraintClause"/>s (for generic types).
        /// </summary>
        public ChildList<ConstraintClause> ConstraintClauses
        {
            get { return _constraintClauses; }
        }

        /// <summary>
        /// True if there are any <see cref="ConstraintClause"/>s.
        /// </summary>
        public bool HasConstraintClauses
        {
            get { return (_constraintClauses != null && _constraintClauses.Count > 0); }
        }

        /// <summary>
        /// True if the type is a class.
        /// </summary>
        public virtual bool IsClass
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is a delegate type.
        /// </summary>
        public virtual bool IsDelegateType
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is an enum.
        /// </summary>
        public virtual bool IsEnum
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is a generic parameter.
        /// </summary>
        public bool IsGenericParameter
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is a generic type (meaning that either it or an enclosing type has type parameters,
        /// and it's not an enum).
        /// </summary>
        public virtual bool IsGenericType
        {
            get { return (HasTypeParameters || (_parent is TypeDecl && ((TypeDecl)_parent).HasTypeParameters)); }
        }

        /// <summary>
        /// True if the type is an interface.
        /// </summary>
        public virtual bool IsInterface
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is a struct.
        /// </summary>
        public virtual bool IsStruct
        {
            get { return false; }
        }

        /// <summary>
        /// True if the type is a value type.
        /// </summary>
        public virtual bool IsValueType
        {
            get { return false; }
        }

        /// <summary>
        /// Get the declaring <see cref="TypeDecl"/>.
        /// </summary>
        public TypeDecl DeclaringType
        {
            get { return (_parent as TypeDecl); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create the list of <see cref="TypeParameter"/>s, or return the existing one.
        /// </summary>
        public ChildList<TypeParameter> CreateTypeParameters()
        {
            if (_typeParameters == null)
                _typeParameters = new ChildList<TypeParameter>(this);
            return _typeParameters;
        }

        /// <summary>
        /// Add one or more <see cref="TypeParameter"/>s.
        /// </summary>
        public void AddTypeParameters(params TypeParameter[] typeParameters)
        {
            CreateTypeParameters().AddRange(typeParameters);
        }

        /// <summary>
        /// Create the list of <see cref="ConstraintClause"/>s, or return the existing one.
        /// </summary>
        public ChildList<ConstraintClause> CreateConstraintClauses()
        {
            if (_constraintClauses == null)
                _constraintClauses = new ChildList<ConstraintClause>(this);
            return _constraintClauses;
        }

        /// <summary>
        /// Add one or more <see cref="ConstraintClause"/>s.
        /// </summary>
        public void AddConstraintClauses(params ConstraintClause[] constraintClauses)
        {
            CreateConstraintClauses().AddRange(constraintClauses);
        }

        /// <summary>
        /// Get any constraints for the specified <see cref="TypeParameter"/> on this type.
        /// </summary>
        public List<TypeParameterConstraint> GetTypeParameterConstraints(TypeParameter typeParameter)
        {
            if (_constraintClauses != null)
            {
                foreach (ConstraintClause constraintClause in _constraintClauses)
                {
                    if (constraintClause.TypeParameter.Reference == typeParameter)
                        return constraintClause.Constraints;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the base type.
        /// </summary>
        public virtual TypeRef GetBaseType()
        {
            return null;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            TypeDecl clone = (TypeDecl)base.Clone();
            clone._typeParameters = ChildListHelpers.Clone(_typeParameters, clone);
            clone._constraintClauses = ChildListHelpers.Clone(_constraintClauses, clone);
            return clone;
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new TypeRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            return new TypeRef(this, isFirstOnLine, typeArguments, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            return new TypeRef(this, isFirstOnLine, typeArguments, null);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            return new TypeRef(this, false, typeArguments, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments)
        {
            return new TypeRef(this, false, typeArguments, null);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, params Expression[] typeArguments)
        {
            return new TypeRef(this, isFirstOnLine, typeArguments);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateRef(params Expression[] typeArguments)
        {
            return new TypeRef(this, false, typeArguments);
        }

        /// <summary>
        /// Create an array reference to this <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateArrayRef(bool isFirstOnLine, params int[] ranks)
        {
            return new TypeRef(this, isFirstOnLine, ranks);
        }

        /// <summary>
        /// Create an array reference to this <see cref="TypeDecl"/>.
        /// </summary>
        public TypeRef CreateArrayRef(params int[] ranks)
        {
            return new TypeRef(this, false, ranks);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef(bool isFirstOnLine)
        {
            return TypeRef.CreateNullable(CreateRef(), isFirstOnLine);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef()
        {
            return TypeRef.CreateNullable(CreateRef(), false);
        }

        /// <summary>
        /// Get all member declarations of the <see cref="TypeDecl"/> (methods, properties, indexers, events, fields, and nested types).
        /// </summary>
        /// <param name="currentPartOnly">True to get members from current part only if the TypeDecl is partial.</param>
        public IEnumerable<INamedCodeObject> GetMemberDecls(bool currentPartOnly)
        {
            if (_body != null)
            {
                foreach (CodeObject codeObject in _body)
                {
                    if (codeObject is IMultiVariableDecl)
                    {
                        foreach (VariableDecl variableDecl in ((IMultiVariableDecl)codeObject))
                            yield return variableDecl;
                    }
                    else if (codeObject is INamedCodeObject && !codeObject.IsGenerated)
                        yield return (INamedCodeObject)codeObject;
                }
            }
            if (IsPartial && !currentPartOnly)
            {
                foreach (TypeDecl otherPart in GetOtherParts())
                {
                    if (otherPart.Body != null)
                    {
                        foreach (CodeObject codeObject in otherPart.Body)
                        {
                            if (codeObject is IMultiVariableDecl)
                            {
                                foreach (VariableDecl variableDecl in ((IMultiVariableDecl)codeObject))
                                    yield return variableDecl;
                            }
                            else if (codeObject is INamedCodeObject && !codeObject.IsGenerated)
                                yield return (INamedCodeObject)codeObject;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all member declarations of the <see cref="TypeDecl"/> (methods, properties, indexers, events, fields, and nested types).
        /// </summary>
        public IEnumerable<INamedCodeObject> GetMemberDecls()
        {
            return GetMemberDecls(false);
        }

        /// <summary>
        /// Get all nested type declarations of the <see cref="TypeDecl"/>.
        /// </summary>
        /// <param name="recursive">True to recursively return all nested type declarations, otherwise false.</param>
        /// <param name="currentPartOnly">True to get nested types from current part only if the TypeDecl is partial, otherwise false.</param>
        public IEnumerable<TypeDecl> GetNestedTypeDecls(bool recursive, bool currentPartOnly)
        {
            if (_body != null)
            {
                foreach (CodeObject codeObject in _body)
                {
                    if (codeObject is TypeDecl)
                    {
                        TypeDecl typeDecl = (TypeDecl)codeObject;
                        yield return typeDecl;

                        if (recursive)
                        {
                            foreach (TypeDecl nestedType in typeDecl.GetNestedTypeDecls(true, currentPartOnly))
                                yield return nestedType;
                        }
                    }
                }
            }
            if (IsPartial && !currentPartOnly)
            {
                foreach (TypeDecl otherPart in GetOtherParts())
                {
                    if (otherPart.Body != null)
                    {
                        foreach (CodeObject codeObject in otherPart.Body)
                        {
                            if (codeObject is TypeDecl)
                            {
                                TypeDecl typeDecl = (TypeDecl)codeObject;
                                yield return typeDecl;

                                if (recursive)
                                {
                                    foreach (TypeDecl nestedType in typeDecl.GetNestedTypeDecls(true))
                                        yield return nestedType;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all nested type declarations of the <see cref="TypeDecl"/>.
        /// </summary>
        /// <param name="recursive">True to recursively return all nested type declarations, otherwise false.</param>
        public IEnumerable<TypeDecl> GetNestedTypeDecls(bool recursive)
        {
            return GetNestedTypeDecls(recursive, false);
        }

        /// <summary>
        /// Get all nested type declarations of the <see cref="TypeDecl"/>.
        /// </summary>
        public IEnumerable<TypeDecl> GetNestedTypeDecls()
        {
            return GetNestedTypeDecls(false, false);
        }

        /// <summary>
        /// Get the non-static constructor with the specified parameters.
        /// </summary>
        public virtual ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes)
        {
            ConstructorDecl found = GetMethod<ConstructorDecl>(_name, parameterTypes);
            if (found != null)
                return (ConstructorRef)found.CreateRef();
            TypeRef baseRef = GetBaseType();
            return (baseRef != null ? baseRef.GetConstructor(parameterTypes) : null);
        }

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        public virtual NamedCodeObjectGroup GetConstructors(bool currentPartOnly)
        {
            NamedCodeObjectGroup constructors = new NamedCodeObjectGroup();
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            if (currentPartOnly && _body != null)
                _body.FindChildren<ConstructorDecl>(_name, found);
            else
                FindInAllParts<ConstructorDecl>(_name, found);
            foreach (INamedCodeObject namedCodeObject in found)
            {
                if (!((ConstructorDecl)namedCodeObject).IsStatic)
                    constructors.Add(namedCodeObject);
            }
            return constructors;
        }

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        public virtual NamedCodeObjectGroup GetConstructors()
        {
            return GetConstructors(false);
        }

        /// <summary>
        /// Get the method with the specified name and parameter types.
        /// </summary>
        public virtual MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
        {
            MethodDeclBase found = GetMethod<MethodDeclBase>(name, parameterTypes);
            if (found != null)
                return (MethodRef)found.CreateRef();
            TypeRef baseRef = GetBaseType();
            return (baseRef != null ? baseRef.GetMethod(name, parameterTypes) : null);
        }

        /// <summary>
        /// Get all methods with the specified name, adding them to the provided NamedCodeObjectGroup.
        /// </summary>
        public virtual void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        {
            FindInAllParts<MethodDeclBase>(name, results);
            if (searchBaseClasses)
            {
                TypeRef baseRef = GetBaseType();
                if (baseRef != null)
                    baseRef.GetMethods(name, true, results);
            }
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="searchBaseClasses">Pass <c>false</c> to NOT search base classes.</param>
        public List<MethodRef> GetMethods(string name, bool searchBaseClasses)
        {
            NamedCodeObjectGroup results = new NamedCodeObjectGroup();
            GetMethods(name, searchBaseClasses, results);
            return MethodRef.MethodRefsFromGroup(results);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        public List<MethodRef> GetMethods(string name)
        {
            return GetMethods(name, false);
        }

        /// <summary>
        /// Get the property with the specified name.
        /// </summary>
        public virtual PropertyRef GetProperty(string name)
        {
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            FindInAllParts<PropertyDecl>(name, found);
            if (found.Count > 0)
                return (PropertyRef)((PropertyDecl)found[0]).CreateRef();
            TypeRef baseRef = GetBaseType();
            return (baseRef != null ? baseRef.GetProperty(name) : null);
        }

        /// <summary>
        /// Get the field with the specified name.
        /// </summary>
        public virtual FieldRef GetField(string name)
        {
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            FindInAllParts<FieldDecl>(name, found);
            if (found.Count > 0)
                return (FieldRef)((FieldDecl)found[0]).CreateRef();
            TypeRef baseRef = GetBaseType();
            return (baseRef != null ? baseRef.GetField(name) : null);
        }

        /// <summary>
        /// Get the nested type with the specified name.
        /// </summary>
        public TypeRef GetNestedType(string name)
        {
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            FindInAllParts<TypeDecl>(name, found);
            if (found.Count > 0)
                return ((TypeDecl)found[0]).CreateRef();
            TypeRef baseRef = GetBaseType();
            return (baseRef != null ? baseRef.GetNestedType(name) : null);
        }

        /// <summary>
        /// Get the method with the specified name and parameters, and of type T.
        /// </summary>
        public T GetMethod<T>(string name, params TypeRefBase[] parameterTypes) where T : MethodDeclBase
        {
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            FindInAllParts<T>(name, found);
            foreach (T methodDecl in found)
            {
                if (methodDecl.MatchParameters(parameterTypes))
                    return methodDecl;
            }
            return null;
        }

        /// <summary>
        /// Find all members of the type with the specified name and of type T, including in other parts of partial types.
        /// </summary>
        protected void FindInAllParts<T>(string name, NamedCodeObjectGroup results) where T : CodeObject
        {
            if (_body != null)
                _body.FindChildren<T>(name, results);
            if (IsPartial)
            {
                foreach (TypeDecl otherPart in GetOtherParts())
                {
                    if (otherPart.Body != null)
                        otherPart.Body.FindChildren<T>(name, results);
                }
            }
        }

        /// <summary>
        /// Find the index of the specified <see cref="TypeParameter"/> in the declaration of the <see cref="TypeDecl"/> or
        /// an enclosing <see cref="TypeDecl"/> if this one is nested.  Also handles partial types.
        /// </summary>
        /// <returns>The index of the <see cref="TypeParameter"/>, or -1 if not found.</returns>
        public int FindTypeParameterIndex(TypeParameter typeParameter)
        {
            int index;
            if (FindTypeParameterIndex(typeParameter, out index))
                return index;
            return -1;
        }

        /// <summary>
        /// Find the index of the specified <see cref="TypeParameter"/> in the declaration of the <see cref="TypeDecl"/> or
        /// an enclosing <see cref="TypeDecl"/> if this one is nested.  Also handles partial types.
        /// </summary>
        /// <returns>Returns true if the index of the TypeParameter was found, otherwise false.</returns>
        protected bool FindTypeParameterIndex(TypeParameter typeParameter, out int index)
        {
            // First, recursively check any parent types
            if (_parent is TypeDecl)
            {
                if (((TypeDecl)_parent).FindTypeParameterIndex(typeParameter, out index))
                    return true;
            }
            else
                index = 0;

            // Then, check the type parameters on the current type
            int startingIndex = index;
            if (FindTypeParameterIndexLocal(typeParameter, ref index))
                return true;

            // Finally, also check the type parameters of any partial types
            if (IsPartial)
            {
                int endingIndex = index;
                foreach (TypeDecl otherPart in GetOtherParts())
                {
                    index = startingIndex;
                    if (otherPart.FindTypeParameterIndexLocal(typeParameter, ref index))
                        return true;
                }
                index = endingIndex;
            }

            return false;
        }

        private bool FindTypeParameterIndexLocal(TypeParameter typeParameter, ref int index)
        {
            if (_typeParameters != null)
            {
                foreach (TypeParameter localTypeParameter in _typeParameters)
                {
                    if (localTypeParameter == typeParameter)
                        return true;
                    ++index;
                }
            }
            return false;
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(_name, this);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(_name, this);
        }

        /// <summary>
        /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
        /// <param name="isPrivate">True if the access is private.</param>
        /// <param name="isProtected">True if the access is protected.</param>
        /// <param name="isInternal">True if the access is internal.</param>
        public void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            // The isTargetOfAssignment flag is needed only for properties/indexers/events, not types
            isPrivate = IsPrivate;
            if (!isPrivate)
            {
                isProtected = IsProtected;
                isInternal = IsInternal;
            }
            else
                isProtected = isInternal = false;
        }

        #region /* PARTIAL TYPES */

        /// <summary>
        /// Get any other parts of this <see cref="TypeDecl"/>.
        /// </summary>
        /// <returns></returns>
        public List<TypeDecl> GetOtherParts()
        {
            List<TypeDecl> otherParts = new List<TypeDecl>();
            GetOtherParts(otherParts, null);
            return otherParts;
        }

        protected void GetOtherParts(List<TypeDecl> otherParts, List<string> parentTypes)
        {
            // Find any other parts of the type
            CodeObject parent = _parent;
            if (parent is NamespaceDecl)
                GetOtherParts(((NamespaceDecl)parent).Namespace.Find(_name), otherParts, parentTypes);
            else if (parent is TypeDecl)
            {
                // Look for other parts of a nested type in the parent type
                TypeDecl parentTypeDecl = (TypeDecl)parent;
                GetOtherParts(parentTypeDecl.Body.FindChildren(_name), otherParts, parentTypes);

                // If the parent type is also partial, then look for other parts of the parent
                // type, and look in them for additional parts of the nested type.
                if (parentTypeDecl.IsPartial)
                {
                    if (parentTypes == null)
                        parentTypes = new List<string>();
                    parentTypes.Insert(0, _name);
                    parentTypeDecl.GetOtherParts(otherParts, parentTypes);
                }
            }
        }

        private void GetOtherParts(object obj, List<TypeDecl> otherParts, List<string> parentTypes)
        {
            if (obj is TypeDecl)
                GetOtherParts((TypeDecl)obj, otherParts, parentTypes);
            else if (obj is NamespaceTypeGroup)
            {
                NamespaceTypeGroup group = (NamespaceTypeGroup)obj;
                // Lock the NamespaceTypeGroup while iterating it to prevent changes while parsing on other threads
                lock (group.SyncRoot)
                {
                    foreach (object @object in group)
                    {
                        if (@object is TypeDecl)
                            GetOtherParts((TypeDecl)@object, otherParts, parentTypes);
                    }
                }
            }
        }

        private void GetOtherParts(TypeDecl typeDecl, List<TypeDecl> otherParts, List<string> parentTypes)
        {
            if (typeDecl != this)
            {
                if (parentTypes == null)
                {
                    if (typeDecl.TypeParameterCount == TypeParameterCount)
                        otherParts.Add(typeDecl);
                }
                else
                {
                    List<string> newList = new List<string>(parentTypes);
                    string parentType = parentTypes[0];
                    newList.RemoveAt(0);
                    GetOtherParts(typeDecl.Body.FindChildren(parentType), otherParts, newList.Count > 0 ? newList : null);
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the delegate parameters of the type (if any).
        /// </summary>
        public virtual ICollection GetDelegateParameters()
        {
            return null;
        }

        /// <summary>
        /// Get the delegate return type of the type (if any).
        /// </summary>
        public virtual TypeRefBase GetDelegateReturnType()
        {
            return null;
        }

        /// <summary>
        /// Determine if the type is assignable from the specified type.
        /// </summary>
        public virtual bool IsAssignableFrom(TypeRef typeRef)
        {
            if (typeRef == null)
                return false;

            TypeRef thisTypeRef = CreateRef();
            return (typeRef.IsSameRef(thisTypeRef) || typeRef.IsSubclassOf(thisTypeRef));
        }

        /// <summary>
        /// Determine if the type is a subclass of the specified type.
        /// </summary>
        public virtual bool IsSubclassOf(TypeRef classTypeRef)
        {
            return classTypeRef.IsSameRef(GetBaseType());
        }

        /// <summary>
        /// Determine if the type implements the specified interface type.
        /// </summary>
        public virtual bool IsImplementationOf(TypeRef interfaceTypeRef)
        {
            return false;
        }

        /// <summary>
        /// Check if the specified TypeDecl is identical to OR has the same name/namespace as the current one (could be different parts).
        /// </summary>
        public bool IsSameAs(TypeDecl typeDecl)
        {
            return (this == typeDecl || (_name == typeDecl.Name && (_typeParameters != null ? _typeParameters.Count : 0) == typeDecl.TypeParameterCount && GetNamespace() == typeDecl.GetNamespace()));
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public string GetFullName(bool descriptive)
        {
            string name = _name;
            if (_typeParameters != null && _typeParameters.Count > 0)
            {
                if (descriptive)
                    name += GetTypeParametersAsString(_typeParameters);
                else
                    name += "`" + TypeParameterCount;
            }
            if (Parent is TypeDecl)
                return ((TypeDecl)Parent).GetFullName(descriptive) + (descriptive ? "." : "+") + name;
            Namespace @namespace = GetNamespace();
            return (@namespace != null && !@namespace.IsGlobal ? @namespace.FullName + "." : "") + name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        /// <summary>
        /// Get the specified type parameters as a descriptive string.
        /// </summary>
        internal static string GetTypeParametersAsString(ChildList<TypeParameter> typeParameters)
        {
            string result = TypeParameter.ParseTokenStart;
            bool isFirst = true;
            foreach (TypeParameter typeParameter in typeParameters)
            {
                result += (isFirst ? "" : ", ") + typeParameter.Name;
                isFirst = false;
            }
            result += TypeParameter.ParseTokenEnd;
            return result;
        }

        #endregion

        #region /* PARSING */

        protected TypeDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Force all type declarations to start on a new line by default
            IsFirstOnLine = true;
        }

        protected void ParseModifiersAndAnnotations(Parser parser)
        {
            _modifiers = ModifiersHelpers.Parse(parser, this);  // Parse any modifiers in reverse from the Unused list
            ParseUnusedAnnotations(parser, this, false);        // Parse attributes and/or doc comments from the Unused list
        }

        protected void ParseNameTypeParameters(Parser parser)
        {
            MoveComments(parser.LastToken);
            _name = parser.GetIdentifierText();                       // Parse the name
            MoveEOLComment(parser.LastToken);                         // Associate any skipped EOL comment
            _typeParameters = TypeParameter.ParseList(parser, this);  // Parse any type parameters
            MoveEOLComment(parser.LastToken);                         // Associate any skipped EOL comment
        }

        protected void ParseConstraintClauses(Parser parser)
        {
            _constraintClauses = ConstraintClause.ParseList(parser, this);  // Parse any constraint clauses
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return true;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return base.NewLines; }
            set
            {
                // If we're changing to or from zero, also change any prefix attributes
                bool isFirstOnLine = (value != 0);
                if (_annotations != null && ((!isFirstOnLine && IsFirstOnLine) || (isFirstOnLine && !IsFirstOnLine)))
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (annotation is Attribute)
                            annotation.IsFirstOnLine = isFirstOnLine;
                    }
                }

                base.NewLines = value;
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_typeParameters == null || _typeParameters.Count == 0 || (!_typeParameters[0].IsFirstOnLine && _typeParameters.IsSingleLine))
                    && (_constraintClauses == null || _constraintClauses.Count == 0 || (!_constraintClauses[0].IsFirstOnLine && _constraintClauses.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_typeParameters != null && _typeParameters.Count > 0)
                    {
                        _typeParameters[0].IsFirstOnLine = false;
                        _typeParameters.IsSingleLine = true;
                    }
                    if (_constraintClauses != null && _constraintClauses.Count > 0)
                    {
                        _constraintClauses[0].IsFirstOnLine = false;
                        _constraintClauses.IsSingleLine = true;
                    }
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            AsTextName(writer, flags);
        }

        public void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.Description) && _parent is TypeDecl)
            {
                ((TypeDecl)_parent).AsTextName(writer, flags);
                Dot.AsTextDot(writer);
            }

            writer.WriteIdentifier(_name, flags);
            if (HasTypeParameters)
                TypeParameter.AsTextTypeParameters(writer, _typeParameters, flags);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            if (!HasConstraintClauses)
                base.AsTextSuffix(writer, flags);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            ConstraintClause.AsTextConstraints(writer, _constraintClauses, flags | RenderFlags.HasTerminator);
            base.AsTextAfter(writer, flags);
        }

        #endregion
    }
}
