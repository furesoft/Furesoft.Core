using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class ModuleStatement : Statement
    {
        public ModuleStatement(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression ModuleName { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("module", Parse);
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var node = new ModuleStatement(parser, parent);
            parser.NextToken();

            node.ModuleName = Expression.Parse(parser, node, false, ";");

            return node;
        }
    }
}