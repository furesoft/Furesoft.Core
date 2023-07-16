using System.Collections;
using System.Reflection;

namespace Furesoft.Core.CLI;

	/// <summary>
	/// A Class to build CommandLine Applications easily
	/// </summary>
	public class App
	{
		private Dictionary<string, ICliCommand> _commands = new();

		public static App Current = new();

		public void AddCommand(ICliCommand cmd)
		{
			_commands.Add(cmd.Name, cmd);
		}

		public event Action BeforeRun;

		/// <summary>
		/// Start The Application
		/// </summary>
		/// <returns>The Return Code</returns>
		public int Run()
		{
			//collect all command processors
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes());

			BeforeRun?.Invoke();

			foreach (var t in types)
			{
				if (t.GetCustomAttribute<DoNotTrackAttribute>() != null)
					continue;

				if (t.IsInterface || t.IsAbstract)
					continue;
				else if (typeof(ICliCommand).IsAssignableFrom(t))
				{
					var instance = (ICliCommand)Activator.CreateInstance(t, Array.Empty<Type>());
					_commands.Add(instance.Name, instance);
				}
			}

			var args = Environment.GetCommandLineArgs();

			if (args.Length == 1)
			{
				PrintAllCommands();
				return -1;
			}

			if (args.Length == 2 && (args[1] == "--interactive" || args[1] == "-i"))
			{
				while (true)
				{
					Console.Write(">> ");
					var input = Console.ReadLine();
					ProcessCommand(input.Split(' ', StringSplitOptions.RemoveEmptyEntries), true);
				}
			}
			else
			{
				return ProcessCommand(args);
			}
		}

		public int EvaluateLine(string cmd)
		{
			return ProcessCommand(cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries)); ;
		}

		private int ProcessCommand(string[] args, bool isInteractive = false)
		{
			if (args.Length == 0)
			{
				PrintAllCommands();
				return 0;
			}

			var name = "";
			if (isInteractive)
				name = args[0];
			else
			{
				name = args[1];
			}

			//find correct processor and invoke it with new argumentvector
			if (_commands.ContainsKey(name))
				return _commands[name].Invoke(new(args));
			else if (name == "help")
			{
				PrintAllCommands();
			}
			else
			{
				// Print list of commands with helptext
				PrintAllCommands();
			}

			return -1;
		}

		public void PrintAllCommands()
		{
			var table = new ConsoleTable(Console.CursorTop, ConsoleTable.Align.Left, new string[] { "Command", "Description" });
			var rows = new ArrayList();

			foreach (var cmd in _commands)
			{
				rows.Add(new string[] { cmd.Key, cmd.Value.Description, cmd.Value.HelpText });
			}

			table.RePrint(rows);
		}
	}