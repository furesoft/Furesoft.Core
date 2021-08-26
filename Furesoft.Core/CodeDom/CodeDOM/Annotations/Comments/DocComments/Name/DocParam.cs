// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Documents a method or indexer parameter.
    /// </summary>
    public class DocParam : DocNameBase
    {
        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocParam(ParameterRef parameterRef, string text)
            : base(parameterRef, text)
        { }

        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocParam(ParameterRef parameterRef, params DocComment[] docComments)
            : base(parameterRef, docComments)
        { }

        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocParam(ParameterDecl parameterDecl, string text)
            : base(parameterDecl.CreateRef(), text)
        { }

        /// <summary>
        /// Create a <see cref="DocPara"/>.
        /// </summary>
        public DocParam(ParameterDecl parameterDecl, params DocComment[] docComments)
            : base(parameterDecl.CreateRef(), docComments)
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
        public new const string ParseToken = "param";

        /// <summary>
        /// Parse a <see cref="DocParam"/>.
        /// </summary>
        public DocParam(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="DocParam"/>.
        /// </summary>
        public static new DocParam Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocParam(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}