// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="RegionDirective"/>, <see cref="EndRegionDirective"/>, <see cref="ErrorDirective"/>,
    /// and <see cref="WarningDirective"/>.
    /// </summary>
    public abstract class MessageDirective : CompilerDirective
    {
        #region /* FIELDS */

        protected string _message;

        #endregion

        #region /* CONSTRUCTORS */

        protected MessageDirective(string message)
        {
            _message = message;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The text content of the message.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        #endregion

        #region /* PARSING */

        protected MessageDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseMessage(Parser parser)
        {
            Token token = parser.NextTokenSameLine(true);  // Move past directive keyword

            // Parse the message as the current token to EOL
            if (token != null)
                _message = parser.GetTokenToEOL();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return !string.IsNullOrEmpty(_message); }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_message);
        }

        #endregion
    }
}
