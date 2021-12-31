using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Name.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Name
{
    /// <summary>
    /// Embeds a reference to a method or indexer parameter in a documentation comment.
    /// </summary>
    public class DocParamRef : DocNameBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "paramref";

        /// <summary>
        /// Create a <see cref="DocParamRef"/>.
        /// </summary>
        public DocParamRef(ParameterRef parameterRef)
            : base(parameterRef, (string)null)
        { }

        /// <summary>
        /// Create a <see cref="DocParamRef"/>.
        /// </summary>
        public DocParamRef(ParameterDecl parameterDecl)
            : base(parameterDecl.CreateRef(), (string)null)
        { }

        /// <summary>
        /// Parse a <see cref="DocParamRef"/>.
        /// </summary>
        public DocParamRef(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

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
        /// Parse a <see cref="DocParamRef"/>.
        /// </summary>
        public static new DocParamRef Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocParamRef(parser, parent);
        }
    }
}
