// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Allows for removing an existing definition of a "pre-processor" symbol - may only appear at the top of a <see cref="CodeUnit"/> (file)!
    /// </summary>
    public class UnDefSymbol : SymbolDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "undef";

        /// <summary>
        /// Create an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(string symbol)
            : base(symbol)
        { }

        /// <summary>
        /// Parse an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseSymbol(parser);
            parser.CodeUnit.UndefineCompilerDirectiveSymbol(_symbol);
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse an <see cref="UnDefSymbol"/>.
        /// </summary>
        public static UnDefSymbol Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new UnDefSymbol(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }
    }
}