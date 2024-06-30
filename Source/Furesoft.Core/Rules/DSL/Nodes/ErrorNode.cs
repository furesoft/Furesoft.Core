using Silverfly.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public record ErrorNode(string Message) : AstNode
{
}