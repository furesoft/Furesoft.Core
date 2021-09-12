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
        #region /* CONSTRUCTORS */

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
        public const string ParseToken = "else";

        internal static void AddParsePoints()
        {
            // Normally, an 'else' is parsed by the 'if' parse logic (see IfBase).
            // This parse-point exists only to catch an orphaned 'else' statement.
            // Use a parse-priority of 200 (ElseIf uses 100)
            Parser.AddParsePoint(ParseToken, 200, ParseOrphan, typeof(IBlock));
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

        /// <summary>
        /// Parse an <see cref="Else"/>.
        /// </summary>
        public static Else Parse(Parser parser, CodeObject parent)
        {
            return new Else(parser, parent);
        }

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

        #endregion

        #region /* RESOLVING */

        public override void ResolveRefUp(string name, Resolving.Resolver resolver)
        {
            if (_body != null)
            {
                _body.ResolveRef(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            // Skip past any If/ElseIf parent (we don't want to match anything in its body)
            if (_parent is IfBase && _parent.Parent != null)
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

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        #endregion
    }
}
