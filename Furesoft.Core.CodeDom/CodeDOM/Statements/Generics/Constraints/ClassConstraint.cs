using System.Reflection;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
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
    }
}