using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Loops;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Loops;

/// <summary>
/// Represents the "while" portion of a <see cref="DoWhile"/> loop, and does NOT have
/// a body.  This class works as a child of the While class, which displays as a "do" loop
/// when IsDoWhile is set to true - it should not be used directly.
/// </summary>
/// <remarks>
/// This separate class is needed for do-while loops so that the 'while' clause can have
/// separate EOL comments and formatting (IsFirstOnLine) from the 'do'.
/// </remarks>
public class DoWhile : Statement
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = While.ParseToken;

    /// <summary>
    /// Create a <see cref="DoWhile"/>.
    /// </summary>
    public DoWhile(While parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Parse a <see cref="DoWhile"/>.
    /// </summary>
    public DoWhile(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        // Use a local because we can't pass a ref to a property
        Expression conditional = ((While)_parent).Conditional;
        ParseKeywordAndArgument(parser, ref conditional);  // Parse the keyword and argument
        ((While)_parent).Conditional = conditional;
        ParseTerminator(parser);

        // Move any EOL comment here, otherwise the Block parsing logic will move it to the parent While (Do)
        MoveEOLComment(parser.LastToken);
    }

    /// <summary>
    /// The conditional <see cref="Expression"/>.
    /// </summary>
    public Expression Conditional
    {
        get { return ((While)_parent).Conditional; }
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

    protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
    {
        Expression conditional = Conditional;
        if (conditional != null)
            conditional.AsText(writer, flags);
    }
}
