/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Collections.Generic;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Contains the set of <see cref="TerminalType" /> objects representing terminals that follow a nonterminal in some sentential form.
	/// If the nonterminal can be the rightmost symbol in some sentential form, the set will contain the stop terminal.
	/// </summary>
	/// <remarks>
	/// The following rules are used to construct the FOLLOW set:
	/// <list type="bullet">
	/// <item><value>$</value> is in FOLLOW(<value>S</value>) where <value>S</value> is the start symbol.</item>
	/// <item>If there is a production <value>A→αBβ</value> and <value>β</value>≠<value>ε</value> then everything in FIRST(<value>β</value>) except <value>ε</value> is in FOLLOW(<value>B</value>).</item>
	/// <item>If there is a production <value>A→αB</value> or a production <value>A→αBβ</value> where FIRST(<value>β</value>) contains <value>ε</value> then everything in FOLLOW(<value>A</value>) is in FOLLOW(<value>B</value>).</item>
	/// </list>
	/// Alfred V. Aho and Jeffrey D. Ullman, Principles of Compiler Design (Addison-Wesley Publishing Company, 1979) p. 189.
	/// </remarks>
	public class FollowSet : HashSet<TerminalType>
	{ }
}