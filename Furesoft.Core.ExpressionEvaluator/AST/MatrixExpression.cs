using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class MatrixExpression : Expression
    {
        public MatrixExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public ChildList<Expression> Storage { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("[", 1, Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            parser.NextToken();

            var result = new MatrixExpression(parser, parent);

            result.Storage = Expression.ParseList(parser, parent, "]");

            if (!result.ParseExpectedToken(parser, "]"))
                return null;

            return result;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("[");

            foreach (var s in Storage)
            {
                s.AsTextExpression(writer, flags);
                writer.Write(" ");
            }

            writer.Write("]");
        }
    }
}