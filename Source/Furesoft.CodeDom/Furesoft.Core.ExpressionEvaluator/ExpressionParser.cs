using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using System.Globalization;

namespace Furesoft.Core.ExpressionEvaluator;

public class ExpressionParser
{
    public readonly Binder Binder = new();
    public Dictionary<string, Module> Modules = new();
    public Scope RootScope = Scope.CreateScope();

    public ExpressionParser()
    {
        RootScope.AddMacro<RuleForMacro>();
        RootScope.AddMacro<ResolveMacro>();
        RootScope.AddMacro<PlotMacro>();
        RootScope.AddMacro<DeriveMacro>();

        InitOperatorOverloads();
    }

    public static void Init()
    {
        Negative.AddParsePoints();

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

        Mod.AddParsePoints();

        IntervalExpression.AddParsePoints();
        MatrixExpression.AddParsePoints();
        InfinityRef.AddParsePoints();

        PowerOperator.AddParsePoints();
        FunctionCompositorOperator.AddParsePoints();
        AbsoluteValueExpression.AddParsePoints();

        Dot.AddParsePoints();
        DeleteExpression.AddParsePoints();
        ValueSetNode.AddParsePoints();

        UseStatement.AddParsePoints();
        ModuleStatement.AddParsePoints();
        AliasNode.AddParsePoints();
        FactorialOperator.AddParsePoints();

        SetDefinitionNode.AddParsePoints();
        SetDefinitionExpression.AddParsePoints();

        GoesToOperator.AddParsePoints();
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
        if (string.IsNullOrEmpty(src)) return null;
        if (string.IsNullOrWhiteSpace(src)) return null;

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
            scope.Import(type, this);

            if (attr.Dependencies != null)
            {
                foreach (var dep in attr.Dependencies)
                {
                    scope.ImportScope(Modules[dep].Scope);
                }
            }

            AddModule(attr.Name, scope);
        }
        else
        {
            RootScope.Import(type);
        }
    }

    internal ValueType EvaluateExpression(Expression expr, Scope scope)
    {
        if (expr is IEvaluatableExpression e)
        {
            return e.Evaluate(this, scope);
        }
        else if (expr is BinaryArithmeticOperator o)
        {
            return EvaluateOperator(o.Symbol, EvaluateExpression(o.Left, scope), EvaluateExpression(o.Right, scope));
        }
        else if (expr is UnaryOperator unary)
        {
            return EvaluateOperator(unary.Symbol, EvaluateExpression(unary.Expression, scope), null);
        }
        else if (expr is Literal lit)
        {
            return double.Parse(lit.Text, CultureInfo.InvariantCulture);
        }
        else if (expr is Call call && call.Expression is UnresolvedRef nameRef)
        {
            string fnName = nameRef.Reference.ToString();
            string mangledName = fnName + ":" + call.ArgumentCount;

            if (RootScope.Functions.TryGetValue(mangledName, out var fn))
            {
                return EvaluateFunction(scope, call, fnName, fn);
            }
            else if (RootScope.ImportedFunctions.TryGetValue(mangledName, out var importedFn))
            {
                return EvaluateImportedFunction(scope, call, fnName, importedFn);
            }
            else
            {
                call.AttachMessage($"{nameRef._AsString} is not defined", MessageSeverity.Error, MessageSource.Resolve);
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

                if (m.Scope.Functions.TryGetValue(mangledName, out var fn))
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

            if (!scope.Variables.ContainsKey(name))
            {
                scope.Variables.Add(name, EvaluateExpression(ass.Right, scope));
            }
            else
            {
                scope.Variables[name] = EvaluateExpression(ass.Right, scope);
            }

            return 0;
        }
        else if (expr is UnresolvedRef uref && uref.Reference is string name)
        {
            if (scope.Variables.ContainsKey(name))
            {
                return scope.GetVariable(name);
            }
            else
            {
                expr.AttachMessage($"Variable '{name}' is not defined", MessageSeverity.Error, MessageSource.Resolve);

                return 0;
            }
        }
        else
        {
            return 0;
        }
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
            if (call.Arguments != null)
            {
                foreach (var arg in call.Arguments)
                {
                    result.AddRange(GetMessagesOfCall(arg));
                }
            }
        }
        else if (obj is FunctionDefinition f && f.Body.First() is CodeObject n && n.HasAnnotations)
        {
            result.AddRange(GetMessagesOfCall(f.Body.First()));
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
        var returnValues = new List<ValueType>();
        var errors = new List<Message>();

        if (boundTree == null) return new EvaluationResult();

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
                returnValues.Add(result);
            }
        }

        foreach (var funcs in boundTree)
        {
            errors.AddRange(GetMessagesOfCall(funcs));
        }

        return new EvaluationResult { Values = returnValues, Errors = errors, ModuleName = moduleName };
    }

    private ValueType Evaluate(CodeObject obj)
    {
        if (obj is IEvaluatableStatement e)
        {
            e.Evaluate(this);

            return null;
        }
        else if (obj is Block blk)
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
            if (expr == null || (expr.Left is null || expr.Right is null)) return false;

            var left = EvaluateExpression(expr.Left, scope);
            var right = EvaluateExpression(expr.Right, scope);

            return expr switch
            {
                LessThan => left.Get<double>() < right.Get<double>(),
                GreaterThan => left.Get<double>() > right.Get<double>(),
                LessThanEqual => left.Get<double>() <= right.Get<double>(),
                GreaterThanEqual => left.Get<double>() >= right.Get<double>(),
                NotEqual => left != right,
                Equal => left == right,
                _ => false,
            };
        }
    }

    private ValueType EvaluateFunction(Scope scope, Call call, string fnName, FunctionDefinition fn)
    {
        Scope fnScope = Scope.CreateScope(RootScope);

        if (fn.ParameterCount != call.ArgumentCount)
        {
            call.AttachMessage($"Argument Count Mismatch. Expected {fn.ParameterCount} but given {call.ArgumentCount} on {fnName}",
                MessageSeverity.Error, MessageSource.Resolve);

            return 0;
        }

        var argumentAssignments = call.Arguments?.Count(_ => _ is Assignment);

        if (argumentAssignments > 0)
        {
            if (argumentAssignments == call.ArgumentCount)
            {
                var namedArguments = call.Arguments.Select(TransformNamedArgument);

                foreach (var arg in namedArguments)
                {
                    fnScope.Variables.Add(arg.Item1, EvaluateExpression(arg.Item2, scope));
                }
            }
            else
            {
                call.AttachMessage($"Named Arguments are not valid at {call.AsText()}", MessageSeverity.Error, MessageSource.Resolve);

                return 0;
            }
        }
        else
        {
            for (int i = 0; i < fn.ParameterCount; i++)
            {
                fnScope.Variables.Add(fn.Parameters[i].Name, EvaluateExpression(call.Arguments[i], scope));
            }
        }

        //f(y = 2, x = 5)

        //überprüfen ob arguments Assignment enthält
        //alle unresolvedreferences mit deren werten extrahieren
        //reihenfolge der funktion herausfinden
        //argumente des calls in reihenfolde der funktion übersetzen
        //argumente in scope übernehmen

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
                    if (EvaluateNumberRoom(c, arg.Value.Get<double>()))
                    {
                        if (condition == null)
                            continue;

                        if (EvaluateCondition((BinaryOperator)condition, fnScope))
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
        double[] args = call.Arguments.Select(_ => EvaluateExpression(_, scope).Get<double>()).ToArray();

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
        catch
        {
            return 0;
        }
    }

    private bool EvaluateNumberRoom(FunctionArgumentConditionDefinition cond, double value)
    {
        return cond.NumberRoom switch
        {
            "N" => value is > 0 and < uint.MaxValue,
            "Z" => value is > int.MinValue and < int.MaxValue,
            "R" => value is > double.MinValue and < double.MaxValue,
            _ => RootScope.SetDefinitions.ContainsKey(cond.NumberRoom),
        };
    }

    private ValueType EvaluateOperator(string symbol, ValueType left, ValueType right)
    {
        if (RootScope.OperatorOverloads.ContainsKey(symbol))
        {
            var lobjType = left.Get().GetType();
            var robjType = right == null ? typeof(object) : right.Get().GetType();

            var opList = RootScope.OperatorOverloads[symbol];

            if (OpMatchOverload(opList, lobjType, robjType, out var operatorOverload))
            {
                return operatorOverload.Invoker(left, right);
            }
        }

        return null;
    }

    private void InitOperatorOverloads()
    {
        RootScope.AddOperatorOverload<double, double>("+", (l, r) => l + r);
        RootScope.AddOperatorOverload<double, double>("*", (l, r) => l * r);
        RootScope.AddOperatorOverload<double, double>("-", (l, r) => l - r);
        RootScope.AddOperatorOverload<double, double>("/", (l, r) => l / r);
        RootScope.AddOperatorOverload<double, double>("%", (l, r) => l % r);
        RootScope.AddOperatorOverload<double, double>("^", (l, r) => Math.Pow(l, r));

        RootScope.AddOperatorOverload<DenseMatrix, double>("*", (m, scalar) => m.Multiply(scalar));
        RootScope.AddOperatorOverload<DenseMatrix, DenseMatrix>("*", (m, scalar) => m.Multiply(scalar));
        RootScope.AddOperatorOverload<DenseMatrix, DenseMatrix>("+", (m, scalar) => m.Add(scalar));
        RootScope.AddOperatorOverload<DenseMatrix, DenseMatrix>("-", (m, scalar) => m.Subtract(scalar));
        RootScope.AddOperatorOverload<DenseMatrix, double>("/", (m, scalar) => m.Divide(scalar));

        RootScope.AddOperatorOverload<double>("-", (l) => -l);
    }

    private bool OpMatchOverload(List<OperatorOverload> opList, Type lobjType, Type robjType, out OperatorOverload operatorOverload)
    {
        foreach (var op in opList)
        {
            if (op.Left == lobjType && op.Right == robjType || op.Left == lobjType && op.Right == null)
            {
                operatorOverload = op;
                return true;
            }
        }

        operatorOverload = null;
        return false;
    }

    private (string, Expression) TransformNamedArgument(Expression arg)
    {
        if (arg is Assignment assignment
            && assignment.Left is UnresolvedRef argRef
            && argRef.Reference is string argName)
        {
            return (argName, assignment.Right);
        }

        return ("", null);
    }
}