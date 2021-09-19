using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a 'for' loop.
    /// </summary>
    /// <remarks>
    /// Has 3 parts - initialization, conditional, and iteration plus a body that is repeatedly executed as
    /// long as the conditional expression evaluates to true.  If the conditional is false at the start, the
    /// body is never executed.  Each of the 3 parts can be left blank, the first and last parts are actually
    /// (comma-separated) lists of 0-N expressions, and the first part can also be a single <see cref="LocalDecl"/>.
    /// </remarks>
    public class For : BlockStatement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "for";

        protected Expression _conditional;

        /// <summary>
        /// Can be either a LocalDecl or a ChildList of Expressions.
        /// </summary>
        protected object _initializations;

        protected ChildList<Expression> _iterations;

        /// <summary>
        /// Create a <see cref="For"/> with the specified <see cref="CodeObject"/> in the body.
        /// </summary>
        public For(Expression initialization, Expression conditional, Expression iteration)
        {
            if (initialization != null)
                Initializations.Add(initialization);
            Conditional = conditional;
            if (iteration != null)
                Iterations.Add(iteration);
        }

        /// <summary>
        /// Create a <see cref="For"/> with the specified <see cref="CodeObject"/> in the body.
        /// </summary>
        public For(LocalDecl initialization, Expression conditional, Expression iteration)
        {
            Initialization = initialization;
            Conditional = conditional;
            if (iteration != null)
                Iterations.Add(iteration);
        }

        /// <summary>
        /// Create a <see cref="For"/> with the specified <see cref="CodeObject"/> in the body.
        /// </summary>
        public For(Expression initialization, Expression conditional, Expression iteration, CodeObject body)
            : base(body, true)
        {
            if (initialization != null)
                Initializations.Add(initialization);
            Conditional = conditional;
            if (iteration != null)
                Iterations.Add(iteration);
        }

        /// <summary>
        /// Create a <see cref="For"/> with the specified <see cref="CodeObject"/> in the body.
        /// </summary>
        public For(LocalDecl initialization, Expression conditional, Expression iteration, CodeObject body)
            : base(body, true)
        {
            Initialization = initialization;
            Conditional = conditional;
            if (iteration != null)
                Iterations.Add(iteration);
        }

        protected For(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'for'
            ParseExpectedToken(parser, Expression.ParseTokenStartGroup);

            // Parse either LocalDecl or Expression List
            if (LocalDecl.PeekLocalDecl(parser))
                _initializations = LocalDecl.Parse(parser, this, false, true);
            else
                _initializations = Expression.ParseList(parser, this, Expression.ParseTokenEndGroup);
            if (_initializations == null)
                MoveAllComments(parser.LastToken, false, false, AnnotationFlags.IsInfix1);
            ParseExpectedToken(parser, Terminator);  // Move past ';'

            SetField(ref _conditional, Expression.Parse(parser, this, true, Terminator + Expression.ParseTokenEndGroup), false);
            if (_conditional == null)
                MoveAllComments(parser.LastToken, false, false, AnnotationFlags.IsInfix2);
            ParseExpectedToken(parser, Terminator);  // Move past ';'

            _iterations = Expression.ParseList(parser, this, Expression.ParseTokenEndGroup);
            if (_iterations == null)
                MoveAllComments(parser.LastToken, false, false, AnnotationFlags.IsInfix3);

            ParseExpectedToken(parser, Expression.ParseTokenEndGroup);

            if (parser.TokenText == Terminator && !parser.Token.IsFirstOnLine)
                ParseTerminator(parser);  // Handle same-line ';' (null body)
            else
                new Block(out _body, parser, this, false);  // Parse the body
        }

        /// <summary>
        /// The conditional <see cref="Expression"/>.
        /// </summary>
        public Expression Conditional
        {
            get { return _conditional; }
            set { SetField(ref _conditional, value, true); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        /// <summary>
        /// A <see cref="LocalDecl"/> used as the loop initializer.
        /// </summary>
        public LocalDecl Initialization
        {
            get { return (_initializations is LocalDecl ? (LocalDecl)_initializations : null); }
            set
            {
                if (value != null && value.Parent != null)
                    throw new Exception("The LocalDecl used for the initialization variable of a For must be new, not one already owned by another Parent object.");
                SetField(ref _initializations, value, true);
            }
        }

        /// <summary>
        /// A list of <see cref="Expression"/>s used as the loop initializer.
        /// </summary>
        public ChildList<Expression> Initializations
        {
            get
            {
                if (_initializations is LocalDecl)
                    return null;
                if (_initializations == null)
                    _initializations = new ChildList<Expression>(this);
                return (ChildList<Expression>)_initializations;
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine
                    && ((_initializations is ChildList<Expression> && (((ChildList<Expression>)_initializations).Count == 0
                        || (!((ChildList<Expression>)_initializations)[0].IsFirstOnLine && ((ChildList<Expression>)_initializations).IsSingleLine)))
                        || (_initializations is CodeObject && !((CodeObject)_initializations).IsFirstOnLine && ((CodeObject)_initializations).IsSingleLine)
                        || _initializations == null)
                    && (_conditional == null || (!_conditional.IsFirstOnLine && _conditional.IsSingleLine))
                    && (_iterations == null || _iterations.Count == 0 || (!_iterations[0].IsFirstOnLine && _iterations.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_initializations is ChildList<Expression>)
                    {
                        ((ChildList<Expression>)_initializations)[0].IsFirstOnLine = false;
                        ((ChildList<Expression>)_initializations).IsSingleLine = true;
                    }
                    else if (_initializations is CodeObject)
                    {
                        ((CodeObject)_initializations).IsFirstOnLine = false;
                        ((CodeObject)_initializations).IsSingleLine = true;
                    }
                    if (_conditional != null)
                    {
                        _conditional.IsFirstOnLine = false;
                        _conditional.IsSingleLine = true;
                    }
                    if (_iterations != null && _iterations.Count > 0)
                    {
                        _iterations[0].IsFirstOnLine = false;
                        _iterations.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// The list of <see cref="Expression"/>s used for the loop iterations section.
        /// </summary>
        public ChildList<Expression> Iterations
        {
            get
            {
                if (_iterations == null)
                    _iterations = new ChildList<Expression>(this);
                return _iterations;
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="For"/>.
        /// </summary>
        public static BlockStatement Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            For @for = new For(parser, parent);

            if (AutomaticCodeCleanup && !parser.IsGenerated)
            {
                // Normalize 'for (;;)' to 'while (true)' (with a null conditional)
                if (@for._initializations == null && @for._conditional == null && @for._iterations == null && !@for.HasInfixComments)
                {
                    While @while = new While(@for);
                    @while.SetLineCol(@for);
                    return @while;
                }
            }

            return @for;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            For clone = (For)base.Clone();
            if (_initializations is ChildList<Expression>)
                clone._initializations = ChildListHelpers.Clone((ChildList<Expression>)_initializations, clone);
            else
                clone.CloneField(ref clone._initializations, _initializations);
            clone.CloneField(ref clone._conditional, _conditional);
            clone._iterations = ChildListHelpers.Clone(_iterations, clone);
            return clone;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            AsTextInfixComments(writer, AnnotationFlags.IsInfix1, flags);
            if (Initialization != null)
                Initialization.AsText(writer, flags);
            else if (Initializations != null && Initializations.Count > 0)
                writer.WriteList(Initializations, flags, this);
            else
                writer.Write(" ");
            writer.Write(ParseTokenTerminator + " ");
            AsTextInfixComments(writer, AnnotationFlags.IsInfix2, flags);
            if (_conditional != null)
                _conditional.AsText(writer, flags);
            writer.Write(ParseTokenTerminator + " ");
            AsTextInfixComments(writer, AnnotationFlags.IsInfix3, flags);
            writer.WriteList(_iterations, flags, this);
        }
    }
}