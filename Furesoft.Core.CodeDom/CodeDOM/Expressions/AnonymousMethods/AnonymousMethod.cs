// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an un-named method that can be assigned to a delegate.
    /// </summary>
    /// <remarks>
    /// An AnonymousMethod distinguishes between having zero parameters and unspecified parameters (no parens
    /// at all).  If the parameters are unspecified, the anonymous method is convertible to a delegate with any
    /// parameter list not containing out parameters.  It's possible that the empty parens will be needed in
    /// some cases to prevent ambiguity, such as when resolving a method reference that is passed an anonymous
    /// method as a parameter, and multiple matches are found that have delegates with different numbers of
    /// parameters (one of them having zero).
    /// </remarks>
    public class AnonymousMethod : Expression, IParameters, IBlock
    {
        #region /* FIELDS */

        protected ChildList<ParameterDecl> _parameters;
        protected Block _body;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethod(CodeObject body, params ParameterDecl[] parameters)
        {
            // Allow derived classes to pass any non-Block code object, in which case it will
            // be wrapped in a Block.
            Body = ((body == null || body is Block) ? (Block)body : new Block(body));

            // Allow the parameter collection to be null (unspecified parameters) if null is specifically passed in (for AnonymousMethod)
            if (parameters != null)
                CreateParameters().AddRange(parameters);
        }

        /// <summary>
        /// Create an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethod(params ParameterDecl[] parameters)
            : this(new Block(), parameters)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The <see cref="Block"/> body.
        /// </summary>
        public Block Body
        {
            get { return _body; }
            set
            {
                _body = value;
                if (_body != null)
                {
                    _body.Parent = this;
                    ReformatBlock();
                }
            }
        }

        /// <summary>
        /// The parameters of the anonymous method (if any).
        /// </summary>
        public ChildList<ParameterDecl> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// True if the anonymous method has any parameters.
        /// </summary>
        public bool HasParameters
        {
            get { return (_parameters != null && _parameters.Count > 0); }
        }

        /// <summary>
        /// The number of parameters the anonymous method has.
        /// </summary>
        public int ParameterCount
        {
            get { return (_parameters != null ? _parameters.Count : 0); }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public bool HasHeader
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsTopLevel
        {
            get { return false; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create a body if one doesn't exist yet.
        /// </summary>
        public Block CreateBody()
        {
            if (_body == null)
                Body = new Block();
            return _body;
        }

        /// <summary>
        /// Create the list of <see cref="ParameterDecl"/>s, or return the existing one.
        /// </summary>
        public ChildList<ParameterDecl> CreateParameters()
        {
            if (_parameters == null)
                _parameters = new ChildList<ParameterDecl>(this);
            return _parameters;
        }

        /// <summary>
        /// Add one or more <see cref="ParameterDecl"/>s.
        /// </summary>
        public void AddParameters(params ParameterDecl[] parameterDecls)
        {
            CreateParameters().AddRange(parameterDecls);
        }

        /// <summary>
        /// Add a <see cref="CodeObject"/> to the <see cref="Block"/> body.
        /// </summary>
        public void Add(CodeObject obj)
        {
            CreateBody().Add(obj);
        }

        /// <summary>
        /// Add multiple <see cref="CodeObject"/>s to the <see cref="Block"/> body.
        /// </summary>
        public void Add(params CodeObject[] objects)
        {
            CreateBody();
            foreach (CodeObject obj in objects)
                _body.Add(obj);
        }

        /// <summary>
        /// Add a code object to the block statement, and also assign the specified comment to the object.
        /// </summary>
        /// <param name="obj">The object to be added.</param>
        /// <param name="comment">The comment text.</param>
        public void Add(CodeObject obj, string comment)
        {
            obj.AttachAnnotation(new Comment(comment));
            Add(obj);
        }

        /// <summary>
        /// Insert a <see cref="CodeObject"/> at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="obj">The object to be inserted.</param>
        public void Insert(int index, CodeObject obj)
        {
            CreateBody().Insert(index, obj);
        }

        /// <summary>
        /// Remove the specified <see cref="CodeObject"/> from the body.
        /// </summary>
        public void Remove(CodeObject obj)
        {
            if (_body != null)
                _body.Remove(obj);
        }

        /// <summary>
        /// Remove all objects from the body.
        /// </summary>
        public void RemoveAll()
        {
            if (_body != null)
                _body.RemoveAll();
        }

        /// <summary>
        /// Determine the return type of the anonymous method (never null - will be 'void' instead).
        /// </summary>
        public virtual TypeRefBase GetReturnType()
        {
            TypeRefBase returnTypeRef = null;
            ScanReturnTypes(ref returnTypeRef, _body);
            return (returnTypeRef ?? TypeRef.VoidRef);
        }

        protected void ScanReturnTypes(ref TypeRefBase returnTypeRef, Block body)
        {
            // Recursively scan all bodies for Return statements
            if (body != null)
            {
                foreach (CodeObject codeObject in body)
                {
                    if (codeObject is Return)
                    {
                        Expression expression = ((Return)codeObject).Expression;
                        TypeRefBase typeRef = (expression == null ? TypeRef.VoidRef : expression.EvaluateType(true));
                        returnTypeRef = (returnTypeRef == null ? typeRef : TypeRef.GetCommonType(returnTypeRef, typeRef));
                    }
                    else if (codeObject is BlockStatement)
                        ScanReturnTypes(ref returnTypeRef, ((BlockStatement)codeObject).Body);
                }
            }
        }

        /// <summary>
        /// Get the parent delegate expression.
        /// </summary>
        public virtual Expression GetParentDelegateExpression()
        {
            // Determine the delegate type to which the anonymous method is being assigned
            Expression delegateExpression = null;
            CodeObject parent = _parent;
            while (parent is Conditional || parent is IfNullThen)
                parent = parent.Parent;
            if (parent is VariableDecl)
            {
                // If the anonymous method is being used to initialize a variable, get the delegate type from the variable's type
                delegateExpression = ((VariableDecl)parent).Type;
            }
            else if (parent is Assignment)
            {
                // If the anonymous method is being assigned to a variable, get the delegate type from the variable's type
                delegateExpression = ((Assignment)parent).Left.EvaluateType();
            }
            else if (parent is Cast)
            {
                // If the anonymous method is being cast, get the delegate type from the cast
                delegateExpression = ((Cast)parent).Type;
            }
            else if (parent is Return)
            {
                // If the anonymous method is being returned from a method, get the delegate type from the method return type
                CodeObject parentMethod = parent.FindParentMethod();
                if (parentMethod is MethodDeclBase)
                    delegateExpression = ((MethodDeclBase)parentMethod).ReturnType;
                else if (parentMethod is AnonymousMethod)
                {
                    // If the anonymous method is being returned from another anonymous method, recursively get the parent
                    // delegate expression of the parent anonymous method, then get the return type of that delegate.
                    delegateExpression = ((AnonymousMethod)parentMethod).GetParentDelegateExpression();
                    if (delegateExpression != null)
                        delegateExpression = delegateExpression.GetDelegateReturnType();
                }
            }
            else if (parent is YieldReturn)
            {
                // If the anonymous method is being returned from an iterator method using 'yield return', get the delegate
                // type from the method return type, minus the IEnumerable<> wrapper.
                CodeObject parentMethod = parent.FindParentMethod();
                if (parentMethod is MethodDeclBase)
                {
                    delegateExpression = ((MethodDeclBase)parentMethod).ReturnType;
                    TypeRefBase typeRefBase = delegateExpression.SkipPrefixes() as TypeRefBase;
                    if (typeRefBase != null && typeRefBase.IsSameGenericType(TypeRef.IEnumerable1Ref))
                        delegateExpression = typeRefBase.TypeArguments[0];
                }
            }
            else if (parent is Initializer)
            {
                // Handle collection initializers
                if (parent.Parent is Initializer && parent.Parent.Parent is NewObject)
                {
                    TypeRefBase typeRefBase = ((NewObject)parent.Parent.Parent).EvaluateType();

                    // Handle a Dictionary where the value is a delegate type
                    if (typeRefBase.IsSameGenericType(TypeRef.Dictionary2Ref))
                        delegateExpression = typeRefBase.TypeArguments[1];
                }
            }
            return delegateExpression;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            AnonymousMethod clone = (AnonymousMethod)base.Clone();
            clone._parameters = ChildListHelpers.Clone(_parameters, clone);
            clone.CloneField(ref clone._body, _body);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = DelegateDecl.ParseToken;

        /// <summary>
        /// The token used to parse the start of the parameter list of an anonymous method.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        /// <summary>
        /// The token used to parse the end of the parameter list of an anonymous method.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 100 (DelegateDecl uses 0)
            Parser.AddParsePoint(ParseToken, 100, Parse);
        }

        /// <summary>
        /// Parse an <see cref="AnonymousMethod"/>.
        /// </summary>
        public static AnonymousMethod Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AnonymousMethod(parser, parent);
        }

        protected AnonymousMethod(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Token startingToken = parser.Token;  // Get the object starting token
            parser.NextToken();                  // Move past 'delegate'

            // Parse the parameter declarations
            bool isEndFirstOnLine;
            _parameters = ParameterDecl.ParseList(parser, this, ParseTokenStart, ParseTokenEnd, true, out isEndFirstOnLine);
            IsEndFirstOnLine = isEndFirstOnLine;

            // If the body is indented less than the parent object, set the NoIndentation flag to prevent it from
            // being formatted relative to the parent object.
            if (parser.CurrentTokenIndentedLessThan(startingToken))
                SetFormatFlag(FormatFlags.NoIndentation, true);

            new Block(out _body, parser, this, true);  // Parse the body
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            ChildListHelpers.Resolve(_parameters, ResolveCategory.CodeObject, flags);
            if (_body != null)
                _body.Resolve(ResolveCategory.CodeObject, flags);
            return this;
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            if (_body != null)
            {
                _body.ResolveRef(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            ChildListHelpers.ResolveRef(_parameters, name, resolver);
            if (_parent != null && !resolver.HasCompleteMatch)
                _parent.ResolveRefUp(name, resolver);
        }

        /// <summary>
        /// Resolve child code objects that match the specified name are valid goto targets, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveGotoTargetUp(string name, Resolver resolver)
        {
            if (_body != null)
            {
                _body.ResolveGotoTargetUp(name, resolver);
                if (resolver.HasCompleteMatch) return;  // Abort if we found a match
            }
            if (_parent != null)
                _parent.ResolveGotoTargetUp(name, resolver);
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_body.HasUnresolvedRef())
                return true;
            if (ChildListHelpers.HasUnresolvedRef(_parameters))
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Determine if the parameters of the anonymous method are compatible with those of the specified delegate type.
        /// </summary>
        public bool AreParametersCompatible(TypeRef toTypeRef)
        {
            ICollection delegateParameters = toTypeRef.GetDelegateParameters();

            // If we have parameters (even if we have 0), then we match normally
            if (_parameters != null)
            {
                // Get the method and delegate parameters
                ChildList<ParameterDecl> parameters = Parameters;
                int delegateParameterCount = (delegateParameters != null ? delegateParameters.Count : 0);

                // The parameter counts must match ('params' are NOT expanded in this situation)
                if ((parameters != null ? parameters.Count : 0) != delegateParameterCount)
                    return false;

                // For each delegate parameter, verify that its type and ref/out modifiers *match* those of
                // the method parameter (no conversions are allowed).
                for (int i = 0; i < delegateParameterCount; ++i)
                {
                    // Get the type of the delegate parameter, and any modifiers
                    bool isRef, isOut;
                    TypeRefBase delegateParameterRef = ParameterRef.GetParameterType(delegateParameters, i, out isRef, out isOut, toTypeRef);

                    // Check if the parameter types and modifiers match
                    ParameterDecl parameterDecl = parameters[i];
                    TypeRefBase parameterTypeRef = parameterDecl.EvaluateType();
                    if (parameterTypeRef == null || !parameterTypeRef.IsSameRef(delegateParameterRef) || parameterDecl.IsRef != isRef || parameterDecl.IsOut != isOut)
                        return false;
                }
            }
            else
            {
                // If we don't have any parameters (they were "unspecified"), then any count and types are allowed
                // to match, except that we have to ensure that there aren't any 'out' parameters.
                if (delegateParameters is List<ParameterDecl>)
                    return Enumerable.All(((List<ParameterDecl>)delegateParameters), delegate(ParameterDecl parameterDecl) { return !parameterDecl.IsOut; });
                if (delegateParameters is ParameterInfo[])
                    return Enumerable.All(((ParameterInfo[])delegateParameters), delegate(ParameterInfo parameterInfo) { return !ParameterInfoUtil.IsOut(parameterInfo); });
            }
            return true;
        }

        /// <summary>
        /// Determine if the return type of the anonymous method is compatible with that of the specified delegate type.
        /// </summary>
        public virtual bool AreReturnTypesCompatible(TypeRef toTypeRef)
        {
            // The types of *all* of the return statements in the block must be convertible to the delegate's return type
            TypeRefBase delegateReturnType = toTypeRef.GetDelegateReturnType();
            if (delegateReturnType == null)
                return false;
            delegateReturnType = delegateReturnType.EvaluateTypeArgumentTypes(toTypeRef);
            return ScanReturnStatements(delegateReturnType, delegateReturnType.IsSameRef(TypeRef.VoidRef), _body);
        }

        protected bool ScanReturnStatements(TypeRefBase delegateReturnType, bool isVoidReturn, Block body)
        {
            // Recursively scan all bodies for Return statements
            if (body != null)
            {
                foreach (CodeObject codeObject in body)
                {
                    if (codeObject is Return)
                    {
                        Expression expression = ((Return)codeObject).Expression;
                        if (expression == null)
                        {
                            // If the return doesn't have an expression, the return type of the delegate must be 'void'
                            if (!isVoidReturn)
                                return false;
                        }
                        else
                        {
                            // Otherwise, the type of the expression must be convertible to the return type of the delegate
                            TypeRefBase typeRefBase = expression.EvaluateType();
                            if (typeRefBase != null)
                            {
                                if (!typeRefBase.IsImplicitlyConvertibleTo(delegateReturnType))
                                    return false;
                            }
                        }
                    }
                    else if (codeObject is BlockStatement)
                    {
                        if (!ScanReturnStatements(delegateReturnType, isVoidReturn, ((BlockStatement)codeObject).Body))
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            return new AnonymousMethodRef(this);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the anonymous method has braces by default.
        /// </summary>
        public virtual bool HasBracesDefault
        {
            get { return true; }
        }

        /// <summary>
        /// True if the anonymous method is delimited by braces.
        /// </summary>
        public bool HasBraces
        {
            get { return (_body != null && _body.HasBraces); }
            set
            {
                CreateBody().HasBraces = value;

                // If we just turned on braces, force a terminator on any expression in the body
                if (value && _body.Count > 0)
                    _body[0].HasTerminator = true;
            }
        }

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        public virtual void ReformatBlock()
        {
            if (_body != null)
            {
                if (!_body.IsGroupingSet)
                    _body.SetFormatFlag(FormatFlags.Grouping, HasBracesDefault);
                IsSingleLine = (IsSingleLineDefault && _body.IsSingleLineDefault && _body.Count < 2);
                if (_body.Count == 0)
                    _body.SetNewLines(0);
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_body == null || (!_body.IsFirstOnLine && _body.IsSingleLine))); }
            set
            {
                // Make sure there's a body, and set its IsFirstOnLine and IsSingleLine properties appropriately
                CreateBody();
                _body.IsFirstOnLine = !value;
                _body.IsSingleLine = value;
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description))
            {
                base.AsTextAfter(writer, flags);
                if (_body != null)
                {
                    // Increase the indent level for the body (unless disabled)
                    if (!HasNoIndentation)
                        writer.BeginIndentOnNewLineRelativeToParentOffset(this, true);
                    _body.AsText(writer, flags);
                    if (!HasNoIndentation)
                        writer.EndIndentation(this);
                }
            }
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            // This method is overridden so that any parens around the anonymous method will be rendered with the close
            // paren after the body (after AsTextAfter).  While we're at it, prefix support is omitted.
            int newLines = NewLines;
            if (newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);

            // Increase the indent level for any newlines that occur within the expression
            bool increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            bool hasParens = HasParens;
            if (hasParens)
                writer.Write(ParseTokenStartGroup);
            AsTextExpression(writer, passFlags | (flags & (RenderFlags.Attribute | RenderFlags.HasDotPrefix | RenderFlags.Declaration)));
            if (HasTerminator && !flags.HasFlag(RenderFlags.Description))
            {
                writer.Write(Statement.ParseTokenTerminator);
                CheckForAlignment(writer);  // Check for alignment of any EOL comments
            }
            if (!flags.HasFlag(RenderFlags.NoEOLComments))
                AsTextEOLComments(writer, flags);

            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));
            if (hasParens)
            {
                if (IsEndFirstOnLine)
                    writer.WriteLine();
                writer.Write(ParseTokenEndGroup);
            }

            if (increaseIndent)
                writer.EndIndentation(this);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (!flags.HasFlag(RenderFlags.Description))
            {
                writer.SetParentOffset();
                UpdateLineCol(writer, flags);
                writer.Write(ParseToken);
                bool showParens = (_parameters != null || HasInfixComments);
                if (showParens)
                    writer.Write(ParseTokenStart);
                AsTextInfixComments(writer, 0, flags);
                writer.WriteList(_parameters, passFlags, this);
                if (showParens)
                    writer.Write(ParseTokenEnd);
            }
            else
            {
                // If rendering as a description, just render as a delegate type
                GetReturnType().AsText(writer, passFlags | RenderFlags.IsPrefix);
                writer.Write(ParseToken);
                writer.Write(ParseTokenStart);
                writer.WriteList(_parameters, passFlags, this);
                if (IsEndFirstOnLine)
                    writer.WriteLine();
                writer.Write(ParseTokenEnd);
            }
        }

        #endregion
    }
}
