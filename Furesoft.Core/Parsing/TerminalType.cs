/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Reflection;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines a <see cref="LanguageElementType" /> that represents a type of a terminal or token identified by an <see cref="TerminalType" />.
	/// </summary>
	/// <remarks>
	/// A <see cref="Terminal"/> is a factory object that can construct the associated <see cref="CreateTerminal"/> object using
	/// <see cref="IGrammar"/>.
	/// </remarks>
	public class TerminalType : LanguageElementType
	{
		#region Fields

		private delegate Terminal Constructor();

		private string m_fullName;
		private string m_name;
		private Constructor m_constructor;
		private string m_pattern;
		private bool m_isStop;
		private bool m_ignore;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TerminalType"/> class.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the <see cref="Terminal"/> described by this <see cref="TerminalType"/>.</param>
		public TerminalType(Type type)
			: base(LanguageElementTypes.Terminal)
		{
			if (!type.IsSubclassOf(typeof(Terminal)))
			{
				throw new ArgumentException("Type must be a subclass of Terminal.", "type");
			}

			m_fullName = type.AssemblyQualifiedName;
			m_name = GetName(type);
			m_constructor = delegate () { return (Terminal)Activator.CreateInstance(type); };

			object[] attributes = type.GetCustomAttributes(typeof(TerminalAttribute), false);
			foreach (object attribute in attributes)
			{
				TerminalAttribute terminalAttribute = (TerminalAttribute)attribute;
				if (terminalAttribute.IsStop)
				{
					m_isStop = true;
				}
				if (terminalAttribute.Ignore)
				{
					m_ignore = true;
				}
				m_pattern = terminalAttribute.Pattern;
			}
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the full name of the <see cref="TerminalType"/>.  This includes the name of the <see cref="Assembly"/> containing
		/// the <see cref="Type"/> used to construct the <see cref="TerminalType"/> and will always uniquely identify the <see cref="TerminalType"/>.
		/// </summary>
		public override string FullName
		{
			get { return m_fullName; }
		}

		/// <summary>
		/// Gets the abbreviated name of the <see cref="TerminalType"/>.  This excludes the name of the <see cref="Assembly"/> containing the
		/// <see cref="Type"/> used to construct the <see cref="TerminalType"/> and may not uniquely identify the <see cref="TerminalType"/>.
		/// </summary>
		public override string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets or sets the regular expression pattern of this <see cref="TerminalType"/>.
		/// </summary>
		public string Pattern
		{
			get { return m_pattern; }
		}

		/// <summary>
		/// Gets or sets a value that indicates if this <see cref="TerminalType"/> is the stopping terminal
		/// of the <see cref="IGrammar"/>.
		/// </summary>
		public bool IsStop
		{
			get { return m_isStop; }
		}

		/// <summary>
		/// Gets or sets a value that indicates if <see cref="Terminal"/> objects associated with this <see cref="TerminalType"/> should be ignored by any
		/// <see cref="IParser"/> that encounters them.
		/// </summary>
		public bool Ignore
		{
			get { return m_ignore; }
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format("[T {0}]", Name);
		}

		/// <summary>
		/// Constructs an instance of a <see cref="Terminal"/> described by this <see cref="TerminalType"/>.
		/// </summary>
		/// <returns>A new <see cref="Terminal"/>.</returns>
		public Terminal CreateTerminal()
		{
			return m_constructor();
		}

		#endregion Public Methods
	}
}