using System;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    public class Core
    {
        [FunctionName("round")]
        public static double Round(double value, double decimals)
        {
            return Math.Round(value, (int)decimals);
        }
    }
}