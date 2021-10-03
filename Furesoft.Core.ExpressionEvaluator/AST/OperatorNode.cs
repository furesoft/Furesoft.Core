using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class OperatorNode : ArgumentsOperator
    {
        public OperatorNode(Parser parser, CodeObject parent) : base(parser, parent)
        {  //operator[104] $(x) = x*x;
            parser.NextToken();

            if (!ParseExpectedToken(parser, "["))
                return;

            Precedence = Literal.Parse(parser, this);

            if (!ParseExpectedToken(parser, "]"))
                return;

            Operator = parser.Token;

            parser.NextToken();

            ParseArguments(parser, this, "(", ")");

            AssignmentExpression = Assignment.Parse(parser, this, false, ";");
        }

        public Expression AssignmentExpression { get; }
        public Token Operator { get; }
        public Expression Precedence { get; }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("operator", 200, true, false, Parse);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }

        public override int GetPrecedence()
        {
            return 200;
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
        }

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new OperatorNode(parser, parent);
        }
    }
}