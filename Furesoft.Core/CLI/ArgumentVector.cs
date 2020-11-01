using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Furesoft.Core.CLI
{
	/// <summary>
	/// Class that represents a collection of key:value pairs
	/// </summary>
	public class ArgumentVector
	{
		public ArgumentVector(Dictionary<string, object> values)
		{
			_values = values;
		}

		private Dictionary<string, object> _values;

		/// <summary>
		/// Get the Value of the argument name
		/// </summary>
		/// <typeparam name="T">The Resulting Type</typeparam>
		/// <param name="name">The Argumentname</param>
		/// <returns></returns>
		public T GetValue<T>(string name)
		{
			if (_values.ContainsKey(name))
			{
				return (T)_values[name];
			}

			return default;
		}

		/// <summary>
		/// Get Value Based on Index
		/// </summary>
		/// <typeparam name="T">The ResultType</typeparam>
		/// <param name="index">The Index</param>
		public T GetValue<T>(int index)
		{
			return (T)_values.Values.ToArray()[index];
		}

		/// <summary>
		/// Get Option Flag
		/// </summary>
		/// <param name="name">The Name</param>
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

		/// <summary>
		/// Get Option Flag For A Shortname And Longname
		/// </summary>
		/// <param name="shortName">The Shortname to use</param>
		/// <param name="longName">the Longname to use</param>
		/// <example>GetOption("-Wall", "--warning-all")</example>
		public bool GetOption(string shortName, string longName)
		{
			return GetOption($"-{shortName}|--{longName}");
		}
	}
}