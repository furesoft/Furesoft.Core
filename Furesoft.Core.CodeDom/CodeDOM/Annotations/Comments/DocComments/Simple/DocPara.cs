using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a separate paragraph in a documentation comment.
    /// </summary>
    public class DocPara : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "para";

        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocPara(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocPara(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocPara"/>.
        /// </summary>
        public DocPara(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocPara"/>.
        /// </summary>
        public static new DocPara Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocPara(parser, parent);
        }
    }
}