/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines the action taken during parsing for a specific <see cref="ParserState" /> and <see cref="ParserActionShift" />.
	/// </summary>
	/// <remarks>
	/// There are three subclasses of <c>ParserAction</c>: <see cref="ParserActionReduce" />, <see cref="ParserActionAccept" /> and <see cref="ActionType" />.
	/// <see cref="NonterminalType" /> identifies the specific subclass of a <c>ParserAction</c>.
	/// </remarks>
	public abstract class ParserAction
	{
		#region Fields

		private ParserActionTypes m_actionType;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserAction"/> class.
		/// </summary>
		/// <param name="actionType">A <see cref="ParserActionTypes"/> that identifies the subclass of this <see cref="ParserAction"/>.</param>
		public ParserAction(ParserActionTypes actionType)
		{
			m_actionType = actionType;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="ParserActionTypes"/> that identifies the subclass of this <see cref="ParserAction"/>.
		/// </summary>
		public ParserActionTypes ActionType
		{
			get { return m_actionType; }
		}

		#endregion Public Properties
	}
}