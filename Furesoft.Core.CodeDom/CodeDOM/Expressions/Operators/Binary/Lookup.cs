using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Specifies that an identifier is in the specified namespace - either 'global' or an aliased
    /// root-level namespace name (specified either with a 'using' or extern alias directive).
    /// </summary>
    public class Lookup : BinaryOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "::";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// Create a <see cref="Lookup"/> operator.
        /// </summary>
        public Lookup(Expression left, Expression right)
            : base(left, right)
        { }

        /// <summary>
        /// Create a <see cref="Lookup"/> operator.
        /// </summary>
        public Lookup(Namespace left, Expression right)
            : base(left.CreateRef(), right)
        { }

        /// <summary>
        /// Create a <see cref="Lookup"/> operator.
        /// </summary>
        public Lookup(ExternAlias left, Expression right)
            : base(left.CreateRef(), right)
        { }

        protected Lookup(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Create a Lookup/Dot expression chain from 2 or more expressions.
        /// </summary>
        public static BinaryOperator Create(Expression left, params SymbolicRef[] expressions)
        {
            BinaryOperator binaryOperator = new Lookup(left, expressions[0]);
            for (int i = 1; i < expressions.Length; ++i)
                binaryOperator = new Dot(binaryOperator, expressions[i]);
            return binaryOperator;
        }

        /// <summary>
        /// Parse a <see cref="Lookup"/> operator.
        /// </summary>
        public static Lookup Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Lookup(parser, parent);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (_left != null)
                _left.AsText(writer, passFlags);
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken);
            if (_right != null)
                _right.AsText(writer, passFlags);
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        /// <summary>
        /// Get the expression on the right of the right-most Lookup or Dot operator (bypass any '::' and '.' prefixes).
        /// </summary>
        public override Expression SkipPrefixes()
        {
            return Right.SkipPrefixes();
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}
