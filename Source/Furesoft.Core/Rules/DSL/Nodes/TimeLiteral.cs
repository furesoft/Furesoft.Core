using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL.Nodes;

public class TimeLiteral : AstNode
{
    public TimeLiteral(List<PostfixOperatorNode> subLiterals)
    {
        SubLiterals = subLiterals;
    }

    public List<PostfixOperatorNode> SubLiterals { get; }
}