namespace Furesoft.Core.ExpressionEvaluator.AST
{
    internal class GoesToOperator : BinaryArithmeticOperator
    {
        private const int Precedence = 500;

        public GoesToOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public GoesToOperator(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol
        {
            get { return "->"; }
        }

        public new static void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("->", Precedence, true, false, Parse);
        }

        public static GoesToOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new GoesToOperator(parser, parent);
        }

        public override T Accept<T>(VisitorBase<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}