/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Diagnostics;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// A concrete implementation of <see cref="IParser" /> created by <see cref="ParserGenerator" />.
	/// </summary>
	public class Parser : IParser
	{
		#region Fields

		private ParserState m_initialState;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Parser"/> class.
		/// </summary>
		/// <param name="initialState">The initial <see cref="ParserState"/>.</param>
		public Parser(ParserState initialState)
		{
			m_initialState = initialState;
		}

		#endregion Constructors

		#region IParser Members

		/// <summary>
		/// Performs syntax analysis against a sequence of terminals according to the <see cref="Grammar"/> used to create the <see cref="Parser"/>.
		/// </summary>
		/// <param name="terminalReader">Retrieves a sequence of <see cref="Terminal"/> objects.</param>
		/// <returns>If syntax analysis succeeds, returns the <see cref="Nonterminal"/> associated with <see cref="Grammar.StartNonterminal"/>.  Otherwise, <value>null</value> is returned.</returns>
		public Nonterminal Parse(ITerminalReader terminalReader)
		{
			ParserStack stack = new ParserStack();
			stack.Push(null, InitialState);

			Terminal terminal = terminalReader.ReadTerminal();
			while (terminal != null)
			{
				if (terminal.ElementType.Ignore)
				{
					terminal = terminalReader.ReadTerminal();
				}
				else
				{
					ParserAction action = stack.Peek().State.GetAction(terminal.ElementType);

					LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_PARSE_ACTION, "{0}", action);

					if (action == null)
					{
						return null;
					}

					switch (action.ActionType)
					{
						case ParserActionTypes.Accept:
							{
								ParserActionAccept reduce = (ParserActionAccept)action;
								RuleType rule = reduce.Rule;

								Nonterminal lhs = Reduce(stack, rule);

								return lhs;
							}

						case ParserActionTypes.Shift:
							{
								ParserActionShift shift = (ParserActionShift)action;

								stack.Push(terminal, shift.State);

								terminal = terminalReader.ReadTerminal();
							}
							break;

						case ParserActionTypes.Reduce:
							{
								ParserActionReduce reduce = (ParserActionReduce)action;
								RuleType rule = reduce.Rule;

								Nonterminal lhs = Reduce(stack, rule);

								// Push the LHS nonterminal on the stack.
								//
								stack.Push(lhs, stack.Peek().State.GetGoto(lhs.ElementType));
							}
							break;

						default:
							throw new InvalidOperationException(string.Format("Unrecognized action type {0}.", action.ActionType));
					}
				}
			};

			return null;
		}

		private static Nonterminal Reduce(ParserStack stack, RuleType rule)
		{
			// Create a language element array big enough to hold the LHS and RHS
			// arguments.
			//
			int parameterCount = 1 + rule.Rhs.Length;
			LanguageElement[] parameters = new LanguageElement[parameterCount];

			// Create the LHS nonterminal.
			//
			Nonterminal lhs = rule.Lhs.CreateNonterminal();
			parameters[0] = lhs;

			// Pop the RHS language elements off the stack.
			//
			for (int idx = 0; idx < rule.Rhs.Length; ++idx)
			{
				parameters[parameterCount - idx - 1] = stack.Pop().LanguageElement;
			}

			// Invoke the rule.
			//
			rule.Invoke(parameters);
			return lhs;
		}

		#endregion IParser Members

		#region Hidden Members

		private ParserState InitialState
		{
			get { return m_initialState; }
		}

		#endregion Hidden Members
	}
}