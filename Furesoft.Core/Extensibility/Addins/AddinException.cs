namespace Furesoft.Core.Extensibility.Addins
{
	using System;

	internal class AddinException : Exception
	{
		public AddinException(string msg)
			: base(msg)
		{
		}
	}
}