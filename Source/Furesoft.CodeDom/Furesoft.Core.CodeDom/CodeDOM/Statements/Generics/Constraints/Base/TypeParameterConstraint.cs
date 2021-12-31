using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints.Base
{
    /// <summary>
    /// The common base class of <see cref="ClassConstraint"/>, <see cref="StructConstraint"/>, <see cref="NewConstraint"/>, and <see cref="TypeConstraint"/>.
    /// </summary>
    public abstract class TypeParameterConstraint : CodeObject
    {
        /// <summary>
        /// The token used to parse between constraints.
        /// </summary>
        public const string ParseTokenSeparator = Expression.ParseTokenSeparator;

        protected TypeParameterConstraint()
        { }

        protected TypeParameterConstraint(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public virtual GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.None; }
        }

        public virtual string ConstraintText
        {
            get { return ""; }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        /// <summary>
        /// Create a collection of type parameter constraints for the specified type parameter.
        /// </summary>
        public static List<TypeParameterConstraint> Create(Type typeParameter)
        {
            List<TypeParameterConstraint> typeParameterConstraints = new();
            foreach (Type typeConstraint in typeParameter.GetGenericParameterConstraints())
                typeParameterConstraints.Add(new TypeConstraint(TypeRef.Create(typeConstraint)));
            GenericParameterAttributes attributes = typeParameter.GenericParameterAttributes;
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))  // class constraint
                typeParameterConstraints.Add(new ClassConstraint());
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))  // struct constraint
                typeParameterConstraints.Add(new StructConstraint());
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                typeParameterConstraints.Add(new NewConstraint());
            return typeParameterConstraints;
        }

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

        public override T Accept<T>(VisitorBase<T> visitor)
        {
            return visitor.Visit(this);
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