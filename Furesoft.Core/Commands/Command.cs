using System.Collections.Generic;

namespace Furesoft.Core.Commands
{
	public class Command
	{
		public string Name { get; set; }
		public List<object> Args { get; set; } = new List<object>();

		public T GetArg<T>(int index)
		{
			if (Args.Count == 0) return default;
			return (T)Args[index];
		}
	}
}