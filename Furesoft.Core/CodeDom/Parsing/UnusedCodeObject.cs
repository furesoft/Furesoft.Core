// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.CodeDOM;

namespace Nova.Parsing
{
    /// <summary>
    /// Wraps a <see cref="CodeObject"/> plus the last <see cref="Token"/> that it was parsed from (so that
    /// any trailing comments are included).  These objects are stored temporarily in the Unused list of the
    /// parser during the parsing process.
    /// </summary>
    public class UnusedCodeObject : ParsedObject
    {
        /// <summary>
        /// The <see cref="CodeObject"/>.
        /// </summary>
        public CodeObject CodeObject;

        /// <summary>
        /// The last <see cref="Token"/> the object was parsed from.
        /// </summary>
        public Token LastToken;

        /// <summary>
        /// Create an <see cref="UnusedCodeObject"/>.
        /// </summary>
        public UnusedCodeObject(CodeObject codeObject, Token lastToken)
        {
            CodeObject = codeObject;
            LastToken = lastToken;
        }

        /// <summary>
        /// True if there are any trailing comments.
        /// </summary>
        public override bool HasTrailingComments
        {
            get { return (LastToken.TrailingComments != null && LastToken.TrailingComments.Count > 0); }
        }

        /// <summary>
        /// True if inside a documentation comment.
        /// </summary>
        public override bool InDocComment
        {
            get { return LastToken.InDocComment; }
        }

        /// <summary>
        /// Get the last token used to parse the <see cref="CodeObject"/>.
        /// </summary>
        public override Token AsToken()
        {
            return LastToken;
        }
    }
}