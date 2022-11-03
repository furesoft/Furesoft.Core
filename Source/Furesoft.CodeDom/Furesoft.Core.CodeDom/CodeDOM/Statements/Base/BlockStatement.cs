using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System;
using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Base;

/// <summary>
/// The common base class of all <see cref="Statement"/>s that can have a <see cref="Block"/> as a body (<see cref="NamespaceDecl"/>,
/// <see cref="TypeDecl"/>, <see cref="MethodDeclBase"/>, <see cref="PropertyDeclBase"/>, <see cref="BlockDecl"/>, <see cref="IfBase"/>,
/// <see cref="Else"/>, <see cref="Switch"/>, <see cref="SwitchItem"/>, <see cref="For"/>, <see cref="ForEach"/>, <see cref="While"/>,
/// <see cref="Try"/>, <see cref="Catch"/>, <see cref="Finally"/>, <see cref="Using"/>, <see cref="Lock"/>, <see cref="CheckedBlock"/>,
/// <see cref="UncheckedBlock"/>).
/// </summary>
public abstract class BlockStatement : Statement, IBlock
{
    /// <summary>
    /// The body is always a Block, which in turn may contain zero or more other code objects,
    /// and it can also be null in special cases (such as for method signatures with no body,
    /// delegate declarations, or a While with the semi-colon on the same line).
    /// </summary>
    protected Block _body;

    /// <summary>
    /// Create a <see cref="BlockStatement"/>.
    /// </summary>
    protected BlockStatement()
    {
        Body = new Block();
    }

    /// <summary>
    /// Create a <see cref="BlockStatement"/> with the specified <see cref="CodeObject"/> in the body.
    /// </summary>
    protected BlockStatement(CodeObject body, bool allowNullBody)
    {
        // Allow derived classes to pass any non-Block code object, in which case it will
        // be wrapped in a Block.
        Body = (body == null ? (allowNullBody ? null : new Block()) : (body is Block ? (Block)body : new Block(body)));
    }

    /// <summary>
    /// Create a <see cref="BlockStatement"/> from an array of <see cref="CodeObject"/>s.
    /// </summary>
    protected BlockStatement(params CodeObject[] objects)
    {
        Add(objects);
    }

    /// <summary>
    /// Create a <see cref="BlockStatement"/> from an existing one, moving the body.
    /// </summary>
    protected BlockStatement(BlockStatement blockStatement)
        : base(blockStatement)
    {
        _body = blockStatement.Body;  // bypass body formatting
        blockStatement.Body = null;
        _body.Parent = this;
    }

    protected BlockStatement(Parser parser, CodeObject parent)
                : base(parser, parent)
    { }

    /// <summary>
    /// The <see cref="Block"/> body.
    /// </summary>
    public Block Body
    {
        get { return _body; }
        set
        {
            _body = value;
            if (_body != null)
            {
                _body.Parent = this;
                ReformatBlock();
            }
            HasTerminator = HasTerminatorDefault;
        }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has an argument.
    /// </summary>
    public override bool HasArgument
    {
        get { return false; }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> has braces.
    /// </summary>
    public bool HasBraces
    {
        get { return (_body != null && _body.HasBraces); }
        set
        {
            if (HasBracesAlways && !value)
                throw new Exception("Braces can't be turned off for the given type of block statement!");
            CreateBody().HasBraces = value;
        }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> always requires braces.
    /// </summary>
    public virtual bool HasBracesAlways
    {
        get { return true; }
    }

    /// <summary>
    /// True for all <see cref="BlockStatement"/>s that have a header (all except <see cref="CodeUnit"/> and <see cref="BlockDecl"/>).
    /// </summary>
    public virtual bool HasHeader
    {
        get { return true; }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has a terminator character by default.
    /// </summary>
    public override bool HasTerminatorDefault
    {
        get { return (_body == null); }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> has compact empty braces by default.
    /// </summary>
    public virtual bool IsCompactIfEmptyDefault
    {
        get { return false; }
    }

    /// <summary>
    /// True for multi-part statements, such as try/catch/finally or if/else.
    /// </summary>
    public virtual bool IsMultiPart
    {
        get { return false; }
    }

    /// <summary>
    /// Determines if the code object only requires a single line for display.
    /// </summary>
    public override bool IsSingleLine
    {
        get { return (base.IsSingleLine && (_body == null || (!_body.IsFirstOnLine && _body.IsSingleLine))); }
        set
        {
            // Make sure there's a body, and set its IsFirstOnLine and IsSingleLine properties appropriately
            CreateBody();
            _body.IsFirstOnLine = !value;
            _body.IsSingleLine = value;
        }
    }

    /// <summary>
    /// True if the code object only requires a single line for display by default.
    /// </summary>
    public override bool IsSingleLineDefault
    {
        get { return false; }
    }

    /// <summary>
    /// True if a <see cref="BlockStatement"/> is at the top level (those that have no header and no indent).
    /// For example, a <see cref="CodeUnit"/>, a <see cref="BlockDecl"/> with no parent or a <see cref="DocComment"/>
    /// parent.
    /// </summary>
    public virtual bool IsTopLevel
    {
        get { return (!HasHeader && (_parent == null || _parent is DocComment)); }
    }

    /// <summary>
    /// True if the <see cref="BlockStatement"/> requires an empty statement if it has an empty block with no braces.
    /// </summary>
    public virtual bool RequiresEmptyStatement
    {
        get { return true; }
    }

    public override T Accept<T>(VisitorBase<T> visitor)
    {
        return visitor.Visit(this);
    }

    /// <summary>
    /// Add a <see cref="CodeObject"/> to the <see cref="BlockStatement"/> body.
    /// </summary>
    public virtual void Add(CodeObject obj)
    {
        CreateBody().Add(obj);
    }

    /// <summary>
    /// Add multiple <see cref="CodeObject"/>s to the <see cref="BlockStatement"/> body.
    /// </summary>
    public virtual void Add(params CodeObject[] objects)
    {
        CreateBody();
        foreach (CodeObject obj in objects)
            _body.Add(obj);
    }

    /// <summary>
    /// Add a collection of <see cref="CodeObject"/>s to the <see cref="BlockStatement"/> body.
    /// </summary>
    /// <param name="collection">The collection to be added.</param>
    public virtual void AddRange(IEnumerable<CodeObject> collection)
    {
        CreateBody().AddRange(collection);
    }

    /// <summary>
    /// Deep-clone the code object.
    /// </summary>
    public override CodeObject Clone()
    {
        BlockStatement clone = (BlockStatement)base.Clone();
        clone.CloneField(ref clone._body, _body);
        return clone;
    }

    /// <summary>
    /// Check if the <see cref="BlockStatement"/> contains the specified <see cref="CodeObject"/>.
    /// </summary>
    /// <param name="codeObject">The object being searched for.</param>
    /// <returns>True if the block contains the object, otherwise false.</returns>
    public bool Contains(CodeObject codeObject)
    {
        return (_body != null && _body.Contains(codeObject));
    }

    /// <summary>
    /// Create a body if one doesn't exist yet.
    /// </summary>
    public Block CreateBody()
    {
        if (_body == null)
            Body = new Block();
        return _body;
    }

    /// <summary>
    /// Enumerate all children with the specified name.
    /// </summary>
    public IEnumerable<CodeObject> Find(string name)
    {
        if (_body != null)
        {
            foreach (CodeObject codeObject in _body.Find(name))
                yield return codeObject;
        }
    }

    /// <summary>
    /// Enumerate all children with the specified name and type.
    /// </summary>
    public IEnumerable<T> Find<T>(string name) where T : CodeObject
    {
        if (_body != null)
        {
            foreach (T codeObject in _body.Find<T>(name))
                yield return codeObject;
        }
    }

    /// <summary>
    /// Enumerate all children of type T.
    /// </summary>
    public IEnumerable<T> Find<T>() where T : CodeObject
    {
        if (_body != null)
        {
            foreach (T codeObject in _body.Find<T>())
                yield return codeObject;
        }
    }

    /// <summary>
    /// Find the first child object with the specified name and type.
    /// </summary>
    public T FindFirst<T>(string name) where T : CodeObject
    {
        return (_body != null ? _body.FindFirst<T>(name) : null);
    }

    /// <summary>
    /// Find the first child object of type T.
    /// </summary>
    public T FindFirst<T>() where T : CodeObject
    {
        return (_body != null ? _body.FindFirst<T>() : null);
    }

    /// <summary>
    /// Find the index of the specified <see cref="CodeObject"/> in the <see cref="BlockStatement"/>.
    /// </summary>
    /// <param name="codeObject">The object being searched for.</param>
    /// <returns>The index of the code object, or -1 if not found.</returns>
    public int FindIndexOf(CodeObject codeObject)
    {
        return (_body != null ? _body.FindIndexOf(codeObject) : -1);
    }

    /// <summary>
    /// Insert a <see cref="CodeObject"/> at the specified index in the <see cref="BlockStatement"/> body.
    /// </summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="obj">The CodeObject to be inserted.</param>
    public virtual void Insert(int index, CodeObject obj)
    {
        CreateBody().Insert(index, obj);
    }

    /// <summary>
    /// Reformat the <see cref="Block"/> body.
    /// </summary>
    public virtual void ReformatBlock()
    {
        if (_body != null)
        {
            if (!_body.IsGroupingSet)
                _body.SetFormatFlag(FormatFlags.Grouping, ShouldHaveBraces());
            if (!IsNewLinesSet)
            {
                IsSingleLine = (IsSingleLineDefault && _body.IsSingleLineDefault && _body.Count < 2);
                if (_body.Count == 0 && IsCompactIfEmptyDefault)
                    _body.SetNewLines(0);
            }
        }
    }

    /// <summary>
    /// Remove the specified <see cref="CodeObject"/> from the <see cref="BlockStatement"/> body.
    /// </summary>
    public virtual void Remove(CodeObject obj)
    {
        if (_body != null)
            _body.Remove(obj);
    }

    /// <summary>
    /// Remove all <see cref="CodeObject"/>s from the <see cref="BlockStatement"/> body.
    /// </summary>
    public virtual void RemoveAll()
    {
        if (_body != null)
            _body.RemoveAll();
    }

    /// <summary>
    /// Remove the <see cref="CodeObject"/> at the specified index from the <see cref="BlockStatement"/>.
    /// </summary>
    public void RemoveAt(int index)
    {
        if (_body != null)
            _body.RemoveAt(index);
    }

    /// <summary>
    /// Replace the specified <see cref="CodeObject"/> with a new one.
    /// </summary>
    /// <returns>True if the code object was found and replaced, otherwise false.</returns>
    public bool Replace(CodeObject oldObject, CodeObject newObject)
    {
        return (_body != null && _body.Replace(oldObject, newObject));
    }

    /// <summary>
    /// Determines if the body of the <see cref="BlockStatement"/> should be formatted with braces.
    /// </summary>
    public virtual bool ShouldHaveBraces()
    {
        // Check if braces aren't optional for the statement
        if (HasBracesAlways)
            return true;
        // No braces are required if we have no body, or it's empty
        if (_body == null || _body.Count == 0)
            return false;
        // Braces are required (by default) if we have multiple objects in the block
        // (this behavior is overridden by SwitchItem, where multiple objects are legal without braces).
        if (_body.Count > 1)
            return true;
        // We only have a single child statement - use braces if it's not single-line
        return !_body[0].IsSingleLine;
    }

    /// <summary>
    /// Default format the code object.
    /// </summary>
    protected internal override void DefaultFormat()
    {
        base.DefaultFormat();

        // Default the braces if they haven't been explicitly set
        if (!IsGroupingSet)
            _formatFlags = ((_formatFlags & ~FormatFlags.Grouping) | (ShouldHaveBraces() ? FormatFlags.Grouping : 0));
    }

    protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
    {
        base.AsTextAfter(writer, flags);
        if (_body != null && !flags.HasFlag(RenderFlags.Description))
            _body.AsText(writer, flags);
    }

    protected void ParseKeywordArgumentBody(Parser parser, ref Expression argument, bool allowNullBody, bool noPostProcessing)
    {
        ParseKeywordAndArgument(parser, ref argument);  // Parse the keyword and argument

        if (allowNullBody && parser.TokenText == Terminator && !parser.Token.IsFirstOnLine)
            ParseTerminator(parser);  // Handle same-line ';' (null body)
        else
            new Block(out _body, parser, this, false);  // Parse the body
    }

    protected void ParseTerminatorOrBody(Parser parser, ParseFlags flags)
    {
        // Check for an optional ';' in place of the body
        if (parser.TokenText == Terminator)
        {
            ParseTerminator(parser);

            // Check for compiler directives, storing them as postfix annotations on the parent
            Block.ParseCompilerDirectives(parser, this, AnnotationFlags.IsPostfix, false);
        }
        else
        {
            if (flags.HasFlag(ParseFlags.SkipMethodBodies))
                Block.SkipParsingBlock(parser, this, true);
            else
                new Block(out _body, parser, this, true);  // Parse the body
        }
    }
}