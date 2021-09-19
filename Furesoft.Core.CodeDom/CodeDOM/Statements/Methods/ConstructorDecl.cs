using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a special method used to initialize an instance of a <see cref="ClassDecl"/> or <see cref="StructDecl"/>.
    /// If a constructor is static, it can't have any parameters, and can't have any other modifiers
    /// except extern (in which case it has no body).
    /// </summary>
    public class ConstructorDecl : MethodDeclBase
    {
        /// <summary>
        /// Optional ThisInitializer or BaseInitializer call to another constructor.
        /// </summary>
        protected ConstructorInitializer _initializer;

        /// <summary>
        /// Create a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorDecl(params ParameterDecl[] parameters)
            : base(null, null, parameters)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorDecl(Modifiers modifiers, params ParameterDecl[] parameters)
            : base(null, null, modifiers, parameters)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorDecl(Modifiers modifiers, CodeObject body, params ParameterDecl[] parameters)
            : base(null, null, modifiers, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="ConstructorDecl"/>.
        /// </summary>
        public ConstructorDecl(CodeObject body, params ParameterDecl[] parameters)
            : base(null, null, body, parameters)
        { }

        protected ConstructorDecl(Parser parser, CodeObject parent, ParseFlags flags)
                    : base(parser, parent)
        {
            ParseMethodNameAndType(parser, parent, true, true);
            _name = null;                          // Clear name (we use the parent's name instead)
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers

            // Move any trailing compiler directives to the Infix2 position (assume we have an initializer)
            MoveAnnotations(AnnotationFlags.IsPostfix, AnnotationFlags.IsInfix2);

            ParseParameters(parser);  // Parse the parameters

            // Check for compiler directives, storing them as infix annotations on the parent
            Block.ParseCompilerDirectives(parser, this, AnnotationFlags.IsInfix2);

            SetField(ref _initializer, ConstructorInitializer.Parse(parser, this), false);

            // If we don't have an initializer, move any trailing compiler directives to the Postfix position
            if (_initializer == null)
                MoveAnnotations(AnnotationFlags.IsInfix2, AnnotationFlags.IsPostfix);

            ParseTerminatorOrBody(parser, flags);
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "constructor"; }
        }

        /// <summary>
        /// An optional <see cref="ConstructorInitializer"/>.
        /// </summary>
        public ConstructorInitializer Initializer
        {
            get { return _initializer; }
            set { SetField(ref _initializer, value, true); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_initializer == null || (!_initializer.IsFirstOnLine && _initializer.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_initializer != null)
                {
                    if (value)
                        _initializer.IsFirstOnLine = false;
                    _initializer.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// The name of the <see cref="ConstructorDecl"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                // Always use the name of the current parent TypeDecl if possible.
                if (_parent is TypeDecl)
                    return ((TypeDecl)_parent).Name;

                // The _name field should normally be null, but might be a string in rare
                // cases, such as a compiler-generated ConstructorDecl for a Type parent.
                return (string)_name;
            }
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 0 (MethodDecl uses 50, LambdaExpression uses 100, Call uses 200, Cast uses 300, Expression parens uses 400)
            Parser.AddParsePoint(ParameterDecl.ParseTokenStart, Parse, typeof(TypeDecl));
        }

        /// <summary>
        /// Parse a <see cref="ConstructorDecl"/>.
        /// </summary>
        public static ConstructorDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Validate that we have an unused identifier token that matches our parent TypeDecl name
            // (otherwise, abort and give MethodDecl a chance to parse it)
            if (parser.HasUnusedIdentifier && parent is TypeDecl && parser.LastUnusedTokenText == ((TypeDecl)parent).Name)
                return new ConstructorDecl(parser, parent, flags);
            return null;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ConstructorDecl clone = (ConstructorDecl)base.Clone();
            clone.CloneField(ref clone._initializer, _initializer);
            return clone;
        }

        /// <summary>
        /// Create a reference to the <see cref="ConstructorDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="ConstructorRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new ConstructorRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Get a reference to the declaring type of the constructor.
        /// </summary>
        public TypeRef GetDeclaringType()
        {
            TypeRef declaringType;
            if (_parent is TypeDecl)
                declaringType = (TypeRef)_parent.CreateRef();
            // Handle the special case where the Parent is null for the generated ConstructorDecl of an external delegate type
            else if (_parent == null && IsGenerated && ParameterCount == 1)
                declaringType = Parameters[0].Type.SkipPrefixes() as TypeRef;
            else
                declaringType = null;
            return declaringType;
        }

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        public override void ReformatBlock()
        {
            base.ReformatBlock();

            // Format the constructor initializer IsFirstOnLine setting to match the body
            if (_initializer != null && !_initializer.IsNewLinesSet)
                _initializer.IsFirstOnLine = _body.IsFirstOnLine;
        }

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(Name);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            AsTextAnnotations(writer, AnnotationFlags.IsInfix2, flags);
            if (_initializer != null && !flags.HasFlag(RenderFlags.Description))
            {
                writer.BeginIndentOnNewLine(this);
                _initializer.AsText(writer, flags | RenderFlags.IncreaseIndent
                    | (_initializer.IsFirstOnLine ? 0 : RenderFlags.PrefixSpace) | RenderFlags.HasTerminator);
                writer.EndIndentation(this);
            }
            base.AsTextAfter(writer, flags);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            if (_initializer == null)
                base.AsTextSuffix(writer, flags);
        }

        protected override void DefaultFormatField(CodeObject field)
        {
            base.DefaultFormatField(field);

            // Default the constructor initializer IsFirstOnLine setting to match the body
            if (!field.IsNewLinesSet)
                field.SetNewLines((_body != null && _body.IsFirstOnLine) ? 1 : 0);
        }
    }
}