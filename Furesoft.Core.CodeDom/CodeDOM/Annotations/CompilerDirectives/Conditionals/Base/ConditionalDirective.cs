// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/>, <see cref="ElIfDirective"/>, and <see cref="ElseDirective"/>.
    /// </summary>
    public abstract class ConditionalDirective : ConditionalDirectiveBase
    {
        protected bool _isActive;
        protected string _skippedText;

        protected ConditionalDirective()
        { }

        protected ConditionalDirective(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// True if this part of a chain of conditional directives is the active one.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
        }

        /// <summary>
        /// The skipped text section if the <see cref="ConditionalDirective"/> expression evaluates to <c>false</c>.
        /// </summary>
        public string SkippedText
        {
            get { return _skippedText; }
            set
            {
                _skippedText = value.Replace("\r\n", "\n");  // Normalize newlines
            }
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            base.AsText(writer, flags);

            if (!string.IsNullOrEmpty(_skippedText) && !flags.HasFlag(RenderFlags.Description))
            {
                bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
                string[] lines = _skippedText.Split('\n');
                foreach (string line in lines)
                {
                    if (!isPrefix)
                        writer.WriteLine();
                    writer.Write(line);
                    if (isPrefix)
                        writer.WriteLine();
                }
            }
        }

        protected void SkipSection(Parser parser)
        {
            int minimumPrefixedSpaces = int.MaxValue;

            // First, associate any skipped EOL comment to the current directive
            MoveEOLComment(parser.LastToken);

            // Skip the section following the conditional directive, parsing all lines and storing them.
            // Detect any directives, keeping track of if/endif groups, so we know when we're done, but
            // just store all lines as plain text.
            int nestLevel = 0;
            while (parser.Token != null)
            {
                // Keep track of nested directives
                if (parser.TokenText == ParseToken)
                {
                    string next = parser.PeekNextTokenText();
                    if (next == IfDirective.ParseToken)
                        ++nestLevel;
                    else if (next == ElIfDirective.ParseToken || next == ElseDirective.ParseToken)
                    {
                        if (nestLevel == 0)
                            break;
                    }
                    else if (next == EndIfDirective.ParseToken)
                    {
                        if (--nestLevel < 0)
                            break;
                    }
                }

                // Get any preceeding comments, the current token, and the entire line.  GetCurrentLine() will include
                // comments when it does a NextToken at the end - this preserves formatting and is more efficient, but
                // the first time into this loop we might have trailing comments on the last token.
                List<CommentBase> comments = parser.LastToken.TrailingComments;
                Token token = parser.Token;
                string line = parser.GetCurrentLine();

                // Keep track of the minimum number of prefixed spaces across all lines
                int prefixedSpaces = StringUtil.CharCount(line, ' ', 0);
                if (prefixedSpaces < minimumPrefixedSpaces)
                    minimumPrefixedSpaces = prefixedSpaces;

                // Emit any preceeding comments for the current line
                if (comments != null)
                {
                    foreach (CommentBase comment in comments)
                    {
                        // Emit any preceeding newlines on the comment
                        if (comment.NewLines > 1)
                            _skippedText += new string('\n', comment.NewLines - 1);

                        // Emit the comment, adding back any removed prefix spaces
                        string prefixSpaces = new string(' ', comment.PrefixSpaceCount);
                        _skippedText += prefixSpaces + comment.AsString().Replace("\n", "\n" + prefixSpaces) + '\n';

                        if (comment.PrefixSpaceCount < minimumPrefixedSpaces)
                            minimumPrefixedSpaces = comment.PrefixSpaceCount;
                    }

                    comments.Clear();
                }

                // Emit any preceeding newlines
                if (token.NewLines > 1)
                    _skippedText += new string('\n', token.NewLines - 1);

                // Special handling for multi-line verbatim strings
                if (token.TokenType == TokenType.VerbatimString && StringUtil.Contains(token.Text, '\n'))
                {
                    // Emit everything up to the last line
                    string text = token.Text;
                    int lastLF = text.LastIndexOf('\n');
                    _skippedText += text.Substring(0, lastLF + 1);
                    minimumPrefixedSpaces = 0;
                }

                // Emit the current line of text, and move to the next one
                _skippedText += line;
            }

            if (!string.IsNullOrEmpty(_skippedText))
            {
                // Remove the trailing '\n'
                _skippedText = _skippedText.Substring(0, _skippedText.Length - 1);

                // Normalize space by removing the minimum from all lines
                if (_skippedText[0] != '\n')
                    _skippedText = _skippedText.Substring(minimumPrefixedSpaces);
                _skippedText = _skippedText.Replace("\n" + new string(' ', minimumPrefixedSpaces), "\n");
            }
        }
    }
}