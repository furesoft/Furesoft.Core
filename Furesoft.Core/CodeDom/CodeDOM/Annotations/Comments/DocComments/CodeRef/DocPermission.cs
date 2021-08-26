// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Documents the access rights of a member.
    /// </summary>
    public class DocPermission : DocCodeRefBase
    {
        /// <summary>
        /// Create a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Expression codeRef, string text)
            : base(codeRef, text)
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
        public new const string ParseToken = "permission";

        /// <summary>
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public static new DocPermission Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocPermission(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}