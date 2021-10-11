using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.ExpressionEvaluator.AST;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("formulars")]
    public static class Formulars
    {
        [Macro]
        [FunctionName("unpackBinominal")]
        public static Expression UnpackBinominal(MacroContext context, Expression formular)
        {
            //fall matchen
            //unpack

            if (IsFirstCases(formular))
            {
                return UnpackFirstCases(formular);
            }

            return new TempExpr();
        }

        private static bool IsFirstCases(Expression argument)
        {
            //(a+b)^2 = a^2+b^+2*qb
            //(a-b)^2 = a^2+b^-2*qb

            return argument is PowerOperator pow && pow.Right._AsString == "2"
                && pow.Left is BinaryOperator bin && (bin.Symbol == "+" || bin.Symbol == "-") && bin.HasParens;
        }

        private static Expression UnpackFirstCases(Expression argument)
        {
            if (argument is PowerOperator pow && pow.Right._AsString == "2"
                && pow.Left is BinaryOperator bin)
            {
                PowerOperator a2 = new PowerOperator(bin.Left, new Literal(2));
                PowerOperator b2 = new PowerOperator(bin.Right, new Literal(2));
                Multiply ab2 = new Multiply(new Literal(2), new Multiply(bin.Left, bin.Right));

                if (bin.Symbol == "+")
                {
                    return new Add(a2,
                        new Add(b2, ab2));
                }
                else if (bin.Symbol == "-")
                {
                    return new Add(a2,
                        new Subtract(b2, ab2));
                }
                else
                {
                    return new TempExpr();
                }
            }

            return new TempExpr();
        }
    }
}