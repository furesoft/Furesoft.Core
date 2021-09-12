using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List
{
    /// <summary>
    /// Represents the description of a <see cref="DocListItem"/> in a <see cref="DocList"/> in a documentation comment.
    /// </summary>
    public class DocListDescription : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocListDescription"/>.
        /// </summary>
        public DocListDescription(string content)
            : base(content)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "description";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocListDescription"/>.
        /// </summary>
        public static new DocListDescription Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocListDescription(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocListDescription"/>.
        /// </summary>
        public DocListDescription(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        #endregion
    }
}
