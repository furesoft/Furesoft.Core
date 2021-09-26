using Furesoft.Core.CLI;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            ExpressionParser.Init();
            ExpressionParser.AddVariable("x", 42);
            //ExpressionParser.Evaluate("g(x) = x*x");

            var result = ExpressionParser.Evaluate("f: x in N  x > 2 < 20; f(x) = 2*x; f(1)");
            //f: x is N {2,10};

            return App.Current.Run();
        }
    }
}