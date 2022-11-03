using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple;

/// <summary>
/// Documents how to use a code object.
/// </summary>
public class DocExample : DocComment
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "example";

    /// <summary>
    /// Create a <see cref="DocExample"/>.
    /// </summary>
    public DocExample(string text)
        : base(text)
    { }

    /// <summary>
    /// Create a <see cref="DocExample"/>.
    /// </summary>
    public DocExample(params DocComment[] docComments)
        : base(docComments)
    { }

    /// <summary>
    /// Parse a <see cref="DocExample"/>.
    /// </summary>
    public DocExample(Parser parser, CodeObject parent)
    {
        ParseTag(parser, parent);  // Ignore any attributes
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
    /// Parse a <see cref="DocExample"/>.
    /// </summary>
    public static new DocExample Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new DocExample(parser, parent);
    }
}
