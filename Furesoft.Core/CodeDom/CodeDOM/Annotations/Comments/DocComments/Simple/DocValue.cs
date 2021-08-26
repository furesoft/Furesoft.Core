// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Documents the value of a property.
    /// </summary>
    public class DocValue : DocComment
    {
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
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = ValueParameterDecl.FixedName;

        /// <summary>
        /// Parse a <see cref="DocValue"/>.
        /// </summary>
        public DocValue(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// Parse a <see cref="DocValue"/>.
        /// </summary>
        public static new DocValue Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocValue(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}