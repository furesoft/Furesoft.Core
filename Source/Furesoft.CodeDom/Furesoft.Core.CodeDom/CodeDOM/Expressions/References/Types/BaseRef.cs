﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;

/// <summary>
/// Represents a reference to the base class of the current object instance.
/// </summary>
public class BaseRef : SelfRef
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = "base";

    /// <summary>
    /// Create a <see cref="BaseRef"/>.
    /// </summary>
    public BaseRef(bool isFirstOnLine)
        : base(isFirstOnLine)
    { }

    /// <summary>
    /// Create a <see cref="BaseRef"/>.
    /// </summary>
    public BaseRef()
        : base(false)
    { }

    protected BaseRef(Parser parser, CodeObject parent)
                : base(parser, parent)
    {
        parser.NextToken();  // Move past 'base'
    }

    /// <summary>
    /// The keyword associated with the <see cref="SelfRef"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// The name of the <see cref="SymbolicRef"/>.
    /// </summary>
    public override string Name
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// The code object to which the <see cref="SymbolicRef"/> refers.
    /// </summary>
    public override object Reference
    {
        get
        {
            // Evaluate to the base type declaration, so that most properties and methods
            // will function according to it.
            TypeDecl typeDecl = FindParent<TypeDecl>();
            return (typeDecl != null ? typeDecl.GetBaseType().Reference : null);
        }
    }

    public static new void AddParsePoints()
    {
        Parser.AddParsePoint(ParseToken, Parse);
    }

    /// <summary>
    /// Parse a <see cref="BaseRef"/>.
    /// </summary>
    public static BaseRef Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new BaseRef(parser, parent);
    }
}