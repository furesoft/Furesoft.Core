// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="DefineSymbol"/> and <see cref="UnDefSymbol"/>.
    /// </summary>
    public abstract class SymbolDirective : CompilerDirective
    {
        protected string _symbol;

        protected SymbolDirective(string symbol)
        {
            _symbol = symbol;
        }

        protected SymbolDirective(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The associated symbol name.
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_symbol);
        }

        protected void ParseSymbol(Parser parser)
        {
            Token token = parser.NextTokenSameLine(false);  // Move past directive keyword
            if (token != null)
            {
                _symbol = parser.TokenText;  // Parse symbol name
                parser.NextToken();          // Move past name
            }
        }
    }
}