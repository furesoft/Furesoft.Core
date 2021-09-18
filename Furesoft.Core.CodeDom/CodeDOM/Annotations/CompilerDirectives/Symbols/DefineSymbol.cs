// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Provides for the definition of a "pre-processor" symbol - may only appear at the very top of a <see cref="CodeUnit"/> (file)!
    /// </summary>
    public class DefineSymbol : SymbolDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "define";

        /// <summary>
        /// Create a <see cref="DefineSymbol"/>.
        /// </summary>
        public DefineSymbol(string symbol)
            : base(symbol)
        { }

        /// <summary>
        /// Parse a <see cref="DefineSymbol"/>.
        /// </summary>
        public DefineSymbol(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseSymbol(parser);
            parser.CodeUnit.DefineCompilerDirectiveSymbol(_symbol);
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse a <see cref="DefineSymbol"/>.
        /// </summary>
        public static DefineSymbol Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DefineSymbol(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }
    }
}