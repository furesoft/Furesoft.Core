// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Text;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a literal value of a particular type (string, integer, boolean, etc).
    /// </summary>
    /// <remarks>
    /// In order to retain the exact text of a literal, such as escape sequences in strings or chars, or the
    /// exact format (such as hex and/or suffix characters) of numerics, the literal value is stored as a string,
    /// which is then converted to an object of the appropriate type for code analysis purposes.
    /// </remarks>
    public class Literal : Expression
    {
        protected string _text;

        /// <summary>
        /// Create a literal with the specified string representation.
        /// </summary>
        /// <remarks>
        /// The isRaw flag must be true for literal strings, chars using an escape sequence, integer
        /// values in hex format, or any other specific format, and the string must include all quotes
        /// and prefixes.  If isRaw is false, then double quotes will be added around the string.
        /// </remarks>
        public Literal(string text, bool isRaw)
        {
            if (text == null)
                _text = ParseTokenNull;
            else if (isRaw)
                Text = text;
            else
                _text = "\"" + text + "\"";
        }

        /// <summary>
        /// Create a literal with the specified string value.
        /// </summary>
        public Literal(string value)
            : this(value, false)
        { }

        /// <summary>
        /// Create a literal with the specified char value.
        /// </summary>
        public Literal(char value)
        {
            _text = "'" + value + "'";
        }

        /// <summary>
        /// Create a literal with the specified int value.
        /// </summary>
        public Literal(int value)
        {
            _text = value.ToString();
        }

        /// <summary>
        /// Create a literal with the specified uint value.
        /// </summary>
        public Literal(uint value)
        {
            _text = value.ToString();
        }

        /// <summary>
        /// Create a literal with the specified long value.
        /// </summary>
        public Literal(long value)
        {
            _text = value.ToString();
        }

        /// <summary>
        /// Create a literal with the specified ulong value.
        /// </summary>
        public Literal(ulong value)
        {
            _text = value.ToString();
        }

        /// <summary>
        /// Create a literal with the specified bool value.
        /// </summary>
        public Literal(bool value)
        {
            _text = (value ? ParseTokenTrue : ParseTokenFalse);
        }

        /// <summary>
        /// Create a literal with the specified float value.
        /// </summary>
        public Literal(float value)
        {
            _text = value + "f";
        }

        /// <summary>
        /// Create a literal with the specified double value.
        /// </summary>
        public Literal(double value)
        {
            // Note that we have to add the 'd' in this case - otherwise, "new Literal(1.0)" would result in a text
            // value of "1", which will be treated as an int.  We can't just force or add the ".0" on the end, because
            // this won't work for all possible types of double values, such as exponential ones.
            _text = value + "d";
        }

        /// <summary>
        /// Create a literal with the specified decimal value.
        /// </summary>
        public Literal(decimal value)
        {
            _text = value + "m";
        }

        /// <summary>
        /// Create a literal from the specified object (which can be any valid literal type, null, or an EnumConstant).
        /// </summary>
        public Literal(object obj, bool hexFormat)
        {
            if (obj == null)
                _text = ParseTokenNull;
            else if (obj is bool)
                _text = ((bool)obj ? ParseTokenTrue : ParseTokenFalse);
            else if (obj is string)
            {
                string text = (string)obj;
                StringBuilder builder = new StringBuilder(text.Length + 8);
                builder.Append('"');
                foreach (char ch in text)
                    CharToEscapedString(builder, ch, false);
                builder.Append('"');
                _text = builder.ToString();
            }
            else if (obj is char)
            {
                StringBuilder builder = new StringBuilder(8);
                builder.Append('\'');
                CharToEscapedString(builder, (char)obj, true);
                builder.Append('\'');
                _text = builder.ToString();
            }
            else
            {
                if (obj is EnumConstant)
                    obj = ((EnumConstant)obj).ConstantValue;
                if (hexFormat)
                {
                    if (obj is long || obj is ulong)
                        _text = string.Format("0x{0:x16}", obj);
                    else if (obj is int || obj is uint)
                        _text = string.Format("0x{0:x8}", obj);
                    else if (obj is short || obj is ushort)
                        _text = string.Format("0x{0:x4}", obj);
                    else if (obj is sbyte || obj is byte)
                        _text = string.Format("0x{0:x2}", obj);
                    else
                        _text = obj.ToString();
                }
                else
                    _text = obj.ToString();
            }
        }

        /// <summary>
        /// Create a literal from the specified object (which can be any valid literal type, null, or an EnumConstant).
        /// </summary>
        public Literal(object obj)
            : this(obj, false)
        { }

        protected static void CharToEscapedString(StringBuilder builder, char ch, bool isChar)
        {
            // Escape char if necessary
            if (ch < 0x20)
            {
                string str;
                switch (ch)
                {
                    case '\0': str = @"\0"; break;
                    case '\a': str = @"\a"; break;
                    case '\b': str = @"\b"; break;
                    case '\f': str = @"\f"; break;
                    case '\n': str = @"\n"; break;
                    case '\r': str = @"\r"; break;
                    case '\t': str = @"\t"; break;
                    case '\v': str = @"\v"; break;
                    default: str = string.Format("\\x{0:X2}", (int)ch); break;
                }
                builder.Append(str);
            }
            else if (ch >= 0x7f)
                builder.Append(string.Format(ch >= 0x100 ? "\\u{0:X4}" : "\\x{0:X2}", (int)ch));
            else
            {
                if (ch == '\\')
                    builder.Append(@"\\");
                else if (isChar && ch == '\'')
                    builder.Append("\'");
                else if (!isChar && ch == '"')
                    builder.Append("\\\"");
                else
                    builder.Append(ch);
            }
        }

        /// <summary>
        /// Always <c>true</c> for <see cref="Literal"/>s.
        /// </summary>
        public override bool IsConst
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="Literal"/> is <c>null</c>.
        /// </summary>
        public bool IsNull
        {
            get { return (_text == ParseTokenNull); }
        }

        /// <summary>
        /// The text content of the <see cref="Literal"/>.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                // Normalize certain literals
                if (_text == @"'\""'")
                    _text = @"'""'";  // Change '\"' to '"'
            }
        }

        /// <summary>
        /// The token used to parse a 'false' literal.
        /// </summary>
        public const string ParseTokenFalse = "false";

        /// <summary>
        /// The token used to parse a 'null' literal.
        /// </summary>
        public const string ParseTokenNull = "null";

        /// <summary>
        /// The token used to parse a 'true' literal.
        /// </summary>
        public const string ParseTokenTrue = "true";

        /// <summary>
        /// Parse a <see cref="Literal"/>.
        /// </summary>
        public Literal(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Text = parser.TokenText;
            parser.NextToken();  // Move past token

            // Move any trailing EOL or inline comment to the expression as an EOL comment
            MoveEOLComment(parser.LastToken);
        }

        /// <summary>
        /// Parse a <see cref="Literal"/>.
        /// </summary>
        public static Literal Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Literal(parser, parent);
        }

        internal static new void AddParsePoints()
        {
            // NOTE: No parse-points are installed for string, char, or numeric literals - instead, the parser
            //       calls the parsing constructor directly based upon the token type.  This is because we want
            //       to parse literals into individual tokens within the parser itself to preserve whitespace.

            // Install parse-points for 'null', 'true', and 'false' literals (without scope restrictions)
            Parser.AddParsePoint(ParseTokenNull, Parse);
            Parser.AddParsePoint(ParseTokenTrue, Parse);
            Parser.AddParsePoint(ParseTokenFalse, Parse);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.EscapeUnicode = false;
            writer.Write(_text);
            writer.EscapeUnicode = true;
        }
    }
}