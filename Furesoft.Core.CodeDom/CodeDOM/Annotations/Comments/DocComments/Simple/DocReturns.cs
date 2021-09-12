using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Documents the return value of a method.
    /// </summary>
    public class DocReturns : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(params DocComment[] docComments)
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
        public new const string ParseToken = "returns";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocReturns"/>.
        /// </summary>
        public static new DocReturns Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocReturns(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        #endregion
    }
}
