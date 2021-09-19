using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// Allows a variable being passed as a parameter to be marked as an 'out' parameter.
    /// This is a special pseudo-operator that is only for use in this special case.
    /// </summary>
    public class Out : RefOutOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ParameterDecl.ParseTokenOut;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 920;

        /// <summary>
        /// Create an <see cref="Out"/> operator for the specified parameter expression.
        /// </summary>
        /// <param name="variable">An expression that evaluates to a <see cref="VariableRef"/>.</param>
        public Out(Expression variable)
            : base(variable)
        { }

        /// <summary>
        /// Create an <see cref="Out"/> operator for the specified <see cref="VariableDecl"/>.
        /// </summary>
        /// <param name="variableDecl">The <see cref="VariableDecl"/> being passed as a parameter (a reference to it will be created).</param>
        public Out(VariableDecl variableDecl)
            : base(variableDecl)
        { }

        protected Out(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.NotAType);
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse an <see cref="Out"/> operator.
        /// </summary>
        public static Out Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Out(parser, parent);
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}
