/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// A <see cref="Terminal" /> returned by an <see cref="ITerminalReader"/> after all input text has been processed.
	/// </summary>
	[Terminal(IsStop = true)]
	public class TerminalStop : Terminal
	{ }
}