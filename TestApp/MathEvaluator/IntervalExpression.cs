using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using TestApp.MathEvaluator;
using TestApp;

namespace TestApp.MathEvaluator
{
    public class IntervalExpression : Expression
    {
        public IntervalExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Maximum { get; set; }

        public Expression Minimum { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("{", Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new IntervalExpression(parser, parent);

            parser.NextToken(); //push after {
            result.Minimum = Expression.Parse(parser, result);

            if (!result.ParseExpectedToken(parser, ","))
            {
                return null;
            }

            result.Maximum = Expression.Parse(parser, result);

            parser.NextToken(); //push after }

            return result;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }
    }
}
