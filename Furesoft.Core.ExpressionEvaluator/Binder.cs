using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
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
        public Dictionary<string, List<FunctionArgumentConditionDefinition>> ArgumentConstraints = new();

        public ExpressionParser ExpressionParser { get; set; }

        public Expression BindExpression(Expression expr, Scope scope)
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

        public List<CodeObject> BindTree(Block tree, ExpressionParser expressionParser)
        {
            var boundTree = new List<CodeObject>();

            ExpressionParser = expressionParser;

            foreach (var node in tree)
            {
                boundTree.Add(BindUnrecognized(node, expressionParser.RootScope));
            }

            return boundTree;
        }

        public CodeObject BindUnrecognized(CodeObject fdef, Scope scope, ExpressionParser expressionParser = null)
        {
            if (expressionParser != null)
            {
                ExpressionParser = expressionParser;
            }

            if (fdef is IBindable b)
            {
                return b.Bind(ExpressionParser, this);
            }
            else if (fdef is Unrecognized u)
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

            return fdef;
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

        private static CodeObject BindFunction(Call c, Expression right)
        {
            var md = new FunctionDefinition(c.Expression._AsString);

            md.Parameters.AddRange(c.Arguments.Select(_ =>
                new ParameterDecl(_.AsString(), new TypeRef(typeof(int)))));

            md.Body.Add(right);

            return md;
        }

        private CodeObject BindUseStatement(UseStatement useStmt)
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