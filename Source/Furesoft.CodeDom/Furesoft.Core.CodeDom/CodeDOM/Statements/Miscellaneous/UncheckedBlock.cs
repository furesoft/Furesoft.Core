﻿using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;

/// <summary>
/// Contains a body of code for which overflow checking is explicitly turned off.
/// </summary>
public class UncheckedBlock : BlockStatement
{
    /// <summary>
    /// Create an <see cref="UncheckedBlock"/>.
    /// </summary>
    public UncheckedBlock(CodeObject body)
        : base(body, false)
    { }

    /// <summary>
    /// Create an <see cref="UncheckedBlock"/>.
    /// </summary>
    public UncheckedBlock()
        : base(null, false)
    { }

    /// <summary>
    /// The keyword associated with the <see cref="Statement"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = Unchecked.ParseToken;

    protected UncheckedBlock(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        parser.NextToken();                        // Move past 'unchecked'
        new Block(out _body, parser, this, true);  // Parse the body
    }

    public static void AddParsePoints()
    {
        // Use a parse-priority of 0 (Unchecked uses 100)
        Parser.AddParsePoint(ParseToken, 0, Parse, typeof(IBlock));
    }

    /// <summary>
    /// Parse an <see cref="UncheckedBlock"/>.
    /// </summary>
    public static UncheckedBlock Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        // Skip if not followed by the start of a block (meaning it's an Unchecked operator)
        if (parser.PeekNextTokenText() == Block.ParseTokenStart)
            return new UncheckedBlock(parser, parent);
        return null;
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return false; }
    }
}
