namespace Creek.Extensibility.Addins
{
	using System;

	internal class AddinException : Exception
	{
		#region Constructors and Destructors

		public AddinException(string msg)
			: base(msg)
		{
		}

		#endregion Constructors and Destructors
	}
}