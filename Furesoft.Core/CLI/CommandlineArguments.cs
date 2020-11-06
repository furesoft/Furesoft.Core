using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Furesoft.Core.CLI
{
	/// <summary>
	/// Arguments class
	/// </summary>
	public class CommandlineArguments
	{
		// Variables
		private readonly StringDictionary Parameters;

		// Constructor
		public CommandlineArguments(IEnumerable<string> args)
		{
			Parameters = new StringDictionary();
			var spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			string parameter = null;
			string[] parts;

			// Valid parameters forms:
			// {-,/,--}param{ ,=,:}((",')value(",'))
			// Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
			foreach (var txt in args)
			{
				// Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
				parts = spliter.Split(txt, 3);
				switch (parts.Length)
				{
					// Found a value (for the last parameter found (space separator))
					case 1:
						if (parameter != null)
						{
							if (!Parameters.ContainsKey(parameter))
							{
								parts[0] = remover.Replace(parts[0], "$1");
								Parameters.Add(parameter, parts[0]);
							}
							parameter = null;
						}
						// else Error: no parameter waiting for a value (skipped)
						break;
					// Found just a parameter
					case 2:
						// The last parameter is still waiting. With no value, set it to true.
						if (parameter != null)
						{
							if (!Parameters.ContainsKey(parameter)) Parameters.Add(parameter, "true");
						}
						parameter = parts[1];
						break;
					// Parameter with enclosed value
					case 3:
						// The last parameter is still waiting. With no value, set it to true.
						if (parameter != null)
						{
							if (!Parameters.ContainsKey(parameter)) Parameters.Add(parameter, "true");
						}
						parameter = parts[1];
						// Remove possible enclosing characters (",')
						if (!Parameters.ContainsKey(parameter))
						{
							parts[2] = remover.Replace(parts[2], "$1");
							Parameters.Add(parameter, parts[2]);
						}
						parameter = null;
						break;
				}
			}
			// In case a parameter is still waiting
			if (parameter != null)
			{
				if (!Parameters.ContainsKey(parameter)) Parameters.Add(parameter, "true");
			}
		}

		// Retrieve a parameter value if it exists
		public string this[string param]
		{
			get { return Parameters[param]; }
		}

		public T GetValue<T>(string param)
		{
			var value = this[param];

			if (value == null) return default;

			var conv = TypeDescriptor.GetConverter(typeof(T));
			return (T)conv.ConvertFromString(value);
		}

		public bool GetOption(string shortTerm, string longTerm)
		{
			foreach (DictionaryEntry item in Parameters)
			{
				if (Regex.IsMatch(item.Key.ToString(), $"{shortTerm}|{longTerm}"))
				{
					return true;
				}
			}

			return false;
		}
	}
}