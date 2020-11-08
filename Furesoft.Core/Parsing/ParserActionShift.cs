/* Copyright (c) 2009 Richard G. Todd.
 * Licensed under the terms of the Microsoft Public License (Ms-PL).
 */

namespace Furesoft.Core.Parsing
{
	/// <summary>
	/// Defines a <see cref="ParserAction" /> that causes a <see cref="ParserStackItem" /> to push a new <see cref="ParserStack"/> onto a <see cref="ParserState"/>.
	/// </summary>
	/// <remarks>
	/// A shift occurs on terminal a when the <see cref="Parser"/> associated with the topmost <c>ParserStackItem</c> represents an LR(0) item of
	/// the form X→Y₁…○a…Yₓ and the parser determines that Y₁…a○…Yₓ potentially recognizes the input.
	/// </remarks>
	public class ParserActionShift : ParserAction
	{
		#region Fields

		private ParserState m_state;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserActionShift"/> class.
		/// </summary>
		/// <param name="state">The <see cref="ParserState"/> associated with this action.</param>
		/// <remarks>
		/// When this action is taken, a <see cref="ParserStackItem"/> containing this state
		/// will be added to the <see cref="ParserStack"/>.
		/// </remarks>
		public ParserActionShift(ParserState state)
			: base(ParserActionTypes.Shift)
		{
			m_state = state;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="ParserState"/> associated with this action.
		/// </summary>
		/// <remarks>
		/// When this action is taken, a <see cref="ParserStackItem"/> containing this state
		/// will be added to the <see cref="ParserStack"/>.
		/// </remarks>
		public ParserState State
		{
			get { return m_state; }
		}

		#endregion Public Properties

		#region Public Methods

		public override string ToString()
		{
			return string.Format("[Shift {0}]", State);
		}

		#endregion Public Methods
	}
}