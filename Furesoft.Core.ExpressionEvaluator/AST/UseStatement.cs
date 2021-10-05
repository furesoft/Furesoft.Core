using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.ExpressionEvaluator.Symbols;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class UseStatement : Statement, IEvaluatableStatement
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

        public void Evaluate(ExpressionParser ep)
        {
            if (Module is ModuleRef modRef)
            {
                if (modRef.Reference is Module mod)
                {
                    ep.RootScope.ImportScope(mod.Scope);
                }
                else if (modRef.Reference is Scope scope)
                {
                    ep.RootScope.ImportScope(scope);
                }
            }
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