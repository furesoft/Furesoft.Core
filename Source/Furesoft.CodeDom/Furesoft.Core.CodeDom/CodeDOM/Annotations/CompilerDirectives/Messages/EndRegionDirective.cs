using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Messages.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Messages
{
    /// <summary>
    /// Represents the end of a section of code that can be collapsed, and has optional descriptive text.
    /// </summary>
    public class EndRegionDirective : MessageDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "endregion";

        /// <summary>
        /// Create an <see cref="EndRegionDirective"/>.
        /// </summary>
        public EndRegionDirective(string message)
            : base(message)
        { }

        /// <summary>
        /// Create an <see cref="EndRegionDirective"/>.
        /// </summary>
        public EndRegionDirective()
            : base(null)
        { }

        /// <summary>
        /// Parse an <see cref="EndRegionDirective"/>.
        /// </summary>
        public EndRegionDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseMessage(parser);

            // Check if we just ended a region of generated code
            if (parser.IsGeneratedRegion)
                parser.IsGenerated = parser.IsGeneratedRegion = false;
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Determines if the compiler directive should be indented.
        /// </summary>
        public override bool HasNoIndentationDefault
        {
            get { return false; }
        }

        public static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="EndRegionDirective"/>.
        /// </summary>
        public static EndRegionDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new EndRegionDirective(parser, parent);
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before an end-region directive
            return 2;
        }
    }
}
