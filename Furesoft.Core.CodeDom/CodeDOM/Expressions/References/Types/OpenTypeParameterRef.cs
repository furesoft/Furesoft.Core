// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="TypeParameter"/> (or <see cref="Type"/>) from <b>outside</b>
    /// the generic type or method declaration that declares it, and exists only temporarily until it is replaced by a concrete type or
    /// <see cref="TypeParameterRef"/> during the type argument evaluation process.
    /// </summary>
    /// <remarks>
    /// In contrast, a <see cref="TypeParameterRef"/> represents a reference to a <see cref="TypeParameter"/> (or
    /// <see cref="Type"/>) from <b>within</b> the generic type or method declaration that declares it.
    /// Like a <see cref="TypeRef"/> and <see cref="TypeParameterRef"/>, an <see cref="OpenTypeParameterRef"/> can include array ranks,
    /// although it doesn't support type arguments.
    /// </remarks>
    public class OpenTypeParameterRef : TypeParameterRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine, List<int> arrayRanks)
            : base(declaration, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, List<int> arrayRanks)
            : base(declaration, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, bool isFirstOnLine, params int[] arrayRanks)
            : base(declaration, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="TypeParameter"/>.
        /// </summary>
        public OpenTypeParameterRef(TypeParameter declaration, params int[] arrayRanks)
            : base(declaration, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine, List<int> arrayRanks)
            : base(type, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine)
            : base(type, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type)
            : base(type, false)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, List<int> arrayRanks)
            : base(type, false, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, bool isFirstOnLine, params int[] arrayRanks)
            : base(type, isFirstOnLine, arrayRanks)
        { }

        /// <summary>
        /// Create an <see cref="OpenTypeParameterRef"/> from a <see cref="Type"/> (which must be a generic parameter).
        /// </summary>
        public OpenTypeParameterRef(Type type, params int[] arrayRanks)
            : base(type, false, arrayRanks)
        { }

        #endregion

        #region /* PROPERTIES */

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Convert into a <see cref="TypeParameterRef"/>.
        /// </summary>
        public TypeParameterRef ConvertToTypeParameterRef()
        {
            object reference = Reference;
            if (reference is TypeParameter)
                return new TypeParameterRef((TypeParameter)reference, IsFirstOnLine, ArrayRanks);
            return new TypeParameterRef((Type)reference, IsFirstOnLine, ArrayRanks);
        }

        #endregion
    }
}
