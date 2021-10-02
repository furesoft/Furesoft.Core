using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.ExpressionEvaluator.AST;
using Furesoft.Core.ExpressionEvaluator.Symbols;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class Binder
    {
        public static Dictionary<string, List<FunctionArgumentConditionDefinition>> ArgumentConstraints = new();

        public static ExpressionParser ExpressionParser { get; set; }

        public static Expression BindExpression(Expression expr, Scope scope)
        {
            if (expr is BinaryOperator op)
            {
                op.Left = BindExpression(op.Left, scope);
                op.Right = BindExpression(op.Right, scope);
            }
            else if (expr is Negative neg)
            {
                neg.Expression = BindExpression(neg.Expression, scope);
            }
            else if (expr is Call call && call.Expression is Dot dot)
            {
                var moduleRef = (SymbolicRef)dot.Left;
                var funcRef = dot.Right;

                if (ExpressionParser.Modules.TryGetValue(moduleRef.Reference.ToString(), out var module))
                {
                    call.Expression = funcRef;

                    return new ModuleFunctionRef(module, call);
                }
                else
                {
                    call.AttachMessage($"Module '{moduleRef.Reference}' not found on call {call._AsString}", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else if (expr is Call c)
            {
                if (c.Expression is UnresolvedRef unresolved)
                {
                    if (ExpressionParser.RootScope.Aliases.ContainsKey(unresolved.Reference.ToString()))
                    {
                        c.Expression = ExpressionParser.RootScope.Aliases[unresolved.Reference.ToString()];

                        return BindExpression(c, scope);
                    }
                }

                for (int i = 0; i < c.Arguments.Count; i++)
                {
                    Expression arg = BindExpression(c.Arguments[i], scope);

                    c.Arguments[i] = arg;
                }
            }
            else if (expr is UnresolvedRef unresolved)
            {
                if (ExpressionParser.RootScope.Aliases.ContainsKey(unresolved.Reference.ToString()))
                {
                    return ExpressionParser.RootScope.Aliases[unresolved.Reference.ToString()];
                }
            }

            return expr;
        }

        public static List<CodeObject> BindTree(Block tree, ExpressionParser expressionParser)
        {
            var boundTree = new List<CodeObject>();

            ExpressionParser = expressionParser;

            foreach (var node in tree)
            {
                boundTree.Add(BindUnrecognized(node, expressionParser.RootScope));
            }

            return boundTree;
        }

        private static CodeObject BindAlias(AliasNode aliasNode)
        {
            if (aliasNode.Name is UnresolvedRef nameRef)
            {
                string name = nameRef.Reference.ToString();

                if (!ExpressionParser.RootScope.Aliases.ContainsKey(name))
                {
                    ExpressionParser.RootScope.Aliases.Add(name, aliasNode.Value);
                }
                else
                {
                    aliasNode.AttachMessage($"Alias '{name}' already exists.", MessageSeverity.Error, MessageSource.Resolve);
                }
            }

            return aliasNode;
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
            if (condition.Left is RelationalOperator l && condition.Right is Literal)
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
                if (interval.Minimum is Negative neg && neg.Expression is InfinityRef)
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
            else if (fdef is UseStatement useStmt)
            {
                return BindUseStatement(useStmt);
            }
            else if (fdef is AliasNode aliasNode)
            {
                return BindAlias(aliasNode);
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

                if (ArgumentConstraints.ContainsKey(facd.Function))
                {
                    ArgumentConstraints[facd.Function].Add(facd);
                }
                else
                {
                    var constrains = new List<FunctionArgumentConditionDefinition>
                    {
                        facd
                    };

                    ArgumentConstraints.Add(facd.Function, constrains);
                }

                return facd;
            }

            return fdef;
        }

        private static CodeObject BindUseStatement(UseStatement useStmt)
        {
            if (useStmt.Module is UnresolvedRef uref)
            {
                if (ExpressionParser.Modules.ContainsKey(uref.Reference.ToString()))
                {
                    useStmt.Module = new ModuleRef(ExpressionParser.Modules[uref.Reference.ToString()]);
                }
                else
                {
                    useStmt.AttachMessage($"'{useStmt.Module._AsString}' is not defined", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else if (useStmt.Module is Literal)
            {
                var filename = useStmt.Module._AsString.ToString().Replace("\"", "");

                if (File.Exists(filename))
                {
                    var content = File.ReadAllText(filename);

                    var ep = new ExpressionParser();
                    var contentResult = ep.Evaluate(content);

                    if (contentResult.Errors.Count > 0)
                    {
                        foreach (var msg in contentResult.Errors)
                        {
                            useStmt.AttachMessage(msg.Text, msg.Severity, msg.Source);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(contentResult.ModuleName))
                        {
                            useStmt.Module = new ModuleRef(ep.RootScope);
                        }
                        else
                        {
                            ExpressionParser.AddModule(contentResult.ModuleName, ep.RootScope);

                            useStmt.Module = new ModuleRef(ExpressionParser.Modules[contentResult.ModuleName]);
                        }
                    }
                }
                else
                {
                    useStmt.AttachMessage($"File {useStmt.Module._AsString} does not exist", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else
            {
                useStmt.AttachMessage($"'{useStmt.Module._AsString}' is not defined", MessageSeverity.Error, MessageSource.Resolve);
            }

            return useStmt;
        }
    }
}