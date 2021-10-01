using Furesoft.Core.CLI;
using System;
using TestApp.MathEvaluator;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            ExpressionParser.Init();

            var ep = new ExpressionParser();

            ep.AddVariable("x", 42);
            ep.RootScope.Import(typeof(Math));

            var geometryScope = new Scope();
            geometryScope.ImportedFunctions.Add("areaRectangle", new Func<double[], double>((x) => { return x[0] * x[1]; }));

            ep.AddModule("geometry", geometryScope);

            var module = ep.Evaluate("use \"geometry.math\"; areaRectangle(5, 3);");

            var floorPi = ep.Evaluate("floor(PI);");
            ep.Evaluate("g(x) = x*x");
            //[1, 5]
            var gres = ep.Evaluate("g: x in N [5, INFINITY];g(4);");
            //ToDo: fix ]1, 5] condition null

            ep.RootScope.ImportedFunctions.Add("display", new Func<double[], double>((x) => { Console.WriteLine(x[0]); return 0; }));

            var result = ep.Evaluate("f: x in N 2 <= x < 20 && x % 2 == 0; f(x) = 2*x; f(2); f(3); f(4);  display(-f(6));|-4**2|");

            //ToDo: implement tests
            //ToDo: add simplification mode instead of evaluation?
            //ToDo: compiler?

            //ToDo: add constrain for return value?
            //ToDo: add measurements for parameters and variables and resolve or specify is return value is in correct measurement: f: x is [m]
            //y is [m/s]
            //measure for f(x) is [m/s*s]
            //ToDo: move to new assembly
            //ToDo: add position to error messages if possible
            //ToDo: add module system
            //ToDo: add module definion parsing: module geometry; identifier or string as argument
            //ToDo: add importing module to global scope: use geometry; identifier or string as argument
            //ToDo: add loading module from file: use "./geometry.math"; automatic import all things to scope

            return App.Current.Run();
        }
    }
}