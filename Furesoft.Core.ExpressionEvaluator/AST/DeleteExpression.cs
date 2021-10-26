using Furesoft.Core.CodeDom.CodeDOM.Annotations;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class DeleteExpression : Expression, IEvaluatableStatement
    {
        public DeleteExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Expression { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("delete", 1, Parse);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("delete ");
            Expression.AsTextExpression(writer, flags);
        }

        public void Evaluate(ExpressionParser ep)
        {
            if (Expression is UnresolvedRef u && u.Reference is string s)
            {
                if (!ep.RootScope.Variables.Remove(s))
                {
                    AttachMessage($"Variable '{s}' not found", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else if (Expression is Call c && c.Expression is UnresolvedRef uc && uc.Reference is string sc)
            {
                if (!ep.RootScope.Functions.Remove(sc + ":" + c.ArgumentCount))
                {
                    AttachMessage($"Function '{c._AsString}' not found", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var r = new DeleteExpression(parser, parent);
            parser.NextToken();

            r.Expression = Expression.Parse(parser, r, false);

            return r;
        }
    }
}