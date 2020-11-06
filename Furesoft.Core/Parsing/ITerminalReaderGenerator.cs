/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines the contract implemented by all Lingua.NET terminal reader generators.
	/// </summary>
	public interface ITerminalReaderGenerator
	{
		#region Methods

		/// <summary>
		/// Constructs a new <see cref="ITerminalReader"/> which can recoganize the specified <see cref="IGrammar"/>.
		/// </summary>
		/// <param name="grammar">The <see cref="IGrammar"/> to be recognized by the <see cref="ITerminalReader"/>.</param>
		/// <returns>A <see cref="TerminalReaderGeneratorResult"/> containing <see cref="ITerminalReader"/> and information pertaining to the
		/// success or failure of the generation process.
		/// </returns>
		TerminalReaderGeneratorResult GenerateTerminalReader(IGrammar grammar);

		#endregion Methods
	}
}