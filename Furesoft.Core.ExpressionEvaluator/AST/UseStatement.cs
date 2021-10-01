using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class UseStatement : Statement
    {
        public UseStatement(Parser parser, CodeObject parent) :
            base(parser, parent)
        {
        }

        public Expression Module { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("use", Parse);
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new UseStatement(parser, parent);
            parser.NextToken();

            result.Module = Expression.Parse(parser, result, false, ";");

            return result;
        }
    }
}
