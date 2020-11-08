/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Text.RegularExpressions;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Specifies the attribute of a <see cref="Terminal" />-derived class.
	/// </summary>
	public class TerminalAttribute : Attribute
	{
		#region Fields

		private string m_pattern;
		private bool m_isStop;
		private bool m_ignore;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TerminalAttribute"/> class.
		/// </summary>
		public TerminalAttribute()
		{
			m_pattern = null;
			m_isStop = false;
			m_ignore = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TerminalAttribute"/> class.
		/// </summary>
		/// <param name="pattern">A <see cref="Regex"/> pattern that defines the terminal.</param>
		public TerminalAttribute(string pattern)
		{
			m_pattern = pattern;
			m_isStop = false;
			m_ignore = false;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the regular expression pattern of the <see cref="Terminal"/> adorned by this attribute.
		/// </summary>
		public string Pattern
		{
			get { return m_pattern; }
			set { m_pattern = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates if the <see cref="Terminal"/> adorned by this attribute is the stopping terminal
		/// of the <see cref="IGrammar"/>.
		/// </summary>
		public bool IsStop
		{
			get { return m_isStop; }
			set { m_isStop = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates if the <see cref="Terminal"/> adorned by this attribute should be ignored by any
		/// <see cref="IParser"/> that encounters it.
		/// </summary>
		public bool Ignore
		{
			get { return m_ignore; }
			set { m_ignore = value; }
		}

		#endregion Public Properties
	}
}