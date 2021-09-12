using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals
{
    /// <summary>
    /// Marks the end of an <see cref="IfDirective"/>.
    /// </summary>
    public class EndIfDirective : ConditionalDirectiveBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="EndIfDirective"/>.
        /// </summary>
        public EndIfDirective()
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "endif";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="EndIfDirective"/>.
        /// </summary>
        public static EndIfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new EndIfDirective(parser, parent);
        }
        
        /// <summary>
        /// Parse an <see cref="EndIfDirective"/>.
        /// </summary>
        public EndIfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'endif'
            MoveEOLComment(parser.LastToken);
            parser.EndConditionalDirective();
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

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
