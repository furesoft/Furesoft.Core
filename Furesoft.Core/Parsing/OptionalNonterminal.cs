namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines a <see cref="Nonterminal"/> that may optionally appear in the RHS side of a rule.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="Nonterminal"/>.</typeparam>
	/// <remarks>
	/// <see cref="OptionalNonterminal{T}"/> is provided as a convienence when specifying rules containing optional
	/// nonterminals.  When a rule of the following form is specified in grammar:
	/// <code>
	/// X → A OptionalNonterminal&lt;B&gt; C;
	/// </code>
	/// the following additional rules are automatically added:
	/// <code>
	/// OptionalNonterminal&lt;B&gt; → B;
	/// OptionalNonterminal&lt;B&gt; → ;
	/// </code>
	/// The <see cref="Nonterminal"/>, if it is used, can be accessed through the <see cref="Rhs"/> property.
	/// </remarks>
	public class OptionalNonterminal<T> : Nonterminal
		where T : Nonterminal
	{
		#region Fields

		private T m_rhs;

		#endregion Fields

		#region Rules

		/// <summary>
		/// The rule that indicates the <see cref="Nonterminal"/> appears in the input text.
		/// </summary>
		/// <param name="lhs">The <see cref="OptionalNonterminal{T}"/> represeting the LHS of the rule.</param>
		/// <param name="rhs">The <see cref="Nonterminal"/> representing the RHS of the rule.</param>
		public static void Rule(OptionalNonterminal<T> lhs, T rhs)
		{
			lhs.Rhs = rhs;
		}

		/// <summary>
		/// The rule that indicates <see cref="Nonterminal"/> does not appear in the input text.
		/// </summary>
		/// <param name="lhs">The <see cref="OptionalNonterminal{T}"/> represeting the LHS of the rule.</param>
		public static void Rule(OptionalNonterminal<T> lhs)
		{ }

		#endregion Rules

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="Nonterminal"/> that appears in the input text; <value>null</value> when <see cref="Nonterminal"/> does not
		/// appear in the input text.
		/// </summary>
		public T Rhs
		{
			get { return m_rhs; }
			private set { m_rhs = value; }
		}

		#endregion Public Properties
	}
}