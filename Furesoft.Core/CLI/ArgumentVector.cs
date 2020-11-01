using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.CLI
{
	public class ArgumentVector
	{
		public ArgumentVector(Dictionary<string, object> values)
		{
			_values = values;
		}

		private Dictionary<string, object> _values;

		public T GetValue<T>(string name)
		{
			if (_values.ContainsKey(name))
			{
				return (T)_values[name];
			}

			return default;
		}

		public T GetValue<T>(int index)
		{
			return (T)_values.Values.ToArray()[index];
		}

		public bool GetOption(string name)
		{
			return _values.ContainsKey(name);
		}
	}
}