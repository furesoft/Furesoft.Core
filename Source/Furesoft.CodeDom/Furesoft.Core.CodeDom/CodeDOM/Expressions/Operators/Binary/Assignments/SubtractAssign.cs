using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Events;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;

/// <summary>
/// Subtracts one <see cref="Expression"/> from another, and assigns the result to the left <see cref="Expression"/>.
/// The left <see cref="Expression"/> must be an assignable object ("lvalue").
/// </summary>
public class SubtractAssign : Assignment
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "-=";

    /// <summary>
    /// Create a <see cref="SubtractAssign"/> operator.
    /// </summary>
    public SubtractAssign(Expression left, Expression right)
        : base(left, right)
    { }

    /// <summary>
    /// Create a <see cref="SubtractAssign"/> operator.
    /// </summary>
    public SubtractAssign(EventDecl left, Expression right)
        : base(left.CreateRef(), right)
    { }

    protected SubtractAssign(Parser parser, CodeObject parent)
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
    /// Parse a <see cref="SubtractAssign"/> operator.
    /// </summary>
    public static new SubtractAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new SubtractAssign(parser, parent);
    }

    /// <summary>
    /// The internal name of the <see cref="BinaryOperator"/>.
    /// </summary>
    public override string GetInternalName()
    {
        return Subtract.InternalName;
    }
}