using Furesoft.Core.CLI;
using Nova.CodeDOM;
using Nova.Parsing;
using Nova.Rendering;
using System.Linq;

namespace TestApp
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var src = "[hello + 4]";

            Parser.AddOperatorParsePoint("+", 2, true, false, parse);
            Parser.AddOperatorParsePoint("*", 1, true, false, parse2);
            Parser.AddParsePoint("[", ParseSquared);
            //CodeUnit.LoadDefaultParsePoints();

            var expr = Expression.Parse(src, out var root);

            var result = Evaluate(expr);

            return App.Current.Run();
        }

        private static int Evaluate(Expression expr)
        {
            if (expr is AddOp add)
            {
                return Evaluate(add.Left) + Evaluate(add.Right);
            }
            else if (expr is MulOp mul)
            {
                return Evaluate(mul.Left) * Evaluate(mul.Right);
            }
            else if (expr is Literal lit)
            {
                return int.Parse(lit.Text);
            }
            else
            {
                return 1;
            }
        }

        private static CodeObject parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AddOp(parser, parent);
        }

        private static CodeObject parse2(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new MulOp(parser, parent);
        }

        private static CodeObject ParseSquared(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var obj = new SquaredExpression();

            parser.NextToken();

            obj.Body = Expression.Parse(parser, obj);

            parser.NextToken();

            return obj;
        }
    }

    internal class AddOp : BinaryArithmeticOperator
    {
        public AddOp(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public AddOp(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol => "+";

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            Left.AsTextExpression(writer, flags);
            AsTextOperator(writer, flags);
            Right.AsTextExpression(writer, flags);
        }

        public override int GetPrecedence()
        {
            return 2;
        }
    }

    internal class MulOp : BinaryArithmeticOperator
    {
        public MulOp(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public MulOp(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol => "*";

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }

        public override int GetPrecedence()
        {
            return 1;
        }
    }

    internal class SquaredExpression : Expression
    {
        public Expression Body { get; set; }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("[");
            Body.AsTextExpression(writer, flags);

            writer.Write("]");
        }
    }
}