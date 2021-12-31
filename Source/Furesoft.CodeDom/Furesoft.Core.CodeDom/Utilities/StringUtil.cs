// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Furesoft.Core.CodeDom.Utilities
{
    /// <summary>
    /// Static helper methods for the <see cref="string"/> class.
    /// </summary>
    public static class StringUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Check if the specified string is empty, blank, or null.
        /// </summary>
        /// <param name="thisStr">The string to check.</param>
        /// <returns>True if string is empty, blank, or null.</returns>
        public static bool IsEmpty(string thisStr)
        {
            return (thisStr == null || thisStr.Trim() == "");
        }

        /// <summary>
        /// Check if the specified string is NOT empty, blank, or null.
        /// </summary>
        /// <param name="thisStr">The string to check.</param>
        /// <returns>True if string is NOT empty, blank, or null.</returns>
        public static bool NotEmpty(string thisStr)
        {
            return !(thisStr == null || thisStr.Trim() == "");
        }

        /// <summary>
        /// Return null if the string is empty, otherwise leave it unchanged.
        /// </summary>
        /// <param name="thisStr">The string to check.</param>
        /// <returns>Null if the string is empty, otherwise the unchanged string.</returns>
        public static string EmptyAsNull(string thisStr)
        {
            return (thisStr.Trim() == "" ? null : thisStr);
        }

        /// <summary>
        /// Get the length of the specified string, or 0 if it's null.
        /// </summary>
        /// <param name="thisStr">The string to check.</param>
        /// <returns>String length or 0 if null.</returns>
        public static int NNLength(string thisStr)
        {
            return (thisStr != null ? thisStr.Length : 0);
        }

        /// <summary>
        /// Compare if one string equals another, ignoring case and leading/trailing whitespace, and
        /// treating null and empty strings as equivalent.
        /// </summary>
        /// <param name="thisStr">The string to be compared.</param>
        /// <param name="str">The string to be compared to.</param>
        /// <returns>True if the strings are equal, otherwise false.</returns>
        public static bool NNEqualsIgnoreCase(string thisStr, string str)
        {
            return string.Equals((thisStr ?? "").Trim(), (str ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if the string contains the specified character, returning false if the string is null.
        /// </summary>
        /// <param name="thisStr">The string being operated on.</param>
        /// <param name="ch">The character being searched for.</param>
        /// <returns>True if the string contains the specified character, otherwise false.</returns>
        public static bool Contains(string thisStr, char ch)
        {
            if (thisStr != null)
                return (thisStr.IndexOf(ch) >= 0);
            return false;
        }

        /// <summary>
        /// Check if the string contains the specified substring using a case-insensitive compare, returning false if the string is null.
        /// </summary>
        /// <param name="thisStr">The string being operated on.</param>
        /// <param name="subString">The sub-string being searched for.</param>
        /// <returns>True if the string contains the specified substring, otherwise false.</returns>
        public static bool ContainsIgnoreCase(string thisStr, string subString)
        {
            if (thisStr != null)
                return (thisStr.IndexOf(subString, StringComparison.CurrentCultureIgnoreCase) >= 0);
            return false;
        }

        /// <summary>
        /// Return the number of consecutive specified chars at the specified starting index.
        /// </summary>
        /// <param name="thisStr">The string being operated on.</param>
        /// <param name="ch">The desired char.</param>
        /// <param name="startIndex">The starting index.</param>
        /// <returns>The number of chars found.</returns>
        public static int CharCount(string thisStr, char ch, int startIndex)
        {
            int count = 0;
            if (thisStr != null)
            {
                int length = thisStr.Length;
                for (int i = startIndex; i < length && thisStr[i] == ch; ++i)
                    ++count;
            }
            return count;
        }

        /// <summary>
        /// Convert all runs of spaces and tabs to a single space, and trim all leading and trailing whitespace.
        /// </summary>
        /// <param name="thisStr">The string to be normalized.</param>
        /// <returns>The normalized string.  An empty string is returned if the input string is null.</returns>
        public static string NormalizeWhitespace(string thisStr)
        {
            return Regex.Replace((thisStr ?? "").Trim(), "[ \t]+", " ");
        }

        /// <summary>
        /// Append one string to another, using an optional separator string if the destination string isn't empty.
        /// Treats both destination and source strings as empty if null.
        /// </summary>
        /// <param name="thisStr">The string to append to.</param>
        /// <param name="separator">The optional separator string to use if the destination string isn't empty.</param>
        /// <param name="str">The string to be appended.</param>
        /// <returns>The new string.</returns>
        public static string Append(string thisStr, string separator, string str)
        {
            return (string.IsNullOrEmpty(thisStr) ? str : (thisStr + separator + str));
        }

        /// <summary>
        /// Format the specified collection into a single string using the specified separating
        /// string to separate each item.  Null items are converted to empty strings.
        /// </summary>
        /// <param name="thisCollection">Collection to be converted to a string.</param>
        /// <param name="separator">Separating string to be inserted between items.</param>
        /// <returns>The collection contents formatted as a string.</returns>
        public static string ToString(IEnumerable thisCollection, string separator)
        {
            string result = "";
            if (thisCollection != null)
            {
                foreach (object obj in thisCollection)
                {
                    if (obj != null)
                        result = Append(result, separator, obj.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Check if the specified string array contains the specified string.
        /// </summary>
        /// <param name="thisArray">The array to be searched.</param>
        /// <param name="value">The string value to search for.</param>
        /// <returns>True if the string array contains the specified value.</returns>
        public static bool Contains(string[] thisArray, string value)
        {
            bool contains = false;
            if (thisArray != null)
            {
                foreach (string str in thisArray)
                {
                    if (str == value)
                    {
                        contains = true;
                        break;
                    }
                }
            }
            return contains;
        }

        /// <summary>
        /// Convert a string to an int, returning a default value if the string is null or isn't a valid integer value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails.</param>
        /// <returns>The parsed int value.</returns>
        public static int ParseInt(string thisStr, int defaultValue)
        {
            return (int.TryParse(thisStr, out int val) ? val : defaultValue);
        }

        /// <summary>
        /// Convert a string to an int, defaulting to 0 if the string is null or isn't a valid integer value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <returns>The parsed value.</returns>
        public static int ParseInt(string thisStr)
        {
            return ParseInt(thisStr, 0);
        }

        /// <summary>
        /// Convert a string to a bool, returning a default value if the string is null or isn't a valid bool value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails.</param>
        /// <returns>The parsed bool value.</returns>
        public static bool ParseBool(string thisStr, bool defaultValue)
        {
            bool result = defaultValue;
            if (thisStr != null)
            {
                if (NNEqualsIgnoreCase(thisStr, "true") || thisStr.Trim() == "1" || NNEqualsIgnoreCase(thisStr, "yes") || NNEqualsIgnoreCase(thisStr, "y"))
                    result = true;
                else if (NNEqualsIgnoreCase(thisStr, "false") || thisStr.Trim() == "0" || NNEqualsIgnoreCase(thisStr, "no") || NNEqualsIgnoreCase(thisStr, "n"))
                    result = false;
            }
            return result;
        }

        /// <summary>
        /// Convert a string to a bool, returning false if the string is null or isn't a valid bool value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <returns>The parsed bool value.</returns>
        public static bool ParseBool(string thisStr)
        {
            return ParseBool(thisStr, false);
        }

        /// <summary>
        /// Convert a string to a double, returning a default value if the string is null or isn't a valid double value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails.</param>
        /// <returns>The parsed double value.</returns>
        public static double ParseDouble(string thisStr, double defaultValue)
        {
            return (double.TryParse(thisStr, out double val) ? val : defaultValue);
        }

        /// <summary>
        /// Convert a string to a double, defaulting to 0 if the string is null or isn't a valid double value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <returns>The parsed value.</returns>
        public static double ParseDouble(string thisStr)
        {
            return ParseDouble(thisStr, 0);
        }

        /// <summary>
        /// Convert a string to a DateTime, returning a default value if the string is null or isn't a valid DateTime value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails.</param>
        /// <returns>The parsed DateTime value.</returns>
        public static DateTime ParseDateTime(string thisStr, DateTime defaultValue)
        {
            return (DateTime.TryParse(thisStr, out DateTime val) ? val : defaultValue);
        }

        /// <summary>
        /// Convert a string to a DateTime, returning DateTime.MinValue if the string is null or isn't a valid DateTime value.
        /// </summary>
        /// <param name="thisStr">The string to parse.</param>
        /// <returns>The parsed DateTime value.</returns>
        public static DateTime ParseDateTime(string thisStr)
        {
            return ParseDateTime(thisStr, DateTime.MinValue);
        }

        /// <summary>
        /// Convert a string to an enum value, returning a default value if the string is null or isn't a valid enum value.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails.</param>
        /// <returns>The parsed enum value.</returns>
        public static T ParseEnum<T>(string thisStr, T defaultValue) where T : struct
        {
#if !TARGET_FRAMEWORK_3_5
            return (Enum.TryParse(thisStr, out T result) ? result : defaultValue);
#else
            T result = defaultValue;
            if (thisStr != null && thisStr.Count > 0)
            {
                try
                {
                    result = (T)Enum.Parse(typeof(T), thisStr, true);
                }
                catch { }
            }
            return result;
#endif
        }

        /// <summary>
        /// Convert a string to a generic value.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="thisStr">The string to parse.</param>
        /// <returns>The generic value</returns>
        public static T Parse<T>(string thisStr) where T : IConvertible
        {
            return (T)Convert.ChangeType(thisStr, typeof(T));
        }

        /// <summary>
        /// Convert a string to a generic value.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValue">The value to return if parsing fails</param>
        /// <returns>The generic value</returns>
        public static T Parse<T>(string thisStr, T defaultValue) where T : IConvertible
        {
            T result = defaultValue;
            if (!string.IsNullOrEmpty(thisStr))
            {
                try
                {
                    result = (T)Convert.ChangeType(thisStr, typeof(T));
                }
                catch { }
            }
            return result;
        }

        /// <summary>
        /// Convert a comma delimited string to an array of generic values
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValues">The values to return if parsing fails</param>
        /// <param name="delim">The delimiter for fields in the string</param>
        /// <returns>An array of generic values</returns>
        public static T[] ParseArray<T>(string thisStr, T[] defaultValues, char delim) where T : IConvertible
        {
            T[] results = defaultValues;
            if (!string.IsNullOrEmpty(thisStr))
            {
                try
                {
                    IEnumerable<T> fields = Enumerable.Select<string, T>(thisStr.Split(delim), delegate(string text) { return Parse<T>(text); });
                    results = Enumerable.ToArray(fields);
                }
                catch { }
            }
            return results;
        }

        /// <summary>
        /// Convert a comma delimited string to an array of generic values
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="thisStr">The string to parse.</param>
        /// <param name="defaultValues">The values to return if parsing fails</param>
        /// <returns>An array of generic values </returns>
        public static T[] ParseArray<T>(string thisStr, T[] defaultValues) where T : IConvertible
        {
            return ParseArray(thisStr, defaultValues, ',');
        }

        /// <summary>
        /// Convert all unicode escape sequences in the specified string that represent letters or digits into chars.
        /// </summary>
        /// <param name="source">The string to be converted.</param>
        /// <returns>A new string with unicode escapes converted to chars.</returns>
        public static string ConvertUnicodeEscapes(string source)
        {
            StringBuilder result = null;
            int i, start = 0;
            for (i = 0; i < source.Length; )
            {
                if (source[i] == '\\')
                {
                    // Check for "\uXXXX" or "\UXXXXXXXX"
                    int len = 0;
                    if (source[i + 1] == 'u')
                        len = 4;
                    else if (source[i + 1] == 'U')
                        len = 8;
                    if (len > 0)
                    {
                        try
                        {
                            if (CheckHex(source, i + 2, len))
                            {
                                // Convert 16-bit values to chars, or 32-bit values to surrogates (double chars)
                                if (result == null)
                                    result = new StringBuilder();
                                result.Append(source, start, i - start);
                                string hex = source.Substring(i + 2, len);
                                if (len == 4)
                                {
                                    ushort u16 = ushort.Parse(hex, NumberStyles.HexNumber);
                                    result.Append((char)u16);
                                }
                                else
                                {
                                    int u32 = int.Parse(hex, NumberStyles.HexNumber);
                                    result.Append(char.ConvertFromUtf32(u32));
                                }
                                i += 2 + len;
                                start = i;
                                continue;
                            }
                        }
                        catch { }
                    }
                }
                ++i;
            }
            return (result == null ? source : result.Append(source, start, i - start).ToString());
        }

        private static bool CheckHex(string str, int offset, int len)
        {
            for (int i = offset; i < offset + len; ++i)
            {
                if (!Uri.IsHexDigit(str[i]))
                    return false;
            }
            return true;
        }

        #endregion
    }
}
