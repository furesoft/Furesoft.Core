using Furesoft.Core.AST;
using Furesoft.Core.CLI;
using Furesoft.Core.Commands;
using System;
using System.IO;

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