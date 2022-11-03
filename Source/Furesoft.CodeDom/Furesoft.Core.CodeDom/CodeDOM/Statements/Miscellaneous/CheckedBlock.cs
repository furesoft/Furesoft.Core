using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;

/// <summary>
/// Contains a body of code for which overflow checking is explicitly turned on.
/// </summary>
public class CheckedBlock : BlockStatement
{
    /// <summary>
    /// Create a <see cref="CheckedBlock"/>.
    /// </summary>
    public CheckedBlock(CodeObject body)
        : base(body, false)
    { }

    /// <summary>
    /// Create a <see cref="CheckedBlock"/>.
    /// </summary>
    public CheckedBlock()
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
    public const string ParseToken = Checked.ParseToken;

    protected CheckedBlock(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        parser.NextToken();                        // Move past 'checked'
        new Block(out _body, parser, this, true);  // Parse the body
    }

    public static void AddParsePoints()
    {
        // Use a parse-priority of 0 (Checked uses 100)
        Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
    }

    /// <summary>
    /// Parse a <see cref="CheckedBlock"/>.
    /// </summary>
    public static CheckedBlock Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        // Skip if not followed by the start of a block (meaning it's a Checked operator)
        if (parser.PeekNextTokenText() == Block.ParseTokenStart)
            return new CheckedBlock(parser, parent);
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
