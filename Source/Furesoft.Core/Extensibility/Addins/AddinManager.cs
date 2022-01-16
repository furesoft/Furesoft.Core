using System.Collections.Generic;

namespace Furesoft.Core.Extensibility.Addins
{
	public class AddinManager
	{
		#region Static Fields

		public static AddinRegistry Registry = new();

		#endregion Static Fields

		#region Public Methods and Operators

		public static IEnumerable<ExtensionNode> GetExtensionObjects(string path)
		{
			foreach (var r in Registry)
			{
				foreach (var en in r.ExtensionNodes)
				{
					if (en._path == path)
					{
						yield return en;
					}
				}
			}
		}

		#endregion Public Methods and Operators
	}
}