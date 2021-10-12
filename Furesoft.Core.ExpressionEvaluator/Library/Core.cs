using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using System;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    public static class Core
    {
        [FunctionName("average")]
        [Macro]
        public static Expression Average(MacroContext mc, Expression[] args)
        {
            var evaluatedArgs = args.Select(_ => mc.ExpressionParser.EvaluateExpression(_, mc.Scope));

            return evaluatedArgs.Sum() / args.Length;
        }

        [Macro]
        [FunctionName("inverse")]
        public static Expression Inverse(MacroContext mc, Expression value)
        {
            return new Divide(new Literal(1), value);
        }

        [FunctionName("root")]
        public static double Root(double exponent, double @base)
        {
            return Math.Pow(@base, 1 / exponent);
        }

        [FunctionName("round")]
        public static double Round(double value, double decimals)
        {
            return Math.Round(value, (int)decimals);
        }
    }
}