/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// A concrete implementation of <see cref="ITerminalReader" /> created by <see cref="TerminalReaderGenerator" />.
	/// </summary>
	public class TerminalReader : ITerminalReader
	{
		#region Fields

		private List<TerminalType> m_terminalTypes = new List<TerminalType>();
		private TerminalType m_stopTerminal;
		private Regex m_regex;

		private string m_text;
		private Match m_match;
		private Terminal m_queuedTerminal;
		private bool m_stopTerminalRead;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TerminalReader"/> class.
		/// </summary>
		/// <param name="terminalTypes">A collection of <see cref="TerminalType"/> objects recognized by the <see cref="TerminalReader"/></param>
		/// <param name="stopTerminal">The <see cref="TerminalType"/> of the stopping terminal.</param>
		public TerminalReader(IEnumerable<TerminalType> terminalTypes, TerminalType stopTerminal)
		{
			m_terminalTypes.AddRange(terminalTypes);
			m_stopTerminal = stopTerminal;
			m_regex = new Regex(CreatePattern());
		}

		#endregion Constructors

		#region ITerminalReader Members

		/// <summary>
		/// Prepares an <see cref="TerminalReader"/> to read from the specified <see cref="String"/>.
		/// </summary>
		/// <param name="text">A <see cref="String"/> containing the input text to be processed by the <see cref="TerminalReader"/>.</param>
		public void Open(string text)
		{
			m_text = text;
			m_match = null;
			m_queuedTerminal = null;
			m_stopTerminalRead = false;
		}

		/// <summary>
		/// Reads the next <see cref="Terminal"/>.
		/// </summary>
		/// <returns>The next <see cref="Terminal"/> in the input text.  If no <see cref="TerminalType"/> recognizes the input text, <value>null</value> is returned.</returns>
		public Terminal ReadTerminal()
		{
			Terminal result;

			if (m_queuedTerminal != null)
			{
				result = m_queuedTerminal;
				m_queuedTerminal = null;
			}
			else
			{
				result = GetNextTerminal();
			}

			LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_PARSE_READTOKEN, "{0}", result);

			return result;
		}

		/// <summary>
		/// Returns the next <see cref="Terminal"/> in the input text without advancing the current position of the <see cref="TerminalReader"/>.
		/// </summary>
		/// <returns>The next <see cref="Terminal"/> in the input text.  If no <see cref="TerminalType"/> recognizes the input text, <value>null</value> is returned.</returns>
		public Terminal PeekTerminal()
		{
			if (m_queuedTerminal == null)
			{
				m_queuedTerminal = GetNextTerminal();
			}

			return m_queuedTerminal;
		}

		/// <summary>
		/// Returns the specified <see cref="Terminal"/> to the input stream.
		/// </summary>
		/// <param name="terminal">A <see cref="Terminal"/> to be returned to the input stream.</param>
		public void PushTerminal(Terminal terminal)
		{
			if (m_queuedTerminal != null)
			{
				throw new InvalidOperationException("Queued terminal already exists.");
			}

			m_queuedTerminal = terminal;
		}

		#endregion ITerminalReader Members

		#region Hidden Members

		private string CreatePattern()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(@"\G(");

			string prefix = null;
			for (int idx = 0; idx < m_terminalTypes.Count; ++idx)
			{
				TerminalType terminal = m_terminalTypes[idx];
				if (!string.IsNullOrEmpty(terminal.Pattern))
				{
					sb.Append(prefix); prefix = "|";

					sb.AppendFormat(
						@"(?<t{0}>{1})",
						idx,
						terminal.Pattern);
				}
			}

			sb.Append(")");

			return sb.ToString();
		}

		private Terminal GetNextTerminal()
		{
			if (m_match == null)
			{
				m_match = m_regex.Match(m_text);
			}
			else
			{
				m_match = m_match.NextMatch();
			}

			if (m_match.Success)
			{
				for (int idx = 0; idx < m_terminalTypes.Count; ++idx)
				{
					Group group = m_match.Groups[string.Format("t{0}", idx)];
					if (group.Success)
					{
						Terminal terminal = m_terminalTypes[idx].CreateTerminal();
						terminal.ElementType = m_terminalTypes[idx];
						terminal.Text = group.Value;
						return terminal;
					}
				}
			}

			if (!m_stopTerminalRead)
			{
				m_stopTerminalRead = true;

				Terminal terminal = m_stopTerminal.CreateTerminal();
				terminal.ElementType = m_stopTerminal;
				return terminal;
			}

			return null;
		}

		#endregion Hidden Members
	}
}