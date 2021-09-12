using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals
{
    /// <summary>
    /// Used for optional compilation of code, must be preceeded by an <see cref="IfDirective"/> or <see cref="ElIfDirective"/>, and
    /// followed by one of <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, or <see cref="EndIfDirective"/>.
    /// </summary>
    public class ElIfDirective : ConditionalExpressionDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ElIfDirective"/> with the specified <see cref="Expression"/>.
        /// </summary>
        public ElIfDirective(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "elif";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="ElIfDirective"/>.
        /// </summary>
        public static ElIfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ElIfDirective(parser, parent);
        }

        /// <summary>
        /// Parse an <see cref="ElIfDirective"/>.
        /// </summary>
        public ElIfDirective(Parser parser, CodeObject parent)
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
