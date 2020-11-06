/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;
using System.Text;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Represents an LR(0) item identified by a <see cref="ParserGenerator" />.  Augments a <see cref="RuleType" /> with a position, or "dot", that represents the portion of the rule that has been
	/// recoganized by the parser.
	/// </summary>
	/// <remarks>
	/// <para><see cref="GeneratorRuleItem" /> implements value equality based on <see cref="Rule" /> and <see cref="Dot" />.</para>
	/// </remarks>
	public class GeneratorRuleItem : IEquatable<GeneratorRuleItem>, IComparable<GeneratorRuleItem>
	{
		#region Fields

		private RuleType m_rule;
		private int m_dot;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorRuleItem"/> class.
		/// </summary>
		/// <param name="rule">The rule associated with the LR(0) item defined by the <c>GeneratorRuleItem</c></param>
		/// <param name="dot">The position of the dot within the LR(0) item defined by the <c>GeneratorRuleItem</c></param>
		public GeneratorRuleItem(RuleType rule, int dot)
		{
			m_rule = rule;
			m_dot = dot;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="RuleType"/> associated with the LR(0) item.
		/// </summary>
		public RuleType Rule
		{
			get { return m_rule; }
		}

		/// <summary>
		/// Gets the position of the dot within the LR(0) item.
		/// </summary>
		/// <remarks>
		/// <see cref="Dot"/> is a zero-based index and identifies the <see cref="LanguageElement"/> in <see cref="RuleType.Rhs"/>
		/// that is preceeded by the dot.  When <see cref="Dot"/> equals <see cref="Array.Length"/>, the dot follows the last
		/// <see cref="LanguageElement"/> in the array.
		/// </remarks>
		public int Dot
		{
			get { return m_dot; }
		}

		/// <summary>
		/// Gets the <see cref="LanguageElementType"/> of the <see cref="LanguageElement"/> in <see cref="RuleType.Rhs"/> that
		/// immediately follows <see cref="Dot"/>.  If <see cref="Dot"/> follows the last <see cref="LanguageElement"/> in
		/// <see cref="RuleType.Rhs"/>, <see cref="DotElement"/> returns <value>null</value>.
		/// </summary>
		public LanguageElementType DotElement
		{
			get
			{
				if (Dot < Rule.Rhs.Length)
				{
					return Rule.Rhs[Dot];
				}

				return null;
			}
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="string"/> that represents the current <see cref="GeneratorRuleItem"/>.
		/// </summary>
		/// <returns>A <see cref="string"/> that represents the current <see cref="GeneratorRuleItem"/>.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append(Rule.Lhs.Name);

			sb.Append(" ::=");

			for (var idx = 0; idx < Rule.Rhs.Length; ++idx)
			{
				sb.Append(" ");
				if (idx == Dot)
				{
					sb.Append("_ ");
				}
				sb.Append(Rule.Rhs[idx].Name);
			}
			if (Dot == Rule.Rhs.Length)
			{
				sb.Append(" _");
			}

			sb.Append(";");

			return sb.ToString();
		}

		/// <summary>
		/// Determines whether this instance of <see cref="GeneratorRuleItem"/> and a specified object, which must also be a <see cref="GeneratorRuleItem"/> object, have the same value.
		/// </summary>
		/// <param name="obj">A <see cref="GeneratorRuleItem"/>.</param>
		/// <returns><value>true</value> if <paramref name="obj"/> is a <see cref="GeneratorRuleItem"/> and its value is the same as this instance; otherwise, <value>false</value>.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			var rhs = obj as GeneratorRuleItem;
			if (rhs == null) return false;

			return (Rule == rhs.Rule)
				   && (Dot == rhs.Dot);
		}

		/// <summary>
		/// Returns the hash code for this <see cref="GeneratorRuleItem"/>.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return Rule.GetHashCode()
				   ^ Dot.GetHashCode();
		}

		/// <summary>
		/// Compares two <see cref="GeneratorRuleItem"/> objects. The result specifies whether the values of the <see cref="Rule"/> and <see cref="Dot"/> properties of the two <see cref="GeneratorRuleItem"/> objects are equal.
		/// </summary>
		/// <param name="lhs">A <see cref="GeneratorRuleItem"/> to compare.</param>
		/// <param name="rhs">A <see cref="GeneratorRuleItem"/> to compare.</param>
		/// <returns><value>true</value> if the <see cref="Rule"/> and <see cref="Dot"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <value>false</value>.</returns>
		public static bool operator ==(GeneratorRuleItem lhs, GeneratorRuleItem rhs)
		{
			if (object.ReferenceEquals(lhs, rhs)) return true;

			if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null)) return false;

			return (lhs.Rule == rhs.Rule)
				   && (lhs.Dot == rhs.Dot);
		}

		/// <summary>
		/// Compares two <see cref="GeneratorRuleItem"/> objects. The result specifies whether the values of the <see cref="Rule"/> and <see cref="Dot"/> properties of the two <see cref="GeneratorRuleItem"/> objects are unequal.
		/// </summary>
		/// <param name="lhs">A <see cref="GeneratorRuleItem"/> to compare.</param>
		/// <param name="rhs">A <see cref="GeneratorRuleItem"/> to compare.</param>
		/// <returns><value>true</value> if the <see cref="Rule"/> and <see cref="Dot"/> values of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <value>false</value>.</returns>
		public static bool operator !=(GeneratorRuleItem lhs, GeneratorRuleItem rhs)
		{
			return !(lhs == rhs);
		}

		#endregion Public Methods

		#region IEquatable<ParserRuleItem> Members

		/// <summary>
		/// Determines whether this instance of <see cref="GeneratorRuleItem"/> is equal to another instance of the same type.
		/// </summary>
		/// <param name="other">A <see cref="GeneratorRuleItem"/>.</param>
		/// <returns><value>true</value> if the value of <paramref name="other"/> is the same as this instance; otherwise, <value>false</value>.</returns>
		public bool Equals(GeneratorRuleItem other)
		{
			if (object.ReferenceEquals(other, null)) return false;

			return (Rule == other.Rule)
				   && (Dot == other.Dot);
		}

		#endregion IEquatable<ParserRuleItem> Members

		#region IComparable<GeneratorRuleItem> Members

		/// <summary>
		/// Compares this instance to a specified <see cref="GeneratorRuleItem"/> object.
		/// </summary>
		/// <param name="other">A <see cref="GeneratorRuleItem"/> object</param>
		/// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
		/// <remarks>
		/// <see cref="GeneratorRuleItem"/> objects are compared based on <see cref="GeneratorRuleItem.Rule"/>.  See
		/// <see cref="RuleType.CompareTo"/> for more information.
		/// </remarks>
		public int CompareTo(GeneratorRuleItem other)
		{
			return Rule.CompareTo(other.Rule);
		}

		#endregion IComparable<GeneratorRuleItem> Members
	}
}