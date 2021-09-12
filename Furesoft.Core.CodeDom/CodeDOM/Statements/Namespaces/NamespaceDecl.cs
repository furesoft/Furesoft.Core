// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares a namespace plus a body of declarations that belong to it.
    /// </summary>
    /// <remarks>
    /// The format of a namespace is (in order):
    ///     - Zero or more "extern alias" directives
    ///     - Zero or more "using" directives (or "using aliasname = ...")
    ///     - Zero or more namespace member declarations (child namespaces and/or type declarations)
    /// Of course, comments and preprocessor directives may be mixed in.
    /// </remarks>
    public class NamespaceDecl : BlockStatement
    {
        #region /* FIELDS */

        /// <summary>
        /// The expression should evaluate to a NamespaceRef (whether pre-existing or created by this statement).
        /// </summary>
        protected Expression _expression;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="NamespaceDecl"/>.
        /// </summary>
        public NamespaceDecl(Expression expression, CodeObject body)
            : base(body, false)
        {
            Expression = expression;
        }

        /// <summary>
        /// Create a <see cref="NamespaceDecl"/>.
        /// </summary>
        public NamespaceDecl(Expression expression)
            : this(expression, null)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        public override CodeObject Parent
        {
            set
            {
                base.Parent = value;

                // Resolve and create any missing namespaces when our parent is set
                ResolveNamespaces();
            }
        }

        /// <summary>
        /// The namespace <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set
            {
                SetField(ref _expression, value, true);

                // Resolve and create any missing namespaces
                ResolveNamespaces();
            }
        }

        /// <summary>
        /// The associated <see cref="Namespace"/>.
        /// </summary>
        public Namespace Namespace
        {
            get
            {
                Expression expression = _expression.SkipPrefixes();
                return (expression is NamespaceRef ? ((NamespaceRef)expression).Namespace : null);
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add a <see cref="CodeObject"/> to the <see cref="Block"/> body.
        /// </summary>
        public override void Add(CodeObject obj)
        {
            base.Add(obj);

            // If a TypeDecl was added, we must also add it to the Namespace
            if (obj is TypeDecl)
                Namespace.Add((TypeDecl)obj);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            NamespaceDecl clone = (NamespaceDecl)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        /// <summary>
        /// Get all <see cref="TypeDecl"/>s declared in the <see cref="NamespaceDecl"/>, or
        /// in any nested NamespaceDecls (recursively).
        /// </summary>
        /// <param name="recursive">True to recursively look in child NamespaceDecls, otherwise false.</param>
        /// <param name="includeNestedTypes">True to include nested types, otherwise false.</param>
        public IEnumerable<TypeDecl> GetTypeDecls(bool recursive, bool includeNestedTypes)
        {
            if (_body != null)
            {
                foreach (CodeObject codeObject in _body)
                {
                    if (codeObject is NamespaceDecl && recursive)
                    {
                        foreach (TypeDecl typeDecl in ((NamespaceDecl)codeObject).GetTypeDecls(true, includeNestedTypes))
                            yield return typeDecl;
                    }
                    else if (codeObject is TypeDecl)
                    {
                        TypeDecl typeDecl = (TypeDecl)codeObject;
                        yield return typeDecl;

                        if (includeNestedTypes)
                        {
                            foreach (TypeDecl nestedType in typeDecl.GetNestedTypeDecls(true, true))
                                yield return nestedType;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all <see cref="TypeDecl"/>s declared in the <see cref="NamespaceDecl"/>, or
        /// in any nested NamespaceDecls (recursively).
        /// </summary>
        /// <param name="recursive">True to recursively look in child NamespaceDecls, otherwise false.</param>
        public IEnumerable<TypeDecl> GetTypeDecls(bool recursive)
        {
            return GetTypeDecls(recursive, false);
        }

        /// <summary>
        /// Get all <see cref="TypeDecl"/>s declared in the <see cref="NamespaceDecl"/>, or
        /// in any nested NamespaceDecls (recursively).
        /// </summary>
        public IEnumerable<TypeDecl> GetTypeDecls()
        {
            return GetTypeDecls(false, false);
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "namespace";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(NamespaceDecl));
        }

        /// <summary>
        /// Parse a <see cref="NamespaceDecl"/>.
        /// </summary>
        public static NamespaceDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new NamespaceDecl(parser, parent);
        }

        protected NamespaceDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'namespace'
            SetField(ref _expression, Expression.Parse(parser, this, true, ParseFlags.NotAType), false);
            ResolveNamespaces();  // Resolve the name-expression, creating any missing namespaces

            // Parse the body, and add all TypeDecls to the Namespace
            new Block(out _body, parser, this, true);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // No need to resolve the namespace Expression here - it will be resolved during parsing
            return base.Resolve(ResolveCategory.CodeObject, flags);
        }

        protected virtual void ResolveNamespaces()
        {
            // Resolve the namespaces using special logic, since it will always work without errors because
            // namespaces are created if they don't exist.  This logic is actually used during parsing, not
            // during the resolve pass.
            if (_parent != null)
                _expression = ResolveNamespaceExpression(GetNamespace(), _expression);
        }

        private static Expression ResolveNamespaceExpression(Namespace parentNamespace, Expression expression)
        {
            if (expression is UnresolvedRef)
            {
                // Find any existing or create the namespace
                UnresolvedRef unresolvedRef = (UnresolvedRef)expression;
                Namespace @namespace = parentNamespace.FindOrCreateChildNamespace(unresolvedRef.Name);
                @namespace.SetDeclarationsInProject(true);
                expression = unresolvedRef.CreateRef(@namespace, true);
            }
            else if (expression is Dot)
            {
                // If multiple namespaces are specified, resolve from left to right
                Dot dot = (Dot)expression;
                dot.Left = ResolveNamespaceExpression(parentNamespace, dot.Left);
                dot.Right = ResolveNamespaceExpression(((NamespaceRef)dot.Left.SkipPrefixes()).Namespace, dot.Right);
            }
            return expression;
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            // Abort if we reach this level and we're looking for a category that can't be found this "high".
            // Don't treat a Method category as "type-level" if we're in a DocCodeRefBase, because it might be a Constructor.
            if (resolver.IsTypeLevelCategory() && !resolver.IsDocCodeRefToMethod)
                return;

            bool skipAbortCheck = false;

            // Search the body of the namespace (skip if we're looking for a root-level namespace name)
            if (_body != null && resolver.ResolveCategory != ResolveCategory.RootNamespace)
            {
                if (resolver.ResolveCategory == ResolveCategory.NamespaceAlias)
                {
                    // Search for aliases (prefer these to extern aliases, in case there is any conflict, such as
                    // an Alias with the name 'global').
                    foreach (Alias alias in _body.Find<Alias>(name))
                        resolver.AddMatch(alias);
                    if (resolver.HasCompleteMatch) return;  // Abort if we found a match

                    // Search for extern aliases
                    foreach (ExternAlias externAlias in _body.Find<ExternAlias>(name))
                        resolver.AddMatch(externAlias);
                }
                else
                {
                    // Search the declared namespace, and then any specified parent namespaces
                    bool isNamespaceDecl = true;
                    Expression expression = _expression;
                    while (expression != null)
                    {
                        Expression parentNamespaces = null;
                        NamespaceRef namespaceRef = expression as NamespaceRef;
                        if (expression is Dot)
                        {
                            Dot dot = (Dot)expression;
                            namespaceRef = dot.Right as NamespaceRef;
                            parentNamespaces = dot.Left;
                        }
                        if (namespaceRef != null)
                        {
                            namespaceRef.ResolveRef(name, resolver);
                            if (resolver.HasCompleteMatch)
                            {
                                // Abort if we found a match - EXCEPT under the special case that we're including
                                // internal types from imported projects and assemblies AND the only matches we have
                                // are all such imported internal types.
                                if (!Project.LoadInternalTypes || resolver.HasMatchesOtherThanImportedInternalTypes())
                                    return;
                                skipAbortCheck = true;
                            }
                        }
                        if (isNamespaceDecl)
                        {
                            // Search for (using) aliases declared in the NamespaceDecl
                            foreach (Alias alias in _body.Find<Alias>(name))
                                resolver.AddMatch(alias);
                            if (!skipAbortCheck && resolver.HasCompleteMatch) return;  // Abort if we found a match

                            // Search all namespaces specified with 'using' directives in this NamespaceDecl
                            if (resolver.ResolveCategory != ResolveCategory.Namespace)
                            {
                                foreach (UsingDirective usingDirective in _body.Find<UsingDirective>())
                                {
                                    // Search the namespace, but don't match any namespaces - although a NamespaceDecl is
                                    // searched for namespaces, the 'using' directives are NOT.
                                    namespaceRef = usingDirective.GetNamespaceRef();
                                    if (namespaceRef != null)
                                        namespaceRef.ResolveRef(name, resolver, true);
                                }
                            }
                            if (!skipAbortCheck && resolver.HasCompleteMatch) return;  // Abort if we found a match
                        }

                        // Repeat for any specified parent namespaces
                        expression = parentNamespaces;
                        isNamespaceDecl = false;
                    }
                }
            }

            // Continue for all nested NamespaceDecls, including the CodeUnit (the global namespace)
            if (_parent != null && (skipAbortCheck || !resolver.HasCompleteMatch))
                _parent.ResolveRefUp(name, resolver);
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_expression != null && _expression.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a namespace declaration
            return 2;
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _expression != null)
                {
                    _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            _expression.AsText(writer, passFlags);
        }

        #endregion
    }
}
