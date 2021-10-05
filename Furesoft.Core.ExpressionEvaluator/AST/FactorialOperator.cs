using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    internal class FactorialOperator : PostUnaryOperator
    {
        private const int Precedence = 2;

        public FactorialOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("!", Precedence, true, false, Parse);
        }

        public static FactorialOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new FactorialOperator(parser, parent);
            return null;
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}