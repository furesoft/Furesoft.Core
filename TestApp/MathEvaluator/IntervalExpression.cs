using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace TestApp.MathEvaluator
{
    public class IntervalExpression : Expression
    {
        public IntervalExpression(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public bool IsMaximumInclusive { get; set; }
        public bool IsMinimumInclusive { get; set; }
        public Token Left { get; set; }
        public Expression Maximum { get; set; }
        public Expression Minimum { get; set; }
        public Token Right { get; set; }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("[", Parse, typeof(FunctionArgumentConditionDefinition));
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new IntervalExpression(parser, parent);
            //ToDo: Make Props to intervalexpression and check if left or right is inclusive or exclusive

            result.Left = parser.Token;

            parser.NextToken();

            result.Minimum = Expression.Parse(parser, result);
            result.IsMinimumInclusive = result.Left.Text == "[";

            if (!result.ParseExpectedToken(parser, ","))
            {
                return null;
            }

            result.Maximum = Expression.Parse(parser, result);

            result.Right = parser.Token; //push after
            result.IsMaximumInclusive = result.Right.Text == "]";

            parser.NextToken();

            return result;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }
    }
}