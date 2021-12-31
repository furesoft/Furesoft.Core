using System;

namespace Furesoft.Core.Commands
{
	public static class CommandParser
	{
		public static Command Parse(string src)
		{
			if (string.IsNullOrEmpty(src)) return null;

			var result = new Command();

			var spl = src.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			result.Name = spl[0];

			if (spl.Length > 1)
			{
				for (var i = 1; i < spl.Length; i++)
				{
					if (int.TryParse(spl[i], out var iR))
					{
						result.Args.Add(iR);
					}
					else if (decimal.TryParse(spl[i], out var dR))
					{
						result.Args.Add(dR);
					}
					else if (bool.TryParse(spl[i], out var bR))
					{
						result.Args.Add(bR);
					}
					else
					{
						result.Args.Add(spl[i]);
					}
				}
			}

			return result;
		}
	}
}