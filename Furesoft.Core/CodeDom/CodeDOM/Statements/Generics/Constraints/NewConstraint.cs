// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to one with a default constructor.
    /// </summary>
    public class NewConstraint : TypeParameterConstraint
    {
        /// <summary>
        /// Create a <see cref="NewConstraint"/>.
        /// </summary>
        public NewConstraint()
        { }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.DefaultConstructorConstraint; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "new";

        /// <summary>
        /// Parse a <see cref="NewConstraint"/>.
        /// </summary>
        public NewConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'new'
            ParseExpectedToken(parser, Expression.ParseTokenStartGroup);  // Move past '('
            ParseExpectedToken(parser, Expression.ParseTokenEndGroup);  // Move past ')'
        }

        public override string ConstraintText
        {
            get { return ParseToken; }
        }

        protected override void AsTextConstraint(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextConstraint(writer, flags);
            writer.Write(Expression.ParseTokenStartGroup + Expression.ParseTokenEndGroup);
        }
    }
}