using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;

/// <summary>
/// Performs a boolean AND operation on two <see cref="Expression"/>s.
/// </summary>
public class BitwiseAnd : BinaryBitwiseOperator
{
    /// <summary>
    /// The internal name of the operator.
    /// </summary>
    public const string InternalName = NamePrefix + "BitwiseAnd";

    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "&";

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 350;

    /// <summary>
    /// Create a <see cref="BitwiseAnd"/> operator.
    /// </summary>
    public BitwiseAnd(Expression left, Expression right)
        : base(left, right)
    { }

    protected BitwiseAnd(Parser parser, CodeObject parent)
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
    /// Parse a <see cref="BitwiseAnd"/> operator.
    /// </summary>
    public static BitwiseAnd Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        // Verify that we have a left expression before proceeding, otherwise abort
        // (this is to give the AddressOf operator a chance at parsing it)
        if (parser.HasUnusedExpression)
            return new BitwiseAnd(parser, parent);
        return null;
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