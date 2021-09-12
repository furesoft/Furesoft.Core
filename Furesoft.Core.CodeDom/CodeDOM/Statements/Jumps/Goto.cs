// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Redirects execution to the specified <see cref="Label"/> or <see cref="SwitchItem"/> (<see cref="Case"/> or <see cref="Default"/>).
    /// </summary>
    public class Goto : Statement
    {
        #region /* FIELDS */

        // Should evaluate to a GotoTargetRef (LabelRef or a SwitchItemRef) or an UnresolvedRef
        protected SymbolicRef _target;

        // The constant expression used by a "goto case ..."
        protected Expression _constantExpression;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Goto"/> to a <see cref="Label"/>.
        /// </summary>
        public Goto(Label label)
        {
            Target = new LabelRef(label);
        }

        /// <summary>
        /// Create a <see cref="Goto"/> to a <see cref="SwitchItem"/>.
        /// </summary>
        public Goto(SwitchItem item)
        {
            Target = new SwitchItemRef(item);
        }

        /// <summary>
        /// Create a <see cref="Goto"/> to a string name.
        /// </summary>
        public Goto(string name)
        {
            Target = new UnresolvedRef(name);
        }

        /// <summary>
        /// Create a <see cref="Goto"/> to a constant <see cref="Expression"/>.
        /// </summary>
        public Goto(Expression constantExpression)
        {
            ConstantExpression = constantExpression;
            Target = new UnresolvedRef(Case.ParseToken + " " + _constantExpression.AsString());
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The target <see cref="GotoTargetRef"/> (<see cref="LabelRef"/> or <see cref="SwitchItemRef"/>) or <see cref="UnresolvedRef"/>.
        /// </summary>
        public SymbolicRef Target
        {
            get { return _target; }
            set { SetField(ref _target, value, true); }
        }

        /// <summary>
        /// The constant expression if this is a "goto case ...", otherwise null.
        /// </summary>
        public Expression ConstantExpression
        {
            get { return _constantExpression; }
            set { SetField(ref _constantExpression, value, true); }
        }

        /// <summary>
        /// The hidden GotoTargetRef (or UnresolvedRef) that represents the goto target if we have a "goto case ...".
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return (_constantExpression != null ? _target : null); }
        }

        /// <summary>
        /// True if this is a 'goto case ...'.
        /// </summary>
        public bool IsGotoCase
        {
            get { return (_constantExpression != null); }
        }

        /// <summary>
        /// True if this is a 'goto default'.
        /// </summary>
        public bool IsGotoDefault
        {
            get { return (_target.AsString() == Default.ParseToken); }
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
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Goto clone = (Goto)base.Clone();
            clone.CloneField(ref clone._target, _target);
            clone.CloneField(ref clone._constantExpression, _constantExpression);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "goto";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="Goto"/>.
        /// </summary>
        public static Goto Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Goto(parser, parent);
        }

        protected Goto(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'goto'
            Token startToken = parser.Token;
            string target = "";

            // Handle "goto case ..."
            if (parser.TokenText == Case.ParseToken)
            {
                parser.NextToken();  // Move past 'case'
                SetField(ref _constantExpression, Expression.Parse(parser, this, true, ParseTokenTerminator), false);
                target = Case.ParseToken + " " + _constantExpression.AsString();
            }
            else  // Handle "goto <label>" or "goto default"
            {
                // Build a symbolic reference from all text up to the ';' (or EOL)
                bool first = true;
                while (parser.TokenText != Terminator && parser.Token != null && !parser.Token.IsFirstOnLine)
                {
                    target += (first ? "" : parser.Token.LeadingWhitespace) + parser.TokenText;
                    parser.NextToken();
                    first = false;
                }
            }
            SetField(ref _target, new UnresolvedRef(target, ResolveCategory.GotoTarget, startToken.LineNumber, startToken.ColumnNumber), false);

            ParseTerminator(parser);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_constantExpression != null)
                _constantExpression = (Expression)_constantExpression.Resolve(ResolveCategory.Expression, flags);
            _target = (SymbolicRef)_target.Resolve(ResolveCategory.GotoTarget, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_constantExpression != null && _constantExpression.HasUnresolvedRef())
                return true;
            if (_target != null && _target.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_target == null || (!_target.IsFirstOnLine && _target.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _target != null)
                {
                    _target.IsFirstOnLine = false;
                    _target.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            // If we have a constant expression ("goto case ..."), always render it instead of
            // the target reference.
            if (_constantExpression != null)
            {
                writer.Write(Case.ParseToken + " ");
                _constantExpression.AsText(writer, flags);
            }
            else
                _target.AsText(writer, flags);
        }

        #endregion
    }
}
