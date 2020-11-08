using System.Globalization;

namespace Furesoft.Core.ObjectDB.Core.Query.Values
{
	internal static class ValuesUtil
	{
		internal static decimal Convert(decimal number)
		{
			return System.Convert.ToDecimal(number.ToString(CultureInfo.InvariantCulture));
		}
	}
}