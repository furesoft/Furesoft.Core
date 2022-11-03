using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Loops;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Loops;

/// <summary>
/// Causes execution to jump back to the top of the active loop.
/// </summary>
public class Continue : Statement
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "continue";

    /// <summary>
    /// Create a <see cref="Continue"/>.
    /// </summary>
    public Continue()
    { }

    protected Continue(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        parser.NextToken();  // Move past 'continue'
        ParseTerminator(parser);
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return false; }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has a terminator character by default.
    /// </summary>
    public override bool HasTerminatorDefault
    {
        get { return true; }
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
    /// Parse a <see cref="Continue"/>.
    /// </summary>
    public static Continue Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new Continue(parser, parent);
    }
}
