using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.ExpressionEvaluator.AST;
using System;
using System.ComponentModel;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    public static class Core
    {
        public static double False = 0;
        public static double True = 1;

        [FunctionName("average")]
        [Macro]
        [Description("Calculate the arithmetic average")]
        public static Expression Average(MacroContext mc, Expression[] args)
        {
            var evaluatedArgs = args.Select(_ => mc.ExpressionParser.EvaluateExpression(_, mc.Scope).Get<double>());

            return evaluatedArgs.Sum() / args.Length;
        }

        [FunctionName("displayTree")]
        [Macro]
        public static Expression DisplayTree(MacroContext mc, Expression[] args)
        {
            if (args.Length == 2 && mc.ExpressionParser.EvaluateExpression(args[1], mc.Scope).Get<double>() == 1)
            {
                var binded = mc.ExpressionParser.Binder.BindExpression(args[0], mc.Scope);

                Console.WriteLine(binded.AsText());
            }
            else
            {
                Console.WriteLine(args[0].AsText());
            }

            return new TempExpr();
        }

        [FunctionName("displayValueTable")]
        [Macro]
        public static Expression DisplayValueTable(MacroContext mc,
            Expression minimum, Expression maximum, Expression step, Expression function)
        {
            Console.WriteLine("ValueTable for f(x) = " + function._AsString);

            double stepEvaluated = mc.ExpressionParser.EvaluateExpression(step, mc.Scope).Get<double>();
            double minimumEvaluated = mc.ExpressionParser.EvaluateExpression(minimum, mc.Scope).Get<double>();
            double maximumEvaluated = mc.ExpressionParser.EvaluateExpression(maximum, mc.Scope).Get<double>();

            for (double i = minimumEvaluated; i < maximumEvaluated; i += stepEvaluated)
            {
                var scope = Scope.CreateScope();
                scope.Variables.Add("x", i);

                var result = mc.ExpressionParser.EvaluateExpression(function, scope);

                Console.WriteLine($"f({i}) = " + result);
            }

            return 0;
        }

        [Macro]
        [FunctionName("inverse")]
        public static Expression Inverse(MacroContext mc, Expression value)
        {
            return 1 / value;
        }

        [FunctionName("product")]
        [Macro]
        public static Expression Product(MacroContext mc, Expression end, Expression def, Expression func)
        {
            double sum = 1;
            if (end is Literal l && def is Assignment a && a.Left is UnresolvedRef nameRef && nameRef.Reference is string name)
            {
                double endRange = mc.ExpressionParser.EvaluateExpression(end, mc.Scope).Get<double>();

                for (int i = (int)mc.ExpressionParser.EvaluateExpression(a.Right, mc.Scope); i <= endRange; i++)
                {
                    var s = Scope.CreateScope(mc.Scope);
                    s.Variables.Add(name, i);

                    sum *= mc.ExpressionParser.EvaluateExpression(func, s).Get<double>();
                }
            }

            return sum;
        }

        [FunctionName("root")]
        [Macro]
        public static Expression Root(MacroContext mc, Expression exponent, Expression @base)
        {
            if (@base is PowerOperator pow)
            {
                return new PowerOperator(mc.ExpressionParser.EvaluateExpression(pow.Left, mc.Scope).Get<double>(), mc.ExpressionParser.EvaluateExpression(pow.Right, mc.Scope).Get<double>() / exponent);
            }
            else
            {
                return new PowerOperator(mc.ExpressionParser.EvaluateExpression(@base, mc.Scope).Get<double>(), 1 / exponent);
            }
        }

        [FunctionName("round")]
        public static double Round(double value, double decimals)
        {
            return Math.Round(value, (int)decimals);
        }

        [FunctionName("sum")]
        [Macro]
        public static Expression Sum(MacroContext mc, Expression end, Expression def, Expression func)
        {
            double sum = 0;
            if (end is Literal l && def is Assignment a && a.Left is UnresolvedRef nameRef && nameRef.Reference is string name)
            {
                double endRange = mc.ExpressionParser.EvaluateExpression(end, mc.Scope).Get<double>();

                for (int i = (int)mc.ExpressionParser.EvaluateExpression(a.Right, mc.Scope); i <= endRange; i++)
                {
                    var s = Scope.CreateScope(mc.Scope);
                    s.Variables.Add(name, i);

                    sum += mc.ExpressionParser.EvaluateExpression(func, s).Get<double>();
                }
            }

            return sum;
        }
    }
}