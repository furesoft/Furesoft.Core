using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Exceptions;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Exceptions
{
    /// <summary>
    /// Enforces the disposal of an object when it's no longer needed.  It accepts either an <see cref="Expression"/> or a
    /// <see cref="LocalDecl"/>, and internally wraps this in a try/finally that calls Dispose() on the object (which must
    /// implement the <see cref="IDisposable"/> interface).
    /// </summary>
    public class Using : BlockStatement
    {
        /// <summary>
        /// True for multi-part statements, such as try/catch/finally or if/else.
        /// </summary>
        public const string ParseToken = "using";

        /// <summary>
        /// Can be an Expression that evaluates to a VariableRef of a type that implements IDisposable, or
        /// a LocalDecl of a type that implements IDisposable.
        /// </summary>
        protected CodeObject _target;

        /// <summary>
        /// Create a <see cref="Using"/>.
        /// </summary>
        public Using(LocalDecl localDecl, CodeObject body)
            : base(body, false)
        {
            Target = localDecl;
        }

        /// <summary>
        /// Create a <see cref="Using"/>.
        /// </summary>
        public Using(LocalDecl localDecl)
            : base(null, false)
        {
            Target = localDecl;
        }

        /// <summary>
        /// Create a <see cref="Using"/>.
        /// </summary>
        public Using(Expression expression, CodeObject body)
            : base(body, false)
        {
            Target = expression;
        }

        /// <summary>
        /// Create a <see cref="Using"/>.
        /// </summary>
        public Using(Expression expression)
            : base(null, false)
        {
            Target = expression;
        }

        protected Using(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'using'
            ParseExpectedToken(parser, Expression.ParseTokenStartGroup);  // Move past '('

            // Parse either LocalDecl or Expression (object)
            if (LocalDecl.PeekLocalDecl(parser))
                SetField(ref _target, LocalDecl.Parse(parser, this, false, true), false);
            else
                SetField(ref _target, Expression.Parse(parser, this, true, Expression.ParseTokenEndGroup), false);

            ParseExpectedToken(parser, Expression.ParseTokenEndGroup);  // Move past ')'

            new Block(out _body, parser, this, false);  // Parse the body
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
        /// True if contains a single nested Using statement as a child.
        /// </summary>
        public bool HasNestedUsing
        {
            get { return (_body != null && !_body.HasBraces && _body.Count == 1 && _body[0] is Using); }
        }

        /// <summary>
        /// True if this is a nested using.
        /// </summary>
        public bool IsNestedUsing
        {
            get { return (_parent is Using && !((Using)_parent).Body.HasBraces && ((Using)_parent).Body.Count == 1); }
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
                if (_target != null)
                {
                    if (value)
                        _target.IsFirstOnLine = false;
                    _target.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The target code object.
        /// </summary>
        public CodeObject Target
        {
            get { return _target; }
            set
            {
                // If the target is a LocalDecl and it already has a parent, then assume it's an existing local
                // and create a ref to it, otherwise assume it's a child-target local (or other expression).
                SetField(ref _target, (value is LocalDecl && value.Parent != null ? value.CreateRef() : value), true);
            }
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 200 (Alias uses 0, UsingDirective uses 100)
            Parser.AddParsePoint(ParseToken, 200, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Pase a <see cref="Using"/>.
        /// </summary>
        public static Using Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Using(parser, parent);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Using clone = (Using)base.Clone();
            clone.CloneField(ref clone._target, _target);
            return clone;
        }

        /// <summary>
        /// Determines if the body of the <see cref="BlockStatement"/> should be formatted with braces.
        /// </summary>
        public override bool ShouldHaveBraces()
        {
            // Turn off braces if we have a nested child using
            return (base.ShouldHaveBraces() && !HasNestedUsing);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags | (HasNestedUsing ? RenderFlags.NoBlockIndent : 0));
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_target != null)
                _target.AsText(writer, flags);
        }

        protected override bool IsChildIndented(CodeObject obj)
        {
            // The child object can only be indented if it's the first thing on the line
            if (obj.IsFirstOnLine)
            {
                // If the child isn't a nested using and isn't a prefix, it should be indented
                return !(HasNestedUsing && _body[0] == obj) && !IsChildPrefix(obj);
            }
            return false;
        }
    }
}
