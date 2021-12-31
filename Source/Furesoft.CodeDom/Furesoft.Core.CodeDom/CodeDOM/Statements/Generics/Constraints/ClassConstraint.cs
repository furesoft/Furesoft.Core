using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints.Base;
using Furesoft.Core.CodeDom.Parsing;
using System.Reflection;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to a reference type.
    /// </summary>
    public class ClassConstraint : TypeParameterConstraint
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "class";

        /// <summary>
        /// Create a <see cref="ClassConstraint"/>.
        /// </summary>
        public ClassConstraint()
        { }

        /// <summary>
        /// Parse a <see cref="ClassConstraint"/>.
        /// </summary>
        public ClassConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'class'
        }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.ReferenceTypeConstraint; }
        }

        public override string ConstraintText
        {
            get { return ParseToken; }
        }

        public override T Accept<T>(VisitorBase<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}