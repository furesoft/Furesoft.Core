using Furesoft.Core.CLI;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Projects;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var src = "a is int";
            CodeObject.AddDefaultParsePoints();

            var ast = CodeUnit.LoadFragment(src, "test.ls");

            return App.Current.Run();
        }
    }
}