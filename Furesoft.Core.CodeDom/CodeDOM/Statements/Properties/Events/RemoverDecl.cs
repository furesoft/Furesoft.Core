// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a special type of method used to remove from an event.
    /// </summary>
    /// <remarks>
    /// The return type of a RemoverDecl is always 'void', and it acquires the type of its 'value'
    /// parameter and its internal name from it's parent EventDecl.
    /// </remarks>
    public class RemoverDecl : AccessorDeclWithValue
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name prefix.
        /// </summary>
        public const string NamePrefix = ParseToken + "__";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="RemoverDecl"/>.
        /// </summary>
        public RemoverDecl(Modifiers modifiers)
            : base(NamePrefix, modifiers)
        { }

        /// <summary>
        /// Create a <see cref="RemoverDecl"/>.
        /// </summary>
        public RemoverDecl()
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
        public const string ParseToken = "remove";

        internal static new void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(EventDecl));
        }

        /// <summary>
        /// Parse a <see cref="RemoverDecl"/>.
        /// </summary>
        public static new RemoverDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new RemoverDecl(parser, parent, flags);
        }

        protected RemoverDecl(Parser parser, CodeObject parent, ParseFlags flags)
            : base(parser, parent, NamePrefix, flags)
        {
            ParseAccessor(parser, flags);
        }

        #endregion
    }
}
