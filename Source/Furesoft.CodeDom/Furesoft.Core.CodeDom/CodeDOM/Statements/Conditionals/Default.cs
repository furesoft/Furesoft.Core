using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

/// <summary>
/// Used as a child of a <see cref="Switch"/>.  Includes a body (a statement or block).
/// </summary>
public class Default : SwitchItem
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "default";

    /// <summary>
    /// Create a <see cref="Default"/>.
    /// </summary>
    public Default(CodeObject body)
        : base(body)
    { }

    /// <summary>
    /// Create a <see cref="Default"/>.
    /// </summary>
    public Default()
        : base(null)
    { }

    protected Default(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        parser.NextToken();              // Move past 'default'
        ParseTerminatorAndBody(parser);  // Parse ':' and body (if any)
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return false; }
    }

    /// <summary>
    /// The keyword associated with the <see cref="Statement"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// Always 'default'.
    /// </summary>
    public override string Name
    {
        get { return ParseToken; }
    }

    public static void AddParsePoints()
    {
        // Use a parse-priority of 0 (DefaultValue uses 100)
        Parser.AddParsePoint(ParseToken, Parse, typeof(Switch));
    }

    /// <summary>
    /// Parse a <see cref="Default"/>.
    /// </summary>
    public static Default Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        // Skip if not followed by a ':', so DefaultValue can give it a try
        if (parser.PeekNextTokenText() == ParseTokenTerminator)
            return new Default(parser, parent);
        return null;
    }
}
