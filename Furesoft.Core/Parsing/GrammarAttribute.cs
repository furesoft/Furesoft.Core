/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

using System;

namespace Creek.Parsing.Generator
{
	/// <summary>
	/// Specifies the grammar attribute of a <see cref="Terminal" /> or <see cref="Nonterminal" />-derived class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class GrammarAttribute : Attribute
	{
		#region Fields

		private string m_name;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GrammarAttribute"/> class.
		/// </summary>
		public GrammarAttribute()
		{ }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets a value that specifies the name of the Grammar the <see cref="Terminal" /> or <see cref="Nonterminal" /> belongs to.
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		#endregion Public Properties
	}
}