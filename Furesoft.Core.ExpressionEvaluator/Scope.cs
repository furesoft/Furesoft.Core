using Furesoft.Core.ExpressionEvaluator.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Furesoft.Core.ExpressionEvaluator
{
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

        public void Evaluate(string content)
        {
            var ep = new ExpressionParser();

            ep.Evaluate(content);

            ImportScope(ep.RootScope);
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

        public void Import(Type t)
        {
            foreach (var mi in t.GetMethods())
            {
                if (mi.IsStatic && !mi.GetParameters().Select(_ => _.ParameterType).Any(_ => _ != typeof(double)))
                {
                    string funcName = mi.Name.ToLower();
                    var attr = mi.GetCustomAttribute<FunctionNameAttribute>();

                    if (attr != null)
                    {
                        funcName = attr.Name;
                    }

                    if (!ImportedFunctions.ContainsKey(mi.Name.ToLower()))
                    {
                        ImportedFunctions.Add(funcName, new Func<double[], double>(args =>
                        {
                            return (double)mi.Invoke(null, args.Cast<object>().ToArray());
                        }));
                    }
                    else
                    {
                        //override registered function
                        ImportedFunctions[funcName] = new Func<double[], double>(args =>
                        {
                            return (double)mi.Invoke(null, args.Cast<object>().ToArray());
                        });
                    }
                }
            }

            foreach (var field in t.GetFields())
            {
                if (field.IsStatic)
                {
                    Variables.Add(field.Name.ToUpper(), (double)field.GetValue(null));
                }
            }
        }

        public void ImportFunction(string name, Func<double[], double> func)
        {
            ImportedFunctions.Add(name, func);
        }

        public void ImportScope(Scope scope)
        {
            foreach (var item in scope.Variables)
            {
                Variables.Add(item.Key, item.Value);
            }
            foreach (var item in scope.Functions)
            {
                Functions.Add(item.Key, item.Value);
            }
            foreach (var item in scope.ImportedFunctions)
            {
                ImportedFunctions.Add(item.Key, item.Value);
            }
        }
    }
}