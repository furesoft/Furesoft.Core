using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using System.Collections.Generic;
using System.Linq;

namespace TestApp
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