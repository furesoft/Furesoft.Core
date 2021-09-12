using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// Represents an index into an array or type with an indexer.
    /// Array indexes must be integral.  Indexers may index with any type.
    /// </summary>
    /// <remarks>
    /// The Expression of the ArgumentsOperator base class should evaluate to a VariableRef of an array
    /// type or a type with an indexer, or an expression that evaluates to an object of an array type
    /// or a type with an indexer.
    /// </remarks>
    public class Index : ArgumentsOperator
    {
        #region /* FIELDS */

        // If the expression resolves to a type with an indexer, there is an implied ".Item" that
        // is omitted by the language for brevity - this hidden reference (IndexerRef) is stored here.
        protected SymbolicRef _indexerRef;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Index"/> operator.
        /// </summary>
        public Index(Expression expression, params Expression[] arguments)
            : base(expression, arguments)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// A hidden <see cref="IndexerRef"/> to an indexer declaration (if any).
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return _indexerRef; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            if (_indexerRef is IndexerRef)
            {
                TypeRefBase parameterTypeRef = MethodRef.GetParameterType(_indexerRef.Reference, argumentIndex, _expression);
                if (parameterTypeRef != null)
                    return parameterTypeRef;
            }
            // By default, assume we're indexing an array type
            return TypeRef.IntRef;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Index clone = (Index)base.Clone();
            clone.CloneField(ref clone._indexerRef, _indexerRef);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of the index operator.
        /// </summary>
        public const string ParseTokenStart = TypeRefBase.ParseTokenArrayStart;

        /// <summary>
        /// The token used to parse the end of the index operator.
        /// </summary>
        public const string ParseTokenEnd = TypeRefBase.ParseTokenArrayEnd;

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
            // Use a parse-priority of 200 (IndexerDecl uses 0, UnresolvedRef uses 100, Attribute uses 300)
            Parser.AddOperatorParsePoint(ParseTokenStart, 200, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Index"/> operator.
        /// </summary>
        public static Index Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have an unused expression before proceeding
            if (parser.HasUnusedExpression)
                return new Index(parser, parent);
            return null;
        }

        protected Index(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Clear any newlines set by the current token in the base initializer, since it will be the open '[',
            // and we move any newlines on that character to the first parameter in ParseArguments below.
            NewLines = 0;

            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);
            ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd);

            // Set the parent starting token to the beginning of the expression
            parser.ParentStartingToken = startingToken;
        }

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
            // Indexers always have a category of Expression
            return base.Resolve(ResolveCategory.Expression, flags);
        }

        protected override void ResolveInvokedExpression(ResolveCategory resolveCategory, ResolveFlags flags, out SymbolicRef oldInvokedRef, out SymbolicRef newInvokedRef)
        {
            // Resolve the invoked (indexed) expression - first, resolve the invoked expression, then any hidden IndexerRef
            base.ResolveInvokedExpression(resolveCategory, flags, out oldInvokedRef, out newInvokedRef);

            // If we failed to resolve the expression, then don't bother with the IndexerRef yet
            if (newInvokedRef is UnresolvedRef)
            {
                // Force the IndexerRef to null if it isn't already
                if (_indexerRef != null)
                    SetField(ref _indexerRef, null, false);
            }
            else
            {
                // Check for implicit indexing, or an explicit IndexerRef expression
                bool implicitIndexing = false;
                if (newInvokedRef is IndexerRef)
                    implicitIndexing = true;
                else
                {
                    // Arrays and strings also use imnplicit indexing
                    TypeRefBase typeRefBase = _expression.EvaluateType();
                    if (typeRefBase != null)
                    {
                        if (typeRefBase.IsArray || typeRefBase.IsSameRef(TypeRef.StringRef))
                            implicitIndexing = true;
                    }
                }

                // If the expression was resolved or changed, or it's not implicit and we don't have a IndexerRef yet, or it's implicit and
                // we have an IndexerRef, then reset the IndexerRef as appropriate.
                if (newInvokedRef != oldInvokedRef || (!implicitIndexing && _indexerRef == null) || (implicitIndexing && _indexerRef != null))
                {
                    // If the indexer is implicit, make the IndexerRef null, otherwise set it to an UnresolvedRef to be resolved
                    SymbolicRef symbolicRef = (implicitIndexing ? null : new UnresolvedRef(IndexerDecl.IndexerName, ResolveCategory.Indexer, LineNumber, ColumnNumber));
                    SetField(ref _indexerRef, symbolicRef, false);
                }

                // Resolve the IndexerRef, treating it as the "invoked reference" now in place of the expression
                oldInvokedRef = _indexerRef;
                if (_indexerRef is UnresolvedRef)
                    _indexerRef = (SymbolicRef)_indexerRef.Resolve(ResolveCategory.Indexer, flags);
                newInvokedRef = _indexerRef;
            }
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_indexerRef != null && _indexerRef.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            TypeRefBase typeRefBase;

            // If we have an IndexerRef, our type is its return type
            if (_indexerRef != null)
            {
                typeRefBase = null;
                if (_indexerRef is IndexerRef)
                {
                    typeRefBase = _indexerRef.EvaluateType(withoutConstants);
                    if (typeRefBase != null)
                        typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(_expression);
                }
                else if (_indexerRef is UnresolvedRef)
                {
                    if (((UnresolvedRef)_indexerRef).ResolveCategory == ResolveCategory.Indexer)
                        typeRefBase = ((UnresolvedRef)_indexerRef).MethodGroupReturnType();
                }
            }
            else
            {
                // Otherwise, we're doing implicit indexing, or have an explicit IndexerRef expression
                typeRefBase = _expression.EvaluateType(withoutConstants);
                if (typeRefBase != null)
                {
                    if (!(_expression.SkipPrefixes() is IndexerRef))
                    {
                        // Determine the element type for arrays and strings
                        if (typeRefBase.IsArray)
                            typeRefBase = typeRefBase.GetElementType();
                        else if (typeRefBase.IsSameRef(TypeRef.StringRef))
                            typeRefBase = TypeRef.CharRef;
                    }
                }
            }

            return typeRefBase;
        }

        /// <summary>
        /// Get the invocation target reference.
        /// </summary>
        public override SymbolicRef GetInvocationTargetRef()
        {
            return _indexerRef;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            _expression.AsText(writer, flags);
            UpdateLineCol(writer, flags);
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseTokenStart);
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
            if (IsEndFirstOnLine)
                writer.WriteLine();
            writer.Write(ParseTokenEnd);
        }

        #endregion
    }
}
