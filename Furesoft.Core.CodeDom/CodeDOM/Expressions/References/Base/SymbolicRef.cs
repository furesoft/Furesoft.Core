// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.AnonymousMethods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Projects.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;
using Furesoft.Core.CodeDom.Utilities.Reflection;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base
{
    /// <summary>
    /// The common base class of all symbolic references, such as <see cref="NamespaceRef"/>, <see cref="TypeRefBase"/>
    /// (common base of <see cref="TypeRef"/>, <see cref="MethodRef"/>, <see cref="UnresolvedRef"/>), <see cref="VariableRef"/>,
    /// <see cref="SelfRef"/>, <see cref="GotoTargetRef"/>, <see cref="ExternAliasRef"/>, and <see cref="DirectiveSymbolRef"/>.
    /// </summary>
    /// <remarks>
    /// A symbolic reference can consist of a string (unresolved reference), or a reference to a <see cref="Namespace"/>
    /// or <see cref="Type"/> (derived from <see cref="MemberInfo"/>), or a reference to a Decl code object (when the code is in the
    /// same solution) or a <see cref="MemberReference"/>/<see cref="MemberInfo"/> object (when the code is in a referenced assembly).
    /// </remarks>
    public abstract class SymbolicRef : Expression
    {
        #region /* FIELDS */

        // Reference can be a string (unresolved), INamedCodeObject, AnonymousMethod, or MemberInfo object.
        // It can also be null: for ThisRef, BaseRef, and VarTypeRef.
        protected object _reference;

        #endregion

        #region /* CONSTRUCTORS */

        protected SymbolicRef(string name, bool isFirstOnLine)
        {
            _reference = name;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(INamedCodeObject namedCodeObject, bool isFirstOnLine)
        {
            _reference = namedCodeObject;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(AnonymousMethod anonymousMethod, bool isFirstOnLine)
        {
            _reference = anonymousMethod;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(IMemberDefinition memberDefinition, bool isFirstOnLine)
        {
            _reference = memberDefinition;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(GenericParameter genericParameter, bool isFirstOnLine)
        {
            _reference = genericParameter;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(MemberInfo memberInfo, bool isFirstOnLine)
        {
            _reference = memberInfo;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(ParameterDefinition parameterDefinition, bool isFirstOnLine)
        {
            _reference = parameterDefinition;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(ParameterInfo parameterInfo, bool isFirstOnLine)
        {
            _reference = parameterInfo;
            IsFirstOnLine = isFirstOnLine;
        }

        protected SymbolicRef(object obj)
        {
            _reference = obj;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (_reference is INamedCodeObject)
                    return ((INamedCodeObject)_reference).Name;
                if (_reference is MemberReference)
                    return ((MemberReference)_reference).Name;
                if (_reference is MemberInfo)
                    return ((MemberInfo)_reference).Name;
                return (_reference != null ? _reference.ToString() : null);
            }
        }

        /// <summary>
        /// The code object to which the <see cref="SymbolicRef"/> refers.
        /// </summary>
        public virtual object Reference
        {
            get { return _reference; }
        }

        /// <summary>
        /// The descriptive category of the <see cref="SymbolicRef"/>.
        /// </summary>
        public virtual string Category
        {
            get
            {
                object reference = Reference;
                if (reference is INamedCodeObject)
                    return ((INamedCodeObject)reference).Category;
                if (reference is MemberInfo)
                    return MemberInfoUtil.GetCategory((MemberInfo)reference);
                if (reference is ParameterInfo)
                    return ParameterInfoUtil.GetCategory((ParameterInfo)reference);
                return null;
            }
        }

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Implicit conversion of a <see cref="Namespace"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="NamespaceRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="Namespace"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type
        /// without having to create a reference first.</remarks>
        /// <param name="namespace">The <see cref="Namespace"/> to be converted.</param>
        /// <returns>A generated <see cref="NamespaceRef"/> to the specified <see cref="Namespace"/>.</returns>
        public static implicit operator SymbolicRef(Namespace @namespace)
        {
            return @namespace.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="TypeDefinition"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="TypeRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="TypeReference"/>s to be passed directly to any method
        /// expecting a <see cref="SymbolicRef"/> type without having to create a reference first.</remarks>
        /// <param name="typeReference">The <see cref="TypeReference"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeRef"/> to the specified <see cref="TypeReference"/>.</returns>
        public static implicit operator SymbolicRef(TypeReference typeReference)
        {
            return TypeRef.Create(typeReference);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="MethodReference"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="MethodRef"/> or <see cref="ConstructorRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="MethodReference"/>s to be passed directly
        /// to any method expecting a <see cref="SymbolicRef"/> type without having to create a reference first.</remarks>
        /// <param name="methodReference">The <see cref="MethodReference"/> to be converted.</param>
        /// <returns>A generated <see cref="MethodRef"/> to the specified <see cref="MethodReference"/>.</returns>
        public static implicit operator SymbolicRef(MethodReference methodReference)
        {
            return MethodRef.Create(methodReference);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="PropertyReference"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="PropertyRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="PropertyReference"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="propertyReference">The <see cref="PropertyReference"/> to be converted.</param>
        /// <returns>A generated <see cref="PropertyRef"/> to the specified <see cref="PropertyReference"/>.</returns>
        public static implicit operator SymbolicRef(PropertyReference propertyReference)
        {
            return PropertyRef.Create(propertyReference);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="EventReference"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="EventRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="EventReference"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="eventReference">The <see cref="EventReference"/> to be converted.</param>
        /// <returns>A generated <see cref="EventRef"/> to the specified <see cref="EventReference"/>.</returns>
        public static implicit operator SymbolicRef(EventReference eventReference)
        {
            return EventRef.Create(eventReference);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="FieldReference"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="FieldRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="FieldReference"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="fieldReference">The <see cref="FieldReference"/> to be converted.</param>
        /// <returns>A generated <see cref="FieldRef"/> to the specified <see cref="FieldReference"/>.</returns>
        public static implicit operator SymbolicRef(FieldReference fieldReference)
        {
            return FieldRef.Create(fieldReference);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Type"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="TypeRef"/>).
        /// </summary>
        /// <remarks>This allows Types such as <c>typeof(int)</c> to be passed directly to any method
        /// expecting a <see cref="SymbolicRef"/> type without having to create a reference first.</remarks>
        /// <param name="type">The <see cref="Type"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeRef"/> to the specified <see cref="Type"/>.</returns>
        public static implicit operator SymbolicRef(Type type)
        {
            return TypeRef.Create(type);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="MethodBase"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="MethodRef"/> or <see cref="ConstructorRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="MethodBase"/>s (<see cref="MethodInfo"/>s or <see cref="ConstructorInfo"/>s) to be passed directly
        /// to any method expecting a <see cref="SymbolicRef"/> type without having to create a reference first.</remarks>
        /// <param name="methodBase">The <see cref="MethodBase"/> to be converted.</param>
        /// <returns>A generated <see cref="MethodRef"/> to the specified <see cref="MethodBase"/>.</returns>
        public static implicit operator SymbolicRef(MethodBase methodBase)
        {
            return MethodRef.Create(methodBase);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="PropertyInfo"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="PropertyRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="PropertyInfo"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="PropertyRef"/> to the specified <see cref="PropertyInfo"/>.</returns>
        public static implicit operator SymbolicRef(PropertyInfo propertyInfo)
        {
            return new PropertyRef(propertyInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="EventInfo"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="EventRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="EventInfo"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="eventInfo">The <see cref="EventInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="EventRef"/> to the specified <see cref="EventInfo"/>.</returns>
        public static implicit operator SymbolicRef(EventInfo eventInfo)
        {
            return new EventRef(eventInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="FieldInfo"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="FieldRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="FieldInfo"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="FieldRef"/> to the specified <see cref="FieldInfo"/>.</returns>
        public static implicit operator SymbolicRef(FieldInfo fieldInfo)
        {
            return new FieldRef(fieldInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="TypeParameter"/> to a <see cref="SymbolicRef"/> (actually, a <see cref="TypeParameterRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="TypeParameter"/>s to be passed directly to any method expecting a <see cref="SymbolicRef"/> type
        /// without having to create a reference first.</remarks>
        /// <param name="typeParameter">The <see cref="TypeParameter"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeParameterRef"/> to the specified <see cref="TypeParameter"/>.</returns>
        public static implicit operator SymbolicRef(TypeParameter typeParameter)
        {
            return typeParameter.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Statement"/> to a <see cref="SymbolicRef"/>.
        /// </summary>
        /// <remarks>This allows declarations to be passed directly to any method expecting a <see cref="SymbolicRef"/>
        /// type without having to create a reference first.</remarks>
        /// <param name="statement">The <see cref="Statement"/> to be converted.</param>
        /// <returns>A generated <see cref="SymbolicRef"/> to the specified <see cref="Statement"/>.</returns>
        public static implicit operator SymbolicRef(Statement statement)
        {
            return statement.CreateRef();
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the declaring type of the referenced object (returns null if none).
        /// </summary>
        /// <remarks>
        /// References which have declaring types include: <see cref="MethodRef"/> (and <see cref="ConstructorRef"/>, <see cref="OperatorRef"/>),
        /// <see cref="PropertyRef"/> (and <see cref="IndexerRef"/>), <see cref="EventRef"/>, <see cref="FieldRef"/>, <see cref="EnumMemberRef"/>.
        /// </remarks>
        public virtual TypeRefBase GetDeclaringType()
        {
            return null;
        }

        /// <summary>
        /// Returns the <see cref="DocSummary"/> documentation comment, or null if none exists.
        /// </summary>
        public override DocSummary GetDocSummary()
        {
            DocSummary docSummary = null;
            object reference = Reference;
            if (reference is CodeObject)
                docSummary = ((CodeObject)reference).GetDocSummary();
            return docSummary;
        }

        #region /* EQUALITY RELATED */

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public virtual bool IsSameRef(SymbolicRef symbolicRef)
        {
            return (symbolicRef != null && Reference == symbolicRef.Reference);
        }

        /// <summary>
        /// Calculate a hash code for the referenced object which is the same for all references where IsSameRef() is true.
        /// </summary>
        /// <remarks>
        /// We don't want to override GetHashCode(), because we want all TypeRefs to have unique hashes so they can be
        /// used as dictionary keys.  However, we also sometimes want hashes to be the same if IsSameRef() is true - this
        /// method allows for that.
        /// </remarks>
        public virtual int GetIsSameRefHashCode()
        {
            // In order to keep the hash code as unique as possible while still identical when necessary, this method
            // should be overloaded by any derived classes that also overload IsSameRef(), incorporating any fields
            // compared by IsSameRef() into the hash code.
            return (Reference != null ? Reference.GetHashCode() : base.GetHashCode());
        }

        #endregion

        #endregion

        #region /* PARSING */

        protected SymbolicRef(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Unresolve instead of resolving if the flag is specified
            if (flags.HasFlag(ResolveFlags.Unresolve))
                return new UnresolvedRef(this, resolveCategory);
            return this;
        }

        /// <summary>
        /// Resolve child code objects that match the specified name.
        /// </summary>
        public virtual void ResolveRef(string name, Resolver resolver)
        { }

        /// <summary>
        /// Resolve indexers.
        /// </summary>
        public virtual void ResolveIndexerRef(Resolver resolver)
        { }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(Name, flags);
        }

        protected static void AsTextDescription(CodeWriter writer, MemberReference memberReference)
        {
            const RenderFlags flags = RenderFlags.ShowParentTypes | RenderFlags.NoPreAnnotations;
            if (memberReference is TypeReference)  // TypeDefinition or GenericParameter
                TypeRefBase.AsTextTypeReference(writer, (TypeReference)memberReference, flags | RenderFlags.Description);
            else if (memberReference is MethodDefinition)
                MethodRef.AsTextMethodDefinition(writer, (MethodDefinition)memberReference, flags);
            else if (memberReference is PropertyDefinition)
                PropertyRef.AsTextPropertyDefinition(writer, (PropertyDefinition)memberReference, flags);
            else if (memberReference is FieldDefinition)
                FieldRef.AsTextFieldDefinition(writer, (FieldDefinition)memberReference, flags);
            else if (memberReference is EventDefinition)
                EventRef.AsTextEventDefinition(writer, (EventDefinition)memberReference, flags);
            else
                writer.Write(memberReference.ToString());
        }

        protected static void AsTextDescription(CodeWriter writer, MemberInfo memberInfo)
        {
            const RenderFlags flags = RenderFlags.ShowParentTypes | RenderFlags.NoPreAnnotations;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    TypeRefBase.AsTextType(writer, (Type)memberInfo, flags | RenderFlags.Description);
                    break;
                case MemberTypes.Constructor:
                    ConstructorRef.AsTextConstructorInfo(writer, (ConstructorInfo)memberInfo, flags);
                    break;
                case MemberTypes.Method:
                    MethodRef.AsTextMethodInfo(writer, (MethodInfo)memberInfo, flags);
                    break;
                case MemberTypes.Property:
                    PropertyRef.AsTextPropertyInfo(writer, (PropertyInfo)memberInfo, flags);
                    break;
                case MemberTypes.Field:
                    FieldRef.AsTextFieldInfo(writer, (FieldInfo)memberInfo, flags);
                    break;
                case MemberTypes.Event:
                    EventRef.AsTextEventInfo(writer, (EventInfo)memberInfo, flags);
                    break;
                default:
                    writer.Write(memberInfo.ToString());
                    break;
            }
        }

        /// <summary>
        /// Get a short text description of the specified <see cref="MemberReference"/>.
        /// This is generally the shortest text representation that uniquely identifies objects, even if
        /// they have the same name, for example: type or return type, name, type parameters, parameters.
        /// </summary>
        public static string GetDescription(MemberReference memberReference)
        {
            using (CodeWriter writer = new CodeWriter())
            {
                try
                {
                    AsTextDescription(writer, memberReference);
                }
                catch
                {
                    writer.Write(memberReference.Name);
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Get a short text description of the specified <see cref="MemberInfo"/>.
        /// This is generally the shortest text representation that uniquely identifies objects, even if
        /// they have the same name, for example: type or return type, name, type parameters, parameters.
        /// </summary>
        public static string GetDescription(MemberInfo memberInfo)
        {
            using (CodeWriter writer = new CodeWriter())
            {
                try
                {
                    AsTextDescription(writer, memberInfo);
                }
                catch
                {
                    writer.Write(memberInfo.Name);
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Get the description of an object which is a <see cref="CodeObject"/> or <see cref="MemberReference"/>/<see cref="MemberInfo"/> (or a <c>string</c>).
        /// </summary>
        /// <param name="object">The object to be described.</param>
        /// <returns>The string description of the object.</returns>
        public static string GetDescription(object @object)
        {
            string description;
            if (@object is CodeObject)
                description = ((CodeObject)@object).GetDescription();
            else if (@object is MemberReference)
                description = GetDescription((MemberReference)@object);
            else if (@object is MemberInfo)
                description = GetDescription((MemberInfo)@object);
            else
                description = @object.ToString();
            return description;
        }

        #endregion

        #region /* IsSameRefComparer */

        /// <summary>
        /// Determines if one <see cref="SymbolicRef"/> is equivalent to another one, meaning they both refer
        /// to the same code object or type.
        /// </summary>
        public class IsSameRefComparer : IEqualityComparer<SymbolicRef>
        {
            /// <summary>
            /// Determines if one <see cref="SymbolicRef"/> is equivalent to another one.
            /// </summary>
            public bool Equals(SymbolicRef x, SymbolicRef y)  // For IEqualityComparer<SymbolicRef>
            {
                return x.IsSameRef(y);
            }

            /// <summary>
            /// Calculate the hash code for the specified <see cref="SymbolicRef"/>.
            /// </summary>
            public int GetHashCode(SymbolicRef obj)  // For IEqualityComparer<SymbolicRef>
            {
                return obj.GetIsSameRefHashCode();
            }
        }

        #endregion
    }
}
