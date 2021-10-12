using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using System;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    public class Core
    {
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