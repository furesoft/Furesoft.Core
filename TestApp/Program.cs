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

            var result = ExpressionParser.Evaluate("f: x in N  2 < x < 20; f(x) = 2*x; f(5)");
            //f: x is N {2,10};

            //ToDo: implement constraint for interval
            //ToDo: implement clr functions
            //ToDo: implement exponent
            //ToDo: if a function has 2 arguments but called with 1 attach error
            //ToDo: add value expression: |-12|
            //ToDo: implement tests
            //

            return App.Current.Run();
        }
    }
}