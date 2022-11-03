﻿using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Messages.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Messages;

/// <summary>
/// Represents the start of a section of code that can be collapsed, and has optional descriptive text.
/// </summary>
public class RegionDirective : MessageDirective
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "region";

    /// <summary>
    /// Create a <see cref="RegionDirective"/>.
    /// </summary>
    public RegionDirective(string message)
        : base(message)
    { }

    /// <summary>
    /// Create a <see cref="RegionDirective"/>.
    /// </summary>
    public RegionDirective()
        : base(null)
    { }

    /// <summary>
    /// Parse a <see cref="RegionDirective"/>.
    /// </summary>
    public RegionDirective(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        ParseMessage(parser);

        // If it's a region of generated code (such as for the MS Component Designer), set the flag in the parser
        if (!parser.IsGenerated && IsGeneratedRegion)
            parser.IsGenerated = parser.IsGeneratedRegion = true;
    }

    /// <summary>
    /// The keyword associated with the compiler directive (if any).
    /// </summary>
    public override string DirectiveKeyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// Determines if the compiler directive should be indented.
    /// </summary>
    public override bool HasNoIndentationDefault
    {
        get { return false; }
    }

    /// <summary>
    /// True if this region contains generated code.
    /// </summary>
    public bool IsGeneratedRegion
    {
        get { return StringUtil.ContainsIgnoreCase(_message, "generated code"); }
    }

    public static void AddParsePoints()
    {
        Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
    }

    /// <summary>
    /// Parse a <see cref="RegionDirective"/>.
    /// </summary>
    public static RegionDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new RegionDirective(parser, parent);
    }

    /// <summary>
    /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
    /// </summary>
    public override int DefaultNewLines(CodeObject previous)
    {
        // Always default to a blank line before a region directive
        return 2;
    }
}
