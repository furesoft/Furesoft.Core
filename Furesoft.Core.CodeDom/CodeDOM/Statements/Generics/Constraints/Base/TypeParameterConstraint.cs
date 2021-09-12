// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using Mono.Cecil;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="ClassConstraint"/>, <see cref="StructConstraint"/>, <see cref="NewConstraint"/>, and <see cref="TypeConstraint"/>.
    /// </summary>
    public abstract class TypeParameterConstraint : CodeObject
    {
        protected TypeParameterConstraint()
        { }

        /// <summary>
        /// Create a collection of type parameter constraints for the specified type parameter.
        /// </summary>
        public static List<TypeParameterConstraint> Create(GenericParameter typeParameter)
        {
            List<TypeParameterConstraint> typeParameterConstraints = new List<TypeParameterConstraint>();
            foreach (var typeConstraint in typeParameter.Constraints)
                typeParameterConstraints.Add(new TypeConstraint(TypeRef.Create(typeConstraint.ConstraintType)));
            GenericParameterAttributes attributes = typeParameter.Attributes;
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                typeParameterConstraints.Add(new ClassConstraint());
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                typeParameterConstraints.Add(new StructConstraint());
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                typeParameterConstraints.Add(new NewConstraint());
            return typeParameterConstraints;
        }

        /// <summary>
        /// Create a collection of type parameter constraints for the specified type parameter.
        /// </summary>
        public static List<TypeParameterConstraint> Create(Type typeParameter)
        {
            List<TypeParameterConstraint> typeParameterConstraints = new List<TypeParameterConstraint>();
            foreach (Type typeConstraint in typeParameter.GetGenericParameterConstraints())
                typeParameterConstraints.Add(new TypeConstraint(TypeRef.Create(typeConstraint)));
            System.Reflection.GenericParameterAttributes attributes = typeParameter.GenericParameterAttributes;
            if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                typeParameterConstraints.Add(new ClassConstraint());
            if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                typeParameterConstraints.Add(new StructConstraint());
            if (attributes.HasFlag(System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint))
                typeParameterConstraints.Add(new NewConstraint());
            return typeParameterConstraints;
        }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public virtual System.Reflection.GenericParameterAttributes ConstraintAttribute
        {
            get { return System.Reflection.GenericParameterAttributes.None; }
        }

        /// <summary>
        /// The token used to parse between constraints.
        /// </summary>
        public const string ParseTokenSeparator = Expression.ParseTokenSeparator;

        protected TypeParameterConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="TypeParameterConstraint"/>.
        /// </summary>
        public static TypeParameterConstraint Parse(Parser parser, CodeObject parent)
        {
            TypeParameterConstraint constraint;
            switch (parser.TokenText)
            {
                case ClassConstraint.ParseToken:
                    constraint = new ClassConstraint(parser, parent);
                    break;

                case StructConstraint.ParseToken:
                    constraint = new StructConstraint(parser, parent);
                    break;

                case NewConstraint.ParseToken:
                    constraint = new NewConstraint(parser, parent);
                    break;

                default:
                    constraint = new TypeConstraint(parser, parent);
                    break;
            }
            return constraint;
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        public virtual string ConstraintText
        {
            get { return ""; }
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            AsTextConstraint(writer, passFlags);
            AsTextEOLComments(writer, flags);
            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));
        }

        protected virtual void AsTextConstraint(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ConstraintText);
        }
    }
}