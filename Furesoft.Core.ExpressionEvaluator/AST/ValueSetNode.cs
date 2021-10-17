using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class ValueSetNode : Expression, IEvaluatableStatement
    {
        public ValueSetNode(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public ChildList<Expression> Items { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("valueset", 1, Parse);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("valueset");
        }

        public void Evaluate(ExpressionParser ep)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Expression item = Items[i];

                if (item is UnresolvedRef uref && uref.Reference is string s)
                {
                    ep.RootScope.Variables.Add(s, i);
                }
            }
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var r = new ValueSetNode(parser, parent);
            parser.NextToken();

            r.Items = Expression.ParseList(parser, r, ";");

            return r;
        }
    }
}