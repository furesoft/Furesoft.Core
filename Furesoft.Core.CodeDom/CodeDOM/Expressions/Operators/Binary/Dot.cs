// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Performs a member lookup (either directly on a namespace or type, or indirectly on the evaluated type
    /// of the expression on the left side).
    /// </summary>
    /// <remarks>
    /// If the left side is a <see cref="NamespaceRef"/> or <see cref="TypeRef"/>, then it's a direct lookup,
    /// otherwise it's an indirect lookup on the evaluated type of the left side.  In either case, the right
    /// side must be a <see cref="SymbolicRef"/> representing the member that is being looked-up.
    /// </remarks>
    public class Dot : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Dot"/> operator.
        /// </summary>
        public Dot(Expression left, SymbolicRef right)
            : base(left, right)
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
            get { return (_right != null && _right.IsConst); }
        }

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Create a Dot expression chain from 2 or more expressions, cloning any SymbolicRefs for convenience.
        /// </summary>
        public static Dot Create(Expression left, params SymbolicRef[] symbolicRefs)
        {
            Dot dot = new Dot(left is SymbolicRef ? (SymbolicRef)left.Clone() : left, (SymbolicRef)symbolicRefs[0].Clone());
            for (int i = 1; i < symbolicRefs.Length; ++i)
                dot = new Dot(dot, (SymbolicRef)symbolicRefs[i].Clone());
            return dot;
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

        /// <summary>
        /// Get the expression on the left of the left-most dot operator.
        /// </summary>
        public override Expression FirstPrefix()
        {
            return Left.FirstPrefix();
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ".";

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
        /// Parse a <see cref="Dot"/> operator.
        /// </summary>
        public static Dot Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Dot(parser, parent);
        }

        protected Dot(Parser parser, CodeObject parent)
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
            // Resolve the Left side of the Dot
            if (_left != null)
            {
                switch (resolveCategory)
                {
                    case ResolveCategory.Type:
                    case ResolveCategory.Interface:
                    case ResolveCategory.Constructor:
                    case ResolveCategory.Attribute:
                        // If we're expecting a type, interface, constructor, or attribute, then the
                        // left side must be a namespace or a parent type.
                        _left = (Expression)_left.Resolve(ResolveCategory.NamespaceOrType, flags);
                        break;
                    case ResolveCategory.Method:
                    case ResolveCategory.Property:
                    case ResolveCategory.Indexer:
                    case ResolveCategory.Event:
                    {
                        // Handle explicit interface implementations: For these categories, if our parent is a
                        // declaration, the left side must be an interface, otherwise we expect an object expression.
                        bool interfaceExpected = (_parent is MethodDeclBase || _parent is PropertyDeclBase);
                        _left = (Expression)_left.Resolve(interfaceExpected ? ResolveCategory.Interface : ResolveCategory.Expression, flags);
                        break;
                    }
                    default:
                        // Default to resolving to the specified category
                        _left = (Expression)_left.Resolve(resolveCategory, flags);
                        break;
                }
            }

            // Resolve the Right side of the Dot
            if (_right != null)
                _right = (Expression)_right.Resolve(resolveCategory, flags);

            // Perform some code tree optimizations - we can't do these during parsing, because the right expression must
            // be resolved first.
            if (AutomaticCodeCleanup && !flags.HasFlag(ResolveFlags.IsGenerated) && _right is TypeRef)
            {
                TypeRef right = (TypeRef)_right;

                // Simplify the code tree by replacing the Dot object with its right expression in some cases:
                // Convert "System.Type" for built-in types to "Type", so that it will display as a keyword.
                // Convert "System.Nullable<Type>" to "Nullable<Type>", which will then be displayed as "Type?".
                // Note that this could technically lead to invalid code if there is no "using System;" in the CodeUnit,
                // but it's not actually a problem since we *always* render "Nullable<Type>" as "Type?" in both text and GUI.
                if ((_left is NamespaceRef && right.IsBuiltInType && ((NamespaceRef)_left).Namespace.FullName == "System") || right.IsNullableType)
                {
                    _right.IsFirstOnLine = IsFirstOnLine;
                    return _right;
                }
            }

            return this;
        }

        /// <summary>
        /// Evaluate the <see cref="Expression"/> to a type or namespace.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/>, <see cref="UnresolvedRef"/>, or <see cref="NamespaceRef"/>.</returns>
        public override SymbolicRef EvaluateTypeOrNamespace(bool withoutConstants)
        {
            // Evaluate the right side
            SymbolicRef symbolicRef = (_right != null ? _right.EvaluateTypeOrNamespace(withoutConstants) : null);

            // Try to evaluate any nested type arguments using the left side - do NOT do this for TypeParameterRefs,
            // because they will have already been evaluated by OpenTypeParameterRef.EvaluateTypeArgumentTypes(), and
            // trying again will not only waste time, but will screw things up.
            if (symbolicRef is TypeRefBase && !(symbolicRef is TypeParameterRef))
                symbolicRef = ((TypeRefBase)symbolicRef).EvaluateTypeArgumentTypes(this, _right);
            return symbolicRef;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            return EvaluateTypeOrNamespace(withoutConstants) as TypeRefBase;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            TypeRefBase typeRefBase = null;

            // First, look on the right side of the Dot - unless we came from there (or we'll have infinite recursion)
            if (_right != originatingChild)
            {
                typeRefBase = _right.FindTypeArgument(typeParameterRef);
                if (typeRefBase is TypeParameterRef)
                {
                    // If we found a type parameter, try to get an actual type from the left side,
                    // but return the original type parameter if we can't.
                    TypeRefBase leftTypeRef = _left.EvaluateType();
                    if (leftTypeRef != null)
                    {
                        leftTypeRef = leftTypeRef.FindTypeArgument((TypeParameterRef)typeRefBase);
                        if (leftTypeRef is TypeRef)
                            return leftTypeRef;
                    }
                }
            }
            if (typeRefBase == null)
            {
                // If we found nothing on the right (or came from there), try the left side
                TypeRefBase leftTypeRef = _left.EvaluateType();
                if (leftTypeRef != null)
                    typeRefBase = leftTypeRef.FindTypeArgument(typeParameterRef);
            }

            return typeRefBase;
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
            // If we're rendering a Description, turn off some flags for the children of the Dot
            RenderFlags passFlags = (flags & (RenderFlags.PassMask & ~(RenderFlags.Description | RenderFlags.ShowParentTypes)));
            if (_left != null)
                _left.AsText(writer, passFlags | RenderFlags.IsPrefix | RenderFlags.NoSpaceSuffix);
            UpdateLineCol(writer, flags);
            AsTextDot(writer);
            if (_right != null)
                _right.AsText(writer, passFlags | RenderFlags.HasDotPrefix | (flags & RenderFlags.Attribute));  // Special case - allow the Attribute flag to pass
        }

        public static void AsTextDot(CodeWriter writer)
        {
            writer.Write(ParseToken);
        }

        #endregion
    }
}
