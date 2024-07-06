using System.Linq.Expressions;
using System.Reflection;
using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.Core.Rules.DSL.Parselets;
using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL;

public class EvaluationVisitor<T> : NodeVisitor<Expression>
    where T : class, new()
{
    private readonly List<string> _errors = [];

    private readonly ParameterExpression _errorsParameterExpression =
        Expression.Parameter(typeof(List<string>), "errors");

    private readonly ParameterExpression _modelParameterExpression = Expression.Parameter(typeof(T), "model");

    public EvaluationVisitor()
    {
        For<IfNode>(Visit);
        For<ErrorNode>(Visit);
        For<NameNode>(Visit);
        For<TimeLiteral>(Visit);
        For<PrefixOperatorNode>(Visit);
        For<PostfixOperatorNode>(Visit);
        For<BinaryOperatorNode>(Visit);
        For<LiteralNode>(Visit);
    }

    private Expression Visit(IfNode ifNode)
    {
        return Expression.Block(Expression.IfThen(Visit(ifNode.Condition), Visit(ifNode.Body)),
            Expression.Constant(true));
    }

    private Expression Visit(ErrorNode error)
    {
        var msg = Expression.Constant(error.Message);
        var addMethod = _errors.Add;

        return Expression.Block(Expression.Call(_errorsParameterExpression, addMethod.Method, msg),
            Expression.Constant(false));
    }

    private Expression Visit(NameNode name)
    {
        return Expression.MakeMemberAccess(_modelParameterExpression, GetMemberInfo(name));
    }

    private static MemberInfo GetMemberInfo(NameNode name)
    {
        return typeof(T).GetProperty(name.Name);
    }

    public LambdaExpression ToLambda(Expression body)
    {
        return Expression.Lambda(body, true, _modelParameterExpression, _errorsParameterExpression);
    }

    private static Expression Visit(LiteralNode node)
    {
        return node.Value switch
        {
            long ci => Expression.Constant(ci),
            ulong uic => Expression.Constant(uic),
            string cs => Expression.Constant(cs),
            bool cb => Expression.Constant(cb),
            double cbd => Expression.Constant(cbd),
            _ => Expression.Empty(),
        };
    }

    private Expression Visit(TimeLiteral timeLiteral)
    {
        Expression tmp = null;
        for (var index = 0; index < timeLiteral.SubLiterals.Count; index++)
        {
            var subLiteral = timeLiteral.SubLiterals[index];
            var visitTimePostFix = VisitTimePostFix(subLiteral,
                TimeLiteralParselet.TimePostfixConverters[subLiteral.Operator.Name]);

            if (index == 0)
            {
                tmp = visitTimePostFix;
                continue;
            }

            tmp = Expression.Add(tmp!, visitTimePostFix);
        }

        return tmp;
    }

    private Expression Visit(PrefixOperatorNode prefixOperatorNode)
    {
        var visited = Visit(prefixOperatorNode.Expr);

        return prefixOperatorNode.Operator.Name switch
        {
            "-" => Expression.Negate(visited),
            _ => visited
        };
    }

    private Expression Visit(PostfixOperatorNode postfix)
    {
        var visited = Visit(postfix.Expr);

        if (TimeLiteralParselet.TimePostfixConverters.TryGetValue(postfix.Operator.Name, out var timeConverter))
        {
            return VisitTimePostFix(postfix, timeConverter);
        }

        return postfix.Operator.Name switch
        {
            "%" => Expression.Divide(Expression.Convert(visited, typeof(double)), Expression.Constant(100.0)),
            _ => visited
        };
    }

    private Expression VisitTimePostFix(PostfixOperatorNode node, string timeConverter)
    {
        var arg = Visit(node.Expr);

        if (arg.Type != typeof(double)) arg = Expression.Convert(arg, typeof(double));

        return Expression.Call(typeof(TimeSpan), timeConverter, null, arg);
    }

    private Expression Visit(BinaryOperatorNode binaryOperatorNode)
    {
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

            "<=" => Expression.LessThanOrEqual(leftVisited, rightVisited),
            ">=" => Expression.GreaterThanOrEqual(leftVisited, rightVisited),
            "<" => Expression.LessThan(leftVisited, rightVisited),
            ">" => Expression.GreaterThan(leftVisited, rightVisited),

            "=" => Expression.Assign(leftVisited, rightVisited),
            
            "%." => Expression.Equal(Expression.Modulo(leftVisited, rightVisited), Expression.Constant(0ul)),

            _ => Expression.Empty()
        };
    }
}