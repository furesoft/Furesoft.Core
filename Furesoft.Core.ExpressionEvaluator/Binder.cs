using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
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
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class Binder
    {
        public Dictionary<string, List<FunctionArgumentConditionDefinition>> ArgumentConstraints = new();

        public Dictionary<string, (object min, object max)> NumberRooms = new()
        {
            ["N"] = (uint.MinValue, uint.MaxValue),
            ["Z"] = (int.MinValue, int.MaxValue),
            ["R"] = (double.MinValue, double.MaxValue),
        };

        public ExpressionParser ExpressionParser { get; set; }

        public Expression BindExpression(Expression expr, Scope scope)
        {
            if (expr is IBindable b)
            {
                return (Expression)b.Bind(ExpressionParser, this);
            }

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
                if (dot.Left is Dot)
                {
                    dot.Left = new UnresolvedRef(dot.Left._AsString);
                }

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
                if (c.Expression is UnresolvedRef unresolved && unresolved.Reference is string s)
                {
                    if (ExpressionParser.RootScope.Aliases.ContainsKey(s))
                    {
                        c.Expression = ExpressionParser.RootScope.Aliases[s];

                        return BindExpression(c, scope);
                    }
                    else if (ExpressionParser.RootScope.Macros.ContainsKey(s))
                    {
                        var arguments = c.CreateArguments();

                        var macro = ExpressionParser.RootScope.Macros[s];

                        var mc = new MacroContext(ExpressionParser, c.Parent, scope);

                        return macro.Invoke(mc, arguments.ToArray());
                    }
                }

                for (int i = 0; i < c.Arguments?.Count; i++)
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

            //Bind Variable from Module: module.variable
            if (expr is Dot d && d.Left is UnresolvedRef lef && lef.Reference is string left
                && d.Right is UnresolvedRef r && r.Reference is string right)
            {
                var s = ExpressionParser.Modules[left].Scope;
                if (s.Variables.ContainsKey(right))
                {
                    return s.Variables[right];
                }
            }

            return expr;
        }

        public Expression BindNumberRoom(Expression expr, UnresolvedRef reference = null)
        {
            if (expr is UnresolvedRef uref && uref.Reference is string name)
            {
                if (ExpressionParser.RootScope.SetDefinitions.ContainsKey(name))
                {
                    return BindConditionParameter(ExpressionParser.RootScope.SetDefinitions[name], reference);
                }
                else if (NumberRooms.ContainsKey(name))
                {
                    // $x > min && $x < max

                    if (reference == null)
                    {
                        reference = new("$x");
                    }

                    var numberRoom = NumberRooms[name];

                    return BindConditionParameter(new And(new GreaterThan(reference, new Literal(numberRoom.min)), new LessThan(reference, new Literal(numberRoom.max))), reference);
                }
            }

            return expr;
        }

        public List<CodeObject> BindTree(Block tree, ExpressionParser expressionParser)
        {
            if (tree == null) return null;

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
                        return BindAssignment(a, scope);
                    }
                    else
                    {
                        return BindExpression(expr, scope);
                    }
                }
            }
            else if (fdef is Assignment a)
            {
                return BindAssignment(a, scope);
            }
            else if (fdef is Expression expr)
            {
                return BindExpression(expr, scope);
            }

            return fdef;
        }

        private CodeObject BindAssignment(Assignment a, Scope scope)
        {
            if (a.Left is Call c)
            {
                a.Right = BindExpression(a.Right, scope);

                return BindFunction(c, a.Right);
            }
            else
            {
                a.Right = BindExpression(a.Right, scope);

                return a;
            }
        }

        private Expression BindConditionParameter(Expression expr, UnresolvedRef reference)
        {
            if (expr is BinaryOperator and)
            {
                and.Left = BindConditionParameter(and.Left, reference ?? new UnresolvedRef("$x"));

                and.Right = BindConditionParameter(and.Right, reference ?? new UnresolvedRef("$x"));
            }
            else if (expr is UnresolvedRef)
            {
                return reference;
            }

            return expr;
        }

        private CodeObject BindFunction(Call c, Expression right)
        {
            var md = new FunctionDefinition(c.Expression._AsString);

            md.Parameters.AddRange(c.Arguments?.Select(_ =>
                new ParameterDecl(_.AsString(), new TypeRef(typeof(int)))));

            md.Body.Add(right);

            return md;
        }
    }
}