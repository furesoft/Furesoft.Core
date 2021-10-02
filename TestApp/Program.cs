using Furesoft.Core.CLI;
using Furesoft.Core.ExpressionEvaluator;
using Furesoft.Core.ExpressionEvaluator.Library;
using System;

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
            ep.Import(typeof(Geometry));

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
            var aliasCall = ep.Evaluate("geometry.areaTriangle(2, 6);");

            //ToDo: add standard library
            //ToDo: add ability to add module with Attribute on clr type
            //ToDo: add call with alias module: if using "use 'geometry.math'" and it contains moduledefinition than don't import scope, add scope to new module to use it call
            //geometry.areaRectangle(1,2);

            //ToDo: add constrain for return value?
            //ToDo: add measurements for parameters and variables and resolve or specify is return value is in correct measurement: f: x is [m]
            //y is [m/s]
            //measure for f(x) is [m/s*s]
            //ToDo: add aliases for function names to be able to translate it or use custom name instead of module.fnName();
            //ToDo: implement custom measurements
            //ToDo: add comments
            //ToDo: add function overloading for clr methods?

            return App.Current.Run();
        }
    }
}