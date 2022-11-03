﻿using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;

/// <summary>
/// Represents a call to another constructor in the same class (constructor initializer).
/// </summary>
public class ThisInitializer : ConstructorInitializer
{
    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public const string ParseToken = ThisRef.ParseToken;

    /// <summary>
    /// Create a <see cref="ThisInitializer"/> operator.
    /// </summary>
    public ThisInitializer(SymbolicRef symbolicRef, params Expression[] parameters)
        : base(symbolicRef, parameters)
    { }

    /// <summary>
    /// Create a <see cref="ThisInitializer"/> operator.
    /// </summary>
    public ThisInitializer(ConstructorRef constructorRef, params Expression[] parameters)
        : base(constructorRef, parameters)
    { }

    /// <summary>
    /// Create a <see cref="ThisInitializer"/> operator.
    /// </summary>
    public ThisInitializer(ConstructorDecl constructorDecl, params Expression[] parameters)
        : base(constructorDecl, parameters)
    { }

    /// <summary>
    /// Parse a <see cref="ThisInitializer"/> operator.
    /// </summary>
    public ThisInitializer(Parser parser, CodeObject parent)
        : base(parser, parent, ParseToken)
    { }

    /// <summary>
    /// The symbol associated with the operator.
    /// </summary>
    public override string Symbol
    {
        get { return ParseToken; }
    }
}
