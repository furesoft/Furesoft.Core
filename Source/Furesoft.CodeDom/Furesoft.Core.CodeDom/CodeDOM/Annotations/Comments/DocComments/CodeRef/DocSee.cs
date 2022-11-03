using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef;

/// <summary>
/// Embeds a reference to a type or member in a documentation comment.
/// </summary>
public class DocSee : DocCodeRefBase
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "see";

    /// <summary>
    /// Create a <see cref="DocSee"/>.
    /// </summary>
    public DocSee(Expression codeRef)
        : base(codeRef, (string)null)
    { }

    /// <summary>
    /// Parse a <see cref="DocSee"/>.
    /// </summary>
    public DocSee(Parser parser, CodeObject parent)
        : base(parser, parent)
    { }

    /// <summary>
    /// True if the code object defaults to starting on a new line.
    /// </summary>
    public override bool IsFirstOnLineDefault
    {
        get { return false; }
    }

    /// <summary>
    /// The XML tag name for the documentation comment.
    /// </summary>
    public override string TagName
    {
        get { return ParseToken; }
    }

    public static void AddParsePoints()
    {
        Parser.AddDocCommentParseTag(ParseToken, Parse);
    }

    /// <summary>
    /// Parse a <see cref="DocSee"/>.
    /// </summary>
    public static new DocSee Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new DocSee(parser, parent);
    }
}
