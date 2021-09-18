// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all comment classes (<see cref="Comment"/> and <see cref="DocComment"/>).
    /// </summary>
    public abstract class CommentBase : Annotation
    {
        protected byte _prefixSpaceCount;

        protected CommentBase()
        { }

        protected CommentBase(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The count of prefix spaces (if any) before the comment delimiter.
        /// </summary>
        public int PrefixSpaceCount
        {
            get { return _prefixSpaceCount; }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a stand-alone comment
            return 2;
        }
    }
}