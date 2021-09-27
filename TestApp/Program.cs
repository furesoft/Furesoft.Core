using Furesoft.Core.CLI;
using System;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            ExpressionParser.Init();
            ExpressionParser.AddVariable("x", 42);
            ExpressionParser.Evaluate("g(x) = x*x");
            ExpressionParser.RootScope.ImportedFunctions.Add("display", new Func<double[], double>((x) => { Console.WriteLine(x[0]); return 0; }));

            var result = ExpressionParser.Evaluate("f: x in N 2 <= x < 20; f(x) = 2*x; f(2); f(3); f(4);  display(-f(5, 2));|-4**2|");
            //f: x is N {2,10};

            //ToDo: implement constraint for interval
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
            //ToDo: add position to error messages if possible

            return App.Current.Run();
        }
    }
}