using System.Linq.Expressions;
using System.Reflection;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL;

public class EvaluationVisitor<T> : IVisitor<AstNode, Expression>
    where T : class, new()
{
    private readonly ParameterExpression _modelParameterExpression = Expression.Parameter(typeof(T), "model");
    
    public Expression Visit(AstNode node)
    {
        Expression body = null;
        
        VisitBinary(node, ref body);
        VisitPrefix(node, ref body);
        VisitPostfix(node, ref body);

        // ToDo: remove temporary fix
        if (node is BinaryOperatorNode bin && bin.Operator.Name == "=")
        {
            body = Expression.Block(body, Expression.Constant(true));
        }

        if (VisitConstant(node, out var visit))
        {
            return visit;
        }

        if (VisitName(node, out var expression))
        {
            return expression;
        }

        return body;
    }

    private bool VisitName(AstNode node, out Expression expression)
    {
        if (node is NameAstNode name)
        {
            expression = Expression.MakeMemberAccess(_modelParameterExpression, GetMemberInfo(name));
            
            return true;
        }
        
        expression = Expression.Empty();
        return false;
    }

    private MemberInfo GetMemberInfo(NameAstNode name)
    {
        return typeof(T).GetProperty(name.Name);
    }

    public LambdaExpression ToLambda(Expression body)
    {
        return Expression.Lambda(body, true, _modelParameterExpression);
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

    private void VisitPrefix(AstNode node, ref Expression result)
    {
        if (node is not PrefixOperatorNode prefixOperatorNode) 
            return;
        
        var visited = Visit(prefixOperatorNode.Expr);

        result = prefixOperatorNode.Operator.Name switch
        {
            "-" => Expression.Negate(visited),
            _ => result
        };
    }
    
    private void VisitPostfix(AstNode node, ref Expression result)
    {
        if (node is not PostfixOperatorNode postfix) 
            return;
        
        var visited = Visit(postfix.Expr);

        result = postfix.Operator.Name switch
        {
            "%" => Expression.Divide( Expression.Convert(visited, typeof(double)), Expression.Constant(100.0)),
            _ => result
        };
    }

    private void VisitBinary(AstNode node, ref Expression result)
    {
        if (node is not BinaryOperatorNode binaryOperatorNode) 
             return;

        var leftVisited = Visit(binaryOperatorNode.LeftExpr);
        var rightVisited = Visit(binaryOperatorNode.RightExpr);

        result = binaryOperatorNode.Operator.Name switch
        {
            "+" => Expression.Add(leftVisited, rightVisited),
            "-" => Expression.Subtract(leftVisited, rightVisited),
            "*" => Expression.Multiply(leftVisited, rightVisited),
            "/" => Expression.Divide(leftVisited, rightVisited),
            
            "==" => Expression.Equal(leftVisited, rightVisited),
            "!=" => Expression.NotEqual(leftVisited, rightVisited),
            
            "=" => Expression.Assign(leftVisited, rightVisited),
            
            _ => result
        };
    }
}