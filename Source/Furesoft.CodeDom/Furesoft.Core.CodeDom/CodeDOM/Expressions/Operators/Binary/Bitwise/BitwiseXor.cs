using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;

/// <summary>
/// Performs a boolean XOR operation on two <see cref="Expression"/>s.
/// </summary>
public class BitwiseXor : BinaryBitwiseOperator
{
    /// <summary>
    /// The internal name of the operator.
    /// </summary>
    public const string InternalName = NamePrefix + "ExclusiveOr";

    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "^";

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 360;

    /// <summary>
    /// Create a <see cref="BitwiseXor"/> operator.
    /// </summary>
    public BitwiseXor(Expression left, Expression right)
        : base(left, right)
    { }

    protected BitwiseXor(Parser parser, CodeObject parent)
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
        Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
    }

    /// <summary>
    /// Parse a <see cref="BitwiseXor"/> operator.
    /// </summary>
    public static BitwiseXor Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new BitwiseXor(parser, parent);
    }

    /// <summary>
    /// The internal name of the <see cref="BinaryOperator"/>.
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