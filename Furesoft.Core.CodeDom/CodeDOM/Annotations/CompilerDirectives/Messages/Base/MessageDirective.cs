﻿// The Nova Project by Ken Beckett.
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
        protected string _message;

        protected MessageDirective(string message)
        {
            _message = message;
        }

        protected MessageDirective(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return !string.IsNullOrEmpty(_message); }
        }

        /// <summary>
        /// The text content of the message.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_message);
        }

        protected void ParseMessage(Parser parser)
        {
            Token token = parser.NextTokenSameLine(true);  // Move past directive keyword

            // Parse the message as the current token to EOL
            if (token != null)
                _message = parser.GetTokenToEOL();
        }
    }
}