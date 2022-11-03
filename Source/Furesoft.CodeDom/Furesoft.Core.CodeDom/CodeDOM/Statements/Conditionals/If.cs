using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

/// <summary>
/// Represents conditional flow control, and includes a conditional expression, a body, and an optional <see cref="Else"/> or <see cref="ElseIf"/> statement.
/// </summary>
public class If : IfBase
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "if";

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional, CodeObject body)
        : base(conditional, body)
    { }

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional, CodeObject body, Else @else)
        : base(conditional, body, @else)
    { }

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional, CodeObject body, ElseIf elseIf)
        : base(conditional, body, elseIf)
    { }

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional)
        : base(conditional)
    { }

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional, Else @else)
        : base(conditional, @else)
    { }

    /// <summary>
    /// Create an <see cref="If"/>.
    /// </summary>
    public If(Expression conditional, ElseIf elseIf)
        : base(conditional, elseIf)
    { }

    protected If(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        ParseIf(parser, parent);  // Delegate to base class to parse 'if'
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
    /// Parse an <see cref="If"/>.
    /// </summary>
    public static If Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new If(parser, parent);
    }
}
