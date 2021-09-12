using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Represents a separate paragraph in a documentation comment.
    /// </summary>
    public class DocPara : DocComment
    {
        #region /* CONSTRUCTORS */

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
        public new const string ParseToken = "para";

        internal static void AddParsePoints()
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

        /// <summary>
        /// Parse a <see cref="DocPara"/>.
        /// </summary>
        public DocPara(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        #endregion
    }
}
