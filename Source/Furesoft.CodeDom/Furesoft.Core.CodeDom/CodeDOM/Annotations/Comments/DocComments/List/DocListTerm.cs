using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List;

/// <summary>
/// Represents the term of a <see cref="DocListItem"/> in a <see cref="DocList"/> in a documentation comment.
/// </summary>
public class DocListTerm : DocComment
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "term";

    /// <summary>
    /// Create a <see cref="DocListTerm"/>.
    /// </summary>
    public DocListTerm(string content)
        : base(content)
    { }

    /// <summary>
    /// Parse a <see cref="DocListTerm"/>.
    /// </summary>
    public DocListTerm(Parser parser, CodeObject parent)
    {
        ParseTag(parser, parent);  // Ignore any attributes
    }

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
    /// Parse a <see cref="DocListTerm"/>.
    /// </summary>
    public static new DocListTerm Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new DocListTerm(parser, parent);
    }
}
