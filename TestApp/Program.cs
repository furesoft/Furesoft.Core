using Furesoft.Core.CLI;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
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
            geometryScope.Evaluate("areaTriangle(width, height) = width * height / 2;");

            ep.RootScope.Macros.Add("rename", new Func<Expression, Expression, Scope, Expression>(RenameFunction));

            var rename = ep.Evaluate("rename(round, rndm);rndm(3.14);");

            // ep.AddModule("geometry", geometryScope);

            var module = ep.Evaluate("use \"geometry.math\"; geometry.areaRectangle(5, 3);");

            var floorPi = ep.Evaluate("floor(PI);");
            ep.Evaluate("g(x) = x*x");
            //[1, 5]
            var gres = ep.Evaluate("g: x in N [5, INFINITY];g(4);");
            //ToDo: fix ]1, 5] condition null

            ep.RootScope.ImportedFunctions.Add("display", new Func<double[], double>((x) => { Console.WriteLine(x[0]); return 0; }));

            var result = ep.Evaluate("f: x in N 2 <= x < 20 && x % 2 == 0; f(x) = 2*x; f(2); f(3); f(4);  display(-f(6));|-4**2|");
            var aliasCall = ep.Evaluate("geometry.areaTriangle(2, 6);");

            //ToDo: add standard library

            //ToDo: add constrain for return value?
            //ToDo: add measurements for parameters and variables and resolve or specify is return value is in correct measurement: f: x is [m]
            //y is [m/s]
            //measure for f(x) is [m/s*s]

            //ToDo: implement custom measurements
            //ToDo: add comments

            //ToDo: add semantic check

            //ToDo: add boolean logic
            //ToDo: Macrosystem:
            //add macro-class with macro context
            //- generate-symbol
            //- rename-function
            //- rename-imported
            //- rename-module
            //- rename-variable

            //named arguments?
            //f(x, y) = 2 * (x + y);
            //f(y: 12, x: 42);

            return App.Current.Run();
        }

        private static Expression RenameFunction(Expression func, Expression newName, Scope scope)
        {
            if (func is UnresolvedRef oldRef && oldRef.Reference is string oldName && newName is UnresolvedRef nameref && nameref.Reference is string newNameString)
            {
                var funcRef = scope.GetImportedFunctionForName(oldName, out var mangledName);

                scope.ImportedFunctions.Remove(mangledName);

                scope.ImportedFunctions.Add(newNameString + ":" + mangledName.Split(':')[1], funcRef);

                return new TempExpr();
            }

            return func;
        }
    }
}