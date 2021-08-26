// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to a reference type.
    /// </summary>
    public class ClassConstraint : TypeParameterConstraint
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ClassConstraint"/>.
        /// </summary>
        public ClassConstraint()
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.ReferenceTypeConstraint; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "class";

        /// <summary>
        /// Parse a <see cref="ClassConstraint"/>.
        /// </summary>
        public ClassConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'class'
        }

        #endregion

        #region /* RENDERING */

        public override string ConstraintText
        {
            get { return ParseToken; }
        }

        #endregion
    }
}
