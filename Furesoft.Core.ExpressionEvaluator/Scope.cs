namespace Furesoft.Core.ExpressionEvaluator
{
    public class Scope
    {
        public Dictionary<string, Expression> Aliases = new();
        public Dictionary<string, FunctionDefinition> Functions = new();
        public Dictionary<string, Func<double[], double>> ImportedFunctions = new();
        public Dictionary<string, Macro> Macros = new();

        public Dictionary<string, List<OperatorOverload>> OperatorOverloads = new();
        public Dictionary<string, Expression> SetDefinitions = new();
        public Dictionary<string, ValueType> Variables = new();
        public Macro Initializer { get; set; }

        public Scope Parent { get; set; }

        public static Scope CreateScope(Scope parent = null)
        {
            return new Scope { Parent = parent };
        }

        public void AddMacro<T>() where T : Macro, new()
        {
            var instance = new T();

            AddMacro(instance);
        }

        public void AddMacro(Macro instance)
        {
            if (!Macros.ContainsKey(instance.Name))
            {
                Macros.Add(instance.Name, instance);
            }
        }

        public void AddOperatorOverload<TLeft, TRight>(string symbol, Func<TLeft, TRight, ValueType> invoker)
        {
            var opOverload = new OperatorOverload();
            opOverload.Symbol = symbol;
            opOverload.Invoker = (l, r) =>
            invoker.Invoke((TLeft)((ValueType)l).Get(), (TRight)((ValueType)r).Get());
            opOverload.Left = typeof(TLeft);
            opOverload.Right = typeof(TRight);

            if (OperatorOverloads.ContainsKey(symbol))
            {
                OperatorOverloads[symbol].Add(opOverload);
            }
            else
            {
                var l = new List<OperatorOverload>();
                l.Add(opOverload);

                OperatorOverloads.Add(symbol, l);
            }
        }

        public void AddOperatorOverload<T>()
            where T : INumber<T>
        {
            AddOperatorOverload<T, T>("+", (l, r) => (double)Convert.ChangeType(l + r, typeof(double)));
            AddOperatorOverload<T, T>("-", (l, r) => (double)Convert.ChangeType(l - r, typeof(double)));
        }

        public void AddOperatorOverload<TLeft>(string symbol, Func<TLeft, ValueType> invoker)
        {
            var opOverload = new OperatorOverload();
            opOverload.Symbol = symbol;
            opOverload.Invoker = (l, r) => invoker.Invoke((TLeft)((ValueType)l).Get());
            opOverload.Left = typeof(TLeft);

            if (OperatorOverloads.ContainsKey(symbol))
            {
                OperatorOverloads[symbol].Add(opOverload);
            }
            else
            {
                var l = new List<OperatorOverload>();
                l.Add(opOverload);

                OperatorOverloads.Add(symbol, l);
            }
        }

        public void Evaluate(string content)
        {
            var ep = new ExpressionParser();

            ep.Evaluate(content);

            ImportScope(ep.RootScope);
        }

        public ValueType GetVariable(string name)
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

        public void Import(Type t, ExpressionParser parser = null)
        {
            var methods = t.GetMethods().Where(_ =>
            _.GetParameters().Any(_ => _.ParameterType == typeof(double))
            || _.GetParameters().Any(_ => _.ParameterType == typeof(MacroContext)));

            foreach (var mi in methods)
            {
                if (mi.IsStatic)
                {
                    string funcName = mi.Name.ToLower();
                    var attr = mi.GetCustomAttribute<FunctionNameAttribute>();
                    var macroAttr = mi.GetCustomAttribute<MacroAttribute>();

                    if (attr != null)
                    {
                        funcName = attr.Name;
                    }

                    if (macroAttr != null)
                    {
                        var m = new ReflectionMacro(funcName, new Func<MacroContext, Expression[], Expression>((c, args) =>
                        {
                            var na = new List<object>();
                            na.Add(c);

                            var parameters = mi.GetParameters();

                            if (parameters.Skip(1).First().ParameterType == typeof(Expression[]))
                            {
                                na.Add(args);
                            }
                            else
                            {
                                na.AddRange(args);
                            }

                            if (args.Length == 0)
                            {
                                na.Add(Array.Empty<Expression>());
                            }

                            return (Expression)mi.Invoke(null, na.ToArray());
                        }));

                        if (macroAttr.IsInitializer)
                        {
                            Initializer = m;
                        }
                        else
                        {
                            if (parser != null)
                            {
                                parser.RootScope.AddMacro(m);
                            }
                            else
                            {
                                AddMacro(m);
                            }
                        }
                    }
                    else
                    {
                        string mangledName = funcName + ":" + mi.GetParameters().Length;
                        if (!ImportedFunctions.ContainsKey(mangledName))
                        {
                            ImportedFunctions.Add(mangledName, new Func<double[], double>(args =>
                            {
                                try
                                {
                                    return (double)mi.Invoke(null, args.Cast<object>().ToArray());
                                }
                                catch
                                {
                                    return 1;
                                }
                            }));
                        }
                        else
                        {
                            //override registered function
                            ImportedFunctions[mangledName] = new Func<double[], double>(args =>
                            {
                                return (double)mi.Invoke(null, args.Cast<object>().ToArray());
                            });
                        }
                    }
                }
            }

            foreach (var field in t.GetFields())
            {
                if (!field.IsStatic)
                {
                    continue;
                }

                if (t.IsEnum)
                {
                    Variables.Add(field.Name.ToLower(), (int)field.GetValue(null));
                }
                else if (field.FieldType.Name is "Double" or "Single" or "Float")
                {
                    Variables.Add(field.Name.ToUpper(), (double)Convert.ChangeType(field.GetValue(null), typeof(double)));
                }
                else
                {
                    //import submodule
                    parser.Import(field.FieldType);
                }
            }
        }

        public void ImportFunction(string name, Func<double[], double> func)
        {
            ImportedFunctions.Add(name, func);
        }

        public void ImportScope(Scope scope)
        {
            foreach (var variableDefinition in scope.Variables)
            {
                Variables.Add(variableDefinition.Key, variableDefinition.Value);
            }
            foreach (var functionDefinition in scope.Functions)
            {
                Functions.Add(functionDefinition.Key, functionDefinition.Value);
            }
            foreach (var importedFunction in scope.ImportedFunctions)
            {
                ImportedFunctions.Add(importedFunction.Key, importedFunction.Value);
            }
            foreach (var macro in scope.Macros)
            {
                if (!Macros.ContainsKey(macro.Key))
                {
                    Macros.Add(macro.Key, macro.Value);
                }
            }
            foreach (var aliasDefinition in scope.Aliases)
            {
                Aliases.Add(aliasDefinition.Key, aliasDefinition.Value);
            }
            foreach (var setDefinition in scope.SetDefinitions)
            {
                SetDefinitions.Add(setDefinition.Key, setDefinition.Value);
            }

            if (scope.Initializer != null)
            {
                scope.Initializer.Invoke(new MacroContext(null, null, this));
            }
        }
    }

   
}