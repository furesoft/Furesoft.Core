using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furesoft.Core.ExpressionEvaluator.AST;
using Furesoft.Core.ExpressionEvaluator.Symbols;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class ExpressionParser
    {
        public Dictionary<string, Module> Modules = new();
        public Scope RootScope = Scope.CreateScope();
        private Binder Binder = new();

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
            Equal.AddParsePoints();

            And.AddParsePoints();
            Or.AddParsePoints();

            Negative.AddParsePoints();
            Mod.AddParsePoints();

            IntervalExpression.AddParsePoints();
            InfinityRef.AddParsePoints();

            PowerOperator.AddParsePoints();
            AbsoluteValueExpression.AddParsePoints();

            UseStatement.AddParsePoints();
        }

        public void AddModule(string name, Scope scope)
        {
            var module = new Module();
            module.Name = name;
            module.Scope = scope;

            if (!Modules.ContainsKey(name))
            {
                Modules.Add(name, module);
            }
        }

        public void AddVariable(string name, double value, Scope scope = null)
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

        public EvaluationResult Evaluate(string src)
        {
            var tree = CodeUnit.LoadFragment(src, "expr").Body;
            var boundTree = Binder.BindTree(tree, this);

            return Evaluate(boundTree);
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

        private EvaluationResult Evaluate(List<CodeObject> boundTree)
        {
            var returnValues = new List<double>();
            var errors = new List<Message>();

            foreach (var node in boundTree)
            {
                returnValues.Add(Evaluate(node));
            }

            /*foreach (var funcs in RootScope.Functions)
            {
                if (funcs.Value.HasAnnotations)
                {
                    errors.AddRange(funcs.Value.Annotations.OfType<Message>());
                }
            }*/

            foreach (var funcs in boundTree)
            {
                errors.AddRange(GetMessagesOfCall(funcs));
            }

            return new() { Values = returnValues, Errors = errors };
        }

        private double Evaluate(CodeObject obj)
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
            else if (obj is UseStatement useStmt)
            {
                return EvaluateUseStatement(useStmt);
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

        private bool EvaluateCondition(BinaryOperator expr, Scope scope)
        {
            if (expr is And rel)
            {
                return EvaluateCondition((BinaryOperator)rel.Left, scope) && EvaluateCondition((BinaryOperator)rel.Right, scope);
            }
            else if (expr is Or o)
            {
                return EvaluateCondition((BinaryOperator)o.Left, scope) || EvaluateCondition((BinaryOperator)o.Right, scope);
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
                    Equal ne => left == right,
                    _ => false,
                };
            }
        }

        private double EvaluateExpression(Expression expr, Scope scope)
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
            else if (expr is Mod mod)
            {
                return EvaluateExpression(mod.Left, scope) % EvaluateExpression(mod.Right, scope);
            }
            else if (expr is AbsoluteValueExpression val)
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

                    if (Binder._argumentConstrains.ContainsKey(fn.Name))
                    {
                        foreach (var c in Binder._argumentConstrains[fn.Name])
                        {
                            (string parameter, Expression condition) constrain = (
                                GetParameterName(c.Condition),
                                Binder.BindExpression(c.Condition, fnScope)
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
                                        call.AttachMessage($"Parameter Constraint Failed On {call._AsString}: {arg.Key} Does Not Match Condition: {constrain.condition._AsString}", MessageSeverity.Error, MessageSource.Resolve);

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

                    try
                    {
                        return (double)importedFn.Invoke(args);
                    }
                    catch (TargetParameterCountException ex)
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

        private double EvaluateUseStatement(UseStatement useStmt)
        {
            if (useStmt.Module is ModuleRef modRef)
            {
                if (modRef.Reference is Module mod)
                {
                    RootScope.ImportScope(mod.Scope);
                }
                else if (modRef.Reference is Scope scope)
                {
                    RootScope.ImportScope(scope);
                }
            }

            return 0;
        }
    }
}
