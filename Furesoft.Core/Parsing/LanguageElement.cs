/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines an instance of a language element processed by a <see cref="Terminal" />.
	/// </summary>
	/// <remarks>
	/// There are two subclasses of <c>LanguageElement</c>: <see cref="Nonterminal" /> and <see cref="ElementType" />.
	/// <see cref="IParser" /> identifies the specific subclass of a <c>LanguageElement</c>.
	/// </remarks>
	public abstract class LanguageElement
	{
		#region Fields

		private LanguageElementType m_elementType;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageElement"/> class.
		/// </summary>
		public LanguageElement()
		{ }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the <see cref="LanguageElementType"/> that identifies the type of this <see cref="LanguageElement"/>.
		/// </summary>
		/// <remarks>
		/// This property is set automatically set by <see cref="NonterminalType.CreateNonterminal"/> and <see cref="TerminalType.CreateTerminal"/>.
		/// </remarks>
		public LanguageElementType ElementType
		{
			get { return m_elementType; }
			set { m_elementType = value; }
		}

		#endregion Public Properties
	}
}