using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;

/// <summary>
/// Negates an <see cref="Expression"/>.
/// </summary>
public class Negative : PreUnaryOperator
{
    /// <summary>
    /// The internal name of the operator.
    /// </summary>
    public const string InternalName = NamePrefix + "UnaryNegation";

    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = Subtract.ParseToken;

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 200;

    /// <summary>
    /// Create a <see cref="Negative"/> operator.
    /// </summary>
    public Negative(Expression expression)
        : base(expression)
    { }

    protected Negative(Parser parser, CodeObject parent)
                : base(parser, parent, false)
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
        // Use a parse-priority of 100 (Subtract uses 0)
        Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, true, Parse);
    }

    /// <summary>
    /// Parse a <see cref="Negative"/> operator.
    /// </summary>
    public static Negative Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new Negative(parser, parent);
    }

    /// <summary>
    /// The internal name of the <see cref="UnaryOperator"/>.
    /// </summary>
    public override string GetInternalName()
    {
        return InternalName;
    }

    /// <summary>
    /// Get the precedence of the operator.
    /// </summary>
    public override int GetPrecedence()
    {
        return Precedence;
    }
}