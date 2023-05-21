using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public class ErrorNode : AstNode
{
    public string Message { get; }

    public ErrorNode(string message)
    {
        Message = message;
    }
}