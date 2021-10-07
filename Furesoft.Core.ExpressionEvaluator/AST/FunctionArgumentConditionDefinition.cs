using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.ExpressionEvaluator.Symbols;
using System.Collections.Generic;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    //f: x is N {2,10}
    //f: x is N x > 10

    public class FunctionArgumentConditionDefinition : Annotation, IBindable
    {
        public FunctionArgumentConditionDefinition(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Condition { get; set; }

        public string Function { get; set; }

        public string NumberRoom { get; set; }
        public Expression Parameter { get; private set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(":", Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var name = parser.RemoveLastUnusedToken();

            var result = new FunctionArgumentConditionDefinition(parser, parent);
            result.Function = name.Text;

            parser.NextToken();

            result.Parameter = SymbolicRef.Parse(parser, result);

            if (!result.ParseExpectedToken(parser, "in"))
            {
                return null;
            }

            result.NumberRoom = parser.GetIdentifierText();
            result.Condition = Expression.Parse(parser, result);

            return result;
        }

        public CodeObject Bind(ExpressionParser ep, Binder binder)
        {
            if (Condition is RelationalOperator rel)
            {
                Condition = BindConstainCondition(rel);
            }
            else if (Condition is And an)
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
            else if (Condition is IntervalExpression interval)
            {
                Condition = BindInterval(interval, Parameter);
            }

            if (ep.RootScope.SetDefinitions.ContainsKey(NumberRoom))
            {
                Condition = binder.BindNumberRoom(new UnresolvedRef(NumberRoom), (UnresolvedRef)Parameter);
            }

            if (binder.ArgumentConstraints.ContainsKey(Function))
            {
                binder.ArgumentConstraints[Function].Add(this);
            }
            else
            {
                var constrains = new List<FunctionArgumentConditionDefinition>
                    {
                        this
                    };

                binder.ArgumentConstraints.Add(Function, constrains);
            }

            return this;
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
    }
}