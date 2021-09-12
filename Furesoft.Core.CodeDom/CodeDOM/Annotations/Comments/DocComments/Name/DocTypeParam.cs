using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Name.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Name;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Name
{
    /// <summary>
    /// Documents a type parameter.
    /// </summary>
    public class DocTypeParam : DocNameBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocTypeParam"/>.
        /// </summary>
        public DocTypeParam(TypeParameterRef typeParameterRef, string text)
            : base(typeParameterRef, text)
        { }

        /// <summary>
        /// Create a <see cref="DocTypeParam"/>.
        /// </summary>
        public DocTypeParam(TypeParameterRef typeParameterRef, params DocComment[] docComments)
            : base(typeParameterRef, docComments)
        { }

        /// <summary>
        /// Create a <see cref="DocTypeParam"/>.
        /// </summary>
        public DocTypeParam(TypeParameter typeParameter, string text)
            : base(typeParameter.CreateRef(), text)
        { }

        /// <summary>
        /// Create a <see cref="DocTypeParam"/>.
        /// </summary>
        public DocTypeParam(TypeParameter typeParameter, params DocComment[] docComments)
            : base(typeParameter.CreateRef(), docComments)
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
        public new const string ParseToken = "typeparam";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocTypeParam"/>.
        /// </summary>
        public static new DocTypeParam Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocTypeParam(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocTypeParam"/>.
        /// </summary>
        public DocTypeParam(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected override ResolveCategory AttributeCategory
        {
            get { return ResolveCategory.LocalTypeParameter; }
        }

        #endregion
    }
}
