using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Variables
{
    /// <summary>
    /// Represents the declaration of a parameter to a method.
    /// </summary>
    /// <remarks>
    /// A ParameterDecl can optionally have a modifier of:
    ///     ref, out, params (last parameter only)
    /// Parameters don't support compound (multi) declarations like other VariableDecls.
    /// </remarks>
    public class ParameterDecl : VariableDecl
    {
        /// <summary>
        /// The token used to parse the end of a parameter list.
        /// </summary>
        public const string ParseTokenEnd = Expression.ParseTokenEndGroup;

        /// <summary>
        /// The token used to parse 'out' parameters.
        /// </summary>
        public const string ParseTokenOut = "out";

        /// <summary>
        /// The token used to parse 'params' parameters.
        /// </summary>
        public const string ParseTokenParams = "params";

        /// <summary>
        /// The token used to parse 'ref' parameters.
        /// </summary>
        public const string ParseTokenRef = "ref";

        /// <summary>
        /// The token used to parse between parameters.
        /// </summary>
        public const string ParseTokenSeparator = Expression.ParseTokenSeparator;

        /// <summary>
        /// The token used to parse the start of a parameter list.
        /// </summary>
        public const string ParseTokenStart = Expression.ParseTokenStartGroup;

        /// <summary>
        /// The token used to parse 'this' parameters.
        /// </summary>
        public const string ParseTokenThis = "this";

        protected ParameterModifier _modifier;

        /// <summary>
        /// Create a <see cref="ParameterDecl"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter</param>
        /// <param name="defaultValue">The default value expression for the parameter.</param>
        public ParameterDecl(string name, Expression type, Expression defaultValue)
            : base(name, type, defaultValue)
        { }

        /// <summary>
        /// Create a <see cref="ParameterDecl"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter</param>
        public ParameterDecl(string name, Expression type)
            : base(name, type, null)
        { }

        /// <summary>
        /// Create a <see cref="ParameterDecl"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter</param>
        /// <param name="modifier">The ParameterModifier for the parameter.</param>
        /// <param name="defaultValue">The default value expression for the parameter.</param>
        public ParameterDecl(string name, Expression type, ParameterModifier modifier, Expression defaultValue)
            : base(name, type, defaultValue)
        {
            _modifier = modifier;
        }

        /// <summary>
        /// Create a <see cref="ParameterDecl"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter</param>
        /// <param name="modifier">The ParameterModifier for the parameter.</param>
        public ParameterDecl(string name, Expression type, ParameterModifier modifier)
            : base(name, type, null)
        {
            _modifier = modifier;
        }

        protected ParameterDecl(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        protected ParameterDecl(Parser parser, CodeObject parent, string parseTokenEnd)
                    : base(parser, parent)
        {
            // Get any comments after the '('
            if (parser.LastToken.Text == ParseTokenStart)
                MoveAllComments(parser.LastToken);

            // If we're starting with an attribute, ignore any newline parsed in the base constructor
            if (parser.TokenText == Attribute.ParseTokenStart)
                IsFirstOnLine = false;

            Attribute.ParseAttributes(parser, this);  // Parse any attributes

            // Parse the optional modifier
            _modifier = ParseParameterModifier(parser.TokenText);
            if (_modifier != ParameterModifier.None)
            {
                parent.MoveFormatting(parser.Token);
                parser.NextToken();  // Move past the modifier
            }

            ParseType(parser);  // Parse the type

            ParseName(parser, parseTokenEnd);
            ParseInitialization(parser, parent);  // Parse the initialization (if any)

            MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "parameter"; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c> for a parameter.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the parameter is being passed out only.
        /// </summary>
        public bool IsOut
        {
            get { return (_modifier == ParameterModifier.Out); }
            set { _modifier = (value ? ParameterModifier.Out : (IsOut ? ParameterModifier.None : _modifier)); }
        }

        /// <summary>
        /// Determines if the parameter is a 'params' parameter.
        /// </summary>
        public bool IsParams
        {
            get { return (_modifier == ParameterModifier.Params); }
            set { _modifier = (value ? ParameterModifier.Params : (IsParams ? ParameterModifier.None : _modifier)); }
        }

        /// <summary>
        /// Determines if the parameter is being passed by reference.
        /// </summary>
        public bool IsRef
        {
            get { return (_modifier == ParameterModifier.Ref); }
            set { _modifier = (value ? ParameterModifier.Ref : (IsRef ? ParameterModifier.None : _modifier)); }
        }

        /// <summary>
        /// Always <c>false</c> for a parameter.
        /// </summary>
        public override bool IsStatic
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// The <see cref="ParameterModifier"/> for the parameter, if any.
        /// </summary>
        public ParameterModifier Modifier
        {
            get { return _modifier; }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return base.NewLines; }
            set
            {
                // If we're changing to zero, also change all prefix attributes to zero
                bool isFirstOnLine = (value != 0);
                if (_annotations != null && !isFirstOnLine && IsFirstOnLine)
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (annotation is Attribute)
                            annotation.IsFirstOnLine = false;
                    }
                }

                base.NewLines = value;
            }
        }

        /// <summary>
        /// Format a <see cref="ParameterModifier"/> as a string.
        /// </summary>
        public static string ParameterModifierToString(ParameterModifier modifier)
        {
            switch (modifier)
            {
                case ParameterModifier.Ref: return ParseTokenRef;
                case ParameterModifier.Out: return ParseTokenOut;
                case ParameterModifier.Params: return ParseTokenParams;
                case ParameterModifier.This: return ParseTokenThis;
            }
            return "";
        }

        /// <summary>
        /// Parse a list of parameters.
        /// </summary>
        public static ChildList<ParameterDecl> ParseList(Parser parser, CodeObject parent, string parseTokenStart,
            string parseTokenEnd, bool forceEmpty, out bool isEndFirstOnLine)
        {
            ChildList<ParameterDecl> parameterDecls = null;
            isEndFirstOnLine = false;
            if (parser.TokenText == parseTokenStart)
            {
                Token lastToken = parser.Token;
                parser.NextToken();  // Move past '(' or '['

                // Force an empty collection (vs null) if the flag is set
                if (forceEmpty)
                    parameterDecls = new ChildList<ParameterDecl>(parent);

                // Create a string of possible terminators (assuming 1 char terminators for now)
                string terminators = parseTokenEnd + ParseTokenTerminator + Block.ParseTokenStart + Block.ParseTokenEnd + Index.ParseTokenEnd;

                while (parser.TokenText != null && (parser.TokenText.Length != 1 || terminators.IndexOf(parser.TokenText[0]) < 0))
                {
                    ParameterDecl parameterDecl = new(parser, parent, parseTokenEnd);

                    // Move any preceeding comments to the current ParameterDecl
                    parameterDecl.MoveComments(lastToken);

                    if (parameterDecls == null)
                        parameterDecls = new ChildList<ParameterDecl>(parent);
                    parameterDecls.Add(parameterDecl);

                    lastToken = parser.Token;
                    if (parser.TokenText == ParseTokenSeparator)
                    {
                        parser.NextToken();  // Move past ','

                        // Associate any EOL comment on the ',' to the last ParameterDecl
                        parameterDecl.MoveEOLComment(lastToken, false, false);

                        // Move any remaining regular comments as Post comments, if on a line by themselves
                        if (parser.Token.IsFirstOnLine)
                            parameterDecl.MoveCommentsAsPost(lastToken);
                    }
                }

                if (parent.ParseExpectedToken(parser, parseTokenEnd))  // Move past ')' or ']'
                {
                    isEndFirstOnLine = parser.LastToken.IsFirstOnLine;
                    if (parameterDecls == null || parameterDecls.Count == 0)
                        parent.MoveAllComments(lastToken, false, false, AnnotationFlags.IsInfix1);
                    parent.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment with the parent
                }
            }
            return parameterDecls;
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> (<see cref="Comment"/>, <see cref="DocComment"/>, <see cref="Attribute"/>, <see cref="CompilerDirective"/>, or <see cref="Message"/>) to the <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="annotation">The <see cref="Annotation"/>.</param>
        /// <param name="atFront">Inserts at the front if true, otherwise adds at the end.</param>
        public override void AttachAnnotation(Annotation annotation, bool atFront)
        {
            // Force attached annotations to same-line for parameters
            annotation.IsFirstOnLine = false;
            base.AttachAnnotation(annotation, atFront);
        }

        /// <summary>
        /// Create a reference to the <see cref="ParameterDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="ParameterRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new ParameterRef(this, isFirstOnLine);
        }

        protected static ParameterModifier ParseParameterModifier(string modifierName)
        {
            ParameterModifier modifier;
            switch (modifierName)
            {
                case ParseTokenRef: modifier = ParameterModifier.Ref; break;
                case ParseTokenOut: modifier = ParameterModifier.Out; break;
                case ParseTokenParams: modifier = ParameterModifier.Params; break;
                case ParseTokenThis: modifier = ParameterModifier.This; break;
                default: modifier = ParameterModifier.None; break;
            }
            return modifier;
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            if (_modifier != ParameterModifier.None)
                writer.Write(ParameterModifierToString(_modifier) + " ");
            AsTextType(writer, flags);
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);
        }
    }
}