using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.Parsing;
using TestApp.MathEvaluator;
using TestApp;

namespace TestApp.MathEvaluator
{
    internal class PowerOperator : BinaryArithmeticOperator
    {
        private const int Precedence = 2;

        public PowerOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("**", Precedence, true, false, Parse);
        }

        public static PowerOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new PowerOperator(parser, parent);
            return null;
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}
