using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.Parsing;
using TestApp.MathEvaluator;
using TestApp;

namespace TestApp.MathEvaluator
{
    public class InfinityRef : SymbolicRef
    {
        public InfinityRef(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public static new void AddParsePoints()
        {
            Parser.AddParsePoint("INFINITY", Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new InfinityRef(parser, parent);

            parser.NextToken();

            return result;
        }
    }
}
