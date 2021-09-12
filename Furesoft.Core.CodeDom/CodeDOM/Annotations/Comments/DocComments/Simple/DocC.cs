// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a fragment of code in a documentation comment.
    /// </summary>
    public class DocC : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocC"/>.
        /// </summary>
        public DocC(Expression content)
            : base(content)
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
        public new const string ParseToken = "c";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocC"/>.
        /// </summary>
        public static new DocC Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocC(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocC"/>.
        /// </summary>
        public DocC(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        protected override bool ParseContent(Parser parser)
        {
            // Parse the content as code unless disabled or the content is empty
            if (DocCodeRefBase.ParseRefsAsCode && parser.Char != ParseTokenTagOpen[0])
            {
                // Override parsing of the content to parse as an expression
                Expression expression = parser.ParseCodeExpressionUntil(ParseTokenTagOpen + ParseTokenEndTag + ParseToken + ParseTokenTagClose, this);
                if (expression != null)
                    SetField(ref _content, expression, false);
                else
                    _content = "";

                // Look for expected end tag
                return ParseEndTag(parser);
            }
            return base.ParseContent(parser);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if ((flags & (ResolveFlags.Phase1 | ResolveFlags.Phase2)) == 0)
            {
                // Resolve the embedded code with no phases - this should work for almost all sample code.  We could do it in 3 phases
                // to handle 100% of cases, but this would result in multiple resolves of expression-level references that fail (such
                // as non-code put inside the code comment).
                flags &= ~(ResolveFlags.Phase1 | ResolveFlags.Phase2 | ResolveFlags.Phase3);
                flags |= ResolveFlags.InDocComment;
                if (_content is ChildList<DocComment>)
                    ChildListHelpers.Resolve((ChildList<DocComment>)_content, ResolveCategory.CodeObject, flags);
                else if (_content is CodeObject)
                    _content = ((CodeObject)_content).Resolve(_content is Expression ? ResolveCategory.Expression : ResolveCategory.CodeObject, flags);
            }
            return this;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        #endregion
    }
}
