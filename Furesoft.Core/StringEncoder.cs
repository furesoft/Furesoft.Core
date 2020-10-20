using System;
using System.Text;

namespace Furesoft.Core
{
	public static class StringEncoder
	{
		public static string Encode(params int[] values)
		{
			var sb = new StringBuilder();

			//convert values to string
			foreach (var v in values)
			{
				sb.Append(GetLetter(v)).Append("-");
			}

			string result = sb.ToString();

			//build checksum and append to stringbuilder
			var checksum = (result.Length - values.Length) ^ 3;
			var checksumConverted = GetLetter(checksum);
			sb.Append(checksumConverted);

			return sb.ToString();
		}

		public static bool Validate(string code)
		{
			var spl = code.Split('-', StringSplitOptions.RemoveEmptyEntries);
			if (spl.Length == 0) return false;
			if (spl.Length == 1) return false;

			var checksum = (code.Length - spl.Length) ^ 3;
			var checksumLetter = GetLetter(checksum);

			if (spl[^1] != checksumLetter) return false;

			return true;
		}

		public static int[] Decode(string code)
		{
			var spl = code.Split('-', StringSplitOptions.RemoveEmptyEntries);
			var buffer = new int[spl.Length - 1];

			for (int i = 0; i < spl.Length - 1; i++)
			{
				buffer[i] = int.Parse(GetNumberString(spl[i]));
			}

			var checksum = (code.Length - spl.Length) ^ 3;
			var checksumLetter = GetLetter(checksum);

			if (spl[^1] == checksumLetter)
			{
				return buffer;
			}

			return null;
		}

		private static string GetNumberString(string v)
		{
			var sb = new StringBuilder();

			foreach (var l in v)
			{
				var index = alphabet.IndexOf(l);
				var digit = index.ToString();

				sb.Append(digit);
			}

			return sb.ToString();
		}

		private const string alphabet = "ACBZXJHFRTKNLMUV";

		private static string GetLetter(int v)
		{
			var str = v.ToString().ToCharArray();

			var sb = new StringBuilder();

			foreach (var d in str)
			{
				sb.Append(alphabet[d - '0']);
			}

			return sb.ToString();
		}
	}
}