/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines the contract implemented by all Lingua.NET terminal readers.
	/// </summary>
	public interface ITerminalReader
	{
		#region Methods

		/// <summary>
		/// Prepares an <see cref="ITerminalReader"/> to read from the specified <see cref="String"/>.
		/// </summary>
		/// <param name="text">A <see cref="String"/> containing the input text to be processed by the <see cref="ITerminalReader"/>.</param>
		void Open(string text);

		/// <summary>
		/// Reads the next <see cref="Terminal"/>.
		/// </summary>
		/// <returns>The next <see cref="Terminal"/> in the input text.  If no <see cref="TerminalType"/> recognizes the input text, <value>null</value> is returned.</returns>
		Terminal ReadTerminal();

		/// <summary>
		/// Returns the next <see cref="Terminal"/> in the input text without advancing the current position of the <see cref="ITerminalReader"/>.
		/// </summary>
		/// <returns>The next <see cref="Terminal"/> in the input text.  If no <see cref="TerminalType"/> recognizes the input text, <value>null</value> is returned.</returns>
		Terminal PeekTerminal();

		/// <summary>
		/// Returns the specified <see cref="Terminal"/> to the input stream.
		/// </summary>
		/// <param name="terminal">A <see cref="Terminal"/> to be returned to the input stream.</param>
		void PushTerminal(Terminal terminal);

		#endregion Methods
	}
}