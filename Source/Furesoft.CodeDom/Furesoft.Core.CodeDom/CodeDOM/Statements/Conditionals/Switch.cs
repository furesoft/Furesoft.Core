using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

/// <summary>
/// Represents conditional flow control, and consists of a constant expression (of integral or string type)
/// along with one or more <see cref="Case"/> or <see cref="Default"/> child statements.
/// </summary>
public class Switch : BlockStatement
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "switch";

    protected Expression _target;

    /// <summary>
    /// Create a <see cref="Switch"/> on the specified target <see cref="Expression"/>.
    /// </summary>
    public Switch(Expression target)
    {
        Target = target;
    }

    /// <summary>
    /// Create a <see cref="Switch"/> on the specified target <see cref="Expression"/>.
    /// </summary>
    public Switch(Expression target, params SwitchItem[] items)
        : this(target)
    {
        foreach (SwitchItem item in items)
            Add(item);
    }

    protected Switch(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        // Parse keyword, argument, and body
        // Do NOT do any post-processing in the body parsing, because we're going to do it below
        ParseKeywordArgumentBody(parser, ref _target, false, true);

        // Do some special processing of switch items:
        for (int i = _body.Count - 1; i >= 0; --i)
        {
            CodeObject item = _body[i];
            if (item is Break || item is Return)
            {
                // Check for Break or Return statements outside of a SwitchItem block, and move them inside
                // the previous block (this can occur when they are outside the SwitchItem's curly braces).
                if (i > 0)
                {
                    CodeObject previousItem = _body[i - 1];
                    if (previousItem is SwitchItem)
                    {
                        ((SwitchItem)previousItem).Body.Add(item);
                        _body.RemoveAt(i);
                    }
                }
            }
            else if (item is CommentBase)
            {
                // Merge any comments into adjacent SwitchItems if appropriate
                _body.PostProcessComment((CommentBase)item, i);
            }
            else if (item is SwitchItem)
            {
                // Check for SwitchItems without braces where the last object in the block is a conditional
                // directive, and move them to the parent block if they don't seem to "belong" to the child.
                // Also, if the last object is a DocComment, move it to the parent block.
                Block itemBody = ((SwitchItem)item).Body;
                if (itemBody != null && !itemBody.HasBraces)
                {
                    bool move = false;
                    CodeObject last = itemBody.Last;
                    if (last is CompilerDirective)
                    {
                        move = true;
                        if (last is EndIfDirective)
                        {
                            // Don't move the endif if there's an associated conditional in the same block
                            for (int n = itemBody.Count - 2; n >= 0; --n)
                            {
                                CodeObject itemMember = itemBody[n];
                                if (itemMember is ConditionalDirective)
                                {
                                    move = false;
                                    break;
                                }
                            }
                        }
                    }
                    else if (last is DocComment)
                        move = true;
                    if (move)
                    {
                        // Move the object up to the parent block
                        _body.Insert(i + 1, last);
                        itemBody.RemoveAt(itemBody.Count - 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return true; }
    }

    /// <summary>
    /// Determines if the code object only requires a single line for display.
    /// </summary>
    public override bool IsSingleLine
    {
        get { return (base.IsSingleLine && (_target == null || (!_target.IsFirstOnLine && _target.IsSingleLine))); }
        set
        {
            base.IsSingleLine = value;
            if (value && _target != null)
            {
                _target.IsFirstOnLine = false;
                _target.IsSingleLine = true;
            }
        }
    }

    /// <summary>
    /// The keyword associated with the <see cref="Statement"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// The target <see cref="Expression"/>.
    /// </summary>
    public Expression Target
    {
        get { return _target; }
        set { SetField(ref _target, value, true); }
    }

    public static void AddParsePoints()
    {
        Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
    }

    /// <summary>
    /// Parse a <see cref="Switch"/>.
    /// </summary>
    public static Switch Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new Switch(parser, parent);
    }

    /// <summary>
    /// Add a <see cref="SwitchItem"/>.
    /// </summary>
    public void Add(SwitchItem item)
    {
        base.Add(item);
    }

    /// <summary>
    /// Deep-clone the code object.
    /// </summary>
    public override CodeObject Clone()
    {
        Switch clone = (Switch)base.Clone();
        clone.CloneField(ref clone._target, _target);
        return clone;
    }

    protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
    {
        if (!flags.HasFlag(RenderFlags.NoPostAnnotations))
            AsTextAnnotations(writer, AnnotationFlags.IsPostfix, flags);

        if (_body != null && !flags.HasFlag(RenderFlags.Description))
        {
            // Check for alignment of same-line single-line SwitchItem bodies
            int alignmentOffset = 0;
            foreach (SwitchItem switchItem in _body.Find<SwitchItem>())
            {
                // If the SwitchItem body is on the same line (and is a single line), then calculate the
                // common alignment offset.  Ignore any SwitchItems if they're not first-on-line.
                bool formatOK = false;
                if (switchItem.IsFirstOnLine)
                {
                    Block switchItemBody = switchItem.Body;
                    if (switchItemBody != null && switchItemBody.Count > 0 && !switchItemBody.IsFirstOnLine && switchItemBody.IsSingleLine)
                    {
                        formatOK = true;
                        int bodyOffset = switchItem.AsTextLength(RenderFlags.Description | RenderFlags.LengthFlags);
                        if (bodyOffset > alignmentOffset)
                            alignmentOffset = bodyOffset;
                    }
                }
                if (!formatOK)
                {
                    // If the SwitchItem doesn't fit the right pattern, abort the formatting
                    alignmentOffset = 0;
                    break;
                }
            }

            // If we're aligning, create an alignment state to hold the alignment offset value so the SwitchItems can find it
            if (alignmentOffset > 0)
                writer.BeginAlignment(this, new[] { alignmentOffset });

            _body.AsText(writer, flags);

            if (alignmentOffset > 0)
                writer.EndAlignment(this);
        }
    }

    protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
    {
        _target.AsText(writer, flags);
    }
}
