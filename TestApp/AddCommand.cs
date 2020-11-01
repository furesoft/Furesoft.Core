using Furesoft.Core.CLI;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestApp
{
    public class AddCommand : ICliCommand
    {
        public string Name => "add";

        public string HelpText => "add <id> (--optimize|-opt)?";

        public string Description => "Add Article from Site to Book";

        public async Task<int> InvokeAsync(ArgumentVector args)
        {
            var id = args.GetValue<int>(0);
            var optimize = args.GetOption("-opt|--optimize");

            return 0;
        }
    }
}