using Furesoft.Core.CLI;
using System.Threading.Tasks;

namespace TestApp
{
    public class AddCommand : ICliCommand
    {
        public string Name => "add";

        public string HelpText => "add <id> (--optimize|-opt)?";

        public string Description => "Add Article from Site to Book";

        public Task<int> InvokeAsync(CommandlineArguments args)
        {
            var id = args["id"];
            var optimize = args.GetValue<bool>("opt");

            return Task.FromResult(0);
        }
    }
}