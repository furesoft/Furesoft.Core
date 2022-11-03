using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Exceptions;

/// <summary>
/// Indicates that an exception should be thrown, and has an expression that should evaluate
/// to an object of type <see cref="Exception"/>.  If the expression is omitted (null), then any
/// currently active exception (inside a <see cref="Catch"/> body) is re-thrown.
/// </summary>
public class Throw : Statement
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "throw";

    protected Expression _expression;

    /// <summary>
    /// Create a <see cref="Throw"/>.
    /// </summary>
    public Throw(Expression expression)
    {
        Expression = expression;
    }

    /// <summary>
    /// Create a <see cref="Throw"/>.
    /// </summary>
    public Throw()
    { }

    protected Throw(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        parser.NextToken();  // Move past 'throw'
        SetField(ref _expression, Expression.Parse(parser, this, true), false);
        ParseTerminator(parser);
    }

    /// <summary>
    /// The optional exception <see cref="Expression"/>.
    /// </summary>
    public Expression Expression
    {
        get { return _expression; }
        set { SetField(ref _expression, value, true); }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return (_expression != null); }
    }

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
    /// Parse a <see cref="Throw"/>.
    /// </summary>
    public static Throw Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new Throw(parser, parent);
    }

    public override T Accept<T>(VisitorBase<T> visitor)
    {
        return visitor.Visit(this);
    }

    /// <summary>
    /// Deep-clone the code object.
    /// </summary>
    public override CodeObject Clone()
    {
        Throw clone = (Throw)base.Clone();
        clone.CloneField(ref clone._expression, _expression);
        return clone;
    }

    protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
    {
        if (_expression != null)
            _expression.AsText(writer, flags);
    }
}