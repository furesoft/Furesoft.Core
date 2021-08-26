// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Used as a child of a <see cref="Switch"/>.  Includes a body (a statement or block).
    /// </summary>
    public class Default : SwitchItem
    {
        /// <summary>
        /// Create a <see cref="Default"/>.
        /// </summary>
        public Default(CodeObject body)
            : base(body)
        { }

        /// <summary>
        /// Create a <see cref="Default"/>.
        /// </summary>
        public Default()
            : base(null)
        { }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Always 'default'.
        /// </summary>
        public override string Name
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "default";

        protected Default(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();              // Move past 'default'
            ParseTerminatorAndBody(parser);  // Parse ':' and body (if any)
        }

        /// <summary>
        /// Parse a <see cref="Default"/>.
        /// </summary>
        public static Default Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Skip if not followed by a ':', so DefaultValue can give it a try
            if (parser.PeekNextTokenText() == ParseTokenTerminator)
                return new Default(parser, parent);
            return null;
        }

        internal static void AddParsePoints()
        {
            // Use a parse-priority of 0 (DefaultValue uses 100)
            Parser.AddParsePoint(ParseToken, Parse, typeof(Switch));
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