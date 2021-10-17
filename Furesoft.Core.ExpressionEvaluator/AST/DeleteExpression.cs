using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
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
            if (Parent is Assignment a && a.Left is UnresolvedRef u && u.Reference is string s)
            {
                scope.Variables.Remove(s);
            }

            return 0;
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var r = new DeleteExpression(parser, parent);
            parser.NextToken();

            return r;
        }
    }
}