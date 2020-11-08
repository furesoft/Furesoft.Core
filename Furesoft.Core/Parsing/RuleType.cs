/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Reflection;
using System.Text;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Represents a rule or production identified by an <see cref="IGrammar" />.
	/// </summary>
	public class RuleType : IComparable<RuleType>
	{
		#region Fields

		private delegate void Invoker(LanguageElement[] parameters);

		private string m_fullName;
		private string m_name;
		private Invoker m_delegate;

		private int m_priority;
		private NonterminalType m_lhs;
		private LanguageElementType[] m_rhs;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleType"/> class.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> associated with this <see cref="RuleType"/>.</param>
		/// <param name="priority">The priority of this rule used to resolve conflicts with other rules during parser generation.</param>
		/// <param name="lhs">The <see cref="NonterminalType"/> representing the LHS of this rule.</param>
		/// <param name="rhs">The collection of <see cref="LanguageElementType"/> representing the RHS of this rule.</param>
		public RuleType(MethodInfo methodInfo, int priority, NonterminalType lhs, LanguageElementType[] rhs)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(methodInfo.Name);
			sb.Append("[");
			string prefix = null;
			foreach (ParameterInfo parameter in methodInfo.GetParameters())
			{
				sb.Append(prefix); prefix = ",";
				sb.Append(parameter.ParameterType.Name);
			}
			sb.Append("]");
			string name = sb.ToString();

			m_fullName = string.Format("{0}::{1}", methodInfo.DeclaringType.FullName, name);
			m_name = name;
			m_delegate = delegate (LanguageElement[] parameters) { methodInfo.Invoke(null, parameters); };

			m_priority = priority;
			m_lhs = lhs;
			m_rhs = rhs;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the full name of the <see cref="RuleType"/>.  This includes the name of the <see cref="Assembly"/> containing
		/// the <see cref="MethodInfo"/> used to construct the <see cref="RuleType"/> and will always uniquely identify the <see cref="RuleType"/>.
		/// </summary>
		public string FullName
		{
			get { return m_fullName; }
		}

		/// <summary>
		/// Gets the abbreviated name of the <see cref="RuleType"/>.  This excludes the name of the <see cref="Assembly"/> containing the
		/// <see cref="MethodInfo"/> used to construct the <see cref="RuleType"/> and may not uniquely identify the <see cref="RuleType"/>.
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets the priority of this rule used to resolve conflicts with other rules during parser generation.
		/// </summary>
		public int Priority
		{
			get { return m_priority; }
		}

		/// <summary>
		/// Gets the <see cref="NonterminalType"/> representing the LHS of this rule.
		/// </summary>
		public NonterminalType Lhs
		{
			get { return m_lhs; }
		}

		/// <summary>
		/// Gets the collection of <see cref="LanguageElementType"/> representing the RHS of this rule.
		/// </summary>
		public LanguageElementType[] Rhs
		{
			get { return m_rhs; }
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="RuleType"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="RuleType"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Lhs.Name);

			sb.Append(" ::=");

			foreach (LanguageElementType item in Rhs)
			{
				sb.Append(" ");
				sb.Append(item.Name);
			}

			sb.Append(";");

			return sb.ToString();
		}

		/// <summary>
		/// Calls the method associated with this <see cref="RuleType"/>.
		/// </summary>
		/// <param name="parameters">The <see cref="LanguageElement"/> objects to be passed to the method.  The first object
		/// represents the LHS of the rule.  The remaining objects represent the RHS of the rule.</param>
		public void Invoke(LanguageElement[] parameters)
		{
			m_delegate.DynamicInvoke(new object[] { parameters });
		}

		#endregion Public Methods

		#region IComparable<RuleType> Members

		/// <summary>
		/// Compares this instance to a specified <see cref="RuleType"/> object.
		/// </summary>
		/// <param name="other">A <see cref="RuleType"/> object</param>
		/// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
		/// <remarks>
		/// <see cref="RuleType"/> objects are compared based on <see cref="RuleType.Priority"/>.
		/// </remarks>
		public int CompareTo(RuleType other)
		{
			return Priority.CompareTo(other.Priority);
		}

		#endregion IComparable<RuleType> Members
	}
}