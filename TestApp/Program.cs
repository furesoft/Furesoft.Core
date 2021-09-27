using Furesoft.Core.CLI;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            ExpressionParser.Init();
            ExpressionParser.AddVariable("x", 42);
            ExpressionParser.Evaluate("g(x) = x*x");

            var result = ExpressionParser.Evaluate("f: x in N 2 < x < 20; f(x) = 2*x; -f(5);(5-3)**2");
            //f: x is N {2,10};

            //ToDo: implement constraint for interval
            //ToDo: implement clr functions
            //ToDo: if a function has 2 arguments but called with 1 attach error
            //ToDo: add value expression: |-12|
            //ToDo: implement tests
            //ToDo: add simplification mode instead of evaluation?
            //ToDo: compiler?

            //ToDo: add module for functionparameterconstrain f: x in N x % 2 == 0
            //ToDo: add boolean operators == !=
            //ToDo: add constrain for return value?
            //ToDo: add measurements for parameters and variables and resolve or specify is return value is in correct measurement: f: x is [m]
            //y is [m/s]
            //measure for f(x) is [m/s*s]
            //ToDo: move to new assembly
            //ToDo: add all return values as tuple

            return App.Current.Run();
        }
    }
}