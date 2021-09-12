// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Globalization;
using System.Text;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Other
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
        #region /* FIELDS */

        protected string _text;

        // We could perhaps cache the actual value of the literal as a member, but since GetValue() shouldn't be
        // called very many times while parsing/analyzing the code, perhaps it's not worth the extra memory.

        #endregion

        #region /* CONSTRUCTORS */

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
                    default:   str = string.Format("\\x{0:X2}", (int)ch); break;
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

        #endregion

        #region /* PROPERTIES */

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

        #endregion

        #region /* METHODS */

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse a 'null' literal.
        /// </summary>
        public const string ParseTokenNull = "null";

        /// <summary>
        /// The token used to parse a 'true' literal.
        /// </summary>
        public const string ParseTokenTrue = "true";

        /// <summary>
        /// The token used to parse a 'false' literal.
        /// </summary>
        public const string ParseTokenFalse = "false";

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

        /// <summary>
        /// Parse a <see cref="Literal"/>.
        /// </summary>
        public static Literal Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Literal(parser, parent);
        }

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

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            object value = GetValue();
            return (withoutConstants ? (value != null ? new TypeRef(value.GetType()) : TypeRef.ObjectRef) : new TypeRef(value));
        }

        /// <summary>
        /// Get the value of the <see cref="Literal"/>.
        /// </summary>
        public object GetValue()
        {
            int length = _text.Length;
            if (length == 0) return this;

            char firstChar = _text[0];
            switch (firstChar)
            {
                case '"':  // string
                    if (length >= 2 && _text[length - 1] == '"')
                    {
                        // Extract the actual string, processing any escape sequences
                        StringBuilder builder = new StringBuilder(length);
                        for (int index = 1; index < length - 1; ++index)
                        {
                            char ch = _text[index];
                            if (ch == '\\' && index < length - 2)
                                ch = CharFromEscapeSequence(_text, length - 1, ref index);
                            builder.Append(ch);
                        }
                        return builder.ToString();
                    }
                    break;
                case '@':  // literal string
                    if (length >= 3 && _text[1] == '"' && _text[length - 1] == '"')
                    {
                        // Extract the actual string, processing any escaped double quotes
                        StringBuilder builder = new StringBuilder(length);
                        for (int i = 2; i < length - 1; ++i)
                        {
                            char ch = _text[i];
                            if (ch == '"' && _text[i + 1] == '"')
                                ++i;
                            builder.Append(ch);
                        }
                        return builder.ToString();
                    }
                    break;
                case '\'':  // char
                    if (length >= 2 && _text[length - 1] == '\'')
                    {
                        // Extract the actual char, processing any escape sequences
                        char ch = _text[1];
                        if (ch == '\\')
                        {
                            int index = 1;
                            ch = CharFromEscapeSequence(_text, length - 1, ref index);
                        }
                        return ch;
                    }
                    break;
                case 'n':  // null
                    if (_text == ParseTokenNull)
                        return null;
                    break;
                case 't':  // true
                    if (_text == ParseTokenTrue)
                        return true;
                    break;
                case 'f':  // false
                    if (_text == ParseTokenFalse)
                        return false;
                    break;
                default:  // numerics
                    if (char.IsDigit(firstChar) || (firstChar == '.') || (firstChar == '-' && _text != "-Infinity") || (firstChar == '+'))
                    {
                        int start = 0;
                        bool negative = false;
                        bool hexFormat = false;
                        NumberStyles style = NumberStyles.None;

                        // Check for signs
                        if (_text[start] == '-')
                        {
                            ++start;
                            negative = true;
                            style = NumberStyles.AllowLeadingSign;
                        }
                        else if (_text[start] == '+')
                            ++start;

                        // Check for hex values
                        if (_text[start] == '0' && start < _text.Length - 1 && _text[start + 1] == 'x')
                        {
                            start += 2;
                            hexFormat = true;
                            style = NumberStyles.AllowHexSpecifier;  // Overwrite/ignore any AllowLeadingSign
                        }

                        int i;
                        uint ui;
                        long l;
                        ulong ul;
                        double d;
                        decimal m;
                        string val = (negative ? "-" : "") + _text.Substring(start);

                        // Check for suffixes
                        if (start < _text.Length)
                        {
                            int end = _text.Length - 1;
                            char last = char.ToLower(_text[end]);
                            char prev = (end > start ? char.ToLower(_text[end - 1]) : '\0');
                            if (!hexFormat)
                            {
                                if (last == 'f')
                                {
                                    float f;
                                    val = val.Substring(0, val.Length - 1);  // Remove suffix or it will fail
                                    float.TryParse(val, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out f);
                                    return f;
                                }
                                if (last == 'd')
                                {
                                    val = val.Substring(0, val.Length - 1);  // Remove suffix or it will fail
                                    double.TryParse(val, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d);
                                    return d;
                                }
                                if (last == 'm')
                                {
                                    val = val.Substring(0, val.Length - 1);  // Remove suffix or it will fail
                                    decimal.TryParse(val, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out m);
                                    return m;
                                }
                            }
                            bool isULong = false;
                            if (last == 'u')
                            {
                                if (prev != 'l')
                                {
                                    // If only 'u' is specified, use the first type that fits: uint, ulong
                                    val = val.Substring(0, val.Length - 1);  // Remove suffix or it will fail
                                    if (uint.TryParse(val, style, CultureInfo.InvariantCulture, out ui))
                                        return ui;
                                }
                                else
                                    val = val.Substring(0, val.Length - 2);  // Remove suffix or it will fail
                                isULong = true;
                            }
                            if (last == 'l')
                            {
                                if (prev != 'u')
                                {
                                    // If only 'l' is specified, use the first type that fits: long, ulong
                                    val = val.Substring(0, val.Length - 1);  // Remove suffix or it will fail
                                    if (long.TryParse(val, style, CultureInfo.InvariantCulture, out l))
                                        return l;
                                }
                                else
                                    val = val.Substring(0, val.Length - 2);  // Remove suffix or it will fail
                                isULong = true;
                            }
                            if (isULong)
                            {
                                ulong.TryParse(val, style, CultureInfo.InvariantCulture, out ul);
                                return ul;
                            }
                        }

                        // If we haven't determined the type yet, parse the string, finding the smallest type
                        // in which it fits.  We have to try signed types first in case smaller positive values
                        // fit, but hex values representing negative values are (erroneously?) parsed, so we
                        // have to check for that case and allow the unsigned type to parse them instead.
                        if (int.TryParse(val, style, CultureInfo.InvariantCulture, out i))
                        {
                            if (!(hexFormat && i < 0))
                                return i;
                        }
                        if (!negative && uint.TryParse(val, style, CultureInfo.InvariantCulture, out ui))
                            return ui;
                        if (long.TryParse(val, style, CultureInfo.InvariantCulture, out l))
                        {
                            if (!(hexFormat && l < 0))
                                return l;
                        }
                        if (!negative && ulong.TryParse(val, style, CultureInfo.InvariantCulture, out ul))
                            return ul;
                        if (!hexFormat)
                        {
                            if (double.TryParse(val, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d))
                                return d;
                            if (decimal.TryParse(val, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out m))
                                return m;
                        }
                    }
                    if (_text == "NaN")
                        return double.NaN;
                    if (_text == "Infinity")
                        return double.PositiveInfinity;
                    if (_text == "-Infinity")
                        return double.NegativeInfinity;
                    break;
            }

            // Return this object if the Literal is invalid
            return this;
        }

        protected char CharFromEscapeSequence(string text, int length, ref int index)
        {
            // Assume index is pointing to a verified '\' starting the sequence
            char ch = _text[index];
            if (++index < length)
            {
                ch = _text[index];  // By default, use the escaped char
                switch (ch)
                {
                    case '0': ch = '\0'; break;
                    case 'a': ch = '\a'; break;
                    case 'b': ch = '\b'; break;
                    case 'f': ch = '\f'; break;
                    case 'n': ch = '\n'; break;
                    case 'r': ch = '\r'; break;
                    case 't': ch = '\t'; break;
                    case 'v': ch = '\v'; break;
                    case 'x':
                    case 'u':
                        ch = CharFromHexString(text, Math.Max(length, index + 4), ref index);
                        break;
                    case 'U':  // values can be up to 0x0010FFFF
                        ch = CharFromHexString(text, Math.Max(length, index + 8), ref index);
                        break;
                }
            }
            return ch;
        }

        protected char CharFromHexString(string text, int length, ref int index)
        {
            // Assume index is pointing to a verified 'x', 'u', or 'U' starting the sequence
            char ch = _text[index];
            if (++index < length)
            {
                ch = '\0';
                while (Uri.IsHexDigit(_text[index]))
                {
                    ch = (char)((ch << 4) + Uri.FromHex(_text[index]));
                    if (++index >= length)
                        break;
                }
            }
            --index;  // Point to the last char that was used
            return ch;
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.EscapeUnicode = false;
            writer.Write(_text);
            writer.EscapeUnicode = true;
        }

        public static void AsTextConstantValue(CodeWriter writer, RenderFlags flags, object constantValue, bool hexFormat, CodeObject parent)
        {
            writer.Write(" = ");
            Literal literal = new Literal(constantValue, hexFormat);
            literal.Parent = parent;
            literal.AsText(writer, flags);
        }

        public static void AsTextConstantValue(CodeWriter writer, RenderFlags flags, object constantValue, bool hexFormat)
        {
            AsTextConstantValue(writer, flags, constantValue, hexFormat, null);
        }

        #endregion
    }
}
