using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols.Base;

/// <summary>
/// The common base class of <see cref="DefineSymbol"/> and <see cref="UnDefSymbol"/>.
/// </summary>
public abstract class SymbolDirective : CompilerDirective
{
    protected string _symbol;

    protected SymbolDirective(string symbol)
    {
        _symbol = symbol;
    }

    protected SymbolDirective(Parser parser, CodeObject parent)
                : base(parser, parent)
    { }

    /// <summary>
    /// The associated symbol name.
    /// </summary>
    public string Symbol
    {
        get { return _symbol; }
        set { _symbol = value; }
    }

    protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
    {
        writer.Write(_symbol);
    }

    protected void ParseSymbol(Parser parser)
    {
        Token token = parser.NextTokenSameLine(false);  // Move past directive keyword
        if (token != null)
        {
            _symbol = parser.TokenText;  // Parse symbol name
            parser.NextToken();          // Move past name
        }
    }
}
