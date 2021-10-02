using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.ExpressionEvaluator.AST;
using Furesoft.Core.ExpressionEvaluator.Symbols;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class ExpressionParser
    {
        public Dictionary<string, Module> Modules = new();
        public Scope RootScope = Scope.CreateScope();
        private readonly Binder Binder = new();

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

            Dot.AddParsePoints();

            UseStatement.AddParsePoints();
            ModuleStatement.AddParsePoints();
        }

        public void AddModule(string name, Scope scope)
        {
            var module = new Module
            {
                Name = name,
                Scope = scope
            };

            if (!Modules.ContainsKey(name))
            {
                Modules.Add(name, module);
            }
            else
            {
                Modules[name].Scope.ImportScope(scope);
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

        public void Import(Type type)
        {
            var attr = type.GetCustomAttribute<ModuleAttribute>();

            if (attr != null)
            {
                var scope = new Scope();
                scope.Import(type);

                AddModule(attr.Name, scope);
            }
            else
            {
                RootScope.Import(type);
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

        private EvaluationResult Evaluate(List<CodeObject> boundTree)
        {
            var returnValues = new List<double>();
            var errors = new List<Message>();

            string moduleName = "";
            foreach (var node in boundTree)
            {
                if (node is ModuleStatement m && m.ModuleName is UnresolvedRef uref && uref.Reference is string modName)
                {
                    moduleName = modName;

                    continue;
                }

                var result = Evaluate(node);

                if (result != null)
                {
                    returnValues.Add(result.Value);
                }
            }

            foreach (var funcs in boundTree)
            {
                errors.AddRange(GetMessagesOfCall(funcs));
            }

            return new() { Values = returnValues, Errors = errors, ModuleName = moduleName };
        }

        private double? Evaluate(CodeObject obj)
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

                return null;
            }
            else if (obj is FunctionArgumentConditionDefinition)
            {
                return null;
            }

            return null;
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
                    LessThan => left < right,
                    GreaterThan => left > right,
                    LessThanEqual => left <= right,
                    GreaterThanEqual => left >= right,
                    NotEqual => left != right,
                    Equal => left == right,
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
                return double.Parse(lit.Text.Replace("d", ""), CultureInfo.InvariantCulture);
            }
            else if (expr is Negative neg)
            {
                return -EvaluateExpression(neg.Expression, scope);
            }
            else if (expr is Call call && call.Expression is UnresolvedRef nameRef)
            {
                string fnName = nameRef.Reference.ToString();
                string mangledName = fnName + ":" + call.ArgumentCount;

                if (RootScope.Functions.TryGetValue(fnName, out var fn))
                {
                    return EvaluateFunction(scope, call, fnName, fn);
                }
                else if (RootScope.ImportedFunctions.TryGetValue(mangledName, out var importedFn))
                {
                    return EvaluateImportedFunction(scope, call, fnName, importedFn);
                }

                return 0;
            }
            else if (expr is ModuleFunctionRef funcRef && funcRef.Reference is Module m)
            {
                Scope s = Scope.CreateScope(RootScope);
                if (funcRef.Call.Expression is UnresolvedRef nr)
                {
                    string fnName = nr.Reference.ToString();
                    string mangledName = fnName + ":" + funcRef.Call.ArgumentCount;

                    if (m.Scope.Functions.TryGetValue(fnName, out var fn))
                    {
                        return EvaluateFunction(s, funcRef.Call, fnName, fn);
                    }
                    else if (m.Scope.ImportedFunctions.TryGetValue(mangledName, out var importedFn))
                    {
                        return EvaluateImportedFunction(scope, funcRef.Call, fnName, importedFn);
                    }
                }

                return 0;
            }
            else if (expr is Assignment ass && ass.Left is UnresolvedRef ureff)
            {
                var name = ureff.Reference.ToString();

                scope.Variables.Add(name, EvaluateExpression(ass.Right, scope));

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

        private double EvaluateFunction(Scope scope, Call call, string fnName, FunctionDefinition fn)
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

            if (Binder.ArgumentConstraints.ContainsKey(fn.Name))
            {
                foreach (var c in Binder.ArgumentConstraints[fn.Name])
                {
                    (string parameter, Expression condition) = (
                        GetParameterName(c.Condition),
                        Binder.BindExpression(c.Condition, fnScope)
                    );

                    foreach (var arg in fnScope.Variables)
                    {
                        if (EvaluateNumberRoom(c, arg.Value))
                        {
                            if (condition == null)
                                continue;

                            if (EvaluateCondition((BinaryBooleanOperator)condition, fnScope))
                            {
                                continue;
                            }
                            else
                            {
                                call.AttachMessage($"Parameter Constraint Failed On {call._AsString}: {arg.Key} Does Not Match Condition: {condition._AsString}", MessageSeverity.Error, MessageSource.Resolve);

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

        private double EvaluateImportedFunction(Scope scope, Call call, string fnName, Func<double[], double> importedFn)
        {
            double[] args = call.Arguments.Select(_ => EvaluateExpression(_, scope)).ToArray();

            try
            {
                return (double)importedFn.Invoke(args);
            }
            catch (TargetParameterCountException)
            {
                call.AttachMessage($"Argument Count Mismatch. Expected {importedFn.Method.GetParameters().Length} given {args.Length} on {fnName}",
                    MessageSeverity.Error, MessageSource.Resolve);

                return 0;
            }
        }

        private double? EvaluateUseStatement(UseStatement useStmt)
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

            return null;
        }
    }
}