using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;

/// <summary>
/// Performs a boolean AND operation on two <see cref="Expression"/>s, and assigns the result to the left <see cref="Expression"/>.
/// The left <see cref="Expression"/> must be an assignable object ("lvalue").
/// </summary>
public class BitwiseAndAssign : Assignment
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "&=";

    /// <summary>
    /// Create a <see cref="BitwiseAndAssign"/> operator.
    /// </summary>
    public BitwiseAndAssign(Expression left, Expression right)
        : base(left, right)
    { }

    protected BitwiseAndAssign(Parser parser, CodeObject parent)
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
    /// Parse a <see cref="BitwiseAndAssign"/> operator.
    /// </summary>
    public static new BitwiseAndAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new BitwiseAndAssign(parser, parent);
    }

    /// <summary>
    /// The internal name of the <see cref="BinaryOperator"/>.
    /// </summary>
    public override string GetInternalName()
    {
        return BitwiseAnd.InternalName;
    }
}