// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a special type of method used to write the value of a property.
    /// </summary>
    /// <remarks>
    /// The return type of a SetterDecl is always 'void', and it acquires the type of its 'value'
    /// parameter and its internal name from it's parent <see cref="PropertyDecl"/> or <see cref="IndexerDecl"/>.
    /// </remarks>
    public class SetterDecl : AccessorDeclWithValue
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name prefix.
        /// </summary>
        public const string NamePrefix = ParseToken + "_";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="SetterDecl"/>.
        /// </summary>
        public SetterDecl(Modifiers modifiers, CodeObject body)
            : base(NamePrefix, modifiers, body)
        { }

        /// <summary>
        /// Create a <see cref="SetterDecl"/>.
        /// </summary>
        public SetterDecl(Modifiers modifiers)
            : base(NamePrefix, modifiers)
        { }

        /// <summary>
        /// Create a <see cref="SetterDecl"/>.
        /// </summary>
        public SetterDecl()
            : base(NamePrefix, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="SetterDecl"/>.
        /// </summary>
        public SetterDecl(CodeObject body)
            : base(NamePrefix, body)
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
        public const string ParseToken = "set";

        internal static new void AddParsePoints()
        {
            // Parse 'set' for Properties, Indexers, and Events
            // (analysis will complain in the last case that it should be 'remove' instead)
            Parser.AddParsePoint(ParseToken, Parse, typeof(PropertyDeclBase));
        }

        /// <summary>
        /// Parse a <see cref="SetterDecl"/>.
        /// </summary>
        public static new SetterDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new SetterDecl(parser, parent, flags);
        }

        protected SetterDecl(Parser parser, CodeObject parent, ParseFlags flags)
            : base(parser, parent, NamePrefix, flags)
        {
            ParseAccessor(parser, flags);
        }

        #endregion
    }
}
