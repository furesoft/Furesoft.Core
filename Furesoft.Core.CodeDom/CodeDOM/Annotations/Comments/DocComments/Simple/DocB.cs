using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Represents a section of bold text in a documentation comment.
    /// </summary>
    public class DocB : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "b";

        /// <summary>
        /// Create a <see cref="DocB"/>.
        /// </summary>
        public DocB(string content)
            : base(content)
        { }

        /// <summary>
        /// Parse a <see cref="DocB"/>.
        /// </summary>
        public DocB(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocB"/>.
        /// </summary>
        public static new DocB Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocB(parser, parent);
        }
    }
}
