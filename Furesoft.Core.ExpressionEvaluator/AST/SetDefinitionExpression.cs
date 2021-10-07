using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class SetDefinitionExpression : Expression
    {
        public SetDefinitionExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public ChildList<Expression> Value { get; private set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("{", Parse, typeof(SetDefinitionNode));
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var node = new SetDefinitionExpression(parser, parent);

            parser.NextToken();

            node.Value = Expression.ParseList(parser, node, "}");

            if (!node.ParseExpectedToken(parser, "}"))
                return null;

            return node;
        }
    }
}