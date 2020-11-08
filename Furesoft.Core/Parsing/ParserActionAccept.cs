/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines a <see cref="ParserAction" /> that causes an <see cref="IParser" /> to accept the text being parsed.
	/// </summary>
	public class ParserActionAccept : ParserAction
	{
		#region Fields

		private RuleType m_rule;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserActionAccept"/> class.
		/// </summary>
		/// <param name="rule">The <see cref="RuleType"/> associated with this action.  The <see cref="NonterminalType.IsStart"/> property of <see cref="RuleType.Lhs"/> should
		/// be <value>true</value>.</param>
		public ParserActionAccept(RuleType rule)
			: base(ParserActionTypes.Accept)
		{
			m_rule = rule;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="RuleType"/> associated with this action.  The <see cref="NonterminalType.IsStart"/> property of <see cref="RuleType.Lhs"/> should
		/// be <value>true</value>.
		/// </summary>
		public RuleType Rule
		{
			get { return m_rule; }
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format("[Accept {0}]", Rule);
		}

		#endregion Public Methods
	}
}