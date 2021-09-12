﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Pragmas.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using static Furesoft.Core.CodeDom.Parsing.Parser;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Pragmas.Base
{
    /// <summary>
    /// The common base class of all pragma directives (<see cref="PragmaChecksumDirective"/>, <see cref="PragmaWarningDirective"/>).
    /// </summary>
    public abstract class PragmaDirective : CompilerDirective
    {
        protected PragmaDirective()
        { }

        public abstract string PragmaType { get; }

        // Map of parse-point tokens to callbacks
        private static readonly Dictionary<string, ParseDelegate> _parsePoints = new();

        /// <summary>
        /// Add a parse-point for a pragma directive - triggers the callback when the specified token appears
        /// </summary>
        public static void AddPragmaParsePoint(string token, ParseDelegate callback)
        {
            _parsePoints.Add(token, callback);
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "pragma";

        protected PragmaDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Parse a <see cref="PragmaDirective"/>.
        /// </summary>
        public static PragmaDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Abort if there isn't a pragma sub-directive name on the same line
            Token next = parser.PeekNextToken();
            if (next == null || next.NewLines > 0)
                return null;

            // Execute the callback if we have a parse-point for the token
            ParseDelegate callback;
            if (_parsePoints.TryGetValue(next.Text, out callback))
                return (PragmaDirective)callback(parser, parent, flags);

            return null;
        }

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(PragmaType);
        }
    }
}