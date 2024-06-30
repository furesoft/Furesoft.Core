using System.Collections.Immutable;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL.Nodes;

public record TimeLiteral(ImmutableList<PostfixOperatorNode> SubLiterals) : AstNode
{
}