/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Specifies the attribute of a method associated with a <see cref="RuleType" />.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class RuleAttribute : Attribute
	{
		#region Fields

		private int m_priority;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleAttribute"/> class.
		/// </summary>
		public RuleAttribute()
		{
			m_priority = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleAttribute"/> class.
		/// </summary>
		/// <param name="priority">A value that indicates the priority of the rule relative to other rules.  Rules assigned
		/// a higher priority value are given precidence during conflict resolution.</param>
		public RuleAttribute(int priority)
		{
			m_priority = priority;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets a value that indicates the priority of the rule relative to other rules.  Rules assigned
		/// a higher priority value are given precidence during conflict resolution.
		/// </summary>
		public int Priority
		{
			get { return m_priority; }
			set { m_priority = value; }
		}

		#endregion Public Properties
	}
}