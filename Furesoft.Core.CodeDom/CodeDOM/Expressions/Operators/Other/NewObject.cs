using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// The NewObject operator is used to create new object instances of class, struct, or delegates.
    /// </summary>
    /// <remarks>
    /// Possible formats:
    /// 
    /// New Object:
    ///    new non-array-type(args)           // Type with constructor call (args is optional)
    /// With initializers (3.0):
    ///    new non-array-type(args) { init }  // Type/constructor call with object or collection initializer (args and init are optional)
    ///    new non-array-type { init }        // Type with default constructor with object or collection initializer (init is optional)
    /// 
    /// </remarks>
    public class NewObject : NewOperator
    {
        #region /* FIELDS */

        // A NewObject has both a TypeRef and an implied, hidden ConstructorRef.  The syntax "new List<int>()" is actually
        // short-hand for "new List<int>.List()".  We store and resolve both references, display the TypeRef name, show the
        // 'new' keyword in the error or warning color if the hidden ConstructorRef has an error or warning, and display the
        // hidden ConstructorRef in the tooltip.  This is necessary to handle type aliases, because the TypeRef and ConstructorRef
        // will have different names in this case, and the aliased type can have type arguments while the alias itself can't.
        // This technique also improves error messages, since it first resolves the type, then the constructor within the type.
        protected SymbolicRef _constructorRef;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="expression">An expression evaluating to the TypeRef of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(Expression expression, params Expression[] parameters)
            : base(expression, parameters)
        { }

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="constructorDecl">The ConstructorDecl of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(ConstructorDecl constructorDecl, params Expression[] parameters)
            : base(constructorDecl.DeclaringType.CreateRef(), parameters)
        {
            SetField(ref _constructorRef, constructorDecl.CreateRef(), false);
        }

        /// <summary>
        /// Create a <see cref="NewObject"/>.
        /// </summary>
        /// <param name="typeDecl">The TypeDecl of the object being created.</param>
        /// <param name="parameters">The constructor parameters (if any).</param>
        public NewObject(TypeDecl typeDecl, params Expression[] parameters)
            : base(typeDecl.CreateRef(), parameters)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The hidden <see cref="ConstructorRef"/> (or <see cref="UnresolvedRef"/>) that represents the constructor being called.
        /// Will be null if the constructor is implied, such as for the default constructor of a value type.
        /// </summary>
        public override SymbolicRef HiddenRef
        {
            get { return _constructorRef; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public override TypeRefBase GetParameterType(int argumentIndex)
        {
            if (_constructorRef is ConstructorRef)
            {
                TypeRefBase parameterTypeRef = MethodRef.GetParameterType(_constructorRef.Reference, argumentIndex, _expression);
                if (parameterTypeRef != null)
                    return parameterTypeRef;
            }
            // By default, assume we're indexing an array type
            return TypeRef.IntRef;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of the arguments.
        /// </summary>
        public const string ParseTokenStart = ParameterDecl.ParseTokenStart;

        /// <summary>
        /// The token used to parse the end of the arguments.
        /// </summary>
        public const string ParseTokenEnd = ParameterDecl.ParseTokenEnd;

        /// <summary>
        /// Parse a <see cref="NewObject"/>.
        /// </summary>
        public NewObject(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Save the starting token for later
            Token startingToken = parser.Token;

            parser.NextToken();  // Move past 'new'

            // Parse the expression representing the TypeRef being instantiated
            SetField(ref _expression, Parse(parser, this, false, ParseTokenStart + Initializer.ParseTokenStart), false);

            // Parse the arguments (if any) of the implied ConstructorRef being called
            if (parser.TokenText == ParseTokenStart)
                ParseArguments(parser, this, ParseTokenStart, ParseTokenEnd);

            // Set the parent starting token for use by the Initializer to determine proper indentation
            parser.ParentStartingToken = startingToken;

            // Parse any object or collection initializer
            ParseInitializer(parser, this);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Resolve the base before the initializer, because the initializer might reference properties of the type
            base.Resolve(ResolveCategory.Type, flags);
            if (_initializer != null)
                _initializer.Resolve(ResolveCategory.Expression, flags);
            return this;
        }

        protected override void ResolveInvokedExpression(ResolveCategory resolveCategory, ResolveFlags flags, out SymbolicRef oldInvokedRef, out SymbolicRef newInvokedRef)
        {
            // Resolve the invoked (called) expression - first, resolve the TypeRef, then any hidden ConstructorRef
            base.ResolveInvokedExpression(resolveCategory, flags, out oldInvokedRef, out newInvokedRef);

            // If we failed to resolve the TypeRef, then don't bother with the ConstructorRef yet
            if (newInvokedRef is UnresolvedRef)
            {
                // Force the ConstructorRef to null if it isn't already
                if (_constructorRef != null)
                    SetField(ref _constructorRef, null, false);
            }
            else
            {
                // If we have a TypeRef that's a value type and we have no arguments, or it's a type parameter, then the constructor is implicit
                bool hasImplicitConstructor = ((newInvokedRef is TypeRef && (((TypeRef)newInvokedRef).IsValueType && ArgumentCount == 0))
                    || newInvokedRef is TypeParameterRef);

                // If the TypeRef was resolved or changed, or it's not implicit and we don't have a ConstructorRef yet, or it's implicit
                // and we have a ConstructorRef, then reset the ConstructorRef as appropriate.
                if (newInvokedRef != oldInvokedRef || (!hasImplicitConstructor && _constructorRef == null) || (hasImplicitConstructor && _constructorRef != null))
                {
                    // If the constructor is implicit, make the ConstructorRef null, otherwise set it to an UnresolvedRef to be resolved
                    SymbolicRef symbolicRef = null;
                    if (!hasImplicitConstructor)
                    {
                        TypeRefBase typeRefBase = _expression.EvaluateType();
                        if (typeRefBase != null)
                            symbolicRef = new UnresolvedRef(typeRefBase, ResolveCategory.Constructor);
                    }
                    SetField(ref _constructorRef, symbolicRef, false);
                }

                // Resolve the ConstructorRef, treating it as the "invoked reference" now in place of the TypeRef
                oldInvokedRef = _constructorRef;
                if (_constructorRef is UnresolvedRef)
                    _constructorRef = (SymbolicRef)_constructorRef.Resolve(ResolveCategory.Constructor, flags);
                newInvokedRef = _constructorRef;
            }
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_constructorRef != null && _constructorRef.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // The NewObject expression evaluates to the TypeRef being instantiated
            return (_expression != null ? _expression.EvaluateType(withoutConstants) : null);
        }

        /// <summary>
        /// Get the invocation target reference.
        /// </summary>
        public override SymbolicRef GetInvocationTargetRef()
        {
            // The invocation target is the ConstructorRef being called
            return _constructorRef;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken);
            if (_expression != null)
                _expression.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Suppress empty parens *if* we have an initializer
            base.AsTextExpression(writer, flags | (_initializer != null ? RenderFlags.NoParensIfEmpty : 0));
        }

        protected override void AsTextStartArguments(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseTokenStart);
        }

        protected override void AsTextEndArguments(CodeWriter writer, RenderFlags flags)
        {
            if (IsEndFirstOnLine)
                writer.WriteLine();
            writer.Write(ParseTokenEnd);
        }

        #endregion
    }
}
