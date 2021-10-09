﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    internal class FactorialOperator : PostUnaryOperator, IEvaluatableExpression
    {
        private const int Precedence = 2;

        public FactorialOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public override string Symbol
        {
            get { return "!"; }
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("!", Precedence, true, false, Parse);
        }

        public static FactorialOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new FactorialOperator(parser, parent);
            return null;
        }

        public double Evaluate(ExpressionParser ep, Scope scope)
        {
            var expr = ep.EvaluateExpression(Expression, scope);

            return Factorial(expr);
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }

        private double Factorial(double n)
        {
            return Enumerable.Range(1, (int)n).Aggregate(1, (p, item) => p * item);
        }
    }
}