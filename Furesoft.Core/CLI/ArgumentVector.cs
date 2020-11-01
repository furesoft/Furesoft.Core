using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

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
			foreach (var key in _values.Keys)
			{
				if (Regex.IsMatch(key, name))
				{
					return true;
				}
			}

			return false;
		}

		public bool GetOption(string shortName, string longName)
		{
			return GetOption($"-{shortName}|--{longName}");
		}
	}
}