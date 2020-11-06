/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines a <see cref="LanguageElement" /> that represents a nonterminal: the LHS of one or more rules defined by an <see cref="Nonterminal"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="ParserActionReduce"/> objects are created during the parsing process when a <see cref="NonterminalType"/> action occurs.
	/// The <see cref="RuleType.Lhs"/> associated with the <see cref="ParserActionRedRuleTypeParserActionReduceal.Rule"/> is used to construct a <see cref="RuleType.Invoke"/>
	/// representing the LHS of the rule being reduced.  After executing the rule using <see cref="ParserSRuleTypenew
	/// <see cref="Nonterminal"/> is created using the <see cref="ParserStack"/> and is pushed into the <see cref="IGrammar"/>.
	/// </remarks>
	public class Nonterminal : LanguageElement
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Nonterminal"/> class.
		/// </summary>
		public Nonterminal()
		{ }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the <see cref="NonterminalType"/> that identifies the type of this <see cref="Nonterminal"/>.
		/// </summary>
		/// <remarks>
		/// This property is set automatically set by <see cref="NonterminalType.CreateNonterminal"/>.
		/// </remarks>
		public new NonterminalType ElementType
		{
			get { return (NonterminalType)base.ElementType; }
			set { base.ElementType = value; }
		}

		#endregion Public Properties
	}
}