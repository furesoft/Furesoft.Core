using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;

/// <summary>
/// Represents a reference to a pre-processor directive symbol.
/// </summary>
/// <remarks>
/// Directive symbols may be defined with <see cref="DefineSymbol"/> or on the compiler command-line,
/// and may be undefined with <see cref="UnDefSymbol"/>.  As such, this reference doesn't refer to any
/// declaration, but instead is a reference by string name only, and also is never marked as "unresolved".
/// </remarks>
public class DirectiveSymbolRef : SymbolicRef
{
    /// <summary>
    /// Create a <see cref="DirectiveSymbolRef"/>.
    /// </summary>
    public DirectiveSymbolRef(string name)
        : base(name, false)  // Directive symbols can never be the first thing on a line
    { }

    /// <summary>
    /// Create a <see cref="DirectiveSymbolRef"/>.
    /// </summary>
    protected internal DirectiveSymbolRef(Token token)
        : this(token.NonVerbatimText)
    {
        SetLineCol(token);
    }

    /// <summary>
    /// The name of the <see cref="SymbolicRef"/>.
    /// </summary>
    public override string Name
    {
        get { return (string)_reference; }
    }

    public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
    {
        UpdateLineCol(writer, flags);
        writer.Write(Name);
    }
}
