// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces
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
        /// <summary>
        /// The expression should evaluate to a NamespaceRef (whether pre-existing or created by this statement).
        /// </summary>
        protected Expression _expression;

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
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
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
                    else if (codeObject is TypeDecl typeDecl)
                    {
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

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "namespace";

        protected NamespaceDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'namespace'
            SetField(ref _expression, Expression.Parse(parser, this, true, ParseFlags.NotAType), false);
            ResolveNamespaces();  // Resolve the name-expression, creating any missing namespaces

            // Parse the body, and add all TypeDecls to the Namespace
            new Block(out _body, parser, this, true);
        }

        public static void AddParsePoints()
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
            // Find any existing or create the namespace
            if (expression is UnresolvedRef unresolvedRef)
            {
                Namespace @namespace = parentNamespace.FindOrCreateChildNamespace(unresolvedRef.Name);
                @namespace.SetDeclarationsInProject(true);
                expression = @namespace.CreateRef();
            }
            else if (expression is Dot dot) // If multiple namespaces are specified, resolve from left to right
            {
                dot.Left = ResolveNamespaceExpression(parentNamespace, dot.Left);
                dot.Right = ResolveNamespaceExpression(((NamespaceRef)dot.Left.SkipPrefixes()).Namespace, dot.Right);
            }
            return expression;
        }

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

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a namespace declaration
            return 2;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            _expression.AsText(writer, passFlags);
        }
    }
}
