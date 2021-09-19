using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef
{
    /// <summary>
    /// Documents a reference to a type or member that should be presented in a "See Also" section.
    /// </summary>
    public class DocSeeAlso : DocCodeRefBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "seealso";

        /// <summary>
        /// Create a <see cref="DocSeeAlso"/>.
        /// </summary>
        public DocSeeAlso(Expression codeRef)
            : base(codeRef, (string)null)
        { }

        /// <summary>
        /// Parse a <see cref="DocSeeAlso"/>.
        /// </summary>
        public DocSeeAlso(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

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
        /// Parse a <see cref="DocSeeAlso"/>.
        /// </summary>
        public static new DocSeeAlso Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocSeeAlso(parser, parent);
        }
    }
}
