using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;

/// <summary>
/// Decrements an <see cref="Expression"/> *after* it is evaluated.
/// Use <see cref="Decrement"/> instead when possible, because it's more efficient.
/// </summary>
public class PostDecrement : PostUnaryOperator
{
    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = Decrement.ParseToken;

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 100;

    /// <summary>
    /// Create a <see cref="PostDecrement"/> operator.
    /// </summary>
    public PostDecrement(Expression expression)
        : base(expression)
    { }

    protected PostDecrement(Parser parser, CodeObject parent)
                : base(parser, parent)
    { }

    /// <summary>
    /// The symbol associated with the operator.
    /// </summary>
    public override string Symbol
    {
        get { return ParseToken; }
    }

    public static new void AddParsePoints()
    {
        // Use a parse-priority of 100 (Decrement uses 0)
        Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, false, Parse);
    }

    /// <summary>
    /// Parse a <see cref="PostDecrement"/> operator.
    /// </summary>
    public static PostDecrement Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new PostDecrement(parser, parent);
    }

    /// <summary>
    /// Get the precedence of the operator.
    /// </summary>
    public override int GetPrecedence()
    {
        return Precedence;
    }
}