using System.Threading.Tasks;

namespace Furesoft.Core.CLI
{
	public interface ICliCommand
	{
		string Name { get; }
		string HelpText { get; }
		string Description { get; }

		Task<int> InvokeAsync(ArgumentVector args);
	}
}