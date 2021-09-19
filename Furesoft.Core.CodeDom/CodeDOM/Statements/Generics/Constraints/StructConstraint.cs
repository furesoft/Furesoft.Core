using System.Reflection;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to a value type.
    /// </summary>
    public class StructConstraint : TypeParameterConstraint
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "struct";

        /// <summary>
        /// Create a <see cref="StructConstraint"/>.
        /// </summary>
        public StructConstraint()
        { }

        /// <summary>
        /// Parse a <see cref="StructConstraint"/>.
        /// </summary>
        public StructConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'struct'
        }

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.NotNullableValueTypeConstraint; }
        }

        public override string ConstraintText
        {
            get { return ParseToken; }
        }
    }
}