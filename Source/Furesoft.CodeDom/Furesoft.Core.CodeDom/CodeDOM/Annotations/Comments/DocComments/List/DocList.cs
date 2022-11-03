using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.List;

/// <summary>
/// Represents a list in a documentation comment.
/// </summary>
public class DocList : DocComment
{
    /// <summary>
    /// The name of the list 'type' attribute.
    /// </summary>
    public const string AttributeName = "type";

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "list";

    /// <summary>
    /// The type of the <see cref="DocList"/> (should be 'bullet', 'number', or 'table').
    /// </summary>
    protected string _type;

    /// <summary>
    /// Create a <see cref="DocList"/> with the specified type.
    /// </summary>
    public DocList(string type, params DocComment[] docComments)
    {
        _type = type;

        foreach (DocComment docComment in docComments)
        {
            // Default-format entries
            Add("\n    ");
            Add(docComment);
        }

        // Default end tag to first-on-line
        Add("\n");
    }

    protected DocList(Parser parser, CodeObject parent)
    {
        ParseTag(parser, parent);
    }

    /// <summary>
    /// The XML tag name for the documentation comment.
    /// </summary>
    public override string TagName
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// The type of the <see cref="DocList"/> (should be 'bullet', 'number', or 'table').
    /// </summary>
    public string Type
    {
        get { return _type; }
        set { _type = value; }
    }

    public static void AddParsePoints()
    {
        Parser.AddDocCommentParseTag(ParseToken, Parse);
    }

    /// <summary>
    /// Parse a <see cref="DocList"/>.
    /// </summary>
    public static new DocList Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new DocList(parser, parent);
    }

    protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
    {
        if (!flags.HasFlag(RenderFlags.Description))
            writer.Write("<" + TagName + " " + AttributeName + "=\"" + _type + "\"" + (_content == null && !MissingEndTag ? "/>" : ">"));
    }

    protected override object ParseAttributeValue(Parser parser, string name)
    {
        object value = base.ParseAttributeValue(parser, name);
        if (StringUtil.NNEqualsIgnoreCase(name, AttributeName))
            _type = value.ToString().Trim();
        return value;
    }
}
