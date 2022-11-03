﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift;

/// <summary>
/// Shifts the bits of the left <see cref="Expression"/> to the RIGHT by the number of bits indicated by the right <see cref="Expression"/>.
/// </summary>
public class RightShift : BinaryShiftOperator
{
    /// <summary>
    /// The internal name of the operator.
    /// </summary>
    public const string InternalName = NamePrefix + "RightShift";

    /// <summary>
    /// True if the operator is left-associative, or false if it's right-associative.
    /// </summary>
    public const bool LeftAssociative = true;

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = ">>";

    /// <summary>
    /// The precedence of the operator.
    /// </summary>
    public const int Precedence = 320;

    /// <summary>
    /// Create a <see cref="RightShift"/> operator.
    /// </summary>
    public RightShift(Expression left, Expression right)
        : base(left, right)
    { }

    protected RightShift(Parser parser, CodeObject parent)
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
    /// Parse a <see cref="RightShift"/> operator.
    /// </summary>
    public static RightShift Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new RightShift(parser, parent);
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