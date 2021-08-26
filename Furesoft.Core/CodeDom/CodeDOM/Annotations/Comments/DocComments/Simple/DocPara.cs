// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents a separate paragraph in a documentation comment.
    /// </summary>
    public class DocPara : DocComment
    {
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

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "para";

        /// <summary>
        /// Parse a <see cref="DocPara"/>.
        /// </summary>
        public DocPara(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// Parse a <see cref="DocPara"/>.
        /// </summary>
        public static new DocPara Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocPara(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}