// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Has a body that is ensured to always be executed whenever its parent <see cref="Try"/> block is exited for any reason.
    /// </summary>
    public class Finally : BlockStatement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "finally";

        /// <summary>
        /// Create a <see cref="Finally"/>.
        /// </summary>
        public Finally(CodeObject body)
            : base(body, false)
        { }

        /// <summary>
        /// Create a <see cref="Finally"/>.
        /// </summary>
        public Finally()
            : base(null, false)
        { }

        protected Finally(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            MoveComments(parser.LastToken);              // Get any comments before 'finally'
            parser.NextToken();                          // Move past 'finally'
            new Block(out _body, parser, this, true);    // Parse the body
            ParseUnusedAnnotations(parser, this, true);  // Parse any annotations from the Unused list

            // Remove any preceeding blank lines if auto-cleanup is on
            if (AutomaticFormattingCleanup && NewLines > 1)
                NewLines = 1;
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse a <see cref="Finally"/>.
        /// </summary>
        public static Finally Parse(Parser parser, CodeObject parent)
        {
            return new Finally(parser, parent);
        }

        /// <summary>
        /// Parse an orphaned <see cref="Finally"/>.
        /// </summary>
        public static Finally ParseOrphan(Parser parser, CodeObject parent, ParseFlags flags)
        {
            Token token = parser.Token;
            Finally @finally = Parse(parser, parent);
            parser.AttachMessage(@finally, "Orphaned 'finally' - missing parent 'try'", token);
            return @finally;
        }

        internal static void AddParsePoints()
        {
            // Normally, a 'finally' is parsed by the 'try' parse logic (see Try).
            // This parse-point exists only to catch an orphaned 'finally' statement.
            Parser.AddParsePoint(ParseToken, ParseOrphan, typeof(IBlock));
        }
    }
}