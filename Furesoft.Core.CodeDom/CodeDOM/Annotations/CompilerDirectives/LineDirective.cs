// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The type of a <see cref="LineDirective"/>.
    /// </summary>
    public enum LineDirectiveType { Number, Default, Hidden }

    /// <summary>
    /// Used to control embedded line number information.
    /// </summary>
    public class LineDirective : CompilerDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "line";

        /// <summary>
        /// The token used to parse the 'default' type.
        /// </summary>
        public const string ParseTokenDefault = "default";

        /// <summary>
        /// The token used to parse the 'hidden' type.
        /// </summary>
        public const string ParseTokenHidden = "hidden";

        protected LineDirectiveType _directiveType;
        protected string _fileName;
        protected int _number;

        /// <summary>
        /// Create a <see cref="LineDirective"/> with the specified type.
        /// </summary>
        public LineDirective(LineDirectiveType directiveType)
        {
            _directiveType = directiveType;
        }

        /// <summary>
        /// Create a <see cref="LineDirective"/> with the specified line number and file name.
        /// </summary>
        public LineDirective(int number, string fileName)
            : this(LineDirectiveType.Number)
        {
            _number = number;
            _fileName = fileName;
        }

        /// <summary>
        /// Parse a <see cref="LineDirective"/>.
        /// </summary>
        public LineDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Token token = parser.NextTokenSameLine(false);  // Move past the keyword
            if (token != null)
            {
                // Parse the directive type
                _directiveType = ParseLineDirectiveType(parser.TokenText);
                if (_directiveType != LineDirectiveType.Number)
                    parser.NextToken();  // Move past the directive type
                else
                {
                    // Parse the line number
                    if (!int.TryParse(token.Text, out _number))
                    {
                        _number = int.MaxValue;
                        parser.AttachMessage(this, "Integer value expected", token);
                    }
                    token = parser.NextTokenSameLine(false);  // Move past the number
                    if (token != null)
                    {
                        // Get the filename
                        _fileName = token.Text;
                        parser.NextToken();  // Move past filename
                    }
                }
            }
            MoveEOLComment(parser.LastToken);
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The type of the <see cref="LineDirective"/>.
        /// </summary>
        public LineDirectiveType DirectiveType
        {
            get { return _directiveType; }
            set { _directiveType = value; }
        }

        /// <summary>
        /// The associated file name.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Determines if the compiler directive should be indented.
        /// </summary>
        public override bool HasNoIndentationDefault
        {
            get { return false; }
        }

        /// <summary>
        /// The associated line number.
        /// </summary>
        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Format a <see cref="LineDirectiveType"/> as a string.
        /// </summary>
        public static string LineDirectiveTypeToString(LineDirectiveType directiveType)
        {
            switch (directiveType)
            {
                case LineDirectiveType.Default: return ParseTokenDefault;
                case LineDirectiveType.Hidden: return ParseTokenHidden;
            }
            return "";
        }

        /// <summary>
        /// Parse a <see cref="LineDirective"/>.
        /// </summary>
        public static LineDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new LineDirective(parser, parent);
        }

        protected static LineDirectiveType ParseLineDirectiveType(string actionName)
        {
            LineDirectiveType action;
            switch (actionName)
            {
                case ParseTokenDefault: action = LineDirectiveType.Default; break;
                case ParseTokenHidden: action = LineDirectiveType.Hidden; break;
                default: action = LineDirectiveType.Number; break;
            }
            return action;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_directiveType == LineDirectiveType.Number)
                writer.Write(_number + " " + _fileName);
            else
                writer.Write(LineDirectiveTypeToString(_directiveType));
        }
    }
}