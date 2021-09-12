// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares a class, which includes a name plus a body, along with various optional modifiers.
    /// </summary>
    /// <remarks>
    /// Non-nested classes can be only public or internal, and default to internal.
    /// Nested classes can be any of the 5 access types, and default to private.
    /// The accessibility of a contained type cannot exceed that of the parent type.
    /// Other valid modifiers include: new, abstract, sealed, partial
    /// Members of a class default to private.
    /// Allowed members are: ConstructorDecls, DestructorDecls, FieldDecls, MethodDecls, PropertyDecls,
    ///    IndexerDecls, OperatorDecls, EventDecls, DelegateDecls, ClassDecls, StructDecls, InterfaceDecls
    /// The optional base list can contain a single base class and/or one or more interfaces.
    /// Optional type parameters can be used for generic types, along with optional constraints.
    /// </remarks>
    public class ClassDecl : BaseListTypeDecl
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ClassDecl"/> with the specified name.
        /// </summary>
        public ClassDecl(string name, Modifiers modifiers)
            : base(name, modifiers)
        {
            // Check if we need to create a compiler-generated default constructor
            CheckGeneratedDefaultConstructor(true);
        }

        /// <summary>
        /// Create a <see cref="ClassDecl"/> with the specified name.
        /// </summary>
        public ClassDecl(string name)
            : this(name, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="ClassDecl"/> with the specified name, modifiers, and base types.
        /// </summary>
        public ClassDecl(string name, Modifiers modifiers, params Expression[] baseTypes)
            : base(name, modifiers, baseTypes)
        {
            // Check if we need to create a compiler-generated default constructor
            CheckGeneratedDefaultConstructor(true);
        }

        /// <summary>
        /// Create a <see cref="ClassDecl"/> with the specified name, modifiers, and type parameters.
        /// </summary>
        public ClassDecl(string name, Modifiers modifiers, params TypeParameter[] typeParameters)
            : base(name, modifiers, typeParameters)
        {
            // Check if we need to create a compiler-generated default constructor
            CheckGeneratedDefaultConstructor(true);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsClass
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

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add a <see cref="CodeObject"/> to the <see cref="ClassDecl"/> body.
        /// </summary>
        public override void Add(CodeObject obj)
        {
            // Check if we need to remove a compiler-generated default constructor
            if (obj is ConstructorDecl)
                CheckRemoveGeneratedDefaultConstructor((ConstructorDecl)obj);
            base.Add(obj);
        }

        /// <summary>
        /// Add multiple <see cref="CodeObject"/>s to the <see cref="ClassDecl"/> body.
        /// </summary>
        public override void Add(params CodeObject[] objects)
        {
            foreach (CodeObject obj in objects)
                Add(obj);
        }

        /// <summary>
        /// Add a collection of <see cref="CodeObject"/>s to the <see cref="ClassDecl"/> body.
        /// </summary>
        /// <param name="collection">The collection to be added.</param>
        public override void AddRange(IEnumerable<CodeObject> collection)
        {
            foreach (CodeObject codeObject in collection)
                Add(codeObject);
        }

        /// <summary>
        /// Insert a <see cref="CodeObject"/> at the specified index in the <see cref="ClassDecl"/> body.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="obj">The CodeObject to be inserted.</param>
        public override void Insert(int index, CodeObject obj)
        {
            // Check if we need to remove a compiler-generated default constructor
            if (obj is ConstructorDecl)
                CheckRemoveGeneratedDefaultConstructor((ConstructorDecl)obj);
            base.Insert(index, obj);
        }

        /// <summary>
        /// Remove the specified <see cref="CodeObject"/> from the <see cref="ClassDecl"/> body.
        /// </summary>
        public override void Remove(CodeObject obj)
        {
            base.Remove(obj);
            // Check if we need to create a compiler-generated default constructor
            if (obj is ConstructorDecl)
                CheckGeneratedDefaultConstructor(true);
        }

        /// <summary>
        /// Remove all <see cref="CodeObject"/>s from the <see cref="ClassDecl"/> body.
        /// </summary>
        public override void RemoveAll()
        {
            base.RemoveAll();
            // Check if we need to create a compiler-generated default constructor
            CheckGeneratedDefaultConstructor(true);
        }

        /// <summary>
        /// Get the base type.
        /// </summary>
        public override TypeRef GetBaseType()
        {
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef baseTypeRef = baseTypeExpression.EvaluateType() as TypeRef;
                    if (baseTypeRef != null && baseTypeRef.IsClass)
                        return baseTypeRef;
                }
            }
            return TypeRef.ObjectRef;
        }

        /// <summary>
        /// Determine if the class is a subclass of the specified class.
        /// </summary>
        public override bool IsSubclassOf(TypeRef classTypeRef)
        {
            TypeRef baseTypeRef = GetBaseType();
            while (baseTypeRef != null)
            {
                if (baseTypeRef.IsSameRef(classTypeRef))
                    return true;
                if (baseTypeRef.IsSameRef(TypeRef.ObjectRef))
                    return false;
                baseTypeRef = baseTypeRef.GetBaseType() as TypeRef;
            }
            return false;
        }

        /// <summary>
        /// Check if we need to create or remove a compiler-generated default constructor.
        /// </summary>
        public void CheckGeneratedDefaultConstructor(bool currentPartOnly)
        {
            NamedCodeObjectGroup constructors = GetConstructors();

            // Add or remove compiler-generated default constructors as necessary
            if (constructors == null || constructors.Count == 0)
            {
                // Add a compiler-generated default public constructor if we don't have any constructors yet,
                // and this isn't a static class.
                if (!IsStatic)
                    base.Add(new ConstructorDecl(Modifiers.Public) { IsGenerated = true, IsSingleLine = true });
            }
            else if (constructors.Count > 1)
            {
                // Remove any duplicate compiler-generated default constructors (can occur for partial types during multithreaded
                // parsing), and if we have any other non-static constructors, then remove all default constructors.
                bool removeAllDefaults = false;
                ConstructorDecl defaultConstructor = null;
                foreach (ConstructorDecl constructor in constructors)
                {
                    ConstructorDecl removeConstructor = null;
                    if (constructor.IsGenerated && constructor.ParameterCount == 0)
                    {
                        if (removeAllDefaults)
                            removeConstructor = constructor;
                        else if (defaultConstructor != null)
                        {
                            if (constructor.Parent == this)
                                removeConstructor = constructor;
                            else
                            {
                                removeConstructor = defaultConstructor;
                                defaultConstructor = constructor;
                            }
                        }
                        else
                            defaultConstructor = constructor;
                    }
                    else if (!constructor.IsStatic)
                    {
                        removeAllDefaults = true;
                        if (defaultConstructor != null)
                            removeConstructor = defaultConstructor;
                    }
                    if (removeConstructor != null)
                    {
                        // Don't remove if we're doing the current part only and it belongs to another part
                        if (!currentPartOnly || removeConstructor.Parent == this)
                        {
                            // Remove via its parent in case it belongs to another part of a partial type
                            ((BlockStatement)removeConstructor.Parent).Body.Remove(removeConstructor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if we need to remove a compiler-generated default constructor when adding a constructor.
        /// </summary>
        protected void CheckRemoveGeneratedDefaultConstructor(ConstructorDecl constructorDecl)
        {
            // If we're adding a non-static constructor, we need to remove any generated ones
            if (!constructorDecl.IsStatic)
            {
                NamedCodeObjectGroup constructors = GetConstructors();
                if (constructors != null && constructors.Count > 0)
                {
                    foreach (ConstructorDecl constructor in constructors)
                    {
                        if (constructor.IsGenerated && constructor.ParameterCount == 0)
                        {
                            // Remove via its parent in case it belongs to another part of a partial type
                            ((BlockStatement)constructor.Parent).Body.Remove(constructor);
                        }
                    }
                }
           }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "class";

        internal static void AddParsePoints()
        {
            // Classes are only valid with a Namespace or TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="ClassDecl"/>.
        /// </summary>
        public static ClassDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ClassDecl(parser, parent);
        }

        protected ClassDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);        // Get any comments before 'class'
            parser.NextToken();                    // Move past 'class'
            ParseNameTypeParameters(parser);       // Parse the name and any type parameters
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers

            // Move any trailing compiler directives to the Infix1 position (assume we have a base-type list)
            MoveAnnotations(AnnotationFlags.IsPostfix, AnnotationFlags.IsInfix1);

            ParseBaseTypeList(parser);        // Parse the optional base-type list
            ParseConstraintClauses(parser);   // Parse any constraint clauses

            // Move any trailing post annotations on the last base type to the first constraint (if any)
            AdjustBaseTypePostComments();

            // If we don't have a base-type list, move any trailing compiler directives to the Postfix position
            if (_baseTypes == null || _baseTypes.Count == 0)
                MoveAnnotations(AnnotationFlags.IsInfix1, AnnotationFlags.IsPostfix);

            new Block(out _body, parser, this, true);  // Parse the body

            // Eat any trailing terminator (they are allowed but not required on non-delegate type declarations)
            if (parser.TokenText == ParseTokenTerminator)
                parser.NextToken();

            // Check if we need to create or remove a compiler-generated default constructor.  In this case, we
            // might end up removing one from a different part than this one - this is necessary for single-
            // threaded parsing, and it's also thread-safe because both this class and any other parts won't
            // be visible by other threads until after the parsing of their parent is complete.
            CheckGeneratedDefaultConstructor(false);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a class declaration
            return 2;
        }

        #endregion
    }
}
