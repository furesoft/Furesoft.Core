// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an un-named method that can be assigned to a delegate.
    /// </summary>
    /// <remarks>
    /// An AnonymousMethod distinguishes between having zero parameters and unspecified parameters (no parens
    /// at all).  If the parameters are unspecified, the anonymous method is convertible to a delegate with any
    /// parameter list not containing out parameters.  It's possible that the empty parens will be needed in
    /// some cases to prevent ambiguity.
    /// </remarks>
    public class AnonymousMethod : Expression, IParameters, IBlock
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = DelegateDecl.ParseToken;

        /// <summary>
        /// The token used to parse the end of the parameter list of an anonymous method.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// The token used to parse the start of the parameter list of an anonymous method.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        protected Block _body;
        protected ChildList<ParameterDecl> _parameters;

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
        /// True if the anonymous method has braces by default.
        /// </summary>
        public virtual bool HasBracesDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public bool HasHeader
        {
            get { return true; }
        }

        /// <summary>
        /// True if the anonymous method has any parameters.
        /// </summary>
        public bool HasParameters
        {
            get { return (_parameters != null && _parameters.Count > 0); }
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

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsTopLevel
        {
            get { return false; }
        }

        /// <summary>
        /// The number of parameters the anonymous method has.
        /// </summary>
        public int ParameterCount
        {
            get { return (_parameters != null ? _parameters.Count : 0); }
        }

        /// <summary>
        /// The parameters of the anonymous method (if any).
        /// </summary>
        public ChildList<ParameterDecl> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Parse an <see cref="AnonymousMethod"/>.
        /// </summary>
        public static AnonymousMethod Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AnonymousMethod(parser, parent);
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
        /// Add one or more <see cref="ParameterDecl"/>s.
        /// </summary>
        public void AddParameters(params ParameterDecl[] parameterDecls)
        {
            CreateParameters().AddRange(parameterDecls);
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
                delegateExpression = ((Assignment)parent).Left.SkipPrefixes();
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
                    TypeRefBase typeRefBase = ((NewObject)parent.Parent.Parent).SkipPrefixes() as TypeRefBase;

                    // Handle a Dictionary where the value is a delegate type
                    if (typeRefBase != null && typeRefBase.IsSameGenericType(TypeRef.Dictionary2Ref))
                        delegateExpression = typeRefBase.TypeArguments[1];
                }
            }
            return delegateExpression;
        }

        /// <summary>
        /// Determine the return type of the anonymous method (never null - will be 'void' instead).
        /// </summary>
        public TypeRefBase GetReturnType()
        {
            TypeRefBase returnTypeRef = null;
            ScanReturnTypes(ref returnTypeRef, _body);
            return (returnTypeRef ?? TypeRef.VoidRef);
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

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 100 (DelegateDecl uses 0)
            Parser.AddParsePoint(ParseToken, 100, Parse);
        }

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
                        TypeRefBase typeRef = (expression == null ? TypeRef.VoidRef : expression.SkipPrefixes() as TypeRefBase);
                        returnTypeRef = (returnTypeRef == null ? typeRef : TypeRef.GetCommonType(returnTypeRef, typeRef));
                    }
                    else if (codeObject is BlockStatement)
                        ScanReturnTypes(ref returnTypeRef, ((BlockStatement)codeObject).Body);
                }
            }
        }
    }
}