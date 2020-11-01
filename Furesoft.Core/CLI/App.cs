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
			var name = args[0]; //ToDo: Parse command line arguments

			//find correct processor and invoke it with new argumentvector
			if (_commands.ContainsKey(name))
			{
				var values = new Dictionary<string, object>();
				values.Add("custom_cover", "hello.jpg");
				values.Add("-Wall", null);
				return _commands[name].Invoke(new ArgumentVector(values));
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
	}
}