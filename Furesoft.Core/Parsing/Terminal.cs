/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines a <see cref="LanguageElement" /> that represents a terminal: a token recognized by an <see cref="ITerminalReader"/>
	/// which appears in the RHS of one more rules defined by an <see cref="Terminal"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="ITerminalReader"/> objects are created by an <see cref="IParser"/> and are consumed by an <see cref="ParserStack"/>.
	/// They are placed on the <see cref="ParserActionShift"/> when a <see cref="ParserStack"/> action occurs, and are subsequently
	/// removed from the <see cref="ParserActionReduce"/> when a <see cref="IGrammar"/> action occurs.
	/// </remarks>
	public class Terminal : LanguageElement
	{
		#region Fields

		private string m_text;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Terminal"/> class.
		/// </summary>
		public Terminal()
		{ }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the input text associated with this <see cref="Terminal"/>.
		/// </summary>
		/// <remarks>
		/// This property should be set by an <see cref="ITerminalReader"/> before returning the <see cref="Terminal"/> to the caller.
		/// </remarks>
		public string Text
		{
			get { return m_text; }
			set { m_text = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="TerminalType"/> that identifies the type of this <see cref="Terminal"/>.
		/// </summary>
		/// <remarks>
		/// This property is set automatically set by <see cref="TerminalType.CreateTerminal"/>.
		/// </remarks>
		public new TerminalType ElementType
		{
			get { return (TerminalType)base.ElementType; }
			set { base.ElementType = value; }
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format(@"[{0} ""{1}""]", ElementType, Text);
		}

		#endregion Public Methods
	}
}