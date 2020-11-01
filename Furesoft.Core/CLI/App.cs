using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Furesoft.Core.Activation;

namespace Furesoft.Core.CLI
{
	/// <summary>
	/// A Class to build CommandLine Applications easily
	/// </summary>
	public class App
	{
		private Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

		/// <summary>
		/// Start The Application
		/// </summary>
		/// <returns>The Return Code</returns>
		public int Run()
		{
			//collect all command processors
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes());

			foreach (var t in types)
			{
				if (t.IsInterface || t.IsAbstract)
				{
					continue;
				}
				else if (typeof(ICommand).IsAssignableFrom(t))
				{
					var instance = DefaultActivator.Instance.CreateInstance<ICommand>(t, Array.Empty<Type>());
					_commands.Add(instance.Name, instance);
				}
			}

			var args = Environment.GetCommandLineArgs();

			if (args.Length == 2 && (args[1] == "--interactive" || args[1] == "-i"))
			{
				while (true)
				{
					Console.Write(">> ");
					var input = Console.ReadLine();
					ProcessCommand(input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
				}
			}
			else
			{
				return ProcessCommand(args);
			}
		}

		private int ProcessCommand(string[] args)
		{
			var name = args[1]; //ToDo: Parse command line arguments

			//find correct processor and invoke it with new argumentvector
			if (_commands.ContainsKey(name))
			{
				BuildArgumentVector(out var av, args); //ToDo: Fix BuildArgumentVector
				return _commands[name].Invoke(av);
			}
			else
			{
				// Print list of commands with helptext
				Console.WriteLine("Command\t\t\tDescription\t\t\tExample");
				Console.WriteLine("-----------------------------");
				foreach (var cmd in _commands)
				{
					Console.WriteLine(cmd.Key + "\t" + cmd.Value.Description + "\t" + cmd.Value.HelpText);
				}
			}

			return -1;
		}

		private void BuildArgumentVector(out ArgumentVector av, string[] args)
		{
			var values = new Dictionary<string, object>();
			for (var i = 2; i < args.Length; i++)
			{
				var key = args[i];

				if (key.StartsWith("-"))
				{
					values.Add(key, true);
				}
				else
				{
					values.Add(key, args[i++]);
				}
			}

			av = new ArgumentVector(values);
		}
	}
}