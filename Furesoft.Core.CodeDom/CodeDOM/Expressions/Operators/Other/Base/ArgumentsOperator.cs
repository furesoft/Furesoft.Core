// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all operators with a variable list of arguments (<see cref="Call"/>, <see cref="Index"/>,
    /// <see cref="NewOperator"/>).
    /// </summary>
    public abstract class ArgumentsOperator : Operator
    {
        #region /* FIELDS */

        protected Expression _expression;
        protected ChildList<Expression> _arguments;

        #endregion

        #region /* CONSTRUCTORS */

        protected ArgumentsOperator(Expression expression, params Expression[] arguments)
        {
            Expression = expression;
            if (arguments != null && arguments.Length > 0)
            {
                CreateArguments().AddRange(arguments);
                foreach (Expression argument in arguments)
                {
                    // Arguments can be null for NewArray
                    if (argument != null)
                        argument.FormatAsArgument();
                }
            }
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The <see cref="Expression"/> being invoked.
        /// </summary>
        public virtual Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// The argument expressions.
        /// </summary>
        public ChildList<Expression> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        /// True if there are any arguments.
        /// </summary>
        public bool HasArguments
        {
            get { return (_arguments != null && _arguments.Count > 0); }
        }

        /// <summary>
        /// The number of arguments.
        /// </summary>
        public int ArgumentCount
        {
            get { return (_arguments != null ? _arguments.Count : 0); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create the list of argument <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        public ChildList<Expression> CreateArguments()
        {
            if (_arguments == null)
                _arguments = new ChildList<Expression>(this);
            return _arguments;
        }

        /// <summary>
        /// Add one or more <see cref="Expression"/>s.
        /// </summary>
        public void AddArguments(params Expression[] expressions)
        {
            CreateArguments().AddRange(expressions);
        }

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public virtual TypeRefBase GetParameterType(int argumentIndex)
        {
            return null;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ArgumentsOperator clone = (ArgumentsOperator)base.Clone();
            clone._arguments = ChildListHelpers.Clone(_arguments, clone);
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        #endregion

        #region /* PARSING */

        protected ArgumentsOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseArguments(Parser parser, CodeObject parent, string parseTokenStart, string parseTokenEnd, bool allowSingleNullArgument)
        {
            Token openParenToken = parser.Token;
            if (!ParseExpectedToken(parser, parseTokenStart))  // Move past '(' (or '[')
                openParenToken = null;

            // Parse the list of argument expressions with our parent set to block bubble-up normalization of EOL comments.
            // This also handles proper parsing of nested Conditional expressions (resetting tracking for call arguments).
            parser.PushNormalizationBlocker(parent);
            _arguments = ParseList(parser, this, parseTokenEnd, ParseFlags.Arguments, allowSingleNullArgument);
            parser.PopNormalizationBlocker();

            if (openParenToken != null)
            {
                // Move any newlines on the open paren (or bracket) to the first argument instead, or just remove them
                // if the first argument is null (such as in '[]').
                if (_arguments != null && _arguments.Count > 0 && _arguments[0] != null)
                    _arguments[0].MoveFormatting(openParenToken);
                else if (openParenToken.IsFirstOnLine)
                    openParenToken.NewLines = 0;

                Token lastToken = parser.LastToken;
                if (ParseExpectedToken(parser, parseTokenEnd))  // Move past ')' (or ']')
                {
                    IsEndFirstOnLine = parser.LastToken.IsFirstOnLine;
                    if (_arguments == null || _arguments.Count == 0)
                        parent.MoveAllComments(lastToken, false, false, AnnotationFlags.IsInfix1);
                    parent.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment with the parent
                }
            }
        }

        protected void ParseArguments(Parser parser, CodeObject parent, string parseTokenStart, string parseTokenEnd)
        {
            ParseArguments(parser, parent, parseTokenStart, parseTokenEnd, false);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Resolve the arguments first, so that argument type matching can occur for the expression
            if (_arguments != null && _arguments.Count > 0)
            {
                for (int i = 0; i < _arguments.Count; ++i)
                {
                    Expression argument = _arguments[i];
                    if (argument != null)
                    {
                        // Any methods being passed to delegate parameters will fail to resolve if there are multiple
                        // matches (since we usually won't know the delegate type yet), but we still need to go ahead and
                        // try in order to create a "method group" UnresolvedRef for use below.
                        _arguments[i] = (Expression)argument.Resolve(ResolveCategory.Expression, flags);
                    }
                }
            }

            // Now, resolve the invoked (called) expression
            if (_expression != null)
            {
                SymbolicRef oldInvokedRef, newInvokedRef;
                ResolveInvokedExpression(resolveCategory, flags, out oldInvokedRef, out newInvokedRef);

                // If the invoked reference was resolved (or changed), we need to check for certain types of
                // arguments that may need to be resolved now.
                if (_arguments != null && _arguments.Count > 0 && newInvokedRef != null)
                {
                    bool changed = !newInvokedRef.IsSameRef(oldInvokedRef);

                    // If the invoked ref is actually a method definition reference in a DocCodeRefBase, close any open type parameters
                    bool isDocCodeReference = false;
                    if (changed && flags.HasFlag(ResolveFlags.InDocCodeRef))
                    {
                        TypeRefBase invokedRef = newInvokedRef as TypeRefBase;
                        isDocCodeReference = (invokedRef != null && invokedRef.HasTypeArguments && invokedRef.IsDocCodeReference);
                    }

                    // Process the arguments
                    for (int i = 0; i < _arguments.Count; ++i)
                    {
                        Expression argument = _arguments[i];
                        if (argument != null)
                        {
                            bool resolve = false;
                            if (argument is UnresolvedRef)
                            {
                                // If the argument is unresolved, it might represent a method group that needed a delegate
                                // type in order to be resolved, so try it again now if the invoked reference was resolved
                                // and the UnresolvedRef has some partial matches (otherwise trying is a waste of time) - OR
                                // if we're in a doc comment, resolve anyway to handle a 'T' with no partial matches that
                                // might now be resolvable in the invoked expression.
                                if (changed && !(newInvokedRef is UnresolvedRef) && ((((UnresolvedRef)argument).IsMethodGroup) || flags.HasFlag(ResolveFlags.InDocCodeRef)))
                                    resolve = true;
                            }
                            else if (argument is Dot)
                            {
                                // Also handle a Dot operator with an unresolved right side
                                if (changed && !(newInvokedRef is UnresolvedRef))
                                {
                                    UnresolvedRef unresolvedRef = ((Dot)argument).Right as UnresolvedRef;
                                    if (unresolvedRef != null && (unresolvedRef.IsMethodGroup || flags.HasFlag(ResolveFlags.InDocCodeRef)))
                                        resolve = true;
                                }
                            }

                            // If it's been determined that we should, resolve the argument expression
                            if (resolve || isDocCodeReference)
                                _arguments[i] = (Expression)argument.Resolve(ResolveCategory.Expression, flags);

                            // If it's a DocCodeRef reference, close any open type parameter references
                            if (isDocCodeReference)
                                _arguments[i] = CloseTypeParametersInExpression(_arguments[i]);
                        }
                    }
                }
            }
            return this;
        }

        protected virtual void ResolveInvokedExpression(ResolveCategory resolveCategory, ResolveFlags flags, out SymbolicRef oldInvokedRef, out SymbolicRef newInvokedRef)
        {
            // Resolve the invoked (called) expression
            if (_expression != null)
            {
                oldInvokedRef = _expression.SkipPrefixes() as SymbolicRef;
                _expression = (Expression)_expression.Resolve(resolveCategory, flags);
                newInvokedRef = _expression.SkipPrefixes() as SymbolicRef;
            }
            else
                oldInvokedRef = newInvokedRef = null;
        }

        protected static Expression CloseTypeParametersInExpression(Expression expression)
        {
            if (expression is OpenTypeParameterRef)
                return ((OpenTypeParameterRef)expression).ConvertToTypeParameterRef();
            if (expression is Dot)
                ((Dot)expression).Right = CloseTypeParametersInExpression(((Dot)expression).Right);
            else if (expression is TypeRefBase)
                ((TypeRefBase)expression).ConvertOpenTypeParameters();
            return expression;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_expression != null && _expression.HasUnresolvedRef())
                return true;
            if (_arguments != null && _arguments.Count > 0 && ChildListHelpers.HasUnresolvedRef(_arguments))
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            // Search the evaluated type of the expression for the type parameter
            TypeRefBase typeRefBase = EvaluateType();
            return (typeRefBase != null ? typeRefBase.FindTypeArgument(typeParameterRef, originatingChild) : null);
        }

        /// <summary>
        /// Determine the delegate type of the parameter with the specified argument index.
        /// </summary>
        public TypeRefBase GetDelegateParameterType(object obj, int argumentIndex)
        {
            return MethodRef.GetDelegateParameterType(obj, argumentIndex, _expression);
        }

        /// <summary>
        /// Get the invocation target reference.
        /// </summary>
        public virtual SymbolicRef GetInvocationTargetRef()
        {
            return null;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))
                    && (_arguments == null || _arguments.Count == 0 || ((_arguments[0] == null || !_arguments[0].IsFirstOnLine) && _arguments.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_expression != null)
                {
                    if (value)
                        _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = value;
                }
                if (_arguments != null && _arguments.Count > 0)
                {
                    if (value)
                        _arguments[0].IsFirstOnLine = false;
                    _arguments.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected abstract void AsTextName(CodeWriter writer, RenderFlags flags);

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool attributeCall = flags.HasFlag(RenderFlags.Attribute);

            // Get the parent position and wrap the name/arguments in its own indent logic to preserve
            // the parent position until the initializer is rendered.
            writer.SetParentOffset();
            writer.BeginIndentOnNewLine(this);

            AsTextName(writer, passFlags | (flags & RenderFlags.Attribute));  // Special case - allow the Attribute flag to pass
            if ((_arguments != null && _arguments.Count > 0) || (!flags.HasFlag(RenderFlags.NoParensIfEmpty) && !attributeCall) || HasInfixComments)
            {
                writer.BeginIndentOnNewLineRelativeToLastIndent(this, _expression);
                AsTextStartArguments(writer, flags);
                AsTextInfixComments(writer, 0, flags);
                writer.WriteList(_arguments, passFlags, this);
                AsTextEndArguments(writer, flags);
                writer.EndIndentation(this);
            }

            writer.EndIndentation(this);
            AsTextInitializer(writer, flags);
        }

        protected abstract void AsTextStartArguments(CodeWriter writer, RenderFlags flags);
        protected abstract void AsTextEndArguments(CodeWriter writer, RenderFlags flags);

        protected virtual void AsTextInitializer(CodeWriter writer, RenderFlags flags)
        { }

        #endregion
    }
}
