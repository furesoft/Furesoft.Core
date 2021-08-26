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

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Always render an AnonymousMethodRef as a description (which will render it's delegate type)
            ((AnonymousMethod)_reference).AsText(writer, flags | RenderFlags.SuppressNewLine | RenderFlags.Description);
        }

        #endregion
    }
}
