using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;

/// <summary>
/// Represents an "indexer" - an indexed property.
/// </summary>
/// <remarks>
/// IndexerDecls are always represented in C# source with a name of 'this', but default to an internal name of 'Item'.
/// The internal name can actually be overridden with the IndexerName attribute (to prevent name collisions, or
/// for no particular reason - the indexer for System.String is named 'Chars'), and therefore indexers are resolved
/// by looking for indexers regardless of their names.  The C# compiler pretends indexers don't exist if you try to
/// reference them directly by name, although VS 2010 intellisense will show their parameter lists.
/// </remarks>
public class IndexerDecl : PropertyDeclBase, IParameters
{
    /// <summary>
    /// The internal name for an indexer.
    /// </summary>
    public const string IndexerName = "Item";

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = ThisRef.ParseToken;

    /// <summary>
    /// The token used to parse the end of the parameters.
    /// </summary>
    public const string ParseTokenEnd = TypeRefBase.ParseTokenArrayEnd;

    /// <summary>
    /// The token used to parse the start of the parameters.
    /// </summary>
    public const string ParseTokenStart = TypeRefBase.ParseTokenArrayStart;

    // The '_name' base-class member should always be an Expression - which should be either a ThisRef,
    // or a Dot operator with a TypeRef to an Interface on the left and a ThisRef on the right (ThisRef
    // is used by default to represent the 'this', even though it's really an IndexerDecl and not a
    // self-reference).
    protected ChildList<ParameterDecl> _parameters;

    /// <summary>
    /// Create an <see cref="IndexerDecl"/>.
    /// </summary>
    public IndexerDecl(Expression name, Expression type, Modifiers modifiers, params ParameterDecl[] parameters)
        : base(CheckUnresolvedThisRef(name), type, modifiers)
    {
        // Save any parameters - these are NOT stored on the SetterDecl/GetterDecl, but are
        // instead accessed through the parent.
        _parameters = new ChildList<ParameterDecl>(parameters, this);
    }

    /// <summary>
    /// Create an <see cref="IndexerDecl"/>.
    /// </summary>
    public IndexerDecl(Expression name, Expression type, params ParameterDecl[] parameters)
        : this(name, type, Modifiers.None, parameters)
    { }

    /// <summary>
    /// Create an <see cref="IndexerDecl"/>.
    /// </summary>
    public IndexerDecl(Expression type, Modifiers modifiers, params ParameterDecl[] parameters)
        : this(new ThisRef(), type, modifiers, parameters)
    { }

    /// <summary>
    /// Create an <see cref="IndexerDecl"/>.
    /// </summary>
    public IndexerDecl(Expression type, params ParameterDecl[] parameters)
        : this(type, Modifiers.None, parameters)
    { }

    protected IndexerDecl(Parser parser, CodeObject parent)
                : base(parser, parent, false)
    {
        // Get the ThisRef or Dot expression.  If it's a Dot, replace the ThisRef with an UnresolvedThisRef,
        // which has an internal name of "Item", but displays as "this".
        Expression expression = parser.RemoveLastUnusedExpression();
        SetField(ref _name, CheckUnresolvedThisRef(expression), false);
        Expression leftExpression = (expression is BinaryOperator ? ((BinaryOperator)expression).Left : expression);
        _lineNumber = leftExpression.LineNumber;
        _columnNumber = (ushort)leftExpression.ColumnNumber;
        ParseTypeModifiersAnnotations(parser);  // Parse type and any modifiers and/or attributes

        // Parse the parameter declarations
        _parameters = ParameterDecl.ParseList(parser, this, ParseTokenStart, ParseTokenEnd, false, out bool isEndFirstOnLine);
        IsEndFirstOnLine = isEndFirstOnLine;

        new Block(out _body, parser, this, true);  // Parse the body
    }

    /// <summary>
    /// The descriptive category of the code object.
    /// </summary>
    public override string Category
    {
        get { return "indexer"; }
    }

    /// <summary>
    /// The 'getter' method for the indexer.
    /// </summary>
    public GetterDecl Getter
    {
        get { return _body.FindFirst<GetterDecl>(); }
        set
        {
            if (_body != null)
            {
                GetterDecl existing = _body.FindFirst<GetterDecl>();
                if (existing != null)
                    _body.Remove(existing);
            }
            Insert(0, value);  // Always put the 'getter' first
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
    /// True if the <see cref="Statement"/> has parens around its argument.
    /// </summary>
    public override bool HasArgumentParens
    {
        get { return false; }
    }

    /// <summary>
    /// True if the indexer has a getter method.
    /// </summary>
    public bool HasGetter
    {
        get { return (_body.FindFirst<GetterDecl>() != null); }
    }

    /// <summary>
    /// True if the indexer has parameters.
    /// </summary>
    public bool HasParameters
    {
        get { return (_parameters != null && _parameters.Count > 0); }
    }

    /// <summary>
    /// True if the indexer has a setter method.
    /// </summary>
    public bool HasSetter
    {
        get { return (_body.FindFirst<SetterDecl>() != null); }
    }

    /// <summary>
    /// True if the indexer is readable.
    /// </summary>
    public override bool IsReadable { get { return HasGetter; } }

    /// <summary>
    /// Determines if the code object only requires a single line for display.
    /// </summary>
    public override bool IsSingleLine
    {
        get { return (base.IsSingleLine && (_parameters == null || _parameters.Count == 0 || (!_parameters[0].IsFirstOnLine && _parameters.IsSingleLine))); }
        set
        {
            base.IsSingleLine = value;
            if (value && _parameters != null && _parameters.Count > 0)
            {
                _parameters[0].IsFirstOnLine = false;
                _parameters.IsSingleLine = true;
            }
        }
    }

    /// <summary>
    /// True if the indexer is writable.
    /// </summary>
    public override bool IsWritable { get { return HasSetter; } }

    /// <summary>
    /// The name of the <see cref="IndexerDecl"/>.
    /// </summary>
    public override string Name
    {
        get
        {
            // The internal name of an Indexer is always 'Item', even though it's always displayed
            // as 'this' in the GUI or text rendering (it's actually 'this.Item').
            if (_name is ThisRef)
                return IndexerName;
            // If it's an explicit interface implementation, use the full name
            if (_name is Expression)
                return ((Expression)_name).AsString();
            return null;
        }
    }

    /// <summary>
    /// The <see cref="ThisRef"/> or <see cref="Dot"/> expression representing the name of the <see cref="IndexerDecl"/>.
    /// </summary>
    public Expression NameExpression
    {
        get { return (_name as Expression); }
    }

    /// <summary>
    /// The number of parameters the indexer has.
    /// </summary>
    public int ParameterCount
    {
        get { return (_parameters != null ? _parameters.Count : 0); }
    }

    /// <summary>
    /// A collection of <see cref="ParameterDecl"/>s for the parameters of the indexer.
    /// </summary>
    public ChildList<ParameterDecl> Parameters
    {
        get { return _parameters; }
    }

    /// <summary>
    /// The 'setter' method for the indexer.
    /// </summary>
    public SetterDecl Setter
    {
        get { return _body.FindFirst<SetterDecl>(); }
        set
        {
            if (_body != null)
            {
                SetterDecl existing = _body.FindFirst<SetterDecl>();
                if (existing != null)
                    _body.Remove(existing);
            }
            Insert((_body != null ? _body.Count : 0), value);  // Always put the 'setter' after any 'getter'
        }
    }

    public static new void AddParsePoints()
    {
        // Indexer declarations are only valid with a TypeDecl parent, but we'll allow any IBlock so that we can
        // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
        // This also allows for them to be embedded in a DocCode object.
        // Use a parse-priority of 0 (TypeRef uses 100, Index uses 200, Attribute uses 300)
        Parser.AddParsePoint(ParseTokenStart, Parse, typeof(IBlock));
    }

    /// <summary>
    /// Parse an <see cref="IndexerDecl"/>.
    /// </summary>
    public static new IndexerDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        // If our parent is a TypeDecl, verify that we have an unused Expression (it can be either an
        // identifier or a Dot operator for explicit interface implementations).  Otherwise, require a
        // possible type in addition to the Expression.
        // If it doesn't seem to match the proper pattern, abort so that other types can try parsing it.
        if ((parent is TypeDecl && parser.HasUnusedExpression) || parser.HasUnusedTypeRefAndExpression)
        {
            // Verify that we have a 'this' or 'Interface.this' (in the first case, the 'this' will be
            // a ThisRef, in the second case, it will be an UnresolvedThisRef).
            CodeObject lastUnusedCodeObject = parser.LastUnusedCodeObject;
            if (lastUnusedCodeObject is ThisRef || (lastUnusedCodeObject is Dot && ((Dot)lastUnusedCodeObject).Right is UnresolvedThisRef))
                return new IndexerDecl(parser, parent);
        }
        return null;
    }

    /// <summary>
    /// Deep-clone the code object.
    /// </summary>
    public override CodeObject Clone()
    {
        IndexerDecl clone = (IndexerDecl)base.Clone();
        clone._parameters = ChildListHelpers.Clone(_parameters, clone);
        return clone;
    }

    /// <summary>
    /// Create a reference to the <see cref="IndexerDecl"/>.
    /// </summary>
    /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
    /// <returns>An <see cref="IndexerRef"/>.</returns>
    public override SymbolicRef CreateRef(bool isFirstOnLine)
    {
        return new IndexerRef(this, isFirstOnLine);
    }

    /// <summary>
    /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
    /// </summary>
    /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
    /// <param name="isPrivate">True if the access is private.</param>
    /// <param name="isProtected">True if the access is protected.</param>
    /// <param name="isInternal">True if the access is internal.</param>
    public override void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
    {
        isPrivate = isProtected = isInternal = false;

        // The access rights of an indexer actually depend on the rights of the corresponding
        // getter/setter, depending upon whether we're assigning to it or not.
        if (isTargetOfAssignment)
        {
            SetterDecl setterDecl = Setter;
            if (setterDecl != null)
            {
                isPrivate = setterDecl.IsPrivate;
                if (!isPrivate)
                {
                    isProtected = setterDecl.IsProtected;
                    isInternal = setterDecl.IsInternal;
                }
            }
        }
        else
        {
            GetterDecl getterDecl = Getter;
            if (getterDecl != null)
            {
                isPrivate = getterDecl.IsPrivate;
                if (!isPrivate)
                {
                    isProtected = getterDecl.IsProtected;
                    isInternal = getterDecl.IsInternal;
                }
            }
        }
    }

    /// <summary>
    /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
    /// </summary>
    /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
    public override string GetFullName(bool descriptive)
    {
        string name;
        if (_name is Expression)
            name = ((Expression)_name).AsString();
        else
            name = ThisRef.ParseToken;
        if (descriptive)
            name += MethodDeclBase.GetParametersAsString(ParseTokenStart, ParseTokenEnd, _parameters);
        if (_parent is TypeDecl)
            name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
        return name;
    }

    protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
    {
        RenderFlags passFlags = (flags & RenderFlags.PassMask);
        writer.Write(ParseTokenStart);
        AsTextInfixComments(writer, 0, flags);
        writer.WriteList(_parameters, passFlags, this);
        if (IsEndFirstOnLine)
            writer.WriteLine();
        writer.Write(ParseTokenEnd);
    }

    protected override void AsTextArgumentPrefix(CodeWriter writer, RenderFlags flags)
    { }

    protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
    {
        RenderFlags passFlags = (flags & RenderFlags.PassMask);
        if (_type != null)
            _type.AsText(writer, passFlags | RenderFlags.IsPrefix);
        UpdateLineCol(writer, flags);
        if (flags.HasFlag(RenderFlags.Description) && _parent is TypeDecl)
        {
            ((TypeDecl)_parent).AsTextName(writer, flags);
            Dot.AsTextDot(writer);
        }
        if (_name is ThisRef)
            ((ThisRef)_name).AsText(writer, passFlags);
        else if (_name is Expression)
            ((Expression)_name).AsText(writer, passFlags & ~(RenderFlags.Description | RenderFlags.ShowParentTypes));
    }

    private static Expression CheckUnresolvedThisRef(Expression expression)
    {
        // If we have an "Interface.this" expression, convert a ThisRef to an UnresolvedThisRef
        if (expression is Dot dot)
        {
            if (dot.Right is ThisRef)
                dot.Right = new UnresolvedThisRef(dot.Right as ThisRef);
        }
        return expression;
    }
}