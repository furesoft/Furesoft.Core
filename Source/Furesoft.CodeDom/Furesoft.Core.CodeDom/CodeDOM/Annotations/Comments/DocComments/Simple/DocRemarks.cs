using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Provides additional comments for a code object.
    /// </summary>
    public class DocRemarks : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "remarks";

        /// <summary>
        /// Create a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocRemarks"/>.
        /// </summary>
        public static new DocRemarks Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocRemarks(parser, parent);
        }
    }
}
