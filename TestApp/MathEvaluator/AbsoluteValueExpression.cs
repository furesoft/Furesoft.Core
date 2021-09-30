using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace TestApp
{
    public class AbsoluteValueExpression : Expression
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
        }
    }
}