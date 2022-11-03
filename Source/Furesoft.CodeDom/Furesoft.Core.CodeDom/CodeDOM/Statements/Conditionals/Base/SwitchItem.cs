﻿using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;

/// <summary>
/// The common base class of the <see cref="Case"/> and <see cref="Default"/> statements (of a <see cref="Switch"/>).
/// </summary>
public abstract class SwitchItem : BlockStatement, INamedCodeObject
{
    /// <summary>
    /// The token used to parse the end of the item.
    /// </summary>
    public new const string ParseTokenTerminator = ":";

    protected SwitchItem(CodeObject body)
                : base(body, true)  // Allow null bodies for Case/Default
    { }

    protected SwitchItem(Parser parser, CodeObject parent)
                : base(parser, parent)
    { }

    /// <summary>
    /// The descriptive category of the code object.
    /// </summary>
    public string Category
    {
        get { return "switch item"; }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return true; }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> always requires braces.
    /// </summary>
    public override bool HasBracesAlways
    {
        get { return false; }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has a terminator character by default.
    /// </summary>
    public override bool HasTerminatorDefault
    {
        get { return true; }
    }

    /// <summary>
    /// The name of the <see cref="SwitchItem"/>.
    /// </summary>
    public virtual string Name
    {
        get { return null; }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> requires an empty statement if it has an empty block with no braces.
    /// </summary>
    public override bool RequiresEmptyStatement
    {
        get { return false; }
    }

    /// <summary>
    /// The terminator character for the <see cref="Statement"/>.
    /// </summary>
    public override string Terminator
    {
        get { return ParseTokenTerminator; }
    }

    /// <summary>
    /// Add the <see cref="CodeObject"/> to the specified dictionary.
    /// </summary>
    public void AddToDictionary(NamedCodeObjectDictionary dictionary)
    {
        // Prefix Labels and SwitchItems with a ':' to segregate them
        dictionary.Add(ParseTokenTerminator + Name, this);
    }

    /// <summary>
    /// Render as a SwitchItemRef target.
    /// </summary>
    public void AsTextGotoTarget(CodeWriter writer, RenderFlags flags)
    {
        AsTextStatement(writer, flags);
        if (HasArgument)
        {
            AsTextArgumentPrefix(writer, flags);
            AsTextArgument(writer, flags);
        }
    }

    /// <summary>
    /// Create a reference to the <see cref="SwitchItem"/>.
    /// </summary>
    /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
    /// <returns>A <see cref="SwitchItemRef"/>.</returns>
    public override SymbolicRef CreateRef(bool isFirstOnLine)
    {
        return new SwitchItemRef(this, isFirstOnLine);
    }

    /// <summary>
    /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
    /// </summary>
    public override int DefaultNewLines(CodeObject previous)
    {
        // Always default to no blank lines before switch items
        return 1;
    }

    /// <summary>
    /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
    /// </summary>
    public string GetFullName(bool descriptive)
    {
        return Name;
    }

    /// <summary>
    /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
    /// </summary>
    public string GetFullName()
    {
        return Name;
    }

    /// <summary>
    /// Remove the <see cref="CodeObject"/> from the specified dictionary.
    /// </summary>
    public void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
    {
        dictionary.Remove(ParseTokenTerminator + Name, this);
    }

    /// <summary>
    /// Determines if the body of the <see cref="BlockStatement"/> should be formatted with braces.
    /// </summary>
    public override bool ShouldHaveBraces()
    {
        // It's actually really hard to determine if a SwitchItem should have braces, because they only need
        // them if one or more LocalDecls are declared and none of them are accessed by any following SwitchItems
        // up until the next one that has braces.  For now, just go by the current status, leaving it up to the
        // coder to get it right.
        return HasBraces;
    }

    protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
    {
        if (!flags.HasFlag(RenderFlags.NoPostAnnotations))
            AsTextAnnotations(writer, AnnotationFlags.IsPostfix, flags);

        if (_body != null && !flags.HasFlag(RenderFlags.Description))
        {
            // Check for alignment of the body (ignore if empty or it doesn't fit the pattern)
            if (IsFirstOnLine && _body != null && _body.Count > 0 && !_body.IsFirstOnLine && _body.IsSingleLine)
            {
                int columnWidth = writer.GetColumnWidth(Parent, 0);
                if (columnWidth > 0)
                {
                    int padding = columnWidth - AsTextLength(RenderFlags.Description | RenderFlags.LengthFlags);
                    if (padding > 0)
                        writer.Write(new string(' ', padding));
                }
            }

            _body.AsText(writer, flags);
        }
    }

    protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
    {
        // Render the terminator even when in Description mode (for references)
        AsTextTerminator(writer, flags);
    }

    protected void ParseTerminatorAndBody(Parser parser)
    {
        ParseTerminator(parser);  // Parse ':'

        // Parse the body until we find the next 'case' or 'default' (or the end of the 'switch' block).
        // If the body starts with a '{', then it will be parsed until it finds the '}'.
        new Block(out _body, parser, this, false, Case.ParseToken, Default.ParseToken);
    }
}
