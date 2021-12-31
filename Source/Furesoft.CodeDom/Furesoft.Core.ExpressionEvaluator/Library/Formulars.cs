using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;

namespace Furesoft.Core.ExpressionEvaluator.Library;

[Module("formulars")]
public static class Formulars
{
    [Macro]
    [FunctionName("lgs3")]
    public static Expression LGS3(MacroContext mc, Expression[] matrices)
    {
        var rights = new List<double>();

        Matrix<double> A = null;
        Vector<double> b = null;

        if (matrices.Length == 2 && matrices[0] is MatrixExpression m && matrices[1] is MatrixExpression v && v.Storage != null && v.Storage.Count == 3 && m.Storage.Count == 9)
        {
            double[] storage = m.Storage.Select(_ => mc.ExpressionParser.EvaluateExpression(_, Scope.CreateScope()).Get<double>()).ToArray();

            var rows = new List<double[]>();
            rows.Add(storage.Take(3).ToArray());
            rows.Add(storage.Skip(3).Take(3).ToArray());
            rows.Add(storage.Skip(6).Take(3).ToArray());

            A = Matrix<double>.Build.DenseOfRowArrays(rows);
            b = Vector<double>.Build.Dense(v.Storage.Select(_ => mc.ExpressionParser.EvaluateExpression(_, Scope.CreateScope()).Get<double>()).ToArray());

            var x = A.Solve(b);

            for (int i = 0; i < x.Count; i++)
            {
                if (mc.ParentCallNode is Assignment a && a.Left is UnresolvedRef u && u.Reference is string s)
                {
                    mc.ExpressionParser.RootScope.Variables.Add(s + i, x[i]);
                }
                else
                {
                    mc.ExpressionParser.RootScope.Variables.Add("x" + i, x[i]);
                }
            }
        }

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
        if (argument is PowerOperator pow && pow.Right._AsString == "2"
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
