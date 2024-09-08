using Furesoft.Core.CLI;
using Furesoft.Core.ExpressionEvaluator;
using Furesoft.Core.ExpressionEvaluator.Library;
using System;
using Furesoft.Core.GraphDb;
using Furesoft.Core.GraphDb.IO;

namespace TestApp;

record Actor(string Name)
{

}

record Movie(string Name)
{

}

class HasRole
{

}

internal static class Program
{
    public static int Main(string[] args)
    {
        DbControl.DbPath = Environment.CurrentDirectory;
        var db = new DbEngine();

        var movie = new Movie("The Big Bang Theory");
        var jim = new Actor("Jim");
        var john = new Actor("John");

        var movieNode = db.AddNode(movie);
        var jimNode = db.AddNode(jim);
        var johnNode = db.AddNode(john);

        db.AddRelation(movieNode, johnNode, new HasRole());
        db.AddRelation(movieNode, jimNode, new HasRole());

        db.SaveChanges();

        var q = db.CreateQuery();
        var qm = q.Match<Movie>(m => m.Name == "The Big Bang Theory");
        q.To<HasRole>();
        q.Match(NodeDescription.Any());

        q.Execute();

        db.Dispose();

        ExpressionParser.Init();

        var ep = new ExpressionParser();

        ep.AddVariable("x", 42);
        ep.RootScope.Import(typeof(Math));
        ep.Import(typeof(Geometry));
        ep.Import(typeof(Core));
        ep.Import(typeof(Formulars));
        ep.Import(typeof(Physics));

        var mscalar = ep.Evaluate("[1,2,3]*2");

        var aliasCall = ep.Evaluate("displayValueTable(1, 10, 1, x ^ 2);");
        var displayTree = ep.Evaluate("use physics.*; G = 0.5; Fg(2);");

        var derivative = ep.Evaluate("rulefor(derive, X ^ Y -> Y * X ^ (Y - 1)); derive(2 ^ 3);");

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
        //add operator overloads for double
        //rootscope.addooperatoroverload<double, double>("+", (l, r)=> l + r);

        return App.Current.Run();
    }
}