/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// A concrete implementation of <see cref="IGrammar" />.  Uses refection to retrieve a grammar from the classes and methods defined by
	/// an <see cref="Assembly" /> or <see cref="Type" />.
	/// </summary>
	/// <remarks>
	/// To construct a grammar, the following methods should be used:
	/// <list type="bullet">
	/// <item>Call <see cref="Load(Assembly,string)" /> and <see cref="Load(Type,string)" /> one or more times to construct the <see cref="TerminalType" /> and <see cref="NonterminalType" /> objects
	/// defined by an <c>Assembly</c> or <c>Type</c>, respectively.</item>
	/// <item>Call <see cref="LoadRules(Assembly,string)" /> and <see cref="LoadRules(Type,string)" /> one or more times to construct the <see cref="RuleType" /> objects
	/// defined by an <c>Assembly</c> or <c>Type</c>, respectively.</item>
	/// <item>Call <see cref="Resolve" /> once after all calls to <c>Load</c> and <c>LoadRules</c> have been made.</item>
	/// </list>
	/// </remarks>
	public class Grammar : IGrammar
	{
		#region Fields

		private Dictionary<Type, TerminalType> m_terminals = new Dictionary<Type, TerminalType>();
		private Dictionary<Type, NonterminalType> m_nonterminals = new Dictionary<Type, NonterminalType>();

		#endregion Fields

		#region Public Methods

		/// <summary>
		/// Retrieves all terminal and nonterminal definitions from the specified <see cref="Assembly"/> and adds them to
		/// the <see cref="Grammar"/>.
		/// </summary>
		/// <param name="assembly">An <see cref="Assembly"/> containing terminal and nonterminal definitions.</param>
		/// <param name="name">The name of the grammar being loaded as defined by <see cref="GrammarAttribute"/>.  Specify <value>null</value> to load <paramref name="assembly"/> regardless of grammar name.</param>
		public void Load(Assembly assembly, string name)
		{
			foreach (Type type in assembly.GetTypes())
			{
				Load(type, name);
			}
		}

		/// <summary>
		/// Retrieves the terminal or nonterminal definition from the specified <see cref="Type"/> and adds it to the
		/// <see cref="Grammar"/>.
		/// </summary>
		/// <param name="type">A <see cref="Type"/> containing a terminal or nonterminal definition.</param>
		/// <param name="name">The name of the grammar being loaded as defined by <see cref="GrammarAttribute"/>.  Specify <value>null</value> to load <paramref name="type"/> regardless of grammar name.</param>
		/// <returns>The <see cref="LanguageElementType"/> added to the <see cref="Grammar"/>.  If the specified <see cref="Type"/>
		/// did not represent a terminal or nonterminal definition, <value>null</value> is returned.
		/// </returns>
		public LanguageElementType Load(Type type, string name)
		{
			if (TypeIsTerminal(type, name))
			{
				TerminalType terminalType = new TerminalType(type);
				m_terminals.Add(type, terminalType);
				return terminalType;
			}

			if (TypeIsNonterminal(type, name))
			{
				NonterminalType nonterminalType = new NonterminalType(type);
				m_nonterminals.Add(type, nonterminalType);
				return nonterminalType;
			}

			return null;
		}

		/// <summary>
		/// Retrieves all rule definitions from the specified <see cref="Assembly"/> and adds them to the <see cref="Grammar"/>.
		/// </summary>
		/// <param name="assembly">An <see cref="Assembly"/> containing rule definitions.</param>
		/// <param name="name">The name of the grammar being loaded as defined by <see cref="GrammarAttribute"/>.  Specify <value>null</value> to load <paramref name="assembly"/> regardless of grammar name.</param>
		public void LoadRules(Assembly assembly, string name)
		{
			foreach (Type type in assembly.GetTypes())
			{
				LoadRules(type, name);
			}
		}

		/// <summary>
		/// Retrieves all rule definitions from the specified <see cref="Type"/> and adds them to the <see cref="Grammar"/>.
		/// </summary>
		/// <param name="type">A <see cref="Type"/> containing rule definitions.</param>
		/// <param name="name">The name of the grammar being loaded as defined by <see cref="GrammarAttribute"/>.  Specify <value>null</value> to load <paramref name="type"/> regardless of grammar name.</param>
		public void LoadRules(Type type, string name)
		{
			// Ensure type is in specified grammar.
			//
			if (!IsGrammarType(type, name))
			{
				return;
			}

			// Load rules defined by type.
			//
			foreach (MethodInfo method in type.GetMethods())
			{
				ParameterInfo[] parameters = method.GetParameters();

				if (MethodIsRule(method, parameters))
				{
					// Determine priority associated with rule.
					//
					int priority = 0;
					object[] attributes = method.GetCustomAttributes(typeof(RuleAttribute), false);
					if (attributes.Length > 0)
					{
						RuleAttribute attribute = attributes[0] as RuleAttribute;
						if (attribute != null)
						{
							priority = attribute.Priority;
						}
					}

					// Find the grammer language elements associated with each parameter member.
					//
					NonterminalType lhs = null;
					List<LanguageElementType> rhs = new List<LanguageElementType>();
					for (int idx = 0; idx < parameters.Length; ++idx)
					{
						Type parameterType = parameters[idx].ParameterType;

						if (idx == 0)
						{
							NonterminalType nonterminal;
							if (m_nonterminals.TryGetValue(parameterType, out nonterminal))
							{
								lhs = nonterminal;
							}
							else
							{
								throw new InvalidOperationException();
							}
						}
						else
						{
							NonterminalType nonterminal;
							if (m_nonterminals.TryGetValue(parameterType, out nonterminal))
							{
								rhs.Add(nonterminal);
							}
							else
							{
								TerminalType terminal;
								if (m_terminals.TryGetValue(parameterType, out terminal))
								{
									rhs.Add(terminal);
								}
								else
								{
									// If parameterType is currently not defined by the grammar, attempt to process it
									// now.  This occurs when OptionalTerminal<> and OptionalNonterminal<> types
									// appear in the RHS side of a rule.
									//
									LanguageElementType languageElementType = Load(parameterType, name);
									if (languageElementType != null)
									{
										rhs.Add(languageElementType);
										LoadRules(parameterType, name);
									}
									else
									{
										throw new InvalidOperationException("Unrecognized rule argument.");
									}
								}
							}
						}
					}

					// Create the rule.
					//
					RuleType rule = new RuleType(method, priority, lhs, rhs.ToArray());
					lhs.Rules.Add(rule);
				}
			}
		}

		/// <summary>
		/// Prepares the <see cref="Grammar"/> for use after all <see cref="Assembly"/> and <see cref="Type"/> objects have been
		/// processed.
		/// </summary>
		public void Resolve()
		{
			if (StopTerminal == null)
			{
				Load(typeof(TerminalStop), null);
			}

			foreach (TerminalType terminal in m_terminals.Values)
			{
				terminal.First.Clear();
			}
			foreach (NonterminalType nonterminal in m_nonterminals.Values)
			{
				nonterminal.First.Clear();
				nonterminal.Follow.Clear();
			}

			ComputeFirst();
			ComputeFollow();
		}

		#endregion Public Methods

		#region IGrammar Members

		/// <summary>
		/// Gets the <see cref="TerminalType" /> representing the stop terminal defined by the <see cref="Grammar" />.
		/// </summary>
		public TerminalType StopTerminal
		{
			get
			{
				foreach (TerminalType terminal in m_terminals.Values)
				{
					if (terminal.IsStop) return terminal;
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the <see cref="NonterminalType" /> representing the starting nonterminal defined by the <see cref="Grammar" />.
		/// </summary>
		public NonterminalType StartNonterminal
		{
			get
			{
				foreach (NonterminalType nonterminal in m_nonterminals.Values)
				{
					if (nonterminal.IsStart) return nonterminal;
				}

				throw new InvalidOperationException("Starting nonterminal not defined by grammar.");
			}
		}

		/// <summary>
		/// Gets all terminals defined by the <see cref="Grammar" />.
		/// </summary>
		/// <returns>An array of <see cref="TerminalType" /> that represents all terminals defined by the <see cref="Grammar" />.</returns>
		public TerminalType[] GetTerminals()
		{
			TerminalType[] terminals = new TerminalType[m_terminals.Values.Count];
			m_terminals.Values.CopyTo(terminals, 0);
			return terminals;
		}

		/// <summary>
		/// Gets all nonterminals defined by the <see cref="Grammar" />.
		/// </summary>
		/// <returns>An array of <see cref="NonterminalType" /> that represents all nonterminals defined by the <see cref="Grammar" />.</returns>
		public NonterminalType[] GetNonterminals()
		{
			NonterminalType[] nonterminals = new NonterminalType[m_nonterminals.Values.Count];
			m_nonterminals.Values.CopyTo(nonterminals, 0);
			return nonterminals;
		}

		#endregion IGrammar Members

		#region Hidden Members

		private void ComputeFirst()
		{
			// All terminals contains themselves in FIRST.
			//
			foreach (TerminalType terminalType in m_terminals.Values)
			{
				terminalType.First.Add(terminalType);
			}

			// For nonterminals, continue adding elements until no more changes to FIRST are made.
			//
			bool change = true;
			while (change)
			{
				change = false;

				// Iterate over all nonterminals and their rules.
				//
				foreach (NonterminalType x in m_nonterminals.Values)
				{
					foreach (RuleType rule in x.Rules)
					{
						if (rule.Rhs.Length == 0)
						{
							// If the rule X -> e exists, add e to FIRST(X).
							//
							if (!x.First.Contains(null))
							{
								x.First.Add(null);
								change = true;
							}
						}
						else
						{
							// For rules of the form X -> Y1 Y2 Y3 ... Yn, add FIRST(Yi) to FIRST(X)
							// when e is in FIRST(Yj) for j < i.  Add e to FIRST(X) if e is in
							// FIRST(Yi) for all i.
							//
							bool epsilonInAllY = true;

							foreach (LanguageElementType y in rule.Rhs)
							{
								HashSet<TerminalType> yFirst = y.First.GetSetExcludingEpsilon();
								if (yFirst.Count > 0
									&& !yFirst.IsSubsetOf(x.First))
								{
									x.First.UnionWith(yFirst);
									change = true;
								}

								if (!y.First.ContainsEpsilon)
								{
									epsilonInAllY = false;
									break;
								}
							}

							if (epsilonInAllY
								&& !x.First.ContainsEpsilon)
							{
								x.First.Add(null);
								change = true;
							}
						}
					}
				}
			}
		}

		private void ComputeFollow()
		{
			StartNonterminal.Follow.Add(StopTerminal);

			// Continue adding elements until no more changes to FOLLOW are made.
			//
			bool change = true;
			while (change)
			{
				change = false;

				// Iterate over all nonterminals and their rules.
				//
				foreach (NonterminalType x in m_nonterminals.Values)
				{
					foreach (RuleType rule in x.Rules)
					{
						// For rules of the form X -> ... Y B1 B2 ... Bn, add FIRST(Bi) (excluding e)
						// to FOLLOW(Y) if FIRST(Bj) contains e for all j < i.  Add FOLLOW(X) to
						// FOLLOW(Y) if FIRST(Bi) contains e for all i.
						//
						for (int idx = 0; idx < rule.Rhs.Length; ++idx)
						{
							if (rule.Rhs[idx].ElementType == LanguageElementTypes.Nonterminal)
							{
								NonterminalType y = (NonterminalType)rule.Rhs[idx];

								bool epsilonInAllB = true;
								for (int j = idx + 1; j < rule.Rhs.Length; ++j)
								{
									LanguageElementType b = rule.Rhs[j];

									HashSet<TerminalType> bFirst = b.First.GetSetExcludingEpsilon();
									if (bFirst.Count > 0
										&& !bFirst.IsSubsetOf(y.Follow))
									{
										y.Follow.UnionWith(bFirst);
										change = true;
									}

									if (!b.First.ContainsEpsilon)
									{
										epsilonInAllB = false;
										break;
									}
								}

								if (epsilonInAllB)
								{
									if (x.Follow.Count > 0
										&& !x.Follow.IsSubsetOf(y.Follow))
									{
										y.Follow.UnionWith(x.Follow);
										change = true;
									}
								}
							}
						}
					}
				}
			}
		}

		private bool TypeIsTerminal(Type type, string name)
		{
			if (!type.IsSubclassOf(typeof(Terminal)))
			{
				return false;
			}

			return IsGrammarType(type, name);
		}

		private bool TypeIsNonterminal(Type type, string name)
		{
			if (!type.IsSubclassOf(typeof(Nonterminal)))
			{
				return false;
			}

			return IsGrammarType(type, name);
		}

		private static bool IsGrammarType(Type type, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}

			foreach (GrammarAttribute grammarAttribute in type.GetCustomAttributes(typeof(GrammarAttribute), true))
			{
				if (grammarAttribute.Name == name)
				{
					return true;
				}
			}

			return false;
		}

		private bool MethodIsRule(MethodInfo method, ParameterInfo[] parameters)
		{
			// A rule must be static.
			//
			if (!method.IsStatic) return false;

			// All rules require at least one parameter.
			//
			if (parameters.Length < 1) return false;

			// The first parameter must derive from Nonterminal.
			//
			if (!parameters[0].ParameterType.IsSubclassOf(typeof(Nonterminal))) return false;

			// Remaining parameters must derive from LanguageElement (i.e. Terminals or Nonterminals).
			//
			for (int idx = 1; idx < parameters.Length; ++idx)
			{
				if (!parameters[idx].ParameterType.IsSubclassOf(typeof(LanguageElement))) return false;
			}

			return true;
		}

		#endregion Hidden Members
	}
}