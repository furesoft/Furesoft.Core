using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using System.Collections.Generic;
using System.Linq;

namespace TestApp.MathEvaluator
{
    public class Binder
    {
        public static Dictionary<string, List<FunctionArgumentConditionDefinition>> _argumentConstrains = new();

        public static Expression BindExpression(Expression expr, Scope scope)
        {
            if (expr is BinaryOperator op)
            {
                op.Left = BindExpression(op.Left, scope);
                op.Right = BindExpression(op.Right, scope);
            }
            /*else if (expr is UnresolvedRef uref)
            {
                return scope.GetVariable(uref.Reference.ToString());
            }*/

            return expr;
        }

        public static List<CodeObject> BindTree(Block tree)
        {
            var boundTree = new List<CodeObject>();
            foreach (var node in tree)
            {
                boundTree.Add(BindUnrecognized(node, ExpressionParser.RootScope));
            }

            return boundTree;
        }

        private static CodeObject BindAssignment(Assignment a)
        {
            if (a.Left is Call c)
            {
                return BindFunction(c, a.Right);
            }
            else
            {
                return a;
            }
        }

        private static Expression BindConstainCondition(RelationalOperator condition)
        {
            if (condition.Left is RelationalOperator l && condition.Right is Literal r)
            {
                if (condition.Symbol == "<")
                {
                    if (l.Left is UnresolvedRef)
                    {
                        condition.Right = new LessThan(l.Left, condition.Right);
                    }
                    else if (l.Right is UnresolvedRef)
                    {
                        condition.Right = new LessThan(l.Right, condition.Right);
                    }
                }
                else if (condition.Symbol == ">")
                {
                    if (l.Left is UnresolvedRef)
                    {
                        condition.Right = new GreaterThan(l.Left, condition.Right);
                    }
                    else if (l.Right is UnresolvedRef)
                    {
                        condition.Right = new GreaterThan(l.Right, condition.Right);
                    }
                }
                else if (condition.Symbol == "<=")
                {
                    if (l.Left is UnresolvedRef)
                    {
                        condition.Right = new LessThanEqual(l.Left, condition.Right);
                    }
                    else if (l.Right is UnresolvedRef)
                    {
                        condition.Right = new LessThanEqual(l.Right, condition.Right);
                    }
                }
                else if (condition.Symbol == ">=")
                {
                    if (l.Left is UnresolvedRef)
                    {
                        condition.Right = new GreaterThanEqual(l.Left, condition.Right);
                    }
                    else if (l.Right is UnresolvedRef)
                    {
                        condition.Right = new GreaterThanEqual(l.Right, condition.Right);
                    }
                }
                else if (condition.Symbol == "!=")
                {
                    if (l.Left is UnresolvedRef)
                    {
                        condition.Right = new NotEqual(l.Left, condition.Right);
                    }
                    else if (l.Right is UnresolvedRef)
                    {
                        condition.Right = new NotEqual(l.Right, condition.Right);
                    }
                }

                return new And(condition.Left, condition.Right);
            }

            return condition;
        }

        private static CodeObject BindFunction(Call c, Expression right)
        {
            var md = new FunctionDefinition(c.Expression._AsString);

            md.Parameters.AddRange(c.Arguments.Select(_ =>
                new ParameterDecl(_.AsString(), new TypeRef(typeof(int)))));

            md.Body.Add(right);

            return md;
        }

        private static Expression BindInterval(IntervalExpression interval, Expression variable)
        {
            return new And(BindMinimum(interval, variable), BindMaximum(interval, variable));
        }

        private static Expression BindMaximum(IntervalExpression interval, Expression variable)
        {
            if (interval.IsMaximumInclusive)
            {
                return new LessThanEqual(variable, interval.Maximum);
            }
            else
            {
                return new LessThan(variable, interval.Maximum);
            }
        }

        private static Expression BindMinimum(IntervalExpression interval, Expression variable)
        {
            if (interval.IsMinimumInclusive)
            {
                if (interval.Minimum is Negative neg && neg.Expression is InfinityRef infinity)
                {
                    interval.Minimum = BindNegInfinity((FunctionArgumentConditionDefinition)interval.Parent);
                }
                if (interval.Maximum is InfinityRef)
                {
                    interval.Maximum = BindPosInfinity((FunctionArgumentConditionDefinition)interval.Parent);
                }

                return new GreaterThanEqual(variable, interval.Minimum);
            }
            else
            {
                return new GreaterThan(variable, interval.Minimum);
            }
        }

        private static Expression BindNegInfinity(FunctionArgumentConditionDefinition facd)
        {
            return facd.NumberRoom switch
            {
                "N" => uint.MinValue,
                "Z" => int.MinValue,
                "R" => double.MinValue,
                _ => false,
            };
        }

        private static Expression BindPosInfinity(FunctionArgumentConditionDefinition facd)
        {
            return facd.NumberRoom switch
            {
                "N" => uint.MaxValue,
                "Z" => int.MaxValue,
                "R" => double.MaxValue,
                _ => false,
            };
        }

        private static CodeObject BindUnrecognized(CodeObject fdef, Scope scope)
        {
            if (fdef is Unrecognized u)
            {
                foreach (var expr in u.Expressions)
                {
                    if (expr is Assignment a)
                    {
                        return BindAssignment(a);
                    }
                    else
                    {
                        return BindExpression(expr, scope);
                    }
                }
            }
            else if (fdef is Assignment a)
            {
                return BindAssignment(a);
            }
            else if (fdef is Expression expr)
            {
                return BindExpression(expr, scope);
            }
            else if (fdef is FunctionArgumentConditionDefinition facd)
            {
                if (facd.Condition is RelationalOperator rel)
                {
                    facd.Condition = BindConstainCondition(rel);
                }
                else if (facd.Condition is And an)
                {
                    if (an.Left is RelationalOperator l)
                    {
                        an.Left = BindConstainCondition(l);
                    }
                    if (an.Right is RelationalOperator r)
                    {
                        an.Right = BindConstainCondition(r);
                    }
                }
                else if (facd.Condition is IntervalExpression interval)
                {
                    facd.Condition = BindInterval(interval, facd.Parameter);
                }

                if (_argumentConstrains.ContainsKey(facd.Function))
                {
                    _argumentConstrains[facd.Function].Add(facd);
                }
                else
                {
                    var constrains = new List<FunctionArgumentConditionDefinition>();
                    constrains.Add(facd);

                    _argumentConstrains.Add(facd.Function, constrains);
                }

                return facd;
            }

            return fdef;
        }
    }
}