// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
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
