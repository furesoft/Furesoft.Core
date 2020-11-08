/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Text.RegularExpressions;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Provides concrete implementation of <see cref="ITerminalReaderGenerator" />.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="TerminalReader" /> which in turn
	/// uses <see cref="Regex" /> to perform token recongnition.
	/// </remarks>
	public class TerminalReaderGenerator : ITerminalReaderGenerator
	{
		#region ITerminalReaderGenerator Members

		/// <summary>
		/// Constructs a new <see cref="TerminalReader"/> which can recoganize the specified <see cref="IGrammar"/>.
		/// </summary>
		/// <param name="grammar">The <see cref="IGrammar"/> to be recognized by the <see cref="TerminalReader"/>.</param>
		/// <returns>A <see cref="TerminalReaderGeneratorResult"/> containing <see cref="TerminalReader"/> and information pertaining to the
		/// success or failure of the generation process.
		/// </returns>
		public TerminalReaderGeneratorResult GenerateTerminalReader(IGrammar grammar)
		{
			ITerminalReader terminalReader = new TerminalReader(grammar.GetTerminals(), grammar.StopTerminal);

			TerminalReaderGeneratorResult result = new TerminalReaderGeneratorResult(terminalReader);
			return result;
		}

		#endregion ITerminalReaderGenerator Members
	}
}