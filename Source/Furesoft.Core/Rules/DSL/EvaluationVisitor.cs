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
        Expression result;
        switch (node)
        {
            case LiteralNode<int> ci:
                result = Expression.Constant(ci.Value);
            {
                visit = result;
                return true;
            }
            case LiteralNode<uint> uic:
                result = Expression.Constant(uic.Value);
            {
                visit = result;
                return true;
            }
            case LiteralNode<string> cs:
                result = Expression.Constant(cs.Value);
            {
                visit = result;
                return true;
            }
            case LiteralNode<bool> cb:
                result = Expression.Constant(cb.Value);
            {
                visit = result;
                return true;
            }
            case LiteralNode<double> cbd:
                result = Expression.Constant(cbd.Value);
            {
                visit = result;
                return true;
            }
        }

        visit = Expression.Empty();
        return false;
    }

    private Expression VisitPrefix(AstNode node, Expression result)
    {
        if (node is PrefixOperatorNode prefixOperatorNode)
        {
            var visited = Visit(prefixOperatorNode.Expr);

            switch (prefixOperatorNode.Operator.Name)
            {
                case "-":
                    result = Expression.Negate(visited);
                    break;
            }
        }

        return result;
    }

    private Expression VisitBinary(AstNode node, Expression result)
    {
        if (node is BinaryOperatorNode binaryOperatorNode)
        {
            var leftVisited = Visit(binaryOperatorNode.LeftExpr);
            var rightVisited = Visit(binaryOperatorNode.RightExpr);

            switch (binaryOperatorNode.Operator.Name)
            {
                case "+":
                    result = Expression.Add(leftVisited, rightVisited);
                    break;
                case "-":
                    result = Expression.Subtract(leftVisited, rightVisited);
                    break;
                case "*":
                    result = Expression.Multiply(leftVisited, rightVisited);
                    break;
                case "/":
                    result = Expression.Divide(leftVisited, rightVisited);
                    break;
            }
        }

        return result;
    }
}