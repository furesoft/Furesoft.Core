/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System.Collections.Generic;
using System.Diagnostics;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// A concrete implementation of <see cref="IParserGenerator" />.
	/// </summary>
	/// <remarks>
	/// Generates an SLR parser.  See <seealso href="http://en.wikipedia.org/wiki/Simple_LR_parser">Simple LR Parser</seealso> for more information.
	/// </remarks>
	public class ParserGenerator : IParserGenerator
	{
		#region IParserGenerator Members

		/// <summary>
		/// Constructs a new <see cref="Parser"/> which can recoganize the specified <see cref="IGrammar"/>.
		/// </summary>
		/// <param name="grammar">The <see cref="IGrammar"/> to be recognized by the <see cref="Parser"/>.</param>
		/// <returns>A <see cref="ParserGeneratorResult"/> containing <see cref="Parser"/> and information pertaining to the
		/// success or failure of the generation process.
		/// </returns>
		public ParserGeneratorResult GenerateParser(IGrammar grammar)
		{
			List<ParserGeneratorParserConflict> conflicts = new List<ParserGeneratorParserConflict>();

			List<GeneratorState> states = CreateStates(grammar);

			// Create a parser state for each generator state.
			//
			Dictionary<GeneratorState, ParserState> parserStates = new Dictionary<GeneratorState, ParserState>();
			foreach (GeneratorState state in states)
			{
				parserStates.Add(state, new ParserState(state.Id));
			}

			foreach (GeneratorState state in states)
			{
				LinguaTrace.TraceEvent(TraceEventType.Verbose, LinguaTraceId.ID_GENERATE_PROCESS_STATE, "{0}", state);

				List<GeneratorStateItem> items = new List<GeneratorStateItem>(state.Items);
				items.Sort();

				// Construct the list of actions associated with the parser state.
				//
				Dictionary<TerminalType, ParserAction> actions = new Dictionary<TerminalType, ParserAction>();
				Dictionary<ParserAction, GeneratorRuleItem> actionRules = new Dictionary<ParserAction, GeneratorRuleItem>();
				foreach (GeneratorStateItem item in items)
				{
					LinguaTrace.TraceEvent(TraceEventType.Verbose, LinguaTraceId.ID_GENERATE_PROCESS_ITEM, "{0}", item);

					if (item.RuleItem.DotElement == null)
					{
						foreach (TerminalType terminal in item.RuleItem.Rule.Lhs.Follow)
						{
							LinguaTrace.TraceEvent(TraceEventType.Verbose, LinguaTraceId.ID_GENERATE_PROCESS_TERMINAL, "{0}", terminal);

							if (actions.ContainsKey(terminal))
							{
								ParserGeneratorParserConflict conflict = new ParserGeneratorParserConflict(
									actionRules[actions[terminal]].ToString(),
									item.RuleItem.ToString());

								LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_PROCESS_CONFLICT, "{0}", conflict);

								conflicts.Add(conflict);
							}
							else if (item.RuleItem.Rule.Lhs.IsStart
									 && terminal.IsStop)
							{
								ParserAction action = new ParserActionAccept(item.RuleItem.Rule);

								LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_PROCESS_ACTION, "{0}", action);

								actions.Add(terminal, action);
								actionRules.Add(action, item.RuleItem);
							}
							else
							{
								ParserAction action = new ParserActionReduce(item.RuleItem.Rule);

								LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_PROCESS_ACTION, "{0}", action);

								actions.Add(terminal, action);
								actionRules.Add(action, item.RuleItem);
							}
						}
					}
					else if (item.RuleItem.DotElement.ElementType == LanguageElementTypes.Terminal)
					{
						TerminalType terminal = (TerminalType)item.RuleItem.DotElement;

						if (actions.ContainsKey(terminal))
						{
							ParserGeneratorParserConflict conflict = new ParserGeneratorParserConflict(
								actionRules[actions[terminal]].ToString(),
								item.RuleItem.ToString());

							LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_PROCESS_CONFLICT, "{0}", conflict);

							conflicts.Add(conflict);
						}
						else
						{
							ParserAction action = new ParserActionShift(parserStates[state.Transitions[terminal]]);

							LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_PROCESS_ACTION, "{0}", action);

							actions.Add(terminal, action);
							actionRules.Add(action, item.RuleItem);
						}
					}
				}

				// Construct the GOTO table
				//
				Dictionary<NonterminalType, ParserState> gotos = new Dictionary<NonterminalType, ParserState>();
				foreach (KeyValuePair<LanguageElementType, GeneratorState> transition in state.Transitions)
				{
					if (transition.Key.ElementType == LanguageElementTypes.Nonterminal)
					{
						NonterminalType nonterminal = (NonterminalType)transition.Key;
						gotos.Add(nonterminal, parserStates[transition.Value]);
					}
				}

				// Update the parser state.
				//
				ParserState parserState = parserStates[state];
				foreach (KeyValuePair<TerminalType, ParserAction> action in actions)
				{
					parserState.Actions.Add(action.Key, action.Value);
				}

				foreach (KeyValuePair<NonterminalType, ParserState> gotoItem in gotos)
				{
					parserState.Gotos.Add(gotoItem.Key, gotoItem.Value);
				}
			}

			Parser parser = new Parser(parserStates[states[0]]);

			ParserGeneratorResult result = new ParserGeneratorResult(parser, conflicts);
			return result;
		}

		#endregion IParserGenerator Members

		#region Hidden Members

		private List<GeneratorState> CreateStates(IGrammar grammar)
		{
			List<GeneratorState> states = new List<GeneratorState>();
			List<GeneratorState> unevaluatedStates = new List<GeneratorState>();
			int stateId = 0;

			// Compute start state.
			//
			{
				HashSet<GeneratorStateItem> items = new HashSet<GeneratorStateItem>();
				foreach (RuleType rule in grammar.StartNonterminal.Rules)
				{
					items.Add(new GeneratorStateItem(new GeneratorRuleItem(rule, 0)));
				}
				ComputeClosure(grammar, items);

				GeneratorState startState = new GeneratorState(stateId++, items);
				states.Add(startState);
				unevaluatedStates.Add(startState);
			}

			List<LanguageElementType> languageElements = new List<LanguageElementType>();
			languageElements.AddRange(grammar.GetTerminals());
			languageElements.AddRange(grammar.GetNonterminals());

			while (unevaluatedStates.Count > 0)
			{
				// Remove one of the evaluated states and process it.
				//
				GeneratorState state = unevaluatedStates[0];
				unevaluatedStates.RemoveAt(0);

				foreach (LanguageElementType languageElement in languageElements)
				{
					HashSet<GeneratorStateItem> items = state.Apply(languageElement);
					if (items != null)
					{
						ComputeClosure(grammar, items);

						GeneratorState toState = null;
						foreach (GeneratorState existingState in states)
						{
							if (existingState.Items.SetEquals(items))
							{
								toState = existingState;
								break;
							}
						}
						if (toState == null)
						{
							toState = new GeneratorState(stateId++, items);
							states.Add(toState);
							unevaluatedStates.Add(toState);
						}

						state.Transitions.Add(languageElement, toState);
					}
				}
			}

			if (LinguaTrace.TraceSource.Switch.ShouldTrace(TraceEventType.Information))
			{
				foreach (GeneratorState state in states)
				{
					LinguaTrace.TraceEvent(TraceEventType.Information, LinguaTraceId.ID_GENERATE_STATE, "{0}", state);
				}
			}

			return states;
		}

		private void ComputeClosure(IGrammar grammar, HashSet<GeneratorStateItem> items)
		{
			// Continue to loop until new more elements are added to the state.
			//
			bool stateModified = true;
			while (stateModified)
			{
				HashSet<GeneratorStateItem> newItems = new HashSet<GeneratorStateItem>();

				// Iterate over the current elements in the state and determine (possible) new
				// elements to be added.
				//
				foreach (GeneratorStateItem item in items)
				{
					LanguageElementType languageElement = item.RuleItem.DotElement;
					if (languageElement != null
						 && languageElement.ElementType == LanguageElementTypes.Nonterminal)
					{
						NonterminalType nonterminal = (NonterminalType)languageElement;

						foreach (RuleType rule in nonterminal.Rules)
						{
							GeneratorStateItem newItem = new GeneratorStateItem(new GeneratorRuleItem(rule, 0));
							newItems.Add(newItem);
						}
					}
				}

				// Exit loop if all potential new elements already exist in state.  Otherwise, add new elements
				// and repeat process.
				//
				if (newItems.IsSubsetOf(items))
				{
					stateModified = false;
				}
				else
				{
					items.UnionWith(newItems);
				}
			}
		}

		#endregion Hidden Members
	}
}