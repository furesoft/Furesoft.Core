// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Types;

/// <summary>
/// Declares an interface, which includes a name plus a body, along with various optional modifiers.
/// Interfaces define contracts - they have no fields or code.
/// </summary>
/// <remarks>
/// Non-nested interfaces can be only public or internal, and default to internal.
/// Nested interfaces can be any of the 5 access types, and default to private.
/// Other valid modifiers include: new, partial
/// Members of an interface are always public - no modifiers are allowed.
/// Allowed members are: MethodDecls, PropertyDecls, IndexerDecls, DelegateDecls, EventDecls
/// Interfaces can implement other interfaces.
/// The optional base list can contain one or more interfaces.
/// </remarks>
public class InterfaceDecl : BaseListTypeDecl
{
    /// <summary>
    /// Create an <see cref="InterfaceDecl"/> with the specified name.
    /// </summary>
    public InterfaceDecl(string name, Modifiers modifiers)
        : base(name, modifiers)
    { }

    /// <summary>
    /// Create an <see cref="InterfaceDecl"/> with the specified name.
    /// </summary>
    public InterfaceDecl(string name)
        : base(name, Modifiers.None)
    { }

    /// <summary>
    /// Create an <see cref="InterfaceDecl"/> with the specified name, modifiers, and type parameters.
    /// </summary>
    public InterfaceDecl(string name, Modifiers modifiers, params TypeParameter[] typeParameters)
        : base(name, modifiers, typeParameters)
    { }

    /// <summary>
    /// Create an <see cref="InterfaceDecl"/> with the specified name, modifiers, and base types.
    /// </summary>
    public InterfaceDecl(string name, Modifiers modifiers, params Expression[] baseTypes)
        : base(name, modifiers, baseTypes)
    { }

    /// <summary>
    /// Always <c>true</c>.
    /// </summary>
    public override bool IsInterface
    {
        get { return true; }
    }

    /// <summary>
    /// The keyword associated with the <see cref="Statement"/>.
    /// </summary>
    public override string Keyword
    {
        get { return ParseToken; }
    }

    /// <summary>
    /// Get the method with the specified name and parameter types.
    /// </summary>
    public override MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
    {
        MethodRef methodRef = base.GetMethod(name, parameterTypes);
        if (methodRef == null)
        {
            // If we didn't find it, search all base interfaces
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (baseTypeRef != null && baseTypeRef.IsInterface)
                    {
                        methodRef = baseTypeRef.GetMethod(name, parameterTypes);
                        if (methodRef != null)
                            break;
                    }
                }
            }
        }
        return methodRef;
    }

    /// <summary>
    /// Get the property with the specified name.
    /// </summary>
    public override PropertyRef GetProperty(string name)
    {
        PropertyRef propertyRef = base.GetProperty(name);
        if (propertyRef == null)
        {
            // If we didn't find it, search all base interfaces
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (baseTypeRef != null && baseTypeRef.IsInterface)
                    {
                        propertyRef = baseTypeRef.GetProperty(name);
                        if (propertyRef != null)
                            break;
                    }
                }
            }
        }
        return propertyRef;
    }

    /// <summary>
    /// The token used to parse the code object.
    /// </summary>
    public new const string ParseToken = "interface";

    /// <summary>
    /// Parse an <see cref="InterfaceDecl"/>.
    /// </summary>
    protected InterfaceDecl(Parser parser, CodeObject parent)
        : base(parser, parent)
    {
        MoveComments(parser.LastToken);        // Get any comments before 'interface'
        parser.NextToken();                    // Move past 'interface'
        ParseNameTypeParameters(parser);       // Parse the name and any optional type parameters
        ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers

        // Move any trailing compiler directives to the Infix1 position (assume we have a base-type list)
        MoveAnnotations(AnnotationFlags.IsPostfix, AnnotationFlags.IsInfix1);

        ParseBaseTypeList(parser);       // Parse the optional base-type list
        ParseConstraintClauses(parser);  // Parse any constraint clauses

        // Move any trailing post annotations on the last base type to the first constraint (if any)
        AdjustBaseTypePostComments();

        // If we don't have a base-type list, move any trailing compiler directives to the Postfix position
        if (_baseTypes == null || _baseTypes.Count == 0)
            MoveAnnotations(AnnotationFlags.IsInfix1, AnnotationFlags.IsPostfix);

        new Block(out _body, parser, this, true);  // Parse the body

        // Eat any trailing terminator (they are allowed but not required on non-delegate type declarations)
        if (parser.TokenText == ParseTokenTerminator)
            parser.NextToken();
    }

    public static void AddParsePoints()
    {
        // Interfaces are only valid with a Namespace or TypeDecl parent, but we'll allow any IBlock so that we can
        // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
        // This also allows for them to be embedded in a DocCode object.
        Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
    }

    /// <summary>
    /// Parse an <see cref="InterfaceDecl"/>.
    /// </summary>
    public static InterfaceDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        return new InterfaceDecl(parser, parent);
    }

    /// <summary>
    /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
    /// </summary>
    public override int DefaultNewLines(CodeObject previous)
    {
        // Always default to a blank line before an interface declaration
        return 2;
    }
}
