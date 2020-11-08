/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Identifies the types of language elements that compose a grammar: terminals and nonterminals.
	/// </summary>
	/// <remarks>
	/// Used to identify subclasses of the abstract <see cref="LanguageElementType" /> class.  Allows the type of <c>LanguageElementType</c> to be determined without
	/// the need for reflection.
	/// </remarks>
	public enum LanguageElementTypes
	{
		/// <summary>
		/// Indicates a terminal language element.
		/// </summary>
		Terminal,

		/// <summary>
		/// Indicates a nonterminal language element.
		/// </summary>
		Nonterminal
	}
}