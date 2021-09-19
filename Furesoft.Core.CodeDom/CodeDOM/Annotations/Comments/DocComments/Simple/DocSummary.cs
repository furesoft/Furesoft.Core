using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Provides a summary comment for a code object.
    /// </summary>
    public class DocSummary : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "summary";

        /// <summary>
        /// Create a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocSummary"/>.
        /// </summary>
        public static new DocSummary Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocSummary(parser, parent);
        }

        /// <summary>
        /// Returns the <see cref="DocSummary"/> itself.
        /// </summary>
        public override DocSummary GetDocSummary()
        {
            return this;
        }
    }
}
