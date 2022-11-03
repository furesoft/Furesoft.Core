﻿using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;

/// <summary>
/// Used for optional compilation of code, must be preceeded by an <see cref="IfDirective"/> or <see cref="ElIfDirective"/>, and
/// followed by an <see cref="EndIfDirective"/>.
/// </summary>
public class ElseDirective : ConditionalDirective
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "else";

    /// <summary>
    /// Create an <see cref="ElseDirective"/>.
    /// </summary>
    public ElseDirective()
    { }

    /// <summary>
    /// Parse an <see cref="ElseDirective"/>.
    /// </summary>
    public ElseDirective(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        parser.NextToken();  // Move past 'else'
        MoveEOLComment(parser.LastToken);

        // Skip the next section of code if an earlier 'if' or 'elif' evaluated to true
        if (parser.CurrentConditionalDirectiveState)
            SkipSection(parser);
        else
            parser.CurrentConditionalDirectiveState = _isActive = true;
    }

    /// <summary>
    /// The keyword associated with the compiler directive (if any).
    /// </summary>
    public override string DirectiveKeyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// True if the compiler directive has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return false; }
    }

    public static void AddParsePoints()
    {
        Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
    }

    /// <summary>
    /// Parse an <see cref="ElseDirective"/>.
    /// </summary>
    public static ElseDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new ElseDirective(parser, parent);
    }
}
