using System;

namespace Furesoft.Core.ObjectDB.Tool.Wrappers;

	internal static class OdbTime
	{
		internal static long GetCurrentTimeInTicks()
		{
			return DateTime.Now.Ticks;
		}

		internal static long GetCurrentTimeInMs()
		{
			return (long)TimeSpan.FromTicks(GetCurrentTimeInTicks()).TotalMilliseconds;
		}
	}