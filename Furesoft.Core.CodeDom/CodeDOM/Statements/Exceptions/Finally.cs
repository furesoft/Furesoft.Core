// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Has a body that is ensured to always be executed whenever its parent <see cref="Try"/> block is exited for any reason.
    /// </summary>
    public class Finally : BlockStatement
    {
        #region /* CONSTRUCTORS */

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

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "finally";

        internal static void AddParsePoints()
        {
            // Normally, a 'finally' is parsed by the 'try' parse logic (see Try).
            // This parse-point exists only to catch an orphaned 'finally' statement.
            Parser.AddParsePoint(ParseToken, ParseOrphan, typeof(IBlock));
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

        /// <summary>
        /// Parse a <see cref="Finally"/>.
        /// </summary>
        public static Finally Parse(Parser parser, CodeObject parent)
        {
            return new Finally(parser, parent);
        }

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

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            if (_body != null)
            {
                _body.ResolveRef(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            // Skip past any Try parent (we don't want to match anything in its body)
            if (_parent is Try && _parent.Parent != null)
                _parent.Parent.ResolveRefUp(name, resolver);
            else if (_parent != null)
                _parent.ResolveRefUp(name, resolver);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        #endregion
    }
}
