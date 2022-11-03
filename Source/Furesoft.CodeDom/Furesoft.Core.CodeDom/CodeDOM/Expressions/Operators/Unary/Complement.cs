using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;

/// <summary>
/// Performs a one's complement (toggles all bits) on an <see cref="Expression"/>.
/// </summary>
public class Complement : PreUnaryOperator
{
    /// <summary>
    /// The internal name of the operator.
    /// </summary>
    public const string InternalName = NamePrefix + "OnesComplement";

    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "~";

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 200;

    /// <summary>
    /// Create a <see cref="Complement"/> operator.
    /// </summary>
    public Complement(Expression expression)
        : base(expression)
    { }

    protected Complement(Parser parser, CodeObject parent)
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
        // Use a parse-priority of 100 (DestructorDecl uses 0)
        Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, true, Parse);
    }

    /// <summary>
    /// Parse a <see cref="Complement"/>.
    /// </summary>
    public static Complement Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new Complement(parser, parent);
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