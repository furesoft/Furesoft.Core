using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List;

/// <summary>
/// Represents the header of a <see cref="DocList"/> in a documentation comment.
/// </summary>
public class DocListHeader : DocComment
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "listheader";

    /// <summary>
    /// Create a <see cref="DocListHeader"/>.
    /// </summary>
    public DocListHeader(params DocComment[] docComments)
    {
        foreach (DocComment docComment in docComments)
        {
            // Default-format entries
            Add("\n        ");
            Add(docComment);
        }

        // Default end tag to first-on-line
        Add("\n    ");
    }

    /// <summary>
    /// Parse a <see cref="DocListHeader"/>.
    /// </summary>
    public DocListHeader(Parser parser, CodeObject parent)
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
    /// Parse a <see cref="DocListHeader"/>.
    /// </summary>
    public static new DocListHeader Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new DocListHeader(parser, parent);
    }
}
