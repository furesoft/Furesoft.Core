using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Generics
{
    /// <summary>
    /// Represents a generic method declaration with type parameters.
    /// </summary>
    public class GenericMethodDecl : MethodDecl, ITypeParameters
    {
        /// <summary>
        /// The alternate token used to parse the start of type arguments inside documentation comments.
        /// </summary>
        public const string ParseTokenAltArgumentEnd = TypeRefBase.ParseTokenAltArgumentEnd;

        /// <summary>
        /// The alternate token used to parse the start of type arguments inside documentation comments.
        /// </summary>
        public const string ParseTokenAltArgumentStart = TypeRefBase.ParseTokenAltArgumentStart;

        /// <summary>
        /// The token used to parse the end of the type arguments.
        /// </summary>
        public const string ParseTokenArgumentEnd = TypeRefBase.ParseTokenArgumentEnd;

        /// <summary>
        /// The token used to parse the start of the type arguments.
        /// </summary>
        public const string ParseTokenArgumentStart = TypeRefBase.ParseTokenArgumentStart;

        protected ChildList<ConstraintClause> _constraintClauses;
        protected ChildList<TypeParameter> _typeParameters;

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name, return type, and modifiers.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, Modifiers modifiers, params TypeParameter[] typeParameters)
            : base(name, returnType, modifiers)
        {
            CreateTypeParameters().AddRange(typeParameters);
        }

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name, return type, and modifiers.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, Modifiers modifiers, CodeObject body, params ParameterDecl[] parameters)
            : base(name, returnType, modifiers, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name, return type, and modifiers.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : base(name, returnType, modifiers, parameters)
        { }

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name and return type.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, params TypeParameter[] typeParameters)
            : this(name, returnType, Modifiers.None, typeParameters)
        { }

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name and return type.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, CodeObject body, params ParameterDecl[] parameters)
            : base(name, returnType, body, parameters)
        { }

        /// <summary>
        /// Create a <see cref="GenericMethodDecl"/> with the specified name and return type.
        /// </summary>
        public GenericMethodDecl(string name, Expression returnType, params ParameterDecl[] parameters)
            : base(name, returnType, parameters)
        { }

        protected internal GenericMethodDecl(Parser parser, CodeObject parent, bool typeParametersAlreadyParsed, ParseFlags flags)
                            : base(parser, parent, false, flags)
        {
            if (typeParametersAlreadyParsed)
            {
                // The type parameters were already parsed on the unused Dot expression - fetch them from there
                UnresolvedRef unresolvedRef = (UnresolvedRef)((Dot)parser.LastUnusedCodeObject).Right;
                _typeParameters = new ChildList<TypeParameter>(this);
                foreach (Expression expression in unresolvedRef.TypeArguments)
                    _typeParameters.Add(new TypeParameter(expression is UnresolvedRef ? ((UnresolvedRef)expression).Name : null));
                unresolvedRef.TypeArguments = null;
            }
            ParseMethodNameAndType(parser, parent, true, false);
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
            if (!typeParametersAlreadyParsed)
                _typeParameters = TypeParameter.ParseList(parser, this);  // Parse any type parameters
            ParseParameters(parser);
            _constraintClauses = ConstraintClause.ParseList(parser, this);  // Parse any constraint clauses
            ParseTerminatorOrBody(parser, flags);
        }

        /// <summary>
        /// The list of <see cref="ConstraintClause"/>s.
        /// </summary>
        public ChildList<ConstraintClause> ConstraintClauses
        {
            get { return _constraintClauses; }
        }

        /// <summary>
        /// True if there are any <see cref="ConstraintClause"/>s.
        /// </summary>
        public bool HasConstraintClauses
        {
            get { return (_constraintClauses != null && _constraintClauses.Count > 0); }
        }

        /// <summary>
        /// True if there are any <see cref="TypeParameter"/>s.
        /// </summary>
        public bool HasTypeParameters
        {
            get { return (_typeParameters != null && _typeParameters.Count > 0); }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsGenericMethod
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_typeParameters == null || _typeParameters.Count == 0 || (!_typeParameters[0].IsFirstOnLine && _typeParameters.IsSingleLine))
                    && (_constraintClauses == null || _constraintClauses.Count == 0 || (!_constraintClauses[0].IsFirstOnLine && _constraintClauses.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_typeParameters != null && _typeParameters.Count > 0)
                    {
                        _typeParameters[0].IsFirstOnLine = false;
                        _typeParameters.IsSingleLine = true;
                    }
                    if (_constraintClauses != null && _constraintClauses.Count > 0)
                    {
                        _constraintClauses[0].IsFirstOnLine = false;
                        _constraintClauses.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// The number of <see cref="TypeParameter"/>s.
        /// </summary>
        public int TypeParameterCount
        {
            get { return (_typeParameters != null ? _typeParameters.Count : 0); }
        }

        /// <summary>
        /// The list of <see cref="TypeParameter"/>s.
        /// </summary>
        public ChildList<TypeParameter> TypeParameters
        {
            get { return _typeParameters; }
        }

        // Alternate type argument delimiters are allowed for code embedded inside documentation comments.
        // The C# style delimiters are also allowed in doc comments, although they shouldn't show up
        // usually, since they cause errors with parsing the XML properly - but they could be used
        // programmatically in certain situations.  Both styles are thus supported inside doc comments,
        // but the open and close delimiters must match for each pair.
        public static new void AddParsePoints()
        {
            // Generic methods are only valid with a TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            // Use a parse-priority of 0 (UnresolvedRef uses 100, LessThan uses 200).
            Parser.AddParsePoint(ParseTokenArgumentStart, 0, Parse, typeof(IBlock));
            // Support alternate symbols for doc comments:
            // Use a parse-priority of 0 (UnresolvedRef uses 100, PropertyDeclBase uses 200, BlockDecl uses 300, Initializer uses 400)
            Parser.AddParsePoint(ParseTokenAltArgumentStart, ParseAlt, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="GenericMethodDecl"/>.
        /// </summary>
        public static new GenericMethodDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If our parent is a TypeDecl, verify that we have an unused identifier (a Dot operator is possible
            // for explicit interface implementations, but is handled by MethodDecl, which then calls the constructor
            // below).  Otherwise, require a possible return type in addition to the identifier.  Also verify that
            // we seem to match a type argument list pattern followed by a '('.
            // If it doesn't seem to match the proper pattern, abort so that other types can try parsing it.
            if (((parent is TypeDecl && parser.HasUnusedIdentifier) || parser.HasUnusedTypeRefAndIdentifier)
                && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenArgumentEnd, flags) && parser.LastPeekedTokenText == ParseTokenStart)
                return new GenericMethodDecl(parser, parent, false, flags);
            return null;
        }

        /// <summary>
        /// Parse a <see cref="GenericMethodDecl"/> using alternate type argument delimiters.
        /// </summary>
        public static GenericMethodDecl ParseAlt(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that this alternate form is inside a doc comment (subroutines will look for the appropriate
            // delimiters according to the parser state) in addition to passing other verifications as above.
            // If it doesn't seem to match the proper pattern, abort so that other types can try parsing it.
            if (parser.InDocComment && ((parent is TypeDecl && parser.HasUnusedIdentifier) || parser.HasUnusedTypeRefAndIdentifier)
                && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenAltArgumentEnd, flags) && parser.LastPeekedTokenText == ParseTokenStart)
                return new GenericMethodDecl(parser, parent, false, flags);
            return null;
        }

        /// <summary>
        /// Add one or more <see cref="ConstraintClause"/>s.
        /// </summary>
        public void AddConstraintClauses(params ConstraintClause[] constraintClauses)
        {
            CreateConstraintClauses().AddRange(constraintClauses);
        }

        /// <summary>
        /// Add one or more <see cref="TypeParameter"/>s.
        /// </summary>
        public void AddTypeParameters(params TypeParameter[] typeParameters)
        {
            CreateTypeParameters().AddRange(typeParameters);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            GenericMethodDecl clone = (GenericMethodDecl)base.Clone();
            clone._typeParameters = ChildListHelpers.Clone(_typeParameters, clone);
            clone._constraintClauses = ChildListHelpers.Clone(_constraintClauses, clone);
            return clone;
        }

        /// <summary>
        /// Create the list of <see cref="ConstraintClause"/>s, or return the existing one.
        /// </summary>
        public ChildList<ConstraintClause> CreateConstraintClauses()
        {
            if (_constraintClauses == null)
                _constraintClauses = new ChildList<ConstraintClause>(this);
            return _constraintClauses;
        }

        /// <summary>
        /// Create a reference to the <see cref="GenericMethodDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new MethodRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="GenericMethodDecl"/>.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public MethodRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            return new MethodRef(this, isFirstOnLine, typeArguments);
        }

        /// <summary>
        /// Create a reference to the <see cref="GenericMethodDecl"/>.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public MethodRef CreateRef(ChildList<Expression> typeArguments)
        {
            return new MethodRef(this, false, typeArguments);
        }

        /// <summary>
        /// Create a reference to the <see cref="GenericMethodDecl"/>.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public MethodRef CreateRef(bool isFirstOnLine, params Expression[] typeArguments)
        {
            return new MethodRef(this, isFirstOnLine, typeArguments);
        }

        /// <summary>
        /// Create a reference to the <see cref="GenericMethodDecl"/>.
        /// </summary>
        /// <returns>A <see cref="MethodRef"/>.</returns>
        public MethodRef CreateRef(params Expression[] typeArguments)
        {
            return new MethodRef(this, false, typeArguments);
        }

        /// <summary>
        /// Create the list of <see cref="TypeParameter"/>s, or return the existing one.
        /// </summary>
        public ChildList<TypeParameter> CreateTypeParameters()
        {
            if (_typeParameters == null)
                _typeParameters = new ChildList<TypeParameter>(this);
            return _typeParameters;
        }

        /// <summary>
        /// Find the index of the specified type parameter.
        /// </summary>
        public int FindTypeParameterIndex(TypeParameter typeParameter)
        {
            int index = 0;
            foreach (TypeParameter genericParameter in TypeParameters)
            {
                if (genericParameter == typeParameter)
                    return index;
                ++index;
            }
            return -1;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            string name = Name;
            if (descriptive)
            {
                if (_typeParameters != null && _typeParameters.Count > 0)
                    name += TypeDecl.GetTypeParametersAsString(_typeParameters);
                name += GetParametersAsString();
            }
            if (_parent is TypeDecl)
                name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
            return name;
        }

        /// <summary>
        /// Get the type parameter at the specified index.
        /// </summary>
        public TypeParameter GetTypeParameter(int index)
        {
            if (_typeParameters != null)
            {
                if (index >= 0 && index < _typeParameters.Count)
                    return _typeParameters[index];
            }
            return null;
        }

        /// <summary>
        /// Get any constraints for the specified <see cref="TypeParameter"/> on this method, or on the base virtual method if this method is an override.
        /// </summary>
        public List<TypeParameterConstraint> GetTypeParameterConstraints(TypeParameter typeParameter)
        {
            // Override methods don't specify constraints - they inherit them from the base virtual method.
            // In order to handle invalid code, just look in the first occurrence of constraints, searching
            // any base method if the current one is an override.
            if (_constraintClauses != null && _constraintClauses.Count > 0)
            {
                foreach (ConstraintClause constraintClause in _constraintClauses)
                {
                    if (constraintClause.TypeParameter.Reference == typeParameter)
                        return constraintClause.Constraints;
                }
            }
            else
            {
                MethodRef baseMethodRef = FindBaseMethod();
                if (baseMethodRef != null)
                {
                    // If the constraints are from a base method, we have to translate the type parameter
                    int index = FindTypeParameterIndex(typeParameter);
                    TypeParameterRef typeParameterRef = baseMethodRef.GetTypeParameter(index);
                    return baseMethodRef.GetTypeParameterConstraints(typeParameterRef);
                }
            }
            return null;
        }

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextName(writer, flags);
            if (HasTypeParameters)
                TypeParameter.AsTextTypeParameters(writer, _typeParameters, flags);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            ConstraintClause.AsTextConstraints(writer, _constraintClauses, flags | RenderFlags.HasTerminator);
            base.AsTextAfter(writer, flags);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            if (!HasConstraintClauses)
                base.AsTextSuffix(writer, flags);
        }
    }
}