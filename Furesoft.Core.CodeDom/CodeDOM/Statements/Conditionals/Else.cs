// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a non-conditional alternative portion of an <see cref="If"/> or <see cref="ElseIf"/> (as a child of one of those object types).
    /// </summary>
    public class Else : BlockStatement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "else";

        /// <summary>
        /// Create an <see cref="Else"/>.
        /// </summary>
        public Else(CodeObject body)
            : base(body, false)
        { }

        /// <summary>
        /// Create an <see cref="Else"/>.
        /// </summary>
        public Else()
            : base(null, false)
        { }

        protected Else(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            MoveComments(parser.LastToken);              // Get any comments before 'else'
            parser.NextToken();                          // Move past 'else'
            new Block(out _body, parser, this, false);   // Parse the body
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
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
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

        public static void AddParsePoints()
        {
            // Normally, an 'else' is parsed by the 'if' parse logic (see IfBase).
            // This parse-point exists only to catch an orphaned 'else' statement.
            // Use a parse-priority of 200 (ElseIf uses 100)
            Parser.AddParsePoint(ParseToken, 200, ParseOrphan, typeof(IBlock));
        }

        /// <summary>
        /// Parse an <see cref="Else"/>.
        /// </summary>
        public static Else Parse(Parser parser, CodeObject parent)
        {
            return new Else(parser, parent);
        }

        /// <summary>
        /// Parse an orphaned <see cref="Else"/>.
        /// </summary>
        public static Else ParseOrphan(Parser parser, CodeObject parent, ParseFlags flags)
        {
            Token token = parser.Token;
            Else @else = Parse(parser, parent);
            parser.AttachMessage(@else, "Orphaned 'else' - missing parent 'if'", token);
            return @else;
        }
    }
}