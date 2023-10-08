using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public class ErrorNode : AstNode
{
    public ErrorNode(string message)
    {
        Message = message;
    }

    public string Message { get; }
}