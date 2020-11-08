/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines an element of the <see cref="ParserStack" />.  Combines a <see cref="ParserState" /> with the <see cref="LanguageElement" /> recognized
	/// when the state was created.  <c>LanguageElement</c> is a <see cref="Terminal" /> if the <c>ParserState</c> was created as a result of a shift
	/// operation; <c>LanguageElement</c> is a <see cref="Nonterminal" /> if the <c>ParserState</c> was created as a result of a reduce operaton.
	/// </summary>
	public class ParserStackItem
	{
		#region Fields

		private LanguageElement m_languageElement;
		private ParserState m_state;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserStackItem"/> class.
		/// </summary>
		/// <param name="languageElement">The <see cref="LanguageElement"/> associated with this <see cref="ParserStackItem"/>.  When created by a shift action,
		/// contains the <see cref="Terminal"/> that triggered the action.  When created by a reduce action, contains a <see cref="Nonterminal"/>
		/// constructed by the <see cref="RuleType.Lhs"/> of the <see cref="RuleType"/> that triggered the action.</param>
		/// <param name="state">The <see cref="ParserState"/> that represents the current parsing state when the <see cref="ParserStackItem"/> is the topmost
		/// item on the <see cref="ParserStack"/>.</param>
		public ParserStackItem(LanguageElement languageElement, ParserState state)
		{
			m_languageElement = languageElement;
			m_state = state;
		}

		#endregion Constructors

		#region Fields

		/// <summary>
		/// Gets the <see cref="LanguageElement"/> associated with this <see cref="ParserStackItem"/>.  When created by a shift action,
		/// contains the <see cref="Terminal"/> that triggered the action.  When created by a reduce action, contains a <see cref="Nonterminal"/>
		/// constructed by the <see cref="RuleType.Lhs"/> of the <see cref="RuleType"/> that triggered the action.
		/// </summary>
		public LanguageElement LanguageElement
		{
			get { return m_languageElement; }
		}

		/// <summary>
		/// Gets the <see cref="ParserState"/> that represents the current parsing state when the <see cref="ParserStackItem"/> is the topmost
		/// item on the <see cref="ParserStack"/>.
		/// </summary>
		public ParserState State
		{
			get { return m_state; }
		}

		#endregion Fields
	}
}