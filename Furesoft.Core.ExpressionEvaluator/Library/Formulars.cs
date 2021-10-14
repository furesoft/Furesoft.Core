using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.ExpressionEvaluator.AST;
using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator.Library
{
    [Module("formulars")]
    public static class Formulars
    {
        [Macro]
        [FunctionName("gauß")]
        public static Expression Gauß(MacroContext mc, Expression[] equations)
        {
            var matrix = new GaußMatrix();

            foreach (var eq in equations)
            {
                if (eq is Assignment a)
                {
                    var coefficients = GetCoefficients(a.Left).Select(_ => mc.ExpressionParser.EvaluateExpression(_, mc.Scope)).ToList();
                    if (coefficients.Count == 2)
                    {
                        coefficients.Add(0);
                    }

                    if (a.Right is Literal result)
                    {
                        coefficients.Add(mc.ExpressionParser.EvaluateExpression(result, mc.Scope));
                    }
                    else if (a.Right is Negative n)
                    {
                        coefficients.Add(mc.ExpressionParser.EvaluateExpression(n, mc.Scope));
                    }

                    matrix.Add(coefficients.ToArray());
                }
            }

            var r = matrix.Solve();

            return new TempExpr();
        }

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
            else if (IsLastCase(formular))
            {
                return UnpackLastCase(formular);
            }

            return new TempExpr();
        }

        private static IEnumerable<Literal> GetCoefficients(Expression expr)
        {
            var result = new List<Literal>();
            //2*x+y+3*z
            if (expr is BinaryArithmeticOperator op)
            {
                if (op.Left is BinaryArithmeticOperator addsub)
                {
                    if (addsub.Left is Multiply lm)
                    {
                        if (lm.Left is Literal lit)
                        {
                            result.Add(lit);
                        }
                        if (lm.Left is Negative neg && neg.Expression is Literal ll)
                        {
                            result.Add(new Literal(double.Parse("-" + ll.Text)));
                        }
                    }
                    else if (addsub.Left is UnresolvedRef)
                    {
                        result.Add(new Literal(1));
                    }
                    else if (addsub.Left is Negative)
                    {
                        result.Add(new Literal(-1));
                    }
                    if (addsub.Right is Multiply rm)
                    {
                        if (rm.Left is Literal lit)
                        {
                            result.Add(lit);
                        }
                    }
                }
                if (op.Left is Literal l)
                {
                    result.Add(l);
                }
                if (op.Right is BinaryArithmeticOperator o)
                {
                    result.AddRange(GetCoefficients(o));
                }
            }

            return result;
        }

        private static bool IsFirstCases(Expression argument)
        {
            //(a+b)^2 = a^2+b^+2*qb
            //(a-b)^2 = a^2+b^-2*qb

            return argument is BitwiseXor pow && pow.Right._AsString == "2"
                && pow.Left is BinaryOperator bin && (bin.Symbol == "+" || bin.Symbol == "-") && bin.HasParens;
        }

        private static bool IsLastCase(Expression formular)
        {
            ///(a+b)*(a-b)

            return formular is Multiply multiply
                && multiply.Left is Add add
                && multiply.Right is Subtract subtract
                && add.HasParens && subtract.HasParens
                && add.Left._AsString == subtract.Left._AsString
                && add.Right._AsString == subtract.Right._AsString;
        }

        private static Expression UnpackFirstCases(Expression argument)
        {
            if (argument is BitwiseXor pow && pow.Right._AsString == "2"
                && pow.Left is BinaryOperator bin)
            {
                var a2 = new PowerOperator(bin.Left, 2);
                var b2 = new PowerOperator(bin.Right, 2);
                var ab2 = 2 * bin.Left * bin.Right;

                if (bin.Symbol == "+")
                {
                    return a2 + b2 + ab2;
                }
                else if (bin.Symbol == "-")
                {
                    return a2 + b2 - ab2;
                }
                else
                {
                    return new TempExpr();
                }
            }

            return new TempExpr();
        }

        private static Expression UnpackLastCase(Expression formular)
        {
            if (formular is Multiply multiply && multiply.Left is Add add && multiply.Right is Subtract)
            {
                //a^2-b^2

                return new PowerOperator(add.Left, 2) - new PowerOperator(add.Right, 2);
            }

            return new TempExpr();
        }
    }
}