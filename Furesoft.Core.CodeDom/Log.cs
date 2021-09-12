// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom;

namespace Furesoft.Core.CodeDom
{
    /// <summary>
    /// Supports logging of messages.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The logging level.
        /// </summary>
        public enum Level
        {
            /// <summary>No logging at all.</summary>
            None,
            /// <summary>Minimal logging, error messages only.  Doesn't log any assembly loading information.</summary>
            Minimal,
            /// <summary>Normal logging, error and warning messages.</summary>
            Normal,
            /// <summary>Detailed logging, all messages.  Includes details of assembly and type loading.</summary>
            Detailed
        }

        /// <summary>
        /// Determines if detail log messages are created.
        /// </summary>
        public static Level LogLevel = Level.Minimal;

        /// <summary>
        /// The callback delegate that gets called to create log entries.
        /// </summary>
        private static Action<string, string> _logWriteLineCallback;

        static Log()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            CodeObject.ForceReference();
        }

        /// <summary>
        /// Set the callback method that gets called for each log entry.
        /// </summary>
        /// <param name="callback">A method that accepts a message to be logged, and a tooltip to be displayed for the message.</param>
        public static void SetLogWriteLineCallback(Action<string, string> callback)
        {
            _logWriteLineCallback = callback;
        }

        /// <summary>
        /// Log a message.
        /// </summary>
        public static void WriteLine(string message)
        {
            WriteLine(message, null);
        }

        /// <summary>
        /// Log a message, with a tooltip to display detailed information in a UI.
        /// </summary>
        public static void WriteLine(string message, string toolTip)
        {
            if (LogLevel > Level.None)
            {
                if (_logWriteLineCallback != null)
                    _logWriteLineCallback(message, toolTip);
                else
                {
                    // Holy crap!  At least in the debugger, this takes forever!  If a few thousand errors
                    // are logged, this takes the total time from half a second to 48 seconds!  The generation
                    // of the messages and GUI display in the callback is nothing compared to this console write!
                    // For this reason, we don't do this unless no logging callback is set.
                    Console.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Log a message if detailed logging is enabled.
        /// </summary>
        public static void DetailWriteLine(string message, string toolTip)
        {
            if (LogLevel >= Level.Detailed)
                WriteLine(message, toolTip);
        }

        /// <summary>
        /// Log a message if detailed logging is enabled.
        /// </summary>
        public static void DetailWriteLine(string message)
        {
            if (LogLevel >= Level.Detailed)
                WriteLine(message, null);
        }

        /// <summary>
        /// Log an exception (also returns the generic message that was logged).
        /// </summary>
        public static string Exception(Exception ex, string message)
        {
            message = "EXCEPTION " + message + ": " + ex.Message;
            if (ex is UnauthorizedAccessException)
                message += "  File might be read-only or in use.";
            string details = ex.ToString().TrimEnd();
            // We put the details in the tooltip in the UI, but we really also need them in the text message (for console apps)
            message += "\n" + details;
            WriteLine(message, details);
            return message;
        }
    }
}
