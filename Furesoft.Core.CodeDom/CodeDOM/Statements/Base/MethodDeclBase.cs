using System.Linq;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Base
{
    /// <summary>
    /// The common base class of <see cref="MethodDecl"/>, <see cref="GenericMethodDecl"/>, <see cref="ConstructorDecl"/>,
    /// and <see cref="DestructorDecl"/>.
    /// </summary>
    public abstract class MethodDeclBase : BlockStatement, INamedCodeObject, IParameters, IModifiers
    {
        /// <summary>
        /// The token used to parse the end of a list of parameters.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// The token used to parse the start of a list of parameters.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        protected Modifiers _modifiers;

        /// <summary>
        /// The name can be a string or an Expression (in which case it should be a Dot operator
        /// with a TypeRef to an Interface on the left and an interface member ref on the right).
        /// For ConstructorDecls and DestructorDecls, the name should be null (they take on the
        /// name of their parent type).
        /// </summary>
        protected object _name;

        protected ChildList<ParameterDecl> _parameters;

        /// <summary>
        /// The return type is an <see cref="Expression"/> that must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected Expression _returnType;

        protected MethodDeclBase(string name, Expression returnType, Modifiers modifiers, CodeObject body, params ParameterDecl[] parameters)
            : base(body, true)
        {
            _name = name;
            if (returnType != null)
                ReturnType = returnType;
            _modifiers = modifiers;
            if (parameters.Length > 0)
                CreateParameters().AddRange(parameters);
        }

        protected MethodDeclBase(string name, Expression returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : this(name, returnType, modifiers, new Block(), parameters)
        { }

        protected MethodDeclBase(string name, Expression returnType, CodeObject body, params ParameterDecl[] parameters)
            : this(name, returnType, Modifiers.None, body, parameters)
        { }

        protected MethodDeclBase(string name, Expression returnType, params ParameterDecl[] parameters)
            : this(name, returnType, Modifiers.None, new Block(), parameters)
        { }

        /// <summary>
        /// This constructor is used for explicit interface members (methods that have an interface name prefixed on their name).
        /// </summary>
        protected MethodDeclBase(Expression name, Expression returnType, CodeObject body, params ParameterDecl[] parameters)
            : this(null, returnType, Modifiers.None, body, parameters)
        {
            _name = name;
        }

        protected MethodDeclBase(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            IsFirstOnLine = true;  // Force all methods to start on a new line by default
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public virtual string Category
        {
            get { return "method"; }
        }

        /// <summary>
        /// Get the declaring <see cref="TypeDecl"/>.
        /// </summary>
        public TypeDecl DeclaringType
        {
            get { return (_parent as TypeDecl); }
        }

        /// <summary>
        /// Get the explicit interface expression (if any).
        /// </summary>
        public Expression ExplicitInterfaceExpression
        {
            get { return _name as Expression; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the method has parameters.
        /// </summary>
        public bool HasParameters
        {
            get { return (_parameters != null && _parameters.Count > 0); }
        }

        /// <summary>
        /// Determines if the method is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return _modifiers.HasFlag(Modifiers.Abstract); }
            set { _modifiers = (value ? _modifiers | Modifiers.Abstract : _modifiers & ~Modifiers.Abstract); }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> has compact empty braces by default.
        /// </summary>
        public override bool IsCompactIfEmptyDefault
        {
            get { return true; }
        }

        /// <summary>
        /// True if this is an explicit interface implementation.
        /// </summary>
        public bool IsExplicitInterfaceImplementation
        {
            get { return _name is Dot; }
        }

        /// <summary>
        /// True if the method is generic.
        /// </summary>
        public virtual bool IsGenericMethod
        {
            get { return false; }
        }

        /// <summary>
        /// True if the method has internal access.
        /// </summary>
        public bool IsInternal
        {
            get { return _modifiers.HasFlag(Modifiers.Internal); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Internal : _modifiers & ~Modifiers.Internal); }
        }

        /// <summary>
        /// True if the method is an override.
        /// </summary>
        public bool IsOverride
        {
            get { return _modifiers.HasFlag(Modifiers.Override); }
            set { _modifiers = (value ? _modifiers | Modifiers.Override : _modifiers & ~Modifiers.Override); }
        }

        /// <summary>
        /// True if the method has private access.
        /// </summary>
        public bool IsPrivate
        {
            get { return _modifiers.HasFlag(Modifiers.Private); }
            // Force other flags off if setting to Private
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Public) | Modifiers.Private : _modifiers & ~Modifiers.Private); }
        }

        /// <summary>
        /// True if the method has protected access.
        /// </summary>
        public bool IsProtected
        {
            get { return _modifiers.HasFlag(Modifiers.Protected); }
            // Force certain other flags off if setting to Protected
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Public) | Modifiers.Protected : _modifiers & ~Modifiers.Protected); }
        }

        /// <summary>
        /// True if the method has public access.
        /// </summary>
        public bool IsPublic
        {
            get { return _modifiers.HasFlag(Modifiers.Public); }
            // Force other flags off if setting to Public
            set { _modifiers = (value ? _modifiers & ~(Modifiers.Private | Modifiers.Protected | Modifiers.Internal) | Modifiers.Public : _modifiers & ~Modifiers.Public); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_returnType == null || (!_returnType.IsFirstOnLine && _returnType.IsSingleLine))
                    && (!(_name is Expression) || (!((Expression)_name).IsFirstOnLine && ((Expression)_name).IsSingleLine))
                    && (_parameters == null || _parameters.Count == 0 || (!_parameters[0].IsFirstOnLine && _parameters.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_returnType != null)
                    {
                        _returnType.IsFirstOnLine = false;
                        _returnType.IsSingleLine = true;
                    }
                    if (_name is Expression)
                    {
                        ((Expression)_name).IsFirstOnLine = false;
                        ((Expression)_name).IsSingleLine = true;
                    }
                    if (_parameters != null && _parameters.Count > 0)
                    {
                        _parameters[0].IsFirstOnLine = false;
                        _parameters.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// True if the method is static.
        /// </summary>
        public bool IsStatic
        {
            get { return _modifiers.HasFlag(Modifiers.Static); }
            set { _modifiers = (value ? _modifiers | Modifiers.Static : _modifiers & ~Modifiers.Static); }
        }

        /// <summary>
        /// True if the method is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get { return _modifiers.HasFlag(Modifiers.Virtual); }
            set { _modifiers = (value ? _modifiers | Modifiers.Virtual : _modifiers & ~Modifiers.Virtual); }
        }

        /// <summary>
        /// Optional <see cref="Modifiers"/> for the method.
        /// </summary>
        public Modifiers Modifiers
        {
            get { return _modifiers; }
            set { _modifiers = value; }
        }

        /// <summary>
        /// The name of the method.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (_name is string)
                    return (string)_name;
                // If it's an explicit interface implementation, use the full name
                if (_name is Expression)
                    return ((Expression)_name).AsString();
                return null;
            }
            set { _name = value; }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return base.NewLines; }
            set
            {
                // If we're changing to or from zero, also change any prefix attributes
                bool isFirstOnLine = (value != 0);
                if (_annotations != null && ((!isFirstOnLine && IsFirstOnLine) || (isFirstOnLine && !IsFirstOnLine)))
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (annotation is Attribute)
                            annotation.IsFirstOnLine = isFirstOnLine;
                    }
                }

                base.NewLines = value;
            }
        }

        /// <summary>
        /// The number of parameters the method has.
        /// </summary>
        public int ParameterCount
        {
            get { return (_parameters != null ? _parameters.Count : 0); }
        }

        /// <summary>
        /// A collection of <see cref="ParameterDecl"/>s for the parameters of the method.
        /// </summary>
        public ChildList<ParameterDecl> Parameters
        {
            get { return _parameters; }
            set { SetField(ref _parameters, value); }
        }

        /// <summary>
        /// The return type of the method (never null - will be type 'void' instead).
        /// </summary>
        public virtual Expression ReturnType
        {
            get { return (_returnType ?? TypeRef.VoidRef); }
            set { SetField(ref _returnType, value, true); }
        }

        /// <summary>
        /// Add one or more <see cref="ParameterDecl"/>s.
        /// </summary>
        public void AddParameters(params ParameterDecl[] parameterDecls)
        {
            CreateParameters().AddRange(parameterDecls);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return true;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            MethodDeclBase clone = (MethodDeclBase)base.Clone();
            clone.CloneField(ref clone._returnType, _returnType);
            clone.CloneField(ref clone._name, _name);
            clone._parameters = ChildListHelpers.Clone(_parameters, clone);
            return clone;
        }

        /// <summary>
        /// Create the list of <see cref="ParameterDecl"/>s, or return the existing one.
        /// </summary>
        public ChildList<ParameterDecl> CreateParameters()
        {
            if (_parameters == null)
                _parameters = new ChildList<ParameterDecl>(this);
            return _parameters;
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a method declaration, unless it's on a
            // single line and preceeded by another single-line method declaration or a comment.
            if (IsSingleLine && ((previous is MethodDeclBase && previous.IsSingleLine) || previous is Comment))
                return 1;
            return 2;
        }

        /// <summary>
        /// Find the base virtual method for this method if it's an override.
        /// </summary>
        public MethodRef FindBaseMethod()
        {
            if (IsOverride)
            {
                TypeDecl declaringType = DeclaringType;
                if (declaringType != null)
                {
                    TypeRef baseTypeRef = declaringType.GetBaseType();
                    if (baseTypeRef != null)
                    {
                        TypeRefBase[] parameterTypes = null;
                        if (_parameters != null)
                            parameterTypes = Enumerable.ToArray(Enumerable.Select<ParameterDecl, TypeRefBase>(_parameters, delegate (ParameterDecl parameterDecl) { return parameterDecl.Type.SkipPrefixes() as TypeRefBase; }));
                        return baseTypeRef.GetMethod(Name, parameterTypes);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
        /// <param name="isPrivate">True if the access is private.</param>
        /// <param name="isProtected">True if the access is protected.</param>
        /// <param name="isInternal">True if the access is internal.</param>
        public void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            // The isTargetOfAssignment flag is needed only for properties/indexers/events, not methods
            isPrivate = IsPrivate;
            if (!isPrivate)
            {
                isProtected = IsProtected;
                isInternal = IsInternal;
            }
            else
                isProtected = isInternal = false;
        }

        /// <summary>
        /// Get the type of the explicit interface (null if none).
        /// </summary>
        public TypeRef GetExplicitInterface()
        {
            if (_name is Dot)
            {
                Expression left = ((Dot)_name).Left;
                if (left != null)
                {
                    TypeRef typeRef = left.SkipPrefixes() as TypeRef;
                    if (typeRef != null && typeRef.IsInterface)
                        return typeRef;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public virtual string GetFullName(bool descriptive)
        {
            string name = Name;
            if (descriptive)
                name += GetParametersAsString();
            if (_parent is TypeDecl)
                name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
            return name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        /// <summary>
        /// Find the parameter of this method with the specified name.
        /// </summary>
        public ParameterRef GetParameter(string name)
        {
            if (_parameters != null)
            {
                foreach (ParameterDecl parameter in _parameters)
                {
                    if (parameter.Name == name)
                        return (ParameterRef)parameter.CreateRef();
                }
            }
            return null;
        }

        /// <summary>
        /// Determine if the specified types match the types of the method's parameters.
        /// </summary>
        public bool MatchParameters(TypeRefBase[] parameterTypes)
        {
            int parameterTypesCount = (parameterTypes != null ? parameterTypes.Length : 0);
            if ((_parameters != null ? _parameters.Count : 0) != parameterTypesCount)
                return false;
            for (int i = 0; i < parameterTypesCount; ++i)
            {
                // Treat a null destination parameter type as a wildcard (match anything)
                TypeRefBase thatParameterTypeRef = parameterTypes[i];
                if (thatParameterTypeRef != null)
                {
                    TypeRefBase thisParameterTypeRef = _parameters[i].Type.SkipPrefixes() as TypeRefBase;
                    // Allow any type parameters to match (necessary when looking for the base virtual method
                    // of a method with a parameter with a TypeParameterRef type).
                    if (thisParameterTypeRef is TypeParameterRef && thatParameterTypeRef is TypeParameterRef)
                        continue;
                    // Otherwise, require the references to be the same to match
                    if (thisParameterTypeRef == null || !thisParameterTypeRef.IsSameRef(thatParameterTypeRef))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        /// <summary>
        /// Get the specified parameters as a descriptive string.
        /// </summary>
        internal static string GetParametersAsString(string tokenStart, string tokenend, ChildList<ParameterDecl> parameterDecls)
        {
            string result = tokenStart;
            if (parameterDecls != null && parameterDecls.Count > 0)
            {
                bool isFirst = true;
                foreach (ParameterDecl parameter in parameterDecls)
                {
                    if (!isFirst)
                        result += ", ";
                    if (parameter.Modifier != ParameterModifier.None)
                        result += ParameterDecl.ParameterModifierToString(parameter.Modifier) + " ";
                    // Don't use GetDescription() on the Type, because we don't want any ShowParentTypes here
                    result += parameter.Type.AsText(RenderFlags.Description) + " " + parameter.Name;
                    isFirst = false;
                }
            }
            result += tokenend;
            return result;
        }

        internal virtual void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            if (_name is string)
                writer.WriteIdentifier((string)_name, flags);
            else if (_name is Expression)
                ((Expression)_name).AsText(writer, flags & ~(RenderFlags.Description | RenderFlags.ShowParentTypes));
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextInfixComments(writer, AnnotationFlags.IsInfix1, flags);
            writer.WriteList(_parameters, passFlags, this);
        }

        protected override void AsTextArgumentPrefix(CodeWriter writer, RenderFlags flags)
        { }

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (_returnType != null)
                _returnType.AsText(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            if (flags.HasFlag(RenderFlags.Description))
            {
                if (_parent is TypeDecl)
                {
                    ((TypeDecl)_parent).AsTextName(writer, flags);
                    writer.Write(Dot.ParseToken);
                }
            }
            AsTextName(writer, passFlags);
        }

        /// <summary>
        /// Get the parameters as a descriptive string.
        /// </summary>
        protected string GetParametersAsString()
        {
            return GetParametersAsString(ParameterDecl.ParseTokenStart, ParameterDecl.ParseTokenEnd, _parameters);
        }

        protected void ParseMethodNameAndType(Parser parser, CodeObject parent, bool unusedName, bool useTokenNewLines)
        {
            // Parse the name
            if (unusedName)
            {
                if (parser.HasUnusedIdentifier)
                {
                    Token lastUnusedToken = parser.RemoveLastUnusedToken();
                    if (useTokenNewLines)
                        NewLines = lastUnusedToken.NewLines;
                    _name = lastUnusedToken.NonVerbatimText;
                    SetLineCol(lastUnusedToken);
                }
                else
                {
                    // Support Dot expressions so we can handle explicit interface members
                    Expression expression = parser.RemoveLastUnusedExpression();
                    SetField(ref _name, expression, false);
                    Expression leftExpression = (expression is BinaryOperator ? ((BinaryOperator)expression).Left : expression);
                    if (leftExpression != null)
                        SetLineCol(leftExpression);
                }
            }
            else
                _name = parser.GetIdentifierText();  // Parse the name

            ParseUnusedType(parser, ref _returnType);  // Parse the return type from the Unused list
        }

        protected void ParseModifiersAndAnnotations(Parser parser)
        {
            _modifiers = ModifiersHelpers.Parse(parser, this);  // Parse any modifiers in reverse from the Unused list
            ParseUnusedAnnotations(parser, this, false);        // Parse attributes and/or doc comments from the Unused list
        }

        protected void ParseParameters(Parser parser)
        {
            // Parse the parameter declarations
            bool isEndFirstOnLine;
            _parameters = ParameterDecl.ParseList(parser, this, ParseTokenStart, ParseTokenEnd, false, out isEndFirstOnLine);
            IsEndFirstOnLine = isEndFirstOnLine;
        }
    }
}
