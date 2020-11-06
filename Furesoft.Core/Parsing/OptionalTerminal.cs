namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines a <see cref="Terminal"/> that may optionally appear in the RHS side of a rule.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="Terminal"/>.</typeparam>
	/// <remarks>
	/// <see cref="OptionalTerminal{T}"/> is provided as a convienence when specifying rules containing optional
	/// terminals.  When a rule of the following form is specified in grammar:
	/// <code>
	/// X → A OptionalTerminal&lt;b&gt; C;
	/// </code>
	/// the following additional rules are automatically added:
	/// <code>
	/// OptionalNonterminal&lt;b&gt; → b;
	/// OptionalNonterminal&lt;b&gt; → ;
	/// </code>
	/// The <see cref="Terminal"/>, if it appears, can be accessed through the <see cref="Rhs"/> property.
	/// </remarks>
	public class OptionalTerminal<T> : Nonterminal
		where T : Terminal
	{
		#region Fields

		private T m_rhs;

		#endregion Fields

		#region Rules

		/// <summary>
		/// The rule that indicates the <see cref="Terminal"/> appears in the input text.
		/// </summary>
		/// <param name="lhs">The <see cref="OptionalTerminal{T}"/> represeting the LHS of the rule.</param>
		/// <param name="rhs">The <see cref="Terminal"/> representing the RHS of the rule.</param>
		public static void Rule(OptionalTerminal<T> lhs, T rhs)
		{
			lhs.Rhs = rhs;
		}

		/// <summary>
		/// The rule that indicates the <see cref="Terminal"/> does not appear in the input text.
		/// </summary>
		/// <param name="lhs">The <see cref="OptionalTerminal{T}"/> represeting the LHS of the rule.</param>
		public static void Rule(OptionalTerminal<T> lhs)
		{ }

		#endregion Rules

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="Terminal"/> that appears in the input text; <value>null</value> when <see cref="Terminal"/> does not
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