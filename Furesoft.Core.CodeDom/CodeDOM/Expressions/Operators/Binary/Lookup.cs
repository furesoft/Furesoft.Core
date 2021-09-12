﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Projects.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Specifies that an identifier is in the specified namespace - either 'global' or an aliased
    /// root-level namespace name (specified either with a 'using' or extern alias directive).
    /// </summary>
    public class Lookup : BinaryOperator
    {
        #region /* CONSTRUCTORS */

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

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
        }

        #endregion

        #region /* STATIC METHODS */

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

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the expression on the right of the right-most Lookup or Dot operator (bypass any '::' and '.' prefixes).
        /// </summary>
        public override Expression SkipPrefixes()
        {
            return Right.SkipPrefixes();
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "::";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="Lookup"/> operator.
        /// </summary>
        public static Lookup Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Lookup(parser, parent);
        }

        protected Lookup(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _left = (Expression)_left.Resolve(ResolveCategory.NamespaceAlias, flags);
            _right = (Expression)_right.Resolve(ResolveCategory.NamespaceOrType, flags);
            return this;
        }

        /// <summary>
        /// Evaluate the <see cref="Expression"/> to a type or namespace.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/>, <see cref="UnresolvedRef"/>, or <see cref="NamespaceRef"/>.</returns>
        public override SymbolicRef EvaluateTypeOrNamespace(bool withoutConstants)
        {
            return _right.EvaluateTypeOrNamespace(withoutConstants);
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            return _right.EvaluateType(withoutConstants);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }
        }

        #endregion

        #region /* RENDERING */

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

        #endregion
    }
}
