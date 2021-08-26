// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Embeds a reference to a type or member in a documentation comment.
    /// </summary>
    public class DocSee : DocCodeRefBase
    {
        /// <summary>
        /// Create a <see cref="DocSee"/>.
        /// </summary>
        public DocSee(Expression codeRef)
            : base(codeRef, (string)null)
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
        public new const string ParseToken = "see";

        /// <summary>
        /// Parse a <see cref="DocSee"/>.
        /// </summary>
        public DocSee(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="DocSee"/>.
        /// </summary>
        public static new DocSee Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocSee(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }
    }
}