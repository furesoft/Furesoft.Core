using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift.Base;

/// <summary>
/// The common base class of <see cref="LeftShift"/> and <see cref="RightShift"/>.
/// </summary>
public abstract class BinaryShiftOperator : BinaryOperator
{
    protected BinaryShiftOperator(Expression left, Expression right)
        : base(left, right)
    { }

    protected BinaryShiftOperator(Parser parser, CodeObject parent)
        : base(parser, parent)
    { }
}
