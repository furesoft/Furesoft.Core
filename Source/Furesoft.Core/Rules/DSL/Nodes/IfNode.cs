using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public class IfNode : AstNode
{
    public AstNode Condition { get; }
    public AstNode Body { get; }

    public IfNode(AstNode condition, AstNode body)
    {
        Condition = condition;
        Body = body;
    }
}