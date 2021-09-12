// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Mono.Cecil;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="FieldDecl"/>, <see cref="LocalDecl"/>, <see cref="ParameterDecl"/>, and <see cref="EnumMemberDecl"/>.
    /// </summary>
    public abstract class VariableDecl : Statement, IVariableDecl
    {
        #region /* FIELDS */

        protected string _name;
        protected Expression _type;
        protected Expression _initialization;

        #endregion

        #region /* CONSTRUCTORS */

        protected VariableDecl(string name, Expression type, Expression initialization)
        {
            _name = name;
            if (type != null)
                SetField(ref _type, type, true);
            Initialization = initialization;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// The type of the variable declaration.
        /// </summary>
        public virtual Expression Type
        {
            get { return _type; }
            set { SetField(ref _type, value, true); }
        }

        /// <summary>
        /// An optional initialization <see cref="Expression"/>.
        /// </summary>
        public Expression Initialization
        {
            get { return _initialization; }
            set { SetField(ref _initialization, value, true); }
        }

        /// <summary>
        /// True if the variable has an initialization <see cref="Expression"/>.
        /// </summary>
        public bool HasInitialization
        {
            get { return (_initialization != null); }
        }

        /// <summary>
        /// True if the variable is const.
        /// </summary>
        public abstract bool IsConst { get; set; }

        /// <summary>
        /// True if the variable is static.
        /// </summary>
        public abstract bool IsStatic { get; set; }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            VariableDecl clone = (VariableDecl)base.Clone();
            clone.CloneField(ref clone._type, _type);
            clone.CloneField(ref clone._initialization, _initialization);
            return clone;
        }

        /// <summary>
        /// Get the full name of the <see cref="VariableDecl"/>, including the namespace name (if any).
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public virtual string GetFullName(bool descriptive)
        {
            return _name;
        }

        /// <summary>
        /// Get the full name of the <see cref="VariableDecl"/>, including the namespace name (if any).
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        #endregion

        #region /* PARSING */

        protected VariableDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// This method is used when parsing forwards starting with the type is possible.
        /// </summary>
        protected void ParseType(Parser parser)
        {
            Expression expression = Expression.Parse(parser, this, true, ParseFlags.Type);
            MoveFormatting(expression);
            SetField(ref _type, expression, false);
        }

        protected void ParseName(Parser parser, string parseTokenEnd)
        {
            if (parser.TokenText != Expression.ParseTokenSeparator && parser.TokenText != parseTokenEnd)
            {
                Token token = parser.Token;
                _name = parser.GetIdentifierText();  // Parse the name
                SetLineCol(token);
            }
        }

        /// <summary>
        /// Parse the initialization (if any).
        /// </summary>
        protected void ParseInitialization(Parser parser, CodeObject parent)
        {
            if (parser.TokenText == Assignment.ParseToken)
            {
                Token equalsToken = parser.Token;
                parser.NextToken();  // Move past the '='
                SetField(ref _initialization, Expression.Parse(parser, this), false);
                if (_initialization != null)
                {
                    // Move any newlines on the '=' to the initialization expression instead
                    _initialization.MoveFormatting(equalsToken);

                    // Move any comments after the '=' to the initialization expression
                    _initialization.MoveCommentsToLeftMost(equalsToken, false);

                    // If the initialization expression is single-line and it's the last thing on the line (the
                    // next token is first-on-line), then move any EOL comment on it to the parent (this handles
                    // the case of EOL comments on intializers in a multi-variable list when the commas occur
                    // *before* each item on the line).
                    if (_initialization.IsSingleLine && (parser.Token == null || parser.Token.IsFirstOnLine))
                        MoveEOLComment(_initialization);
                }
            }
        }

        /// <summary>
        /// Move NewLines, LineNumber, Column, and any EOL comment from the specified <see cref="Token"/>.
        /// </summary>
        protected void MoveLocationAndComment(Token token)
        {
            NewLines = token.NewLines;
            SetLineCol(token);
            MoveEOLComment(token);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Resolve the type first, so it can be used to avoid ambiguities while resolving the Initialization (such as for method groups)
            if (_type != null && !(_parent is MultiLocalDecl))
                _type = (Expression)_type.Resolve(ResolveCategory.Type, flags);
            if (_initialization != null)
                _initialization = (Expression)_initialization.Resolve(ResolveCategory.Expression, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_type != null && _type.HasUnresolvedRef())
                return true;
            if (_initialization != null && _initialization.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Get the constant value of the variable (if any).
        /// </summary>
        /// <returns>An object of the type of the variable with a value of the constant used
        /// to initialize it (if any), or null if the variable isn't a constant.</returns>
        public virtual object GetValue()
        {
            if (IsConst && _initialization != null)
            {
                TypeRefBase typeRefBase = _initialization.EvaluateType();
                if (typeRefBase != null)
                {
                    object value = typeRefBase.GetConstantValue();
                    if (value is EnumConstant)
                        value = ((EnumConstant)value).ConstantValue;
                    if (value != null)
                    {
                        // Determine the type of the variable.  We're dealing with a constant, so it should
                        // be a built-in type or an enum, so use SkipPrefixes() instead of EvaluateType() in
                        // order to avoid infinite recursion (VariableDecl.EvaluateType() calls GetValue()).
                        Expression typeExpression = Type;
                        if (typeExpression != null)
                        {
                            TypeRef typeRef = typeExpression.SkipPrefixes() as TypeRef;
                            if (typeRef != null)
                            {
                                object reference = typeRef.GetReferencedType();
                                if (reference == null)
                                    return null;

                                // Make sure the value is of the proper type
                                if (typeRef.IsEnum)
                                {
                                    // Get the underlying type if it's an enum
                                    if (reference is EnumDecl)
                                        reference = ((EnumDecl)reference).UnderlyingType.EvaluateType().GetReferencedType();
                                    else if (reference is TypeDefinition)
                                        return TypeRef.ChangeTypeOfConstant(value, TypeDefinitionUtil.GetUnderlyingTypeOfEnum((TypeDefinition)reference));
                                    else //if (reference is Type)
                                        return TypeUtil.ChangeType(value, Enum.GetUnderlyingType((Type)reference));
                                }
                                if (reference is TypeDefinition)
                                    value = TypeRef.ChangeTypeOfConstant(value, (TypeDefinition)reference);
                                else if (reference is Type)
                                    value = TypeUtil.ChangeType(value, (Type)reference);
                            }
                        }
                    }
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="VariableDecl"/>.
        /// </summary>
        /// <remarks>This method evaluates the type expression into a <see cref="TypeRefBase"/>, which will properly evaluate the type arguments
        /// of nested types.  It also handles constants and the type being null.</remarks>
        public virtual TypeRefBase EvaluateType(bool withoutConstants)
        {
            Expression typeExpression = Type;
            return (typeExpression != null ? typeExpression.EvaluateType(withoutConstants) : null);
        }

        /// <summary>
        /// Evaluate the type of the <see cref="VariableDecl"/>.
        /// </summary>
        /// <remarks>This method evaluates the type expression into a <see cref="TypeRefBase"/>, which will properly evaluate the type arguments
        /// of nested types.  It also handles constants and the type being null.</remarks>
        public TypeRefBase EvaluateType()
        {
            Expression typeExpression = Type;
            return (typeExpression != null ? typeExpression.EvaluateType(false) : null);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
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
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return !HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_type == null || (!_type.IsFirstOnLine && _type.IsSingleLine))
                    && (_initialization == null || (!_initialization.IsFirstOnLine && _initialization.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_type != null)
                {
                    if (value)
                    {
                        _type.IsFirstOnLine = false;
                        _type.IsSingleLine = true;
                    }
                }
                if (_initialization != null)
                {
                    if (value)
                        _initialization.IsFirstOnLine = false;
                    _initialization.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        public virtual void AsTextType(CodeWriter writer, RenderFlags flags)
        {
            Expression type = Type;
            if (type != null)
            {
                RenderFlags passFlags = (flags & RenderFlags.PassMask);
                type.AsText(writer, passFlags | RenderFlags.IsPrefix | RenderFlags.Declaration);
            }
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            AsTextType(writer, flags);
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);
        }

        protected void AsTextConstantValue(CodeWriter writer, RenderFlags flags)
        {
            Literal.AsTextConstantValue(writer, flags, GetValue(), flags.HasFlag(RenderFlags.FormatAsHex), this);
        }

        protected void AsTextInitialization(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(" " + Assignment.ParseToken);
            _initialization.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        #endregion
    }
}
