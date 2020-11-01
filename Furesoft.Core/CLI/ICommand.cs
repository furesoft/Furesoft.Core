using System.Collections.Generic;

namespace Furesoft.Core.CLI
{
	public interface ICommand
	{
		string Name { get; }
		string HelpText { get; }
		string Description { get; }

		int Invoke(ArgumentVector args);
	}
}