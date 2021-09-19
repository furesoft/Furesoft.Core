// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Marks the beginning of a section of conditionally compiled code.
    /// </summary>
    /// <remarks>
    /// It may be optionally followed by <see cref="ElIfDirective"/>s and/or <see cref="ElseDirective"/>, and must be
    /// eventually terminated with an <see cref="EndIfDirective"/>.
    /// </remarks>
    public class IfDirective : ConditionalExpressionDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "if";

        /// <summary>
        /// Create an <see cref="IfDirective"/> with the specified <see cref="Expression"/>.
        /// </summary>
        public IfDirective(Expression expression)
            : base(expression)
        { }

        /// <summary>
        /// Parse an <see cref="IfDirective"/>.
        /// </summary>
        public IfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="IfDirective"/>.
        /// </summary>
        public static IfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            parser.StartConditionalDirective();
            return new IfDirective(parser, parent);
        }
    }
}