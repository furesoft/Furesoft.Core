using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public class IfNode : AstNode
{
    public IfNode(AstNode condition, AstNode body)
    {
        Condition = condition;
        Body = body;
    }

    public AstNode Condition { get; }
    public AstNode Body { get; }
}