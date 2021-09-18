// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a conditional alternative portion of an <see cref="If"/> or <see cref="ElseIf"/> (as a child of one of those object types).
    /// </summary>
    public class ElseIf : IfBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional, CodeObject body)
            : base(conditional, body)
        { }

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional, CodeObject body, Else @else)
            : base(conditional, body, @else)
        { }

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional, CodeObject body, ElseIf elseIf)
            : base(conditional, body, elseIf)
        { }

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional)
            : base(conditional)
        { }

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional, Else @else)
            : base(conditional, @else)
        { }

        /// <summary>
        /// Create an <see cref="ElseIf"/>.
        /// </summary>
        public ElseIf(Expression conditional, ElseIf elseIf)
            : base(conditional, elseIf)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken1 + " " + ParseToken2; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The first token used to parse the code object.
        /// </summary>
        public const string ParseToken1 = "else";

        /// <summary>
        /// The second token used to parse the code object.
        /// </summary>
        public const string ParseToken2 = "if";

        internal static void AddParsePoints()
        {
            // Normally, an 'else if' is parsed by the 'if' parse logic (see IfBase).
            // This parse-point exists only to catch an orphaned 'else if' statement.
            // Use a parse-priority of 100 (Else uses 200)
            Parser.AddParsePoint(ParseToken1, 100, ParseOrphan, typeof(IBlock));
        }

        /// <summary>
        /// Parse an orphaned <see cref="ElseIf"/>.
        /// </summary>
        public static ElseIf ParseOrphan(Parser parser, CodeObject parent, ParseFlags flags)
        {
            Token token = parser.Token;
            ElseIf elseIf = Parse(parser, parent);
            if (elseIf != null)
                parser.AttachMessage(elseIf, "Orphaned 'else if' - missing parent 'if'", token);
            return elseIf;
        }

        /// <summary>
        /// Parse an <see cref="ElseIf"/>.
        /// </summary>
        public static ElseIf Parse(Parser parser, CodeObject parent)
        {
            if (parser.PeekNextTokenText() == ParseToken2)
                return new ElseIf(parser, parent);
            return null;
        }

        protected ElseIf(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);                  // Get any comments before 'else'
            parser.NextToken();                              // Move past 'else'
            MoveAllComments(parser.LastToken, false, true);  // Get any comments after 'else' as regular comments
            ParseUnusedAnnotations(parser, this, true);      // Parse any annotations from the Unused list
            ParseIf(parser, parent);                         // Delegate to base class to parse 'if'

            // Remove any preceeding blank lines if auto-cleanup is on
            if (AutomaticFormattingCleanup && NewLines > 1)
                NewLines = 1;
        }

        #endregion

        #region /* RENDERING */

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            base.AsText(writer, flags | RenderFlags.IncreaseIndent);
        }

        #endregion
    }
}
