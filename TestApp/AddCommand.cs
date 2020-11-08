using Furesoft.Core.CLI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TestApp
{
    public class ShowAllCommand : ICliCommand
    {
        public string Name => "all";

        public string HelpText => throw new System.NotImplementedException();

        public string Description => throw new System.NotImplementedException();

        public int Invoke(CommandlineArguments args)
        {
            var dbFile = Path.Combine(Environment.CurrentDirectory, "cowdb.data");

            using var db = new CowDatabase(dbFile);
            foreach (var cow in db.FindAll())
            {
                Console.WriteLine(cow);
            }

            return 0;
        }
    }

    public class AddCommand : ICliCommand
    {
        public string Name => "add";

        public string HelpText => "add <id> (--optimize|-opt)?";

        public string Description => "Add Article from Site to Book";

        public int Invoke(CommandlineArguments args)
        {
            var id = args["id"];
            var optimize = args.GetOption("opt", "optimize");

            return 0;
        }
    }
}