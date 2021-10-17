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
            ep.Import(typeof(Core));
            ep.Import(typeof(Formulars));

            var aliasCall = ep.Evaluate("displayValueTable(1, 10, 1, x ^ 2);");
            var displayTree = ep.Evaluate("use formulars; displayTree(unpackBinominal((1+2)^2), TRUE);");

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
            //ToDo: add ISyntaxReceiver
            //Macro can register for syntax to do something or rebind node

            //ability to define rules for
            //rulefor(resolve, "coefficient", ...);
            //benötigt möglichkeit zum pattern matching:
            //a*_+b*_+c erstellt temporären scope mit den entsprechenden werten im argument.

            //verschiedene werttypen. beispiel vektoren/brüche
            //tuples
            //operator überladung

            //ToDo: named arguments: mixed mode - differenz bilden und dann scope mit rest befüllen

            //ToDo: implement RuleFor clr methods: generate pattern

            //ToDo: check initializer macro call
            //ToDo: implement different value types:
            //  double, Matrix

            return App.Current.Run();
        }
    }
}