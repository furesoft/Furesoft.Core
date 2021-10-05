using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.Parsing;
using System;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    internal class PowerOperator : BinaryArithmeticOperator, IEvaluatableExpression
    {
        private const int Precedence = 2;

        public PowerOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("^", Precedence, true, false, Parse);
        }

        public static PowerOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new PowerOperator(parser, parent);
            return null;
        }

        public double Evaluate(ExpressionParser ep, Scope scope)
        {
            return Math.Pow(ep.EvaluateExpression(Left, scope), ep.EvaluateExpression(Right, scope));
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}