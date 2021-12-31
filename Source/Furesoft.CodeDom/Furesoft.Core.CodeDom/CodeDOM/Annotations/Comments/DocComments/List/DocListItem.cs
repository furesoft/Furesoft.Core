using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List
{
    /// <summary>
    /// Represents an item in a <see cref="DocList"/> in a documentation comment.
    /// </summary>
    public class DocListItem : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "item";

        /// <summary>
        /// Create a <see cref="DocListItem"/>.
        /// </summary>
        public DocListItem(params DocComment[] docComments)
        {
            foreach (DocComment docComment in docComments)
            {
                // Default-format entries
                Add("\n        ");
                Add(docComment);
            }

            // Default end tag to first-on-line
            Add("\n    ");
        }

        /// <summary>
        /// Parse a <see cref="DocListItem"/>.
        /// </summary>
        public DocListItem(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocListItem"/>.
        /// </summary>
        public static new DocListItem Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocListItem(parser, parent);
        }
    }
}
