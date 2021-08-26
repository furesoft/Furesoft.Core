// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Used for optional compilation of code, must be preceeded by an <see cref="IfDirective"/> or <see cref="ElIfDirective"/>, and
    /// followed by an <see cref="EndIfDirective"/>.
    /// </summary>
    public class ElseDirective : ConditionalDirective
    {
        /// <summary>
        /// Create an <see cref="ElseDirective"/>.
        /// </summary>
        public ElseDirective()
        { }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "else";

        /// <summary>
        /// Parse an <see cref="ElseDirective"/>.
        /// </summary>
        public ElseDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'else'
            MoveEOLComment(parser.LastToken);

            // Skip the next section of code if an earlier 'if' or 'elif' evaluated to true
            if (parser.CurrentConditionalDirectiveState)
                SkipSection(parser);
            else
                parser.CurrentConditionalDirectiveState = _isActive = true;
        }

        /// <summary>
        /// Parse an <see cref="ElseDirective"/>.
        /// </summary>
        public static ElseDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ElseDirective(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }
    }
}