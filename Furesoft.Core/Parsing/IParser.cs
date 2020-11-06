/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines the contract implemented by all Lingua.NET parsers.
	/// </summary>
	public interface IParser
	{
		#region Methods

		/// <summary>
		/// Performs syntax analysis against a sequence of terminals according to the <see cref="IGrammar"/> used to create the <see cref="IParser"/>.
		/// </summary>
		/// <param name="terminalReader">Retrieves a sequence of <see cref="Terminal"/> objects.</param>
		/// <returns>If syntax analysis succeeds, returns the <see cref="Nonterminal"/> associated with <see cref="IGrammar.StartNonterminal"/>.  Otherwise, <value>null</value> is returned.</returns>
		Nonterminal Parse(ITerminalReader terminalReader);

		#endregion Methods
	}
}