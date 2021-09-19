using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Documents the value of a property.
    /// </summary>
    public class DocValue : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = ValueParameterDecl.FixedName;

        /// <summary>
        /// Create a <see cref="DocValue"/>.
        /// </summary>
        public DocValue(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocValue"/>.
        /// </summary>
        public DocValue(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocValue"/>.
        /// </summary>
        public DocValue(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocValue"/>.
        /// </summary>
        public static new DocValue Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocValue(parser, parent);
        }
    }
}
