using System.Linq.Expressions;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL;

public class EvaluationVisitor<T> : IVisitor<Expression>
    where T : class, new()
{
    public Expression Visit(AstNode node)
    {
        var body = Expression.Constant(new RuleResult() { });
        
        return Expression.Lambda(body, true, Expression.Parameter(typeof(T)));
    }
}