using System.Linq.Expressions;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL;

public class EvaluationVisitor<T> : IVisitor<AstNode, Expression>
    where T : class, new()
{
    public Expression Visit(AstNode node)
    {
        if (node is BinaryOperatorNode binaryOperatorNode)
        {
            switch (binaryOperatorNode.Operator.Name)
            {
                case "+":
                    return Expression.Add(Visit(binaryOperatorNode.LeftExpr), Visit(binaryOperatorNode.RightExpr));
            }
        }
        
        
        
        var body = Expression.Constant(new RuleResult() { });
        
        return Expression.Lambda(body, true, Expression.Parameter(typeof(T)));
    }
}