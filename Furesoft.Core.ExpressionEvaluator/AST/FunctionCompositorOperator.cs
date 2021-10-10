using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    internal class FunctionCompositorOperator : BinaryArithmeticOperator, IBindable
    {
        private const int Precedence = 2;

        public FunctionCompositorOperator(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public FunctionCompositorOperator(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol
        {
            get { return "@"; }
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint("@", Precedence, true, false, Parse);
        }

        public static FunctionCompositorOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new FunctionCompositorOperator(parser, parent);
            return null;
        }

        public CodeObject Bind(ExpressionParser ep, Binder binder)
        {
            if (Parent is Assignment assignment && assignment.Left is Call call && call.Arguments[0] is UnresolvedRef uref)
            {
                return new Call(Left, new Call(Right, uref));
            }
            else
            {
                AttachMessage($"{Left._AsString} and {Right._AsString} can only combined in a function definition", MessageSeverity.Error, MessageSource.Resolve);

                return this;
            }
        }

        public override int GetPrecedence()
        {
            return Precedence;
        }
    }
}