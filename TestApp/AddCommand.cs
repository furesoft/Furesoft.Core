using Furesoft.Core.CLI;
using System.Diagnostics;

namespace TestApp
{
    public class AddCommand : ICommand
    {
        public string Name => "add";

        public string HelpText => "add <id> (--optimize|-opt)?";

        public string Description => "Add Article from Site to Book";

        public int Invoke(ArgumentVector args)
        {
            var id = args.GetValue<int>(0);
            var optimize = args.GetOption("-opt|--optimize");

            return 0;
        }
    }
}