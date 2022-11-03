using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;

/// <summary>
/// The common base class of the <see cref="TypeOf"/>, <see cref="SizeOf"/>, and <see cref="DefaultValue"/> operators.
/// </summary>
public abstract class TypeOperator : SingleArgumentOperator
{
    /// <summary>
    /// Create a type operator - the expression must evaluate to a <see cref="TypeRef"/> in valid code.
    /// </summary>
    protected TypeOperator(Expression type)
        : base(type)
    { }

    protected TypeOperator(Parser parser, CodeObject parent)
        : base(parser, parent)
    { }
}
