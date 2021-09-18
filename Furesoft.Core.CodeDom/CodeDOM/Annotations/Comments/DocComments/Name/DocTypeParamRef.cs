// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Embeds a reference to a type parameter in a documentation comment.
    /// </summary>
    public class DocTypeParamRef : DocNameBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "typeparamref";

        /// <summary>
        /// Create a <see cref="DocTypeParamRef"/>.
        /// </summary>
        public DocTypeParamRef(TypeParameterRef typeParameterRef)
            : base(typeParameterRef, (string)null)
        { }

        /// <summary>
        /// Create a <see cref="DocTypeParamRef"/>.
        /// </summary>
        public DocTypeParamRef(TypeParameter typeParameter)
            : base(typeParameter.CreateRef(), (string)null)
        { }

        /// <summary>
        /// Parse a <see cref="DocTypeParamRef"/>.
        /// </summary>
        public DocTypeParamRef(Parser parser, CodeObject parent)
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

        /// <summary>
        /// Parse a <see cref="DocTypeParamRef"/>.
        /// </summary>
        public static new DocTypeParamRef Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocTypeParamRef(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}