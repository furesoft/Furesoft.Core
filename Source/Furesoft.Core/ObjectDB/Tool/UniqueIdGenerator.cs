using Furesoft.Core.ObjectDB.Tool.Wrappers;

namespace Furesoft.Core.ObjectDB.Tool;

	internal static class UniqueIdGenerator
	{
		internal static long GetRandomLongId()
		{
			lock (typeof(UniqueIdGenerator))
			{
				return (long)(OdbRandom.GetRandomDouble() * long.MaxValue);
			}
		}
	}