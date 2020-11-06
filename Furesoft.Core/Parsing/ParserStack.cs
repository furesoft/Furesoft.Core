/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Collections.Generic;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Maintains the <see cref="ParserState"/> objects used by a <see cref="Parser"/> to represent the current state of syntax anaylsis.
	/// </summary>
	/// <remarks>
	/// Elements are pushed on the <c>ParserState</c> as a result of <see cref="ParserActionShift"/> operations and removed as a result of
	/// <see cref="ParserActionReduce"/> operations.
	/// </remarks>
	public class ParserStack : Stack<ParserStackItem>
	{
		#region Public Methods

		/// <summary>
		/// Adds a new <see cref="ParserStackItem"/> to the <see cref="ParserStack"/>.
		/// </summary>
		/// <param name="languageElement">The <see cref="LanguageElement"/> associated with this <see cref="ParserStackItem"/>.  When created by a shift action,
		/// contains the <see cref="Terminal"/> that triggered the action.  When created by a reduce action, contains a <see cref="Nonterminal"/>
		/// constructed by the <see cref="RuleType.Lhs"/> of the <see cref="RuleType"/> that triggered the action.</param>
		/// <param name="state">The <see cref="ParserState"/> that represents the current parsing state when the <see cref="ParserStackItem"/> is the topmost
		/// item on the <see cref="ParserStack"/>.</param>
		public void Push(LanguageElement languageElement, ParserState state)
		{
			Push(new ParserStackItem(languageElement, state));
		}

		#endregion Public Methods
	}
}