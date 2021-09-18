// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;
using System.Collections.Generic;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="Alias"/>, which can in turn refer to a <see cref="NamespaceRef"/>
    /// or a <see cref="TypeRef"/> (or an <see cref="UnresolvedRef"/>).
    /// </summary>
    /// <remarks>
    /// <see cref="AliasRef"/> subclasses <see cref="TypeRef"/> so that it can have array ranks just like a <see cref="TypeRef"/>
    /// if it's a type alias.  The aliased type can be an instance of a generic type (with type arguments), but the <see cref="AliasRef"/>
    /// itself can't have any type arguments.  In such a case, the HasTypeArguments and TypeArguments
    /// properties will return the values of the aliased type, so that the AliasRef may be used synonymously
    /// with the generic aliased type (but the type arguments will not be displayed on the AliasRef).
    /// An AliasRef can have array ranks regardless of whether or not the aliased type has any.
    /// </remarks>
    public class AliasRef : TypeRef
    {
        /// <summary>
        /// Create an <see cref="AliasRef"/> from an <see cref="Alias"/>.
        /// </summary>
        public AliasRef(Alias aliasDecl, bool isFirstOnLine)
            : base(aliasDecl, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="AliasRef"/> from an <see cref="Alias"/>.
        /// </summary>
        public AliasRef(Alias aliasDecl)
            : base(aliasDecl, false)
        { }

        /// <summary>
        /// Create an <see cref="AliasRef"/> from an <see cref="Alias"/>.
        /// </summary>
        public AliasRef(Alias aliasDecl, bool isFirstOnLine, List<int> arrayRanks)
            : base(aliasDecl, isFirstOnLine, null, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="AliasRef"/> from an <see cref="Alias"/>.
        /// </summary>
        public AliasRef(Alias aliasDecl, bool isFirstOnLine, params int[] arrayRanks)
            : this(aliasDecl, isFirstOnLine, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        /// <summary>
        /// Create an <see cref="AliasRef"/> from an <see cref="Alias"/>.
        /// </summary>
        public AliasRef(Alias aliasDecl, params int[] arrayRanks)
            : this(aliasDecl, false, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        // NOTE: We can't just override the Reference property, and have it refer to what the alias refers
        // to, because we want to treat the alias as a new type that can have array indexes, etc.

        /// <summary>
        /// Get the referenced <see cref="Alias"/> object.
        /// </summary>
        public Alias Alias
        {
            get { return (Alias)_reference; }
        }

        /// <summary>
        /// True if the referenced <see cref="Alias"/> is a namespace alias.
        /// </summary>
        public bool IsNamespace
        {
            get { return ((Alias)_reference).IsNamespace; }
        }

        /// <summary>
        /// True if the referenced <see cref="Alias"/> is a type alias.
        /// </summary>
        public bool IsType
        {
            get { return ((Alias)_reference).IsType; }
        }

        /// <summary>
        /// The name of the <see cref="AliasRef"/>.
        /// </summary>
        public override string Name
        {
            get { return ((Alias)_reference).Name; }
        }

        /// <summary>
        /// The <see cref="Namespace"/> of the referenced alias if it's a namespace alias (otherwise null).
        /// </summary>
        public NamespaceRef Namespace
        {
            get { return ((Alias)_reference).Namespace; }
        }

        /// <summary>
        /// The <see cref="Type"/> of the referenced alias if it's a type alias (otherwise null).
        /// </summary>
        public TypeRef Type
        {
            get { return ((Alias)_reference).Type; }
        }

        /// <summary>
        /// The type argument <see cref="Expression"/>s of the reference (if any).
        /// </summary>
        public override ChildList<Expression> TypeArguments
        {
            get { return (IsType ? Type.TypeArguments : null); }
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(Name, flags);
            AsTextTypeArguments(writer, _typeArguments, flags);
            AsTextArrayRanks(writer, flags);
        }

        /// <summary>
        /// Get the actual type reference.
        /// </summary>
        /// <returns>The <see cref="ITypeDecl"/> (<see cref="TypeDecl"/> or <see cref="TypeParameter"/>, but NOT <see cref="Alias"/>)
        /// or <see cref="Type"/> (or null if the type is unresolved or if the <see cref="Alias"/> is to a <see cref="NamespaceRef"/>).</returns>
        public override object GetReferencedType()
        {
            TypeRefBase typeRefBase = Type;
            return (typeRefBase != null ? typeRefBase.GetReferencedType() : null);
        }

        /// <summary>
        /// Determine if the specified <see cref="TypeRefBase"/> refers to the same generic type, regardless of actual type arguments.
        /// </summary>
        public override bool IsSameGenericType(TypeRefBase typeRefBase)
        {
            TypeRefBase typeRef = Type;
            return (typeRef != null && typeRef.IsSameGenericType(typeRefBase));
        }

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public override bool IsSameRef(SymbolicRef symbolicRef)
        {
            return (base.IsSameRef(symbolicRef) || (IsType && Type.IsSameRef(symbolicRef)) || (IsNamespace && Namespace.IsSameRef(symbolicRef)));
        }
    }
}