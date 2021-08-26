// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that have <see cref="TypeParameters"/> (<see cref="TypeDecl"/>, <see cref="GenericMethodDecl"/>).
    /// </summary>
    public interface ITypeParameters
    {
        /// <summary>
        /// The list of <see cref="ConstraintClause"/>s.
        /// </summary>
        ChildList<ConstraintClause> ConstraintClauses { get; }

        /// <summary>
        /// True if there are any <see cref="ConstraintClause"/>s.
        /// </summary>
        bool HasConstraintClauses { get; }

        /// <summary>
        /// True if there are any <see cref="TypeParameter"/>s.
        /// </summary>
        bool HasTypeParameters { get; }

        /// <summary>
        /// The number of <see cref="TypeParameter"/>s.
        /// </summary>
        int TypeParameterCount { get; }

        /// <summary>
        /// The list of <see cref="TypeParameter"/>s.
        /// </summary>
        ChildList<TypeParameter> TypeParameters { get; }

        /// <summary>
        /// Add one or more <see cref="ConstraintClause"/>s.
        /// </summary>
        void AddConstraintClauses(params ConstraintClause[] constraintClause);

        /// <summary>
        /// Add one or more <see cref="TypeParameter"/>s.
        /// </summary>
        void AddTypeParameters(params TypeParameter[] typeParameters);

        /// <summary>
        /// Create the list of <see cref="ConstraintClause"/>s, or return the existing one.
        /// </summary>
        ChildList<ConstraintClause> CreateConstraintClauses();

        /// <summary>
        /// Create the list of <see cref="TypeParameter"/>s, or return the existing one.
        /// </summary>
        ChildList<TypeParameter> CreateTypeParameters();

        /// <summary>
        /// Get any constraints for the specified <see cref="TypeParameter"/>.
        /// </summary>
        List<TypeParameterConstraint> GetTypeParameterConstraints(TypeParameter typeParameter);
    }
}