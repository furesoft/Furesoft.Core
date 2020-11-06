/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Defines a <see cref="LanguageElementType" /> that represents a type of a nonterminal identified by an <see cref="IGrammar" />.
	/// </summary>
	/// <remarks>
	/// A <see cref="NonterminalType"/> is a factory object that can construct the associated <see cref="Nonterminal"/> object using
	/// <see cref="CreateNonterminal"/>.
	/// </remarks>
	public class NonterminalType : LanguageElementType
	{
		#region Fields

		private delegate Nonterminal Constructor();

		private string m_fullName;
		private string m_name;
		private Constructor m_constructor;
		private bool m_isStart;

		private List<RuleType> m_rules = new List<RuleType>();
		private FollowSet m_follow = new FollowSet();

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NonterminalType"/> class.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the <see cref="Nonterminal"/> described by this <see cref="NonterminalType"/>.</param>
		public NonterminalType(Type type)
			: base(LanguageElementTypes.Nonterminal)
		{
			if (!type.IsSubclassOf(typeof(Nonterminal)))
			{
				throw new ArgumentException("Type must be a subclass of Nonterminal.", "type");
			}

			m_fullName = type.AssemblyQualifiedName;
			m_name = GetName(type);
			m_constructor = delegate () { return (Nonterminal)Activator.CreateInstance(type); };

			object[] attributes = type.GetCustomAttributes(typeof(NonterminalAttribute), false);
			foreach (object attribute in attributes)
			{
				NonterminalAttribute nonterminalAttribute = (NonterminalAttribute)attribute;
				if (nonterminalAttribute.IsStart)
				{
					m_isStart = true;
				}
			}
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the full name of the <see cref="NonterminalType"/>.  This includes the name of the <see cref="Assembly"/> containing
		/// the <see cref="Type"/> used to construct the <see cref="NonterminalType"/> and will always uniquely identify the <see cref="NonterminalType"/>.
		/// </summary>
		public override string FullName
		{
			get { return m_fullName; }
		}

		/// <summary>
		/// Gets the abbreviated name of the <see cref="NonterminalType"/>.  This excludes the name of the <see cref="Assembly"/> containing the
		/// <see cref="Type"/> used to construct the <see cref="NonterminalType"/> and may not uniquely identify the <see cref="NonterminalType"/>.
		/// </summary>
		public override string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets a value that indicates if this <see cref="Nonterminal"/> is the starting nonterminal of the <see cref="IGrammar"/>.
		/// </summary>
		public bool IsStart
		{
			get { return m_isStart; }
		}

		/// <summary>
		/// Gets a collection of <see cref="RuleType"/> objects describing the rules for which this <see cref="NonterminalType"/> is
		/// <see cref="RuleType.Lhs"/>.
		/// </summary>
		public List<RuleType> Rules
		{
			get { return m_rules; }
		}

		/// <summary>
		/// Gets the set of <see cref="NonterminalType"/> objects that appear in the the FOLLOW set of the <see cref="TerminalType"/>.
		/// </summary>
		public FollowSet Follow
		{
			get { return m_follow; }
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Constructs an instance of a <see cref="Nonterminal"/> described by this <see cref="NonterminalType"/>.
		/// </summary>
		/// <returns>A new <see cref="Nonterminal"/>.</returns>
		public Nonterminal CreateNonterminal()
		{
			Nonterminal result = m_constructor();
			result.ElementType = this;
			return result;
		}

		#endregion Public Methods
	}
}