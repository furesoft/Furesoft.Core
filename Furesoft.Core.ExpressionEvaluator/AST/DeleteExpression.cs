using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class DeleteExpression : Expression, IEvaluatableExpression
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
            writer.Write("delete");
        }

        public double Evaluate(ExpressionParser ep, Scope scope)
        {
            if (Expression is UnresolvedRef u && u.Reference is string s)
            {
                if (!scope.Variables.Remove(s))
                {
                    AttachMessage($"Variable '{s}' not found", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else if (Expression is Call c && c.Expression is UnresolvedRef uc && uc.Reference is string sc)
            {
                if (!scope.Functions.Remove(sc + ":" + c.ArgumentCount))
                {
                    AttachMessage($"Function '{c._AsString}' not found", MessageSeverity.Error, MessageSource.Resolve);
                }
            }

            return 0;
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