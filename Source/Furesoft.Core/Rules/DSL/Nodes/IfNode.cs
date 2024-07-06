using Silverfly.Nodes;

namespace Furesoft.Core.Rules.DSL.Nodes;

public record IfNode(AstNode Condition, AstNode Body) : AstNode
{
}