using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base
{
    /// <summary>
    /// The common base class of <see cref="FieldDecl"/>, <see cref="LocalDecl"/>, <see cref="ParameterDecl"/>, and <see cref="EnumMemberDecl"/>.
    /// </summary>
    public abstract class VariableDecl : Statement, IVariableDecl
    {
        protected Expression _initialization;
        protected string _name;
        protected Expression _type;

        protected VariableDecl(string name, Expression type, Expression initialization)
        {
            _name = name;
            if (type != null)
                SetField(ref _type, type, true);
            Initialization = initialization;
        }

        protected VariableDecl(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the variable has an initialization <see cref="Expression"/>.
        /// </summary>
        public bool HasInitialization
        {
            get { return (_initialization != null); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// An optional initialization <see cref="Expression"/>.
        /// </summary>
        public Expression Initialization
        {
            get { return _initialization; }
            set { SetField(ref _initialization, value, true); }
        }

        /// <summary>
        /// True if the variable is const.
        /// </summary>
        public abstract bool IsConst { get; set; }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_type == null || (!_type.IsFirstOnLine && _type.IsSingleLine))
                    && (_initialization == null || (!_initialization.IsFirstOnLine && _initialization.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_type != null)
                {
                    if (value)
                    {
                        _type.IsFirstOnLine = false;
                        _type.IsSingleLine = true;
                    }
                }
                if (_initialization != null)
                {
                    if (value)
                        _initialization.IsFirstOnLine = false;
                    _initialization.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return !HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// True if the variable is static.
        /// </summary>
        public abstract bool IsStatic { get; set; }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The type of the variable declaration.
        /// </summary>
        public virtual Expression Type
        {
            get { return _type; }
            set { SetField(ref _type, value, true); }
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        public virtual void AsTextType(CodeWriter writer, RenderFlags flags)
        {
            Expression type = Type;
            if (type != null)
            {
                RenderFlags passFlags = (flags & RenderFlags.PassMask);
                type.AsText(writer, passFlags | RenderFlags.IsPrefix | RenderFlags.Declaration);
            }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            VariableDecl clone = (VariableDecl)base.Clone();
            clone.CloneField(ref clone._type, _type);
            clone.CloneField(ref clone._initialization, _initialization);
            return clone;
        }

        /// <summary>
        /// Get the full name of the <see cref="VariableDecl"/>, including the namespace name (if any).
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public virtual string GetFullName(bool descriptive)
        {
            return _name;
        }

        /// <summary>
        /// Get the full name of the <see cref="VariableDecl"/>, including the namespace name (if any).
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        protected void AsTextInitialization(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(" " + Assignment.ParseToken);
            _initialization.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            AsTextType(writer, flags);
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);
        }

        /// <summary>
        /// Move NewLines, LineNumber, Column, and any EOL comment from the specified <see cref="Token"/>.
        /// </summary>
        protected void MoveLocationAndComment(Token token)
        {
            NewLines = token.NewLines;
            SetLineCol(token);
            MoveEOLComment(token);
        }

        /// <summary>
        /// Parse the initialization (if any).
        /// </summary>
        protected void ParseInitialization(Parser parser, CodeObject parent)
        {
            if (parser.TokenText == Assignment.ParseToken)
            {
                Token equalsToken = parser.Token;
                parser.NextToken();  // Move past the '='
                SetField(ref _initialization, Expression.Parse(parser, parent), false);
                if (_initialization != null)
                {
                    // Move any newlines on the '=' to the initialization expression instead
                    _initialization.MoveFormatting(equalsToken);

                    // Move any comments after the '=' to the initialization expression
                    _initialization.MoveCommentsToLeftMost(equalsToken, false);

                    // If the initialization expression is single-line and it's the last thing on the line (the
                    // next token is first-on-line), then move any EOL comment on it to the parent (this handles
                    // the case of EOL comments on intializers in a multi-variable list when the commas occur
                    // *before* each item on the line).
                    if (_initialization.IsSingleLine && (parser.Token == null || parser.Token.IsFirstOnLine))
                        MoveEOLComment(_initialization);
                }
            }
        }

        protected void ParseName(Parser parser, string parseTokenEnd)
        {
            if (parser.TokenText != Expression.ParseTokenSeparator && parser.TokenText != parseTokenEnd)
            {
                Token token = parser.Token;
                _name = parser.GetIdentifierText();  // Parse the name
                SetLineCol(token);
            }
        }

        /// <summary>
        /// This method is used when parsing forwards starting with the type is possible.
        /// </summary>
        protected void ParseType(Parser parser)
        {
            Expression expression = Expression.Parse(parser, this, true, ParseFlags.Type);
            MoveFormatting(expression);
            SetField(ref _type, expression, false);
        }
    }
}