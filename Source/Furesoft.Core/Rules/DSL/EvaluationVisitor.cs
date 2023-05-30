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
        Expression result = null;
        result = VisitBinary(node, result);

        result = VisitPrefix(node, result);

        if (VisitConstant(node, out var visit))
        {
            return visit;
        }
        
        var body = Expression.Constant(new RuleResult() { Result = result });
        
        return Expression.Lambda(body, true, Expression.Parameter(typeof(T)));
    }

    private static bool VisitConstant(AstNode node, out Expression visit)
    {
        switch (node)
        {
            case LiteralNode<long> ci:
                visit = Expression.Constant(ci.Value);
                return true;
            
            case LiteralNode<ulong> uic:
                visit = Expression.Constant(uic.Value);
                return true;
            
            case LiteralNode<string> cs:
                visit = Expression.Constant(cs.Value);
                return true;
            
            case LiteralNode<bool> cb:
                visit = Expression.Constant(cb.Value);
                return true;
            
            case LiteralNode<double> cbd:
                visit = Expression.Constant(cbd.Value);
                return true;
            default:
                visit = Expression.Empty();
                return false;
        }
    }

    private Expression VisitPrefix(AstNode node, Expression result)
    {
        if (node is not PrefixOperatorNode prefixOperatorNode) 
            return result;
        
        var visited = Visit(prefixOperatorNode.Expr);

        return prefixOperatorNode.Operator.Name switch
        {
            "-" => Expression.Negate(visited),
            _ => result
        };
    }

    private Expression VisitBinary(AstNode node, Expression result)
    {
        if (node is not BinaryOperatorNode binaryOperatorNode) 
            return result;
        
        var leftVisited = Visit(binaryOperatorNode.LeftExpr);
        var rightVisited = Visit(binaryOperatorNode.RightExpr);

        return binaryOperatorNode.Operator.Name switch
        {
            "+" => Expression.Add(leftVisited, rightVisited),
            "-" => Expression.Subtract(leftVisited, rightVisited),
            "*" => Expression.Multiply(leftVisited, rightVisited),
            "/" => Expression.Divide(leftVisited, rightVisited),
            
            "==" => Expression.Equal(leftVisited, rightVisited),
            "!=" => Expression.NotEqual(leftVisited, rightVisited),
            
            _ => result
        };
    }
}