using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;

/// <summary>
/// The common base class of the <see cref="Ref"/> and <see cref="Out"/> pseudo-operators.
/// </summary>
public abstract class RefOutOperator : SingleArgumentOperator
{
    /// <summary>
    /// Create a Ref/Out operator instance.
    /// </summary>
    /// <param name="variable">An expression that evaluates to a <see cref="VariableRef"/>.</param>
    protected RefOutOperator(Expression variable)
        : base(variable)
    { }

    /// <summary>
    /// Create a Ref/Out operator instance.
    /// </summary>
    protected RefOutOperator(VariableDecl variableDecl)
        : base(variableDecl.CreateRef())
    { }

    protected RefOutOperator(Parser parser, CodeObject parent)
        : base(parser, parent)
    { }

    /// <summary>
    /// True if the argument has parens around it.
    /// </summary>
    public override bool HasArgumentParens
    {
        get { return false; }
    }
}
