﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Events;

/// <summary>
/// Represents a special type of method used to add to an event.
/// </summary>
/// <remarks>
/// The return type of an AdderDecl is always 'void', and it acquires the type of its 'value'
/// parameter and its internal name from it's parent EventDecl.
/// </remarks>
public class AdderDecl : AccessorDeclWithValue
{
    /// <summary>
    /// The internal name prefix.
    /// </summary>
    public const string NamePrefix = ParseToken + "__";

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "add";

    /// <summary>
    /// Create an <see cref="AdderDecl"/>.
    /// </summary>
    public AdderDecl(Modifiers modifiers)
        : base(NamePrefix, modifiers)
    { }

    /// <summary>
    /// Create an <see cref="AdderDecl"/>.
    /// </summary>
    public AdderDecl()
        : base(NamePrefix, Modifiers.None)
    { }

    protected AdderDecl(Parser parser, CodeObject parent, ParseFlags flags)
                : base(parser, parent, NamePrefix, flags)
    {
        ParseAccessor(parser, flags);
    }

    /// <summary>
    /// The keyword associated with the <see cref="Statement"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    public static new void AddParsePoints()
    {
        Parser.AddParsePoint(ParseToken, Parse, typeof(EventDecl));
    }

    /// <summary>
    /// Parse an <see cref="AdderDecl"/>.
    /// </summary>
    public static new AdderDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new AdderDecl(parser, parent, flags);
    }
}