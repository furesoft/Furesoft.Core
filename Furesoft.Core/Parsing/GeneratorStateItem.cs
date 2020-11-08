/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines an element of a <see cref="GeneratorState" /> created by a <see cref="ParserGenerator" />.  Combines a <see cref="GeneratorRuleItem" /> with,
	/// optionally, the look-ahead terminals that support a transition to another <see cref="GeneratorState" />.
	/// </summary>
	/// <remarks>
	/// <see cref="GeneratorStateItem" /> implements value equality based on <see cref="RuleItem" />.
	/// </remarks>
	public class GeneratorStateItem : IEquatable<GeneratorStateItem>, IComparable<GeneratorStateItem>
	{
		#region Fields

		private GeneratorRuleItem m_ruleItem;
		private HashSet<TerminalType> m_lookaheads = new HashSet<TerminalType>();

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorStateItem"/> class.
		/// </summary>
		/// <param name="ruleItem">The <see cref="GeneratorRuleItem"/> representing the LR(0) item identified by this <see cref="GeneratorStateItem"/>.</param>
		public GeneratorStateItem(GeneratorRuleItem ruleItem)
		{
			m_ruleItem = ruleItem;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorStateItem"/> class.
		/// </summary>
		/// <param name="ruleItem">The <see cref="GeneratorRuleItem"/> representing the LR(0) item identified by this <see cref="GeneratorStateItem"/>.</param>
		/// <param name="lookaheads">A collection containing lookahead terminals associated with this <see cref="GeneratorStateItem"/>.</param>
		public GeneratorStateItem(GeneratorRuleItem ruleItem, IEnumerable<TerminalType> lookaheads)
		{
			m_ruleItem = ruleItem;
			m_lookaheads.UnionWith(lookaheads);
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="GeneratorRuleItem"/> representing the LR(0) item identified by this <see cref="GeneratorStateItem"/>.
		/// </summary>
		public GeneratorRuleItem RuleItem
		{
			get { return m_ruleItem; }
		}

		/// <summary>
		/// Gets the <see cref="HashSet{TerminalType}"/> containing the lookahead terminals associated with this <see cref="GeneratorStateItem"/>.
		/// </summary>
		/// <remarks>
		/// This property is not used when constructing SLR parsers.
		/// </remarks>
		public HashSet<TerminalType> Lookaheads
		{
			get { return m_lookaheads; }
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="GeneratorStateItem"/>.
		/// </summary>
		/// <returns>A <see cref="String"/> that represents the current <see cref="GeneratorStateItem"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(RuleItem.ToString());

			if (Lookaheads.Count > 0)
			{
				sb.Append("{");
				string prefix = null;
				foreach (TerminalType terminal in Lookaheads)
				{
					sb.Append(prefix); prefix = " ";
					sb.Append(terminal.Name);
				}
				sb.Append("}");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Determines whether this instance of <see cref="GeneratorStateItem"/> and a specified object, which must also be a <see cref="GeneratorStateItem"/> object, have the same value.
		/// </summary>
		/// <param name="obj">A <see cref="GeneratorStateItem"/>.</param>
		/// <returns><value>true</value> if <paramref name="obj"/> is a <see cref="GeneratorStateItem"/> and its value is the same as this instance; otherwise, <value>false</value>.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			GeneratorStateItem rhs = obj as GeneratorStateItem;
			if (rhs == null) return false;

			return (RuleItem == rhs.RuleItem)
				   && Lookaheads.SetEquals(rhs.Lookaheads);
		}

		/// <summary>
		/// Returns the hash code for this <see cref="GeneratorStateItem"/>.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks>
		/// We are disregarding <see cref="Lookaheads"/> when computing the hash code.
		/// </remarks>
		public override int GetHashCode()
		{
			return RuleItem.GetHashCode();
		}

		/// <summary>
		/// Compares two <see cref="GeneratorStateItem"/> objects. The result specifies whether the values of the <see cref="RuleItem"/> and <see cref="Lookaheads"/> properties of the two <see cref="GeneratorStateItem"/> objects are equal.
		/// </summary>
		/// <param name="lhs">A <see cref="GeneratorStateItem"/> to compare.</param>
		/// <param name="rhs">A <see cref="GeneratorStateItem"/> to compare.</param>
		/// <returns><value>true</value> if the <see cref="RuleItem"/> and <see cref="Lookaheads"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <value>false</value>.</returns>
		public static bool operator ==(GeneratorStateItem lhs, GeneratorStateItem rhs)
		{
			if (object.ReferenceEquals(lhs, rhs)) return true;

			if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null)) return false;

			return (lhs.RuleItem == rhs.RuleItem)
				   && lhs.Lookaheads.SetEquals(rhs.Lookaheads);
		}

		/// <summary>
		/// Compares two <see cref="GeneratorStateItem"/> objects. The result specifies whether the values of the <see cref="RuleItem"/> and <see cref="Lookaheads"/> properties of the two <see cref="GeneratorStateItem"/> objects are unequal.
		/// </summary>
		/// <param name="lhs">A <see cref="GeneratorStateItem"/> to compare.</param>
		/// <param name="rhs">A <see cref="GeneratorStateItem"/> to compare.</param>
		/// <returns><value>true</value> if the <see cref="RuleItem"/> and <see cref="Lookaheads"/> values of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <value>false</value>.</returns>
		public static bool operator !=(GeneratorStateItem lhs, GeneratorStateItem rhs)
		{
			return !(lhs == rhs);
		}

		#endregion Public Methods

		#region IEquatable<ParserStateElement> Members

		/// <summary>
		/// Determines whether this instance of <see cref="GeneratorStateItem"/> is equal to another instance of the same type.
		/// </summary>
		/// <param name="other">A <see cref="GeneratorStateItem"/>.</param>
		/// <returns><value>true</value> if the value of <paramref name="other"/> is the same as this instance; otherwise, <value>false</value>.</returns>
		public bool Equals(GeneratorStateItem other)
		{
			if (object.ReferenceEquals(other, null)) return false;

			return (RuleItem == other.RuleItem)
				   && Lookaheads.SetEquals(other.Lookaheads);
		}

		#endregion IEquatable<ParserStateElement> Members

		#region IComparable<GeneratorStateItem> Members

		/// <summary>
		/// Compares this instance to a specified <see cref="GeneratorStateItem"/> object.
		/// </summary>
		/// <param name="other">A <see cref="GeneratorStateItem"/> object</param>
		/// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
		/// <remarks>
		/// <see cref="GeneratorStateItem"/> objects are compared based on <see cref="GeneratorStateItem.RuleItem"/>.  See
		/// <see cref="GeneratorRuleItem.CompareTo"/> for more information.
		/// </remarks>
		public int CompareTo(GeneratorStateItem other)
		{
			return RuleItem.CompareTo(other.RuleItem);
		}

		#endregion IComparable<GeneratorStateItem> Members
	}
}