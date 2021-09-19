using System.Reflection;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to one with a default constructor.
    /// </summary>
    public class NewConstraint : TypeParameterConstraint
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "new";

        /// <summary>
        /// Create a <see cref="NewConstraint"/>.
        /// </summary>
        public NewConstraint()
        { }

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

        /// <summary>
        /// The attribute of the constraint.
        /// </summary>
        public override GenericParameterAttributes ConstraintAttribute
        {
            get { return GenericParameterAttributes.DefaultConstructorConstraint; }
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