/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Collections.Generic;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines the possible actions that can be taken by a <see cref="Parser" /> at a particular point during syntax analysis.
	/// </summary>
	/// <remarks>
	/// Implicitly represents one or more LR(0) items: rules of the form <value>X→Y₁Y₂Y₃…Yₓ</value> augumented with a dot (<value>○</value>) to identify the portion of
	/// the rule already recognized.  For example, <value>E→E+○T</value> is an item that indicates <value>E+</value> has been recognized and the parser is prepared to
	/// recognize <value>T</value>.
	/// </remarks>
	public class ParserState
	{
		#region Fields

		private int m_id;

		private Dictionary<TerminalType, ParserAction> m_actions = new Dictionary<TerminalType, ParserAction>();
		private Dictionary<NonterminalType, ParserState> m_gotos = new Dictionary<NonterminalType, ParserState>();

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserState"/> class.
		/// </summary>
		/// <param name="id">A numeric value that uniquely identifies this <see cref="ParserState"/>.</param>
		public ParserState(int id)
		{
			m_id = id;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets a numeric value that uniquely identifies this <see cref="ParserState"/>.
		/// </summary>
		public int Id
		{
			get { return m_id; }
		}

		/// <summary>
		/// Maps <see cref="ParserAction"/> objects to the <see cref="TerminalType"/> objects that are allowed by this <see cref="ParserState"/>.
		/// </summary>
		public Dictionary<TerminalType, ParserAction> Actions
		{
			get { return m_actions; }
		}

		/// <summary>
		/// Maps <see cref="ParserState"/> objects to the <see cref="NonterminalType"/> objects representing the LHS of rules after a reduction occurs.
		/// </summary>
		public Dictionary<NonterminalType, ParserState> Gotos
		{
			get { return m_gotos; }
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format("[State {0}]", Id);
		}

		/// <summary>
		/// Gets the <see cref="ParserAction"/> associated with a <see cref="TerminalType"/> as defined by <see cref="Actions"/>.
		/// </summary>
		/// <param name="terminalType">A <see cref="TerminalType"/> object.</param>
		/// <returns>The <see cref="ParserAction"/> associated with <paramref name="terminalType"/> as defined by <see cref="Actions"/>.
		/// Returns <value>null</value> if no such <see cref="ParserAction"/> exists.</returns>
		public ParserAction GetAction(TerminalType terminalType)
		{
			ParserAction action;
			if (m_actions.TryGetValue(terminalType, out action))
			{
				return action;
			}
			return null;
		}

		/// <summary>
		/// Gets the <see cref="ParserState"/> associated with a <see cref="NonterminalType"/> as defined by <see cref="Gotos"/>.
		/// </summary>
		/// <param name="nonterminalType">A <see cref="NonterminalType"/> object.</param>
		/// <returns>The <see cref="ParserState"/> associated with <paramref name="nonterminalType"/> as defined by <see cref="Gotos"/>.
		/// Returns <value>null</value> if no such <see cref="ParserState"/> exists.</returns>
		public ParserState GetGoto(NonterminalType nonterminalType)
		{
			ParserState state;
			if (m_gotos.TryGetValue(nonterminalType, out state))
			{
				return state;
			}
			return null;
		}

		#endregion Public Methods
	}
}