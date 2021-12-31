

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class AbsoluteValueExpression : Expression, IEvaluatableExpression
    {
        public AbsoluteValueExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Expression { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("|", Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            parser.NextToken();

            var result = new AbsoluteValueExpression(parser, parent);

            result.Expression = Expression.Parse(parser, parent);

            if (!result.ParseExpectedToken(parser, "|"))
                return null;

            return result;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("|");

            Expression.AsTextExpression(writer, flags);

            writer.Write("|");
        }

        public ValueType Evaluate(ExpressionParser ep, Scope scope)
        {
            return Math.Abs(ep.EvaluateExpression(Expression, scope).Get<double>());
        }
    }
}