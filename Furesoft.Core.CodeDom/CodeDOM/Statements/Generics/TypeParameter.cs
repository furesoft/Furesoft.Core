using System.Collections;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics.Constraints;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Generics
{
    /// <summary>
    /// Represents a type parameter of a generic type or method declaration.
    /// </summary>
    public class TypeParameter : CodeObject, ITypeDecl
    {
        /// <summary>
        /// The alternate token used to parse the end of a list of type parameters inside a documentation comment.
        /// </summary>
        public const string ParseTokenAltEnd = TypeRefBase.ParseTokenAltArgumentEnd;

        /// <summary>
        /// The alternate token used to parse the start of a list of type parameters inside a documentation comment.
        /// </summary>
        public const string ParseTokenAltStart = TypeRefBase.ParseTokenAltArgumentStart;

        /// <summary>
        /// The token used to parse the end of a list of type parameters.
        /// </summary>
        public const string ParseTokenEnd = TypeRefBase.ParseTokenArgumentEnd;

        /// <summary>
        /// The token used to parse an 'in' type parameter.
        /// </summary>
        public const string ParseTokenIn = "in";

        /// <summary>
        /// The token used to parse an 'out' type parameter.
        /// </summary>
        public const string ParseTokenOut = "out";

        /// <summary>
        /// The token used to parse between type parameters.
        /// </summary>
        public const string ParseTokenSeparator = ParameterDecl.ParseTokenSeparator;

        /// <summary>
        /// The token used to parse the start of a list of type parameters.
        /// </summary>
        public const string ParseTokenStart = TypeRefBase.ParseTokenArgumentStart;

        protected string _name;

        /// <summary>
        /// Create a <see cref="TypeParameter"/> with the specified name.
        /// </summary>
        public TypeParameter(string name)
        {
            _name = name;
        }

        // Alternate type argument delimiters allowed for code embedded inside documentation comments.
        // The C# style delimiters are also allowed in doc comments, although they shouldn't show up
        // usually, since they cause errors with parsing the XML properly - but they could be used
        // programmatically in certain situations.  Both styles are thus supported inside doc comments,
        // but the open and close delimiters must match for each pair.  Note that in the case of a
        // type declaration, the alternate type argument delimiters are ambiguous with block delimiters -
        // this is handled by looking for a type argument pattern.
        protected TypeParameter(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // If we're starting with an attribute, ignore any newline parsed in the base constructor
            if (parser.TokenText == Attribute.ParseTokenStart)
                IsFirstOnLine = false;

            Attribute.ParseAttributes(parser, this);  // Parse any attributes

            _name = parser.GetIdentifierText();  // Parse the name
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "type parameter"; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsAbstract
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsClass
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="TypeParameter"/> has a delegate type.
        /// </summary>
        public bool IsDelegateType
        {
            get { return GetTypeConstraint().IsDelegateType; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsEnum
        {
            get { return false; }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public bool IsGenericParameter
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsGenericType
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsInterface
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public bool IsNested
        {
            // TypeParameters are considered nested types (including in .NET reflection)
            get { return true; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsNullableType
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsPartial
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsStruct
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsValueType
        {
            get { return false; }
        }

        /// <summary>
        /// The name of the <see cref="TypeParameter"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
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
                // If we're changing to zero, also change all prefix attributes to zero
                bool isFirstOnLine = (value != 0);
                if (_annotations != null && !isFirstOnLine && IsFirstOnLine)
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (annotation is Attribute)
                            annotation.IsFirstOnLine = false;
                    }
                }

                base.NewLines = value;
            }
        }

        /// <summary>
        /// Always <c>0</c>.
        /// </summary>
        public int TypeParameterCount
        {
            get { return 0; }
        }

        public static void AsTextTypeParameters(CodeWriter writer, ChildList<TypeParameter> typeParameters, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            // Render the angle brackets as braces if we're inside a documentation comment
            writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltStart : ParseTokenStart);
            writer.WriteList(typeParameters, passFlags, typeParameters.Parent);
            writer.Write(flags.HasFlag(RenderFlags.InDocComment) ? ParseTokenAltEnd : ParseTokenEnd);
        }

        /// <summary>
        /// Parse a list of type parameters.
        /// </summary>
        public static ChildList<TypeParameter> ParseList(Parser parser, CodeObject parent)
        {
            ChildList<TypeParameter> parameters = null;
            if (parser.TokenText == ParseTokenStart || (parser.InDocComment && parser.TokenText == ParseTokenAltStart
                && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenAltArgumentEnd, ParseFlags.None)))
            {
                string argumentEnd = (parser.TokenText == ParseTokenAltStart ? ParseTokenAltEnd : ParseTokenEnd);
                parent.MoveAllComments(parser.LastToken);  // Move any skipped comments to the parent
                parser.NextToken();                        // Move past '<'

                // Create a string of possible terminators (assuming 1 char terminators for now)
                string terminators = argumentEnd + MethodDeclBase.ParseTokenStart + ConstraintClause.ParseTokenSeparator + Statement.ParseTokenTerminator;

                while (parser.Token != null && (parser.TokenText.Length != 1 || terminators.IndexOf(parser.TokenText[0]) < 0))
                {
                    TypeParameter typeParameter = new TypeParameter(parser, parent);
                    if (typeParameter.Name != null)
                    {
                        if (parameters == null)
                            parameters = new ChildList<TypeParameter>(parent);
                        parameters.Add(typeParameter);
                        if (parser.TokenText == ParseTokenSeparator)
                            parser.NextToken();  // Move past ','
                    }
                    else
                        parser.NextToken();  // Move past bad token (non-identifier)
                }
                parser.NextToken();  // Move past '>'
            }
            return parameters;
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");
            AsTextAnnotations(writer, passFlags);
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);
        }

        /// <summary>
        /// Create an array reference to this <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateArrayRef(bool isFirstOnLine, params int[] ranks)
        {
            return new TypeParameterRef(this, isFirstOnLine, ranks);
        }

        /// <summary>
        /// Create an array reference to this <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateArrayRef(params int[] ranks)
        {
            return new TypeParameterRef(this, false, ranks);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef(bool isFirstOnLine)
        {
            return TypeRef.CreateNullable(CreateRef(), isFirstOnLine);
        }

        /// <summary>
        /// Create a nullable reference to this <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/>.</returns>
        public TypeRef CreateNullableRef()
        {
            return TypeRef.CreateNullable(CreateRef());
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeParameter"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new TypeParameterRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            // Ignore any type arguments - a TypeParameterRef can't have any
            return new TypeParameterRef(this, isFirstOnLine, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateRef(bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            // Ignore any type arguments - a TypeParameterRef can't have any
            return new TypeParameterRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            // Ignore any type arguments - a TypeParameterRef can't have any
            return new TypeParameterRef(this, false, arrayRanks);
        }

        /// <summary>
        /// Create a reference to the <see cref="TypeParameter"/>.
        /// </summary>
        /// <returns>A <see cref="TypeParameterRef"/>.</returns>
        public TypeRef CreateRef(ChildList<Expression> typeArguments)
        {
            // Ignore any type arguments - a TypeParameterRef can't have any
            return new TypeParameterRef(this, false);
        }

        /// <summary>
        /// Get the base type - always a <see cref="TypeRef"/> to the 'object' type.
        /// </summary>
        public TypeRef GetBaseType()
        {
            return TypeRef.ObjectRef;
        }

        /// <summary>
        /// Get any constraints for this <see cref="TypeParameter"/> from the parent type or generic method.
        /// </summary>
        public List<TypeParameterConstraint> GetConstraints()
        {
            return ((ITypeParameters)_parent).GetTypeParameterConstraints(this);
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes)
        {
            return null;
        }

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors(bool currentPartOnly)
        {
            return null;
        }

        /// <summary>
        /// Get all non-static constructors for this type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors()
        {
            return null;
        }

        /// <summary>
        /// Get the delegate parameters of the constraining type (if any).
        /// </summary>
        public ICollection GetDelegateParameters()
        {
            return GetTypeConstraint().GetDelegateParameters();
        }

        /// <summary>
        /// Get the delegate return type of the constraining type (if any).
        /// </summary>
        public TypeRefBase GetDelegateReturnType()
        {
            return GetTypeConstraint().GetDelegateReturnType();
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public FieldRef GetField(string name)
        {
            return null;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public string GetFullName(bool descriptive)
        {
            if (Parent is TypeDecl)
                return ((TypeDecl)Parent).GetFullName(descriptive) + "." + _name;
            if (Parent is GenericMethodDecl)
                return ((GenericMethodDecl)Parent).GetFullName(descriptive) + "." + _name;
            return _name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return GetFullName(false);
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
        {
            return null;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        { }

        /// <summary>
        /// Always returns <c>null</c> for this type.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="searchBaseClasses">Pass <c>false</c> to NOT search base classes.</param>
        public List<MethodRef> GetMethods(string name, bool searchBaseClasses)
        {
            return null;
        }

        /// <summary>
        /// Always returns <c>null</c> for this type.
        /// </summary>
        /// <param name="name">The method name.</param>
        public List<MethodRef> GetMethods(string name)
        {
            return null;
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public TypeRef GetNestedType(string name)
        {
            return null;
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public PropertyRef GetProperty(string name)
        {
            return null;
        }

        /// <summary>
        /// Get the main constraining type (if any - not including interfaces).
        /// </summary>
        public TypeRef GetTypeConstraint()
        {
            List<TypeParameterConstraint> constraints = GetConstraints();
            if (constraints != null)
            {
                foreach (TypeParameterConstraint constraint in constraints)
                {
                    if (constraint is TypeConstraint)
                    {
                        // Ignore if an UnresolvedRef, because we can't distinguish class vs interface
                        TypeRef typeRef = ((TypeConstraint)constraint).Type.SkipPrefixes() as TypeRef;
                        if (typeRef != null && typeRef.IsClass)
                            return typeRef;
                    }
                    else if (constraint is ClassConstraint)
                        return TypeRef.ObjectRef;
                    else if (constraint is StructConstraint)
                        return TypeRef.ValueTypeRef;
                }
            }
            return TypeRef.ObjectRef;
        }

        /// <summary>
        /// Determine if the constraining type is assignable from the specified type.
        /// </summary>
        public bool IsAssignableFrom(TypeRef typeRef)
        {
            return GetTypeConstraint().IsAssignableFrom(typeRef);
        }

        /// <summary>
        /// Determine if the constraining type implements the specified interface type.
        /// </summary>
        public bool IsImplementationOf(TypeRef interfaceTypeRef)
        {
            return GetTypeConstraint().IsImplementationOf(interfaceTypeRef);
        }

        /// <summary>
        /// Determine if the constraining type is a subclass of the specified type.
        /// </summary>
        public bool IsSubclassOf(TypeRef classTypeRef)
        {
            return GetTypeConstraint().IsSubclassOf(classTypeRef);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }
    }
}
