using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base
{
    /// <summary>
    /// The common base class of <see cref="NewObject"/> and <see cref="NewArray"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Expression"/> of the <see cref="ArgumentsOperator"/> base class should evaluate to a <see cref="TypeRef"/>.
    /// For <see cref="NewObject"/>, an additional hidden <see cref="ConstructorRef"/> exists.
    /// </remarks>
    public abstract class NewOperator : ArgumentsOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "new";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// Optional array initializer.
        /// </summary>
        protected Initializer _initializer;

        protected NewOperator(Expression expression, params Expression[] parameters)
            : base(expression, parameters)
        { }

        protected NewOperator(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// Optional array initializer.
        /// </summary>
        public Initializer Initializer
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
                    _initializer.IsFirstOnLine = !value;
                    _initializer.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse a <see cref="NewObject"/> or <see cref="NewArray"/> operator.
        /// </summary>
        public static NewOperator Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Abort if our parent is a TypeDecl (the 'new' is probably part of a method declaration)
            if (parent is TypeDecl)
                return null;

            NewOperator result = null;

            // Peek ahead to see if we have a valid non-array type
            TypeRefBase.PeekType(parser, parser.PeekNextToken(), true, flags | ParseFlags.Type);
            Token token = parser.LastPeekedToken;
            if (token != null)
            {
                // If we found a '[', assume NewArray
                if (token.Text == NewArray.ParseTokenStart)
                    result = new NewArray(parser, parent);
                // If we found '(' or '{', assume NewObject
                else if (token.Text == ParameterDecl.ParseTokenStart || token.Text == Initializer.ParseTokenStart)
                    result = new NewObject(parser, parent);
            }

            // Last chance - invalid code might still parse better as a NewObject, so assume that's
            // what it is if our parent is a VariableDecl.
            if (result == null && parent is VariableDecl)
                result = new NewObject(parser, parent);

            // If we didn't create an object, return null (the 'new' is probably part of a method declaration)
            return result;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            NewOperator clone = (NewOperator)base.Clone();
            clone.CloneField(ref clone._initializer, _initializer);
            return clone;
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        protected override void AsTextInitializer(CodeWriter writer, RenderFlags flags)
        {
            if (_initializer != null)
            {
                // Make the indent level for the initializer relative to the parent (unless disabled)
                if (!_initializer.HasNoIndentation)
                    writer.BeginIndentOnNewLineRelativeToParentOffset(this, true);
                _initializer.AsText(writer, flags | RenderFlags.PrefixSpace);
                if (!_initializer.HasNoIndentation)
                    writer.EndIndentation(this);
            }
        }

        protected void ParseInitializer(Parser parser, CodeObject parent)
        {
            if (parser.TokenText == Initializer.ParseTokenStart)
                SetField(ref _initializer, new Initializer(parser, parent), false);
        }
    }
}
