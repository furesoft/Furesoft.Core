using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef
{
    /// <summary>
    /// Documents the access rights of a member.
    /// </summary>
    public class DocPermission : DocCodeRefBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Expression codeRef, string text)
            : base(codeRef, text)
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
        public new const string ParseToken = "permission";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public static new DocPermission Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocPermission(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
