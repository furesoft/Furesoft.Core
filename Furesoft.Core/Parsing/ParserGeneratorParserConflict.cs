/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Identifies a parsing rule conflict identified when generating an <see cref="IParser" />
	/// </summary>
	public class ParserGeneratorParserConflict
	{
		#region Fields

		private string m_rule;
		private string m_conflictingRule;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserGeneratorParserConflict"/> class.
		/// </summary>
		/// <param name="rule">The rule selected by the <see cref="IParserGenerator"/>.</param>
		/// <param name="conflictingRule">The rule conflicting with the rule selected by the <see cref="IParserGenerator"/>.</param>
		public ParserGeneratorParserConflict(string rule, string conflictingRule)
		{
			m_rule = rule;
			m_conflictingRule = conflictingRule;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the rule selected by the <see cref="IParserGenerator"/>.
		/// </summary>
		public string Rule
		{
			get
			{
				return m_rule;
			}
		}

		/// <summary>
		/// Gets the rule conflicting with the rule selected by the <see cref="IParserGenerator"/>.
		/// </summary>
		public string ConflictingRule
		{
			get
			{
				return m_conflictingRule;
			}
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format("[Conflict {0}, {1}]", Rule, ConflictingRule);
		}

		#endregion Public Methods
	}
}