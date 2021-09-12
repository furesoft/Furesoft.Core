using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals
{
    /// <summary>
    /// Marks the beginning of a section of conditionally compiled code.
    /// </summary>
    /// <remarks>
    /// It may be optionally followed by <see cref="ElIfDirective"/>s and/or <see cref="ElseDirective"/>, and must be
    /// eventually terminated with an <see cref="EndIfDirective"/>.
    /// </remarks>
    public class IfDirective : ConditionalExpressionDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="IfDirective"/> with the specified <see cref="Expression"/>.
        /// </summary>
        public IfDirective(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "if";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="IfDirective"/>.
        /// </summary>
        public static IfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            parser.StartConditionalDirective();
            return new IfDirective(parser, parent);
        }

        /// <summary>
        /// Parse an <see cref="IfDirective"/>.
        /// </summary>
        public IfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        #endregion
    }
}
