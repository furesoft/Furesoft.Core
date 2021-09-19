// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Documents a reference to a type or member that should be presented in a "See Also" section.
    /// </summary>
    public class DocSeeAlso : DocCodeRefBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "seealso";

        /// <summary>
        /// Create a <see cref="DocSeeAlso"/>.
        /// </summary>
        public DocSeeAlso(Expression codeRef)
            : base(codeRef, (string)null)
        { }

        /// <summary>
        /// Parse a <see cref="DocSeeAlso"/>.
        /// </summary>
        public DocSeeAlso(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="DocSeeAlso"/>.
        /// </summary>
        public static new DocSeeAlso Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocSeeAlso(parser, parent);
        }
    }
}