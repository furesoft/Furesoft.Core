using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestApp
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; }
        public double Value { get; set; }
    }

    public class ExpressionParser
    {
        public static Scope RootScope = Scope.CreateScope();

        private static Dictionary<string, List<FunctionArgumentConditionDefinition>> _argumentConstrains = new();

        public static void AddVariable(string name, int value, Scope scope = null)
        {
            if (scope == null)
            {
                RootScope.Variables.Add(name, value);
            }
            else
            {
                scope.Variables.Add(name, value);
            }
        }

        public static EvaluationResult Evaluate(string src)
        {
            var tree = CodeUnit.LoadFragment(src, "expr").Body;
            var boundTree = new List<CodeObject>();
            foreach (var node in tree)
            {
                boundTree.Add(BindUnrecognized(node, RootScope));
            }

            return Evaluate(boundTree);
        }

        public static void Init()
        {
            Add.AddParsePoints();
            Multiply.AddParsePoints();
            Divide.AddParsePoints();
            Subtract.AddParsePoints();
            Assignment.AddParsePoints();
            Call.AddParsePoints();
            Expression.AddParsePoints();
            FunctionArgumentConditionDefinition.AddParsePoints();

            GreaterThan.AddParsePoints();
            LessThan.AddParsePoints();
            GreaterThanEqual.AddParsePoints();
            LessThanEqual.AddParsePoints();
            NotEqual.AddParsePoints();

            Negative.AddParsePoints();

            IntervalExpression.AddParsePoints();
            InfinityRef.AddParsePoints();

            PowerOperator.AddParsePoints();
            ValueExpression.AddParsePoints();
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

        private static Expression BindExpression(Expression expr, Scope scope)
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
                facd.Condition = BindConstainCondition((RelationalOperator)facd.Condition);

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

        private static EvaluationResult Evaluate(List<CodeObject> boundTree)
        {
            double returnValue = 0;
            var errors = new List<Message>();

            foreach (var node in boundTree)
            {
                returnValue = Evaluate(node);
            }

            foreach (var funcs in RootScope.Functions)
            {
                if (funcs.Value.HasAnnotations)
                {
                    errors.AddRange(funcs.Value.Annotations.OfType<Message>());
                }
            }

            foreach (var funcs in boundTree)
            {
                errors.AddRange(GetMessagesOfCall(funcs));
            }

            return new() { Value = returnValue, Errors = errors };
        }

        private static double Evaluate(CodeObject obj)
        {
            if (obj is Block blk)
            {
                foreach (var cn in blk)
                {
                    Evaluate(cn);
                }
            }
            else if (obj is Expression expr)
            {
                return EvaluateExpression(expr, RootScope);
            }
            else if (obj is FunctionDefinition funcDef)
            {
                RootScope.Functions.Add(funcDef.Name, funcDef);
            }
            else if (obj is FunctionArgumentConditionDefinition)
            {
                return 0;
            }

            return 0;
        }

        private static bool EvaluateCondition(BinaryOperator expr, Scope scope)
        {
            //ToDo: capsulate more than one condition on param to (&& Expression) then evaluate here

            if (expr is And rel)
            {
                return EvaluateCondition((BinaryOperator)rel.Left, scope) && EvaluateCondition((BinaryOperator)rel.Right, scope);
            }
            else
            {
                var left = EvaluateExpression(expr.Left, scope);
                var right = EvaluateExpression(expr.Right, scope);

                return expr switch
                {
                    LessThan lt => left < right,
                    GreaterThan gt => left > right,
                    LessThanEqual gt => left <= right,
                    GreaterThanEqual gt => left >= right,
                    NotEqual ne => left != right,
                    _ => false,
                };
            }
        }

        private static double EvaluateExpression(Expression expr, Scope scope)
        {
            if (expr is Add add)
            {
                return EvaluateExpression(add.Left, scope) + EvaluateExpression(add.Right, scope);
            }
            else if (expr is Multiply mul)
            {
                return EvaluateExpression(mul.Left, scope) * EvaluateExpression(mul.Right, scope);
            }
            else if (expr is Subtract sub)
            {
                return EvaluateExpression(sub.Left, scope) - EvaluateExpression(sub.Right, scope);
            }
            else if (expr is Divide div)
            {
                return EvaluateExpression(div.Left, scope) / EvaluateExpression(div.Right, scope);
            }
            else if (expr is PowerOperator pow)
            {
                return Math.Pow(EvaluateExpression(pow.Left, scope), EvaluateExpression(pow.Right, scope));
            }
            else if (expr is ValueExpression val)
            {
                return Math.Abs(EvaluateExpression(val.Expression, scope));
            }
            else if (expr is Literal lit)
            {
                return double.Parse(lit.Text.Replace("d", ""));
            }
            else if (expr is Negative neg)
            {
                return -EvaluateExpression(neg.Expression, scope);
            }
            else if (expr is Call call && call.Expression is UnresolvedRef nameRef)
            {
                string fnName = nameRef.Reference.ToString();
                if (RootScope.Functions.TryGetValue(fnName, out var fn))
                {
                    Scope fnScope = Scope.CreateScope(RootScope);

                    if (fn.ParameterCount != call.ArgumentCount)
                    {
                        call.AttachMessage($"Argument Count Mismatch. Expected {fn.ParameterCount} but given {call.ArgumentCount} on {fnName}",
                            MessageSeverity.Error, MessageSource.Resolve);

                        return 0;
                    }

                    for (int i = 0; i < fn.ParameterCount; i++)
                    {
                        fnScope.Variables.Add(fn.Parameters[i].Name, EvaluateExpression(call.Arguments[i], scope));
                    }

                    if (_argumentConstrains.ContainsKey(fn.Name))
                    {
                        foreach (var c in _argumentConstrains[fn.Name])
                        {
                            (string parameter, Expression condition) constrain = (
                                GetParameterName(c.Condition),
                                BindExpression(c.Condition, fnScope)
                            );

                            foreach (var arg in fnScope.Variables)
                            {
                                if (EvaluateNumberRoom(c, arg.Value))
                                {
                                    if (EvaluateCondition((BinaryBooleanOperator)constrain.condition, fnScope))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        fn.AttachMessage($"'{arg.Key}' is not in range '{constrain.condition}'", MessageSeverity.Error, MessageSource.Resolve);

                                        return 0;
                                    }
                                }
                                else
                                {
                                    fn.AttachMessage($"'{arg.Key}' is not in {c.NumberRoom}", MessageSeverity.Error, MessageSource.Resolve);

                                    return 0;
                                }
                            }
                        }
                    }

                    return EvaluateExpression((Expression)fn.Body.First(), fnScope);
                }
                else if (RootScope.ImportedFunctions.TryGetValue(fnName, out var importedFn))
                {
                    double[] args = call.Arguments.Select(_ => EvaluateExpression(_, scope)).ToArray();

                    if (MatchesArguments(importedFn.Method, args))
                    {
                        return (double)importedFn.Invoke(args);
                    }
                    else
                    {
                        call.AttachMessage($"Argument Count Mismatch. Expected {importedFn.Method.GetParameters().Count()} given {args.Length} on {fnName}",
                            MessageSeverity.Error, MessageSource.Resolve);

                        return 0;
                    }
                }

                return 0;
            }
            else if (expr is UnresolvedRef uref)
            {
                return scope.GetVariable(uref.Reference.ToString());
            }
            else
            {
                return 0;
            }
        }

        private static bool EvaluateNumberRoom(FunctionArgumentConditionDefinition cond, double value)
        {
            return cond.NumberRoom switch
            {
                "N" => value is > 0 and < uint.MaxValue,
                "Z" => value is > int.MinValue and < int.MaxValue,
                "R" => value is > double.MinValue and < double.MaxValue,
                _ => false,
            };
        }

        private static IEnumerable<Message> GetMessagesOfCall(CodeObject obj)
        {
            var result = new List<Message>();

            if (obj.HasAnnotations)
            {
                result.AddRange(obj.Annotations.OfType<Message>());
            }

            if (obj is Call call)
            {
                foreach (var arg in call.Arguments)
                {
                    result.AddRange(GetMessagesOfCall(arg));
                }
            }
            else if (obj is Negative neg)
            {
                result.AddRange(GetMessagesOfCall(neg.Expression));
            }

            return result;
        }

        private static string GetParameterName(Expression condition)
        {
            if (condition is UnresolvedRef uref)
            {
                return uref.Reference.ToString();
            }
            else if (condition is BinaryBooleanOperator bbool)
            {
                var left = GetParameterName(bbool.Left);
                var right = GetParameterName(bbool.Right);

                return left ?? right;
            }

            return null;
        }

        private static bool MatchesArguments(MethodInfo mi, double[] parameters)
        {
            return mi.GetParameters().Count() == parameters.Length;
        }

        public class Scope
        {
            public Dictionary<string, FunctionDefinition> Functions = new();
            public Dictionary<string, Func<double[], double>> ImportedFunctions = new();
            public Dictionary<string, double> Variables = new();
            public Scope Parent { get; set; }

            public static Scope CreateScope(Scope parent = null)
            {
                return new Scope { Parent = parent };
            }

            public double GetVariable(string name)
            {
                Scope currentScope = this;

                while (currentScope != null)
                {
                    if (currentScope.Variables.ContainsKey(name))
                    {
                        return currentScope.Variables[name];
                    }

                    currentScope = currentScope.Parent;
                }

                return 0;
            }

            public void ImportFunction(string name, Func<double[], double> func)
            {
                ImportedFunctions.Add(name, func);
            }
        }
    }
}