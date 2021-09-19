// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Documents the access rights of a member.
    /// </summary>
    public class DocPermission : DocCodeRefBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "permission";

        /// <summary>
        /// Create a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Expression codeRef, string text)
            : base(codeRef, text)
        { }

        /// <summary>
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public DocPermission(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocPermission"/>.
        /// </summary>
        public static new DocPermission Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocPermission(parser, parent);
        }
    }
}