﻿using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;

/// <summary>
/// Marks the end of an <see cref="IfDirective"/>.
/// </summary>
public class EndIfDirective : ConditionalDirectiveBase
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "endif";

    /// <summary>
    /// Create an <see cref="EndIfDirective"/>.
    /// </summary>
    public EndIfDirective()
    { }

    /// <summary>
    /// Parse an <see cref="EndIfDirective"/>.
    /// </summary>
    public EndIfDirective(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        parser.NextToken();  // Move past 'endif'
        MoveEOLComment(parser.LastToken);
        parser.EndConditionalDirective();
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
    /// Parse an <see cref="EndIfDirective"/>.
    /// </summary>
    public static EndIfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new EndIfDirective(parser, parent);
    }

    /// <summary>
    /// Determine if the specified comment should be associated with the current code object during parsing.
    /// </summary>
    public override bool AssociateCommentWhenParsing(CommentBase comment)
    {
        return false;
    }
}
