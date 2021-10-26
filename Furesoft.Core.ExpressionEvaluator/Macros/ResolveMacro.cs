using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class ResolveMacro : Macro
    {
        public override string Name => "resolve";

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            //ToDo: implement resolve macro

            if (arguments[0] is Assignment a && arguments[1] is UnresolvedRef variable)
            {
                return Transform(a, a.Left, a.Right, variable);
            }

            return 6;
        }

        private Expression Transform(Assignment a, Expression left, Expression right, UnresolvedRef variable)
        {
            if (left is Add add)
            {
                a.Right = a.Right - add.Right;
                a.Left = add.Left;
            }
            if (left is Subtract sub)
            {
                a.Right = a.Right + sub.Right;
                a.Left = sub.Left;
            }
            else if (left is BitwiseXor pow)
            {
                a.Right = new Call(new UnresolvedRef("sqrt"), a.Right);
                a.Left = pow.Left;
            }
            else if (left is Call c && c.Expression is UnresolvedRef cr && cr.Reference.ToString() == "sqrt")
            {
                a.Right = new PowerOperator(c.Arguments[0], 2);
                a.Left = c.Arguments[0];
            }

            if (a.Left is UnresolvedRef u && variable.Reference.ToString() == u.Reference.ToString())
            {
                return a;
            }
            else
            {
                return Transform(a, a.Left, a.Right, variable);
            }
        }
    }
}