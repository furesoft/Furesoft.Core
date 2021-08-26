// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Signals the end of an iterator.
    /// </summary>
    /// <remarks>
    /// Yield statements must only appear in method, operator, or accessor bodies.
    /// They must not appear in anonymous functions or a 'finally' clause.
    /// 'yield' isn't a reserved word - it has special meaning only before a 'return' or 'break'.
    /// </remarks>
    public class YieldBreak : YieldStatement
    {
        /// <summary>
        /// Create a <see cref="YieldBreak"/>.
        /// </summary>
        public YieldBreak()
        { }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken1 + " " + ParseToken2; }
        }

        /// <summary>
        /// The second token used to parse the code object.
        /// </summary>
        public const string ParseToken2 = "break";

        /// <summary>
        /// Parse a <see cref="YieldBreak"/>.
        /// </summary>
        public YieldBreak(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'yield'
            parser.NextToken();  // Move past 'break'
            ParseTerminator(parser);
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }
    }
}