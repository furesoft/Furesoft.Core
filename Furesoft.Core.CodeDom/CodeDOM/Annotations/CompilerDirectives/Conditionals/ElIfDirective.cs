using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals
{
    /// <summary>
    /// Used for optional compilation of code, must be preceeded by an <see cref="IfDirective"/> or <see cref="ElIfDirective"/>, and
    /// followed by one of <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, or <see cref="EndIfDirective"/>.
    /// </summary>
    public class ElIfDirective : ConditionalExpressionDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "elif";

        /// <summary>
        /// Create an <see cref="ElIfDirective"/> with the specified <see cref="Expression"/>.
        /// </summary>
        public ElIfDirective(Expression expression)
            : base(expression)
        { }

        /// <summary>
        /// Parse an <see cref="ElIfDirective"/>.
        /// </summary>
        public ElIfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
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
    }
}
