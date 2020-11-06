/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines the contract implemented by all Lingua.NET grammars.
	/// </summary>
	public interface IGrammar
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="TerminalType" /> representing the stop terminal defined by the <see cref="Grammar" />.
		/// </summary>
		TerminalType StopTerminal { get; }

		/// <summary>
		/// Gets the <see cref="NonterminalType" /> representing the starting nonterminal defined by the <see cref="Grammar" />.
		/// </summary>
		NonterminalType StartNonterminal { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets all terminals defined by the <see cref="Grammar" />.
		/// </summary>
		/// <returns>An array of <see cref="TerminalType" /> that represents all terminals defined by the <see cref="Grammar" />.</returns>
		TerminalType[] GetTerminals();

		/// <summary>
		/// Gets all nonterminals defined by the <see cref="Grammar" />.
		/// </summary>
		/// <returns>An array of <see cref="NonterminalType" /> that represents all nonterminals defined by the <see cref="Grammar" />.</returns>
		NonterminalType[] GetNonterminals();

		#endregion Methods
	}
}