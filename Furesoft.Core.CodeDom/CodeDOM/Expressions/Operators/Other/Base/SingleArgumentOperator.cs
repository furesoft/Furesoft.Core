// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all operators with a fixed single argument (<see cref="Ref"/>, <see cref="Out"/>,
    /// <see cref="Checked"/>, <see cref="Unchecked"/>, <see cref="TypeOf"/>, <see cref="SizeOf"/>, <see cref="DefaultValue"/>).
    /// </summary>
    public abstract class SingleArgumentOperator : Operator
    {
        protected Expression _expression;

        protected SingleArgumentOperator(Expression expression)
        {
            Expression = expression;
        }

        protected SingleArgumentOperator(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The <see cref="Expression"/> being operated on.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// True if the argument has parens around it.
        /// </summary>
        public virtual bool HasArgumentParens
        {
            get { return true; }  // Default is argument has parens
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

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool hasParens = HasArgumentParens;
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            writer.Write(hasParens ? ParseTokenStartGroup : " ");
            if (_expression != null)
                _expression.AsText(writer, passFlags);
            if (hasParens)
                writer.Write(ParseTokenEndGroup);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            SingleArgumentOperator clone = (SingleArgumentOperator)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        protected void ParseKeywordAndArgument(Parser parser, ParseFlags flags)
        {
            // Save the starting token of the expression for later
            Token startingToken = parser.ParentStartingToken;

            parser.NextToken();  // Move past the keyword

            // If the argument has parens, it's a normal operator, like 'typeof()', otherwise it's a top-level
            // operator (ref/out) and we have to parse it as such.
            if (HasArgumentParens)
            {
                ParseExpectedToken(parser, ParseTokenStartGroup);  // Move past '('
                SetField(ref _expression, Parse(parser, this, false, ParseTokenEndGroup, flags), false);
                ParseExpectedToken(parser, ParseTokenEndGroup);  // Move past ')'
            }
            else
                SetField(ref _expression, Parse(parser, this, true, null, flags), false);

            // Set the parent starting token to the beginning of the expression
            parser.ParentStartingToken = startingToken;
        }
    }
}