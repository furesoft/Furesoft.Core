// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// The action of a <see cref="PragmaWarningDirective"/>.
    /// </summary>
    public enum PragmaWarningAction { Invalid, Disable, Restore }

    /// <summary>
    /// Used to turn warnings on or off.
    /// </summary>
    public class PragmaWarningDirective : PragmaDirective
    {
        protected PragmaWarningAction _pragmaWarningAction;
        protected List<int> _warningNumbers;

        /// <summary>
        /// Create a <see cref="PragmaWarningDirective"/> with the specified action.
        /// </summary>
        public PragmaWarningDirective(PragmaWarningAction pragmaWarningAction)
        {
            _pragmaWarningAction = pragmaWarningAction;
        }

        /// <summary>
        /// Create a <see cref="PragmaWarningDirective"/> with the specified action and warning numbers.
        /// </summary>
        public PragmaWarningDirective(PragmaWarningAction pragmaWarningAction, params int[] warningNumbers)
            : this(pragmaWarningAction)
        {
            CreateNumbers().AddRange(warningNumbers);
        }

        /// <summary>
        /// True if there are any warning numbers.
        /// </summary>
        public bool HasWarningNumbers
        {
            get { return (_warningNumbers != null && _warningNumbers.Count > 0); }
        }

        public override string PragmaType { get { return ParseToken; } }

        /// <summary>
        /// The associated <see cref="PragmaWarningAction"/>.
        /// </summary>
        public PragmaWarningAction PragmaWarningAction
        {
            get { return _pragmaWarningAction; }
            set { _pragmaWarningAction = value; }
        }

        /// <summary>
        /// The associated warning numbers.
        /// </summary>
        public List<int> WarningNumbers
        {
            get { return _warningNumbers; }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            PragmaWarningDirective clone = (PragmaWarningDirective)base.Clone();
            if (_warningNumbers != null && _warningNumbers.Count > 0)
                clone._warningNumbers = new List<int>(_warningNumbers);
            return clone;
        }

        /// <summary>
        /// Create the list of warning numbers, or return the existing one.
        /// </summary>
        public List<int> CreateNumbers()
        {
            if (_warningNumbers == null)
                _warningNumbers = new List<int>();
            return _warningNumbers;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "warning";

        /// <summary>
        /// The token used to parse the 'disable' action.
        /// </summary>
        public const string ParseTokenDisable = "disable";

        /// <summary>
        /// The token used to parse the 'restore' action.
        /// </summary>
        public const string ParseTokenRestore = "restore";

        /// <summary>
        /// The token used to separate warning numbers.
        /// </summary>
        public const string ParseTokenSeparator = Expression.ParseTokenSeparator;

        /// <summary>
        /// Parse a <see cref="PragmaWarningDirective"/>.
        /// </summary>
        public PragmaWarningDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'pragma'
            Token token = parser.NextTokenSameLine(false);  // Move past 'warning'
            if (token != null)
            {
                // Parse the warning action
                _pragmaWarningAction = ParseAction(parser.TokenText);
                if (_pragmaWarningAction != PragmaWarningAction.Invalid)
                {
                    token = parser.NextTokenSameLine(false);  // Move past the action

                    // Parse the list of warning numbers
                    while (token != null && token.IsNumeric)
                    {
                        int number;
                        if (!int.TryParse(token.Text, out number))
                        {
                            number = int.MaxValue;
                            parser.AttachMessage(this, "Integer value expected", token);
                        }
                        CreateNumbers().Add(number);
                        token = parser.NextTokenSameLine(false);  // Move past the number
                        if (token != null && token.Text == ParseTokenSeparator)
                            token = parser.NextTokenSameLine(false);  // Move past ','
                        else
                            break;
                    }
                }
            }
            MoveEOLComment(parser.LastToken);
        }

        /// <summary>
        /// Parse a <see cref="PragmaWarningDirective"/>.
        /// </summary>
        public static new PragmaWarningDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new PragmaWarningDirective(parser, parent);
        }

        internal static new void AddParsePoints()
        {
            AddPragmaParsePoint(ParseToken, Parse);
        }

        protected static PragmaWarningAction ParseAction(string actionName)
        {
            PragmaWarningAction action;
            switch (actionName)
            {
                case ParseTokenDisable: action = PragmaWarningAction.Disable; break;
                case ParseTokenRestore: action = PragmaWarningAction.Restore; break;
                default: action = PragmaWarningAction.Invalid; break;
            }
            return action;
        }

        /// <summary>
        /// Format a <see cref="PragmaWarningAction"/> as a string.
        /// </summary>
        public static string PragmaWarningActionToString(PragmaWarningAction pragmaWarningAction)
        {
            switch (pragmaWarningAction)
            {
                case PragmaWarningAction.Disable: return ParseTokenDisable;
                case PragmaWarningAction.Restore: return ParseTokenRestore;
            }
            return "";
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextArgument(writer, flags);
            writer.Write(" " + PragmaWarningActionToString(_pragmaWarningAction));
            if (HasWarningNumbers)
            {
                writer.Write(" ");
                bool first = true;
                foreach (int number in _warningNumbers)
                {
                    if (!first)
                        writer.Write(ParseTokenSeparator + " ");
                    writer.Write(number.ToString());
                    first = false;
                }
            }
        }
    }
}