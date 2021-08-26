// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to a value type.
    /// </summary>
    public class StructConstraint : TypeParameterConstraint
    {
        /// <summary>
        /// Create a <see cref="StructConstraint"/>.
        /// </summary>
        public StructConstraint()
        { }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.NotNullableValueTypeConstraint; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "struct";

        /// <summary>
        /// Parse a <see cref="StructConstraint"/>.
        /// </summary>
        public StructConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'struct'
        }

        public override string ConstraintText
        {
            get { return ParseToken; }
        }
    }
}