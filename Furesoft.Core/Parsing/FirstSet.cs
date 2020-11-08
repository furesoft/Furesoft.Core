/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Collections.Generic;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Contains the set of <see cref="TerminalType" /> objects representing terminals that can begin strings derived from a language element.
	/// If a nonterminal can be reduced to epsilon, the set will contain null.
	/// </summary>
	/// <remarks>
	/// The following rules are used to construct the FOLLOW set:
	/// <list type="bullet">
	/// <item>If <value>a</value> is a terminal then FIRST(<value>a</value>) contains <value>a</value>.</item>
	/// <item>If <value>X→aα</value> is a production then FIRST(<value>X</value>) contains <value>a</value>.</item>
	/// <item>If <value>X→ε</value> is a production then FIRST(<value>X</value>) contains <value>ε</value>.</item>
	/// <item>If <value>X→Y₁Y₂Y₃…Yᵢ…Yₓ</value> is a production and FIRST(<value>Yₐ</value>) contains <value>ε</value> for all <value>a</value> &lt; <value>i</value> then FIRST(<value>X</value>) contains all non-<value>ε</value> in FIRST(<value>Yᵢ</value>).</item>
	/// <item>If <value>X→Y₁Y₂Y₃…Yₓ</value> is a production and FIRST(<value>Yₐ</value>) contains <value>ε</value> for all <value>a</value> then FIRST(<value>X</value>) contains<value>ε</value>.</item>
	/// </list>
	/// Alfred V. Aho and Jeffrey D. Ullman, Principles of Compiler Design (Addison-Wesley Publishing Company, 1979) p. 188.
	/// </remarks>
	public class FirstSet : HashSet<TerminalType>
	{
		#region Public Properties

		/// <summary>
		/// Gets a value indicating if the set contains epsilon (null).
		/// </summary>
		public bool ContainsEpsilon
		{
			get { return Contains(null); }
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Gets a subset containing all members of the set excluding epsilon (null).
		/// </summary>
		/// <returns>A subset containing all members of the set excluding epsilon (null).</returns>
		public HashSet<TerminalType> GetSetExcludingEpsilon()
		{
			HashSet<TerminalType> result = new HashSet<TerminalType>(this);

			result.Remove(null);

			return result;
		}

		#endregion Public Methods
	}
}