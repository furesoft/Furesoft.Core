﻿using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.ExpressionEvaluator.Symbols;
using System.IO;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class UseStatement : Statement, IEvaluatableStatement, IBindable
    {
        public UseStatement(Parser parser, CodeObject parent) :
            base(parser, parent)
        {
        }

        public Expression Module { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("use", Parse);
        }

        public CodeObject Bind(ExpressionParser ep, Binder binder)
        {
            if (Module is UnresolvedRef uref)
            {
                if (ep.Modules.ContainsKey(uref.Reference.ToString()))
                {
                    Module = new ModuleRef(ep.Modules[uref.Reference.ToString()]);
                }
                else
                {
                    AttachMessage($"'{Module._AsString}' is not defined", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else if (Module is Literal)
            {
                var filename = Module._AsString.ToString().Replace("\"", "");

                if (File.Exists(filename))
                {
                    var content = File.ReadAllText(filename);

                    var cep = new ExpressionParser();
                    var contentResult = cep.Evaluate(content);

                    if (contentResult.Errors.Count > 0)
                    {
                        foreach (var msg in contentResult.Errors)
                        {
                            AttachMessage(msg.Text, msg.Severity, msg.Source);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(contentResult.ModuleName))
                        {
                            Module = new ModuleRef(cep.RootScope);
                        }
                        else
                        {
                            ep.AddModule(contentResult.ModuleName, cep.RootScope);

                            Module = new ModuleRef(ep.Modules[contentResult.ModuleName]);
                        }
                    }
                }
                else
                {
                    AttachMessage($"File {Module._AsString} does not exist", MessageSeverity.Error, MessageSource.Resolve);
                }
            }
            else
            {
                AttachMessage($"'{Module._AsString}' is not defined", MessageSeverity.Error, MessageSource.Resolve);
            }

            return this;
        }

        public void Evaluate(ExpressionParser ep)
        {
            if (Module is ModuleRef modRef)
            {
                if (modRef.Reference is Module mod)
                {
                    ep.RootScope.ImportScope(mod.Scope);
                }
                else if (modRef.Reference is Scope scope)
                {
                    ep.RootScope.ImportScope(scope);
                }
            }
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new UseStatement(parser, parent);
            parser.NextToken();

            result.Module = Expression.Parse(parser, result, false, ";");

            return result;
        }
    }
}