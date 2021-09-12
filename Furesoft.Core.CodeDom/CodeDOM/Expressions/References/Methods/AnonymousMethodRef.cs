// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="AnonymousMethod"/>.
    /// </summary>
    /// <remarks>
    /// Although anonymous methods don't have names, they can still be referenced, such as when the
    /// type of an expression is evaluated, resulting in an <see cref="AnonymousMethodRef"/>, which
    /// can be used during the resolution process, such as calling IsImplicitlyConvertibleTo() on it.
    /// </remarks>
    public class AnonymousMethodRef : MethodRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="AnonymousMethodRef"/> from an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethodRef(AnonymousMethod anonymousMethod, bool isFirstOnLine)
            : base(anonymousMethod, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="AnonymousMethodRef"/> from an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethodRef(AnonymousMethod anonymousMethod)
            : base(anonymousMethod, false)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Always returns null for an <see cref="AnonymousMethodRef"/>.
        /// </summary>
        public override string Name
        {
            // Anonymous methods don't have a name
            get { return null; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine the return type of the anonymous method (never null - will be 'void' instead).
        /// </summary>
        public override TypeRefBase GetReturnType()
        {
            return ((AnonymousMethod)_reference).GetReturnType();
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            return null;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            return null;
        }

        /// <summary>
        /// Determine if the <see cref="AnonymousMethodRef"/> is implicitly convertible to the specified <see cref="TypeRefBase"/>.
        /// </summary>
        /// <param name="toTypeRefBase">The <see cref="TypeRef"/>, <see cref="MethodRef"/>, or <see cref="UnresolvedRef"/> being checked.</param>
        /// <param name="standardConversionsOnly">True if only standard conversions should be allowed.</param>
        public override bool IsImplicitlyConvertibleTo(TypeRefBase toTypeRefBase, bool standardConversionsOnly)
        {
            // AnonymousMethods are only convertible to resolved delegate types

            // Fail if the destination type isn't a TypeRef
            TypeRef toTypeRef = toTypeRefBase as TypeRef;
            if (toTypeRef == null)
                return false;

            // Fail if the destination type isn't a delegate type
            if (!toTypeRef.IsDelegateType)
                return false;

            // Check if the parameters are compatible
            AnonymousMethod anonymousMethod = (AnonymousMethod)_reference;
            if (!anonymousMethod.AreParametersCompatible(toTypeRef))
                return false;

            // Check if the return types are compatible
            if (!anonymousMethod.AreReturnTypesCompatible(toTypeRef))
                return false;

            return true;
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Always render an AnonymousMethodRef as a description (which will render it's delegate type)
            ((AnonymousMethod)_reference).AsText(writer, flags | RenderFlags.SuppressNewLine | RenderFlags.Description);
        }

        #endregion
    }
}
