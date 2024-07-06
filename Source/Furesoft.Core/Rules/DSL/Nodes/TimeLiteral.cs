using Silverfly.Nodes;
using Silverfly.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL.Nodes;

public record TimeLiteral(List<PostfixOperatorNode> SubLiterals) : AstNode
{
}