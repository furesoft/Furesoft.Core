using System.Linq.Expressions;
using System.Reflection;
using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.Core.Rules.DSL.Parselets;
using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;

namespace Furesoft.Core.Rules.DSL;

//TODO: update visitor to for<>()
public class EvaluationVisitor<T> : NodeVisitor<Expression>
    where T : class, new()
{
    private readonly List<string> _errors = [];

    private readonly ParameterExpression _errorsParameterExpression =
        Expression.Parameter(typeof(List<string>), "errors");

    private readonly ParameterExpression _modelParameterExpression = Expression.Parameter(typeof(T), "model");

    public Expression Visit(AstNode node)
    {
        Expression body = null;

        VisitTimeLiteral(node, ref body);
        VisitBinary(node, ref body);
        VisitPrefix(node, ref body);
        VisitPostfix(node, ref body);
        VisitError(node, ref body);
        VisitIf(node, ref body);

        // ToDo: remove temporary fix
        if (node is BinaryOperatorNode bin && bin.Operator.Text.ToString() == "=")
            body = Expression.Block(body, Expression.Constant(true));

        if (VisitConstant(node, out var visit)) return visit;

        if (VisitName(node, out var expression)) return expression;

        return body;
    }

    private void VisitIf(AstNode node, ref Expression body)
    {
        if (node is not IfNode ifNode) return;

        body = Expression.Block(Expression.IfThen(Visit(ifNode.Condition), Visit(ifNode.Body)),
            Expression.Constant(true));
    }

    private void VisitError(AstNode node, ref Expression body)
    {
        if (node is not ErrorNode error) return;

        var msg = Expression.Constant(error.Message);
        var addMethod = _errors.Add;

        body = Expression.Block(Expression.Call(_errorsParameterExpression, addMethod.Method, msg),
            Expression.Constant(false));
    }

    private bool VisitName(AstNode node, out Expression expression)
    {
        if (node is NameNode name)
        {
            expression = Expression.MakeMemberAccess(_modelParameterExpression, GetMemberInfo(name));

            return true;
        }

        expression = Expression.Empty();
        return false;
    }

    private MemberInfo GetMemberInfo(NameNode name)
    {
        return typeof(T).GetProperty(name.Token.Text.ToString());
    }

    public LambdaExpression ToLambda(Expression body)
    {
        return Expression.Lambda(body, true, _modelParameterExpression, _errorsParameterExpression);
    }

    private static bool VisitConstant(AstNode node, out Expression visit)
    {
        if(node is not LiteralNode l)
        {
            visit = Expression.Empty();
            return false;
        }

        switch (l.Value)
        {
            case long ci:
                visit = Expression.Constant(ci);
                return true;

            case ulong uic:
                visit = Expression.Constant(uic);
                return true;

            case string cs:
                visit = Expression.Constant(cs);
                return true;

            case bool cb:
                visit = Expression.Constant(cb);
                return true;

            case double cbd:
                visit = Expression.Constant(cbd);
                return true;
            default:
                visit = Expression.Empty();
                return false;
        }
    }

    private void VisitTimeLiteral(AstNode node, ref Expression result)
    {
        if (node is not TimeLiteral timeLiteral)
            return;

        Expression tmp = null;
        for (var index = 0; index < timeLiteral.SubLiterals.Count; index++)
        {
            var subLiteral = timeLiteral.SubLiterals[index];
            var visitTimePostFix = VisitTimePostFix(subLiteral,
                TimeLiteralParselet.TimePostfixConverters[subLiteral.Operator.Text.ToString()]);

            if (index == 0)
            {
                tmp = visitTimePostFix;
                continue;
            }

            tmp = Expression.Add(tmp!, visitTimePostFix);
        }

        result = tmp;
    }

    private void VisitPrefix(AstNode node, ref Expression result)
    {
        if (node is not PrefixOperatorNode prefixOperatorNode)
            return;

        var visited = Visit(prefixOperatorNode.Expr);

        result = prefixOperatorNode.Operator.Text.ToString() switch
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

        if (TimeLiteralParselet.TimePostfixConverters.TryGetValue(postfix.Operator.Text.ToString(), out var timeConverter))
        {
            result = VisitTimePostFix(postfix, timeConverter);

            return;
        }

        result = postfix.Operator.Text.ToString() switch
        {
            "%" => Expression.Divide(Expression.Convert(visited, typeof(double)), Expression.Constant(100.0)),
            _ => result
        };
    }

    private Expression VisitTimePostFix(PostfixOperatorNode node, string timeConverter)
    {
        var arg = Visit(node.Expr);

        if (arg.Type != typeof(double)) arg = Expression.Convert(arg, typeof(double));

        return Expression.Call(typeof(TimeSpan), timeConverter, null, arg);
    }

    private void VisitBinary(AstNode node, ref Expression result)
    {
        if (node is not BinaryOperatorNode binaryOperatorNode)
            return;

        var leftVisited = Visit(binaryOperatorNode.LeftExpr);
        var rightVisited = Visit(binaryOperatorNode.RightExpr);

        result = binaryOperatorNode.Operator.Text.ToString() switch
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

            _ => result
        };
    }
}