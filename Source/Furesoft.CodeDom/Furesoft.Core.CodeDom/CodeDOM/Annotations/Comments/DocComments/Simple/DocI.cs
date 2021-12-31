using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Represents a section of italic text in a documentation comment.
    /// </summary>
    public class DocI : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "i";

        /// <summary>
        /// Create a <see cref="DocI"/>.
        /// </summary>
        public DocI(string content)
            : base(content)
        { }

        /// <summary>
        /// Parse a <see cref="DocI"/>.
        /// </summary>
        public DocI(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
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
        /// Parse a <see cref="DocI"/>.
        /// </summary>
        public static new DocI Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocI(parser, parent);
        }
    }
}
