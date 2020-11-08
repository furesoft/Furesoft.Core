/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines the contract implemented by all Lingua.NET parser generators.
	/// </summary>
	public interface IParserGenerator
	{
		#region Methods

		/// <summary>
		/// Constructs a new <see cref="IParser"/> which can recoganize the specified <see cref="IGrammar"/>.
		/// </summary>
		/// <param name="grammar">The <see cref="IGrammar"/> to be recognized by the <see cref="IParser"/>.</param>
		/// <returns>A <see cref="ParserGeneratorResult"/> containing <see cref="IParser"/> and information pertaining to the
		/// success or failure of the generation process.
		/// </returns>
		ParserGeneratorResult GenerateParser(IGrammar grammar);

		#endregion Methods
	}
}