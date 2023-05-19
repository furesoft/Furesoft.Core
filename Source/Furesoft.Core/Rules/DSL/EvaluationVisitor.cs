using System.Linq.Expressions;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL;

public class EvaluationVisitor : IVisitor<Expression>
{
    public Expression Visit(AstNode node)
    {
        return Expression.Empty();
    }
}