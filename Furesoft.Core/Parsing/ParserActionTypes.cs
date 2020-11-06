/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Identifies a parser action.
	/// </summary>
	/// <remarks>
	/// Used to identify subclasses of the abstract <see cref="ParserAction" /> class.  Allows the type of <c>ParserAction</c> to be determined without
	/// the need for reflection.
	/// </remarks>
	public enum ParserActionTypes
	{
		/// <summary>
		/// Indicates the shift action.
		/// </summary>
		Shift,

		/// <summary>
		/// Indicates the reduce action.
		/// </summary>
		Reduce,

		/// <summary>
		/// Indicates the accept action.
		/// </summary>
		Accept
	}
}