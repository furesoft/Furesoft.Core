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

        switch (node)
        {
            case LiteralNode<int> ci:
                result = Expression.Constant(ci.Value);
                return result;
            case LiteralNode<uint> uic:
                result = Expression.Constant(uic.Value);
                return result;
            case LiteralNode<string> cs:
                result = Expression.Constant(cs.Value);
                return result;
            case LiteralNode<bool> cb:
                result = Expression.Constant(cb.Value);
                return result;
            case LiteralNode<double> cbd:
                result = Expression.Constant(cbd.Value);
                return result;
        }
        
        var body = Expression.Constant(new RuleResult() { Result = result });
        
        return Expression.Lambda(body, true, Expression.Parameter(typeof(T)));
    }
}