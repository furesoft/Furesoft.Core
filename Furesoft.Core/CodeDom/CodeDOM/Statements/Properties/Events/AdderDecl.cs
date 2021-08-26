// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a special type of method used to add to an event.
    /// </summary>
    /// <remarks>
    /// The return type of an AdderDecl is always 'void', and it acquires the type of its 'value'
    /// parameter and its internal name from it's parent EventDecl.
    /// </remarks>
    public class AdderDecl : AccessorDeclWithValue
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name prefix.
        /// </summary>
        public const string NamePrefix = ParseToken + "__";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="AdderDecl"/>.
        /// </summary>
        public AdderDecl(Modifiers modifiers)
            : base(NamePrefix, modifiers)
        { }

        /// <summary>
        /// Create an <see cref="AdderDecl"/>.
        /// </summary>
        public AdderDecl()
            : base(NamePrefix, Modifiers.None)
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
        public const string ParseToken = "add";

        internal static new void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(EventDecl));
        }

        /// <summary>
        /// Parse an <see cref="AdderDecl"/>.
        /// </summary>
        public static new AdderDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AdderDecl(parser, parent, flags);
        }

        protected AdderDecl(Parser parser, CodeObject parent, ParseFlags flags)
            : base(parser, parent, NamePrefix, flags)
        {
            ParseAccessor(parser, flags);
        }

        #endregion
    }
}
