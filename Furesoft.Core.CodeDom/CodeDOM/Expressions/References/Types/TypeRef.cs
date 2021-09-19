using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Utilities;
using static Furesoft.Core.CodeDom.Utilities.Reflection.TypeUtil;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types
{
    /// <summary>
    /// Represents a constant value of a specified enum type, stored in the underlying type of the enum.
    /// </summary>
    /// <remarks>
    /// This class is necessary because we can't dynamically create an instance of the enum type with the appropriate
    /// value since the type of the enum might be an <see cref="EnumDecl"/>, and we can't rely on being able to create a
    /// <see cref="Type"/> that represents such a declaration because there could possibly be compilation errors.
    /// </remarks>
    public class EnumConstant
    {
        /// <summary>
        /// The constant value of the enum, as an object of the underlying type
        /// </summary>
        public object ConstantValue;

        /// <summary>
        /// The <see cref="EnumDecl"/> or <see cref="Type"/> representing the type of the enum.
        /// </summary>
        public TypeRef EnumTypeRef;

        /// <summary>
        /// Create an <see cref="EnumConstant"/>.
        /// </summary>
        public EnumConstant(TypeRef enumTypeRef, object constantValue)
        {
            EnumTypeRef = enumTypeRef;

            // Just in case, default a null constant value to a 0 'int' value
            if (constantValue == null)
                constantValue = 0;
            // If the constant is another EnumConstant, extract it's value
            else if (constantValue is EnumConstant)
                constantValue = ((EnumConstant)constantValue).ConstantValue;

            // Force the constant value to that of the underlying type of the enum if necessary and possible.
            // This is required because the evaluation of constant expressions will promote smaller types to
            // ints, and the enum's underlying type might be smaller.  It's better to do this here rather
            // than inside all of the EvaluateConstants() methods in the various operators.
            ConstantValue = TypeRef.ChangeTypeOfConstant(constantValue, enumTypeRef.GetUnderlyingTypeOfEnum());
        }

        /// <summary>
        /// Create an <see cref="EnumConstant"/>.
        /// </summary>
        public EnumConstant(Type enumType, object constantValue)
            : this(new TypeRef(enumType), constantValue)
        { }

        /// <summary>
        /// True if the <see cref="EnumConstant"/> belongs to a bit-flags enum.
        /// </summary>
        public bool IsBitFlags
        {
            get { return EnumTypeRef.IsBitFlagsEnum; }
        }
    }

    /// <summary>
    /// Represents a reference to an <see cref="ITypeDecl"/> (<see cref="TypeDecl"/>, <see cref="TypeParameter"/>,
    /// <see cref="Alias"/>) or an external <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// A TypeRef will reference an ITypeDecl code object for types defined in the same solution, otherwise
    /// it will directly reference a <see cref="Type"/> object.  An ITypeDecl might be a TypeDecl, a
    /// TypeParameter (using the TypeParameterRef derived class), an Alias to a type (using the AliasRef derived
    /// class), an Anonymous Type (using the AnonymousTypeRef derived class), or a Var Type (using the VarTypeRef
    /// derived class).  When referencing TypeDecls or TypeDefinitions/GenericParameters, any array ranks and/or
    /// type arguments are part of the TypeRef itself.  When referencing Types, array and generic instance Types
    /// can be directly referenced.  In the hybrid case of a generic Type with one or more TypeDecls as type
    /// parameters, the TypeRef will reference the generic Type with empty type arguments, with the actual type
    /// parameters stored in the TypeRef.
    ///
    /// If the TypeRef is actually a VarTypeRef, then the _reference member will be null, and the Reference
    /// property must be used to get the reference.  In this case, the Reference can be a string if the type
    /// of the VarTypeRef's type is an UnresolvedRef.
    ///
    /// A TypeRef may also reference a constant, which has both a type and a value.  Most constants are of
    /// built-in types, and the reference will be to the 'int' or 'string' object, etc.  For enums, the
    /// reference will be to an EnumConstant object, which stores a TypeRef to the enum type plus an object
    /// of the underlying type of the enum containing the value of the constant.  For 'null' constants, the
    /// reference will be to the ITypeDecl or Type as normal, with a flag set to indicate the 'null' constant.
    /// </remarks>
    public class TypeRef : TypeRefBase
    {
        /// <summary>
        /// A hash set of primitive type names (the "System" namespace prefix is NOT included on the names).
        /// </summary>
        public static readonly HashSet<string> PrimitiveTypeNames = new HashSet<string>
            {
                "SByte",
                "Byte",
                "Int16",
                "UInt16",
                "Int32",
                "UInt32",
                "Int64",
                "UInt64",
                "IntPtr",
                "UIntPtr",
                "Char",
                "Boolean",
                "Single",
                "Double"
            };

        /// <summary>
        /// A map of type names to TypeCodes (the "System" namespace prefix is NOT included on the names).
        /// </summary>
        public static readonly Dictionary<string, TypeCode> TypeNameToTypeCodeMap = new Dictionary<string, TypeCode>(17)
            {
                { "Object",   TypeCode.Object   },
                { "SByte",    TypeCode.SByte    },
                { "Byte",     TypeCode.Byte     },
                { "Int16",    TypeCode.Int16    },
                { "UInt16",   TypeCode.UInt16   },
                { "Int32",    TypeCode.Int32    },
                { "UInt32",   TypeCode.UInt32   },
                { "Int64",    TypeCode.Int64    },
                { "UInt64",   TypeCode.UInt64   },
                { "Char",     TypeCode.Char     },
                { "Boolean",  TypeCode.Boolean  },
                { "String",   TypeCode.String   },
                { "Single",   TypeCode.Single   },
                { "Double",   TypeCode.Double   },
                { "Decimal",  TypeCode.Decimal  },
                { "DateTime", TypeCode.DateTime },
                { "DBNull",   TypeCode.DBNull   }
            };

        /// <summary>
        /// A map of type names to primitive and built-in .NET types.
        /// </summary>
        public static readonly Dictionary<string, Type> TypeNameToTypeMap = new Dictionary<string, Type>(16)
            {
                { "System.Object",  typeof(object)  },
                { "System.Void",    typeof(void)    },
                { "System.SByte",   typeof(sbyte)   },
                { "System.Byte",    typeof(byte)    },
                { "System.Int16",   typeof(short)   },
                { "System.UInt16",  typeof(ushort)  },
                { "System.Int32",   typeof(int)     },
                { "System.UInt32",  typeof(uint)    },
                { "System.Int64",   typeof(long)    },
                { "System.UInt64",  typeof(ulong)   },
                { "System.IntPtr",  typeof(IntPtr)  },
                { "System.UIntPtr", typeof(UIntPtr) },
                { "System.Char",    typeof(char)    },
                { "System.Boolean", typeof(bool)    },
                { "System.String",  typeof(string)  },
                { "System.Single",  typeof(float)   },
                { "System.Double",  typeof(double)  },
                { "System.Decimal", typeof(decimal) }
            };

        public static TypeRef ArrayRef;

        public static TypeRef AsyncCallbackRef;

        public static TypeRef BoolRef;

        public static TypeRef ByteRef;

        public static TypeRef CharRef;

        public static TypeRef[,] CommonTypeRefMap;

        public static TypeRef DecimalRef;

        public static TypeRef DelegateRef;

        public static TypeRef Dictionary2Ref;

        public static TypeRef DoubleRef;

        public static TypeRef EnumRef;

        public static TypeRef FlagsAttributeRef;

        public static TypeRef FloatRef;

        public static TypeRef IAsyncResultRef;

        public static TypeRef ICloneableRef;

        public static TypeRef ICollection1Ref;

        public static TypeRef ICollectionRef;

        public static TypeRef IEnumerable1Ref;

        public static TypeRef IEnumerableRef;

        public static TypeRef IList1Ref;

        public static TypeRef IListRef;

        public static TypeRef IntRef;

        public static TypeRef ISerializableRef;

        public static Dictionary<string, TypeRef> KeywordToTypeRefMap = new Dictionary<string, TypeRef>(16);

        public static TypeRef LongRef;

        public static TypeRef MulticastDelegateRef;

        public static TypeRef Nullable1Ref;

        // These static TypeRefs can be re-used anywhere *except* for references that appear
        // directly in source code (because they may have different formatting information).
        public static TypeRef ObjectRef;

        public static TypeRef SByteRef;
        public static TypeRef ShortRef;
        public static TypeRef StringRef;
        public static TypeRef TypeTypeRef;
        public static TypeRef TypeUtilTRef;
        public static TypeRef UIntRef;
        public static TypeRef ULongRef;
        public static TypeRef UShortRef;
        public static TypeRef ValueTypeRef;
        public static TypeRef VoidRef;
        private static readonly Dictionary<Type, TypeRef> TypeToTypeRefMap = new Dictionary<Type, TypeRef>();

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
            : base(iTypeDecl, isFirstOnLine)
        {
            // If the ITypeDecl is generic, and no type arguments were specified, use the declared type parameters (including those from any
            // enclosing types).  If only local arguments were specified, then prefix the declared type parameters from any enclosing types.
            // TypeParameters won't have type arguments, and neither should a reference to an Alias, so we only need to check TypeDecls.
            if (iTypeDecl is TypeDecl && iTypeDecl.IsGenericType)
                typeArguments = DefaultTypeArguments((TypeDecl)iTypeDecl, typeArguments);

            TypeArguments = typeArguments;
            ArrayRanks = arrayRanks;
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, bool isFirstOnLine, ChildList<Expression> typeArguments)
            : this(iTypeDecl, isFirstOnLine, typeArguments, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, bool isFirstOnLine)
            : this(iTypeDecl, isFirstOnLine, (ChildList<Expression>)null, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl)
            : this(iTypeDecl, false, (ChildList<Expression>)null, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, ChildList<Expression> typeArguments, List<int> arrayRanks)
            : this(iTypeDecl, false, typeArguments, arrayRanks)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, ChildList<Expression> typeArguments)
            : this(iTypeDecl, false, typeArguments, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, List<int> arrayRanks)
            : this(iTypeDecl, false, null, arrayRanks)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, bool isFirstOnLine, params Expression[] typeArguments)
            : this(iTypeDecl, isFirstOnLine, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, params Expression[] typeArguments)
            : this(iTypeDecl, false, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, bool isFirstOnLine, params int[] arrayRanks)
            : this(iTypeDecl, isFirstOnLine, null, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from an <see cref="ITypeDecl"/>.
        /// </summary>
        public TypeRef(ITypeDecl iTypeDecl, params int[] arrayRanks)
            : this(iTypeDecl, false, null, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
            : base((Type)null, isFirstOnLine)
        {
            // If the type is an array, and no array ranks were specified, use the existing array ranks (otherwise override them)
            if (type.IsArray)
            {
                bool useExistingRanks = (arrayRanks == null);
                if (useExistingRanks)
                    arrayRanks = new List<int>();
                do
                {
                    if (useExistingRanks)
                        arrayRanks.Add(type.GetArrayRank());
                    type = type.GetElementType();
                }
                while (type.IsArray);
            }

            // Dereference (remove the trailing '&') if it's a reference type
            if (type.IsByRef)
                type = type.GetElementType();

            // If the type is generic, and no type arguments were specified, use the existing arguments (including those
            // from any enclosing types).  If only local arguments were specified, then prefix those from any enclosing types.
            if (type.IsGenericType)
                typeArguments = DefaultTypeArguments(type, typeArguments);

            _reference = type;
            TypeArguments = typeArguments;
            ArrayRanks = arrayRanks;
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, bool isFirstOnLine, ChildList<Expression> typeArguments)
            : this(type, isFirstOnLine, typeArguments, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, bool isFirstOnLine)
            : this(type, isFirstOnLine, (ChildList<Expression>)null, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, ChildList<Expression> typeArguments, List<int> arrayRanks)
            : this(type, false, typeArguments, arrayRanks)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, ChildList<Expression> typeArguments)
            : this(type, false, typeArguments, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type)
            : this(type, false, (ChildList<Expression>)null, null)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, List<int> arrayRanks)
            : this(type, false, null, arrayRanks)
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, bool isFirstOnLine, params Expression[] typeArguments)
            : this(type, isFirstOnLine, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, params Expression[] typeArguments)
            : this(type, false, ((typeArguments != null && typeArguments.Length > 0) ? new ChildList<Expression>(typeArguments) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, bool isFirstOnLine, params int[] arrayRanks)
            : this(type, isFirstOnLine, null, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// If the <see cref="Type"/> might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>)
        /// or an array of one of those, use one of the TypeRef.Create() factory methods instead.
        /// </summary>
        public TypeRef(Type type, params int[] arrayRanks)
            : this(type, false, null, ((arrayRanks != null && arrayRanks.Length > 0) ? new List<int>(arrayRanks) : null))
        { }

        /// <summary>
        /// Create a <see cref="TypeRef"/> from a constant value.
        /// </summary>
        /// <remarks>
        /// The <see cref="TypeRef"/> will reference the constant instance object, the type of which will be retrieved
        /// as needed using <see cref="object.GetType()"/>.
        /// </remarks>
        public TypeRef(object constantValue)
            : base(constantValue ?? typeof(object))
        {
            _formatFlags |= FormatFlags.Const;
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from the specified type and constant value.
        /// </summary>
        /// <remarks>
        /// If the specified TypeRef refers to an Enum type, the created TypeRef will reference a special EnumConstant
        /// instance, which will hold the TypeRef of the enum type plus an instance object of the enum's underlying
        /// type holding the constant value (it's not possible to reference an enum value member instead, because
        /// the constant value of an enum might be outside the range of its members, or not have a one-to-one mapping
        /// if it's a flags enum; it's also not possible to construct an instance of the enum type with the proper
        /// constant value, since an EnumDecl might not be in a state that allows an instance to be created, and it
        /// could be modified by editing, thus invalidating the type).
        /// Otherwise, if the specified constant value isn't null, an attempt will be made to convert it to the
        /// specified TypeRef.  If the constant value is null, or the conversion attempt fails, then a null constant
        /// of the specified TypeRef will be created.
        /// </remarks>
        public TypeRef(TypeRef typeRef, object constantValue)
            : base(null)
        {
            bool isOK = true;
            if (typeRef.IsEnum)
                _reference = new EnumConstant(typeRef, constantValue);
            else
            {
                // Convert the constant value to the specified type if possible
                if (constantValue != null)
                {
                    // If the destination is type 'object', the only valid constant type is 'null', so skip this logic
                    // (which means that a numeric constant cast to type 'object' results in a non-constant 'object').
                    if (!typeRef.IsSameRef(ObjectRef))
                    {
                        if (constantValue is EnumConstant)
                            constantValue = ((EnumConstant)constantValue).ConstantValue;
                        _reference = ChangeTypeOfConstant(constantValue, typeRef);
                    }
                    // If the conversion fails, lose the constant status
                    if (_reference == null)
                        isOK = false;
                }
                // If the constant value is null, or the conversion above failed, create a copy of specified type, copying
                // the internals of the passed TypeRef.  If the conversion failed, it will remain non-const, but if it's
                // a null constant, the const flag will be set below on exit.
                if (_reference == null)
                {
                    _reference = typeRef._reference;
                    if (typeRef._arrayRanks != null)
                        _arrayRanks = new List<int>(typeRef._arrayRanks);
                    if (typeRef._typeArguments != null)
                        _typeArguments = ChildListHelpers.Clone(typeRef._typeArguments, this);
                }
            }
            if (isOK)
                _formatFlags |= FormatFlags.Const;
        }

        /// <summary>
        /// Construct a reference to a built-in type or nullable type, or non-built-in nullable type.
        /// </summary>
        protected TypeRef(Parser parser, CodeObject parent, bool isBuiltIn, ParseFlags flags)
            : base(parser, parent)
        {
            // Handle built-in types
            if (isBuiltIn)
            {
                // Resolve built-in type name to actual Type
                TypeRef typeRef = KeywordToTypeRefMap[parser.TokenText];
                _reference = typeRef.Reference;
                parser.NextToken();  // Move past type name

                // Check for nullable types
                if (parser.Token != null)
                {
                    // For better performance, go ahead and parse any trailing '?' now if we have a value type, there's
                    // no whitespace before the '?', and our parent isn't an Is operator.  Otherwise, the '?' might be
                    // a Conditional, so we'll allow it to try parsing it instead - if it fails, we'll end up back here
                    // later in ParseNullableType, which will take care of it.
                    if (parser.TokenText == ParseTokenNullable && typeRef.IsValueType
                        && parser.Token.LeadingWhitespace.Length == 0 && !(_parent is Is))
                    {
                        parser.NextToken();  // Move past '?'
                        typeRef = (TypeRef)typeRef.Clone();
                        typeRef.SetLineCol(this);
                        CreateTypeArguments().Add(typeRef);
                        _reference = Nullable1Ref.Reference;
                    }
                }
            }
            else  // Handle nullable types
            {
                _reference = Nullable1Ref.Reference;
                parser.NextToken();  // Move past '?'
                Expression expression = parser.RemoveLastUnusedExpression();
                Expression leftExpression = expression;
                while (leftExpression is BinaryOperator)
                    leftExpression = ((BinaryOperator)leftExpression).Left;
                if (leftExpression != null)
                    SetLineCol(leftExpression);
                IsFirstOnLine = expression.IsFirstOnLine;
                expression.IsFirstOnLine = false;
                CreateTypeArguments().Add(expression);
            }

            // Handle array types
            if (parser.TokenText == ParseTokenArrayStart && !flags.HasFlag(ParseFlags.NoArrays))
            {
                Token next = parser.PeekNextToken();
                if (next != null && (next.Text == ParseTokenArrayEnd || next.Text == ParseTokenSeparator))
                    ParseArrayRanks(parser);
            }
        }

        /// <summary>
        /// The descriptive category of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Category
        {
            get
            {
                if (_reference is ITypeDecl)
                    return ((ITypeDecl)_reference).Category;
                if (_reference is Type)
                    return MemberInfoUtil.GetCategory((Type)_reference);
                if (_reference is EnumConstant)
                    return "enum constant";
                return "constant";
            }
        }

        /// <summary>
        /// True if the referenced type is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsAbstract;
                if (reference is Type)
                    return ((Type)reference).IsAbstract;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a bit-flags enum.
        /// </summary>
        public bool IsBitFlagsEnum
        {
            get
            {
                if (IsEnum)
                {
                    if (_reference is EnumDecl)
                        return ((EnumDecl)_reference).IsBitFlags;
                    if (_reference is Type)
                        return TypeUtil.IsBitFlagsEnum((Type)_reference);
                    if (_reference is EnumConstant)
                        return ((EnumConstant)_reference).EnumTypeRef.IsBitFlagsEnum;
                }
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a built-in type (has a keyword). The built-in types are:
        /// object, void, sbyte, byte, short, ushort, int, uint, long, ulong, char, bool, string, float, double, decimal
        /// </summary>
        public override bool IsBuiltInType
        {
            get
            {
                if (HasArrayRanks) return false;
                // Check the type name first for efficiency, then verify the namespace name.
                // We can't use IsPrimitive, because it's missing a few types and has a couple that aren't built-ins.
                return (TypeNameToKeywordMap.ContainsKey(Name) && NamespaceName == "System");
            }
        }

        /// <summary>
        /// True if the referenced type is a class.
        /// </summary>
        public bool IsClass
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsClass;
                if (reference is Type)
                    return ((Type)reference).IsClass;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type has a constant value.
        /// </summary>
        public override bool IsConst
        {
            get { return (_formatFlags.HasFlag(FormatFlags.Const)); }
        }

        /// <summary>
        /// True if the referenced type is a delegate type.
        /// </summary>
        public override bool IsDelegateType
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsDelegateType;
                if (reference is Type)
                    return TypeUtil.IsDelegateType((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is an enum.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsEnum;
                if (reference is Type)
                    return ((Type)reference).IsEnum;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a generic type parameter.
        /// </summary>
        public virtual bool IsGenericParameter
        {
            get
            {
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsGenericParameter;
                if (reference is Type)
                    return ((Type)reference).IsGenericParameter;
                return false;
            }
        }

        /// <summary>
        /// True if the type is a generic type (meaning that either it or an enclosing type has type arguments).
        /// </summary>
        public bool IsGenericType
        {
            get
            {
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsGenericType;
                if (reference is Type)
                    return ((Type)reference).IsGenericType;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is an interface.
        /// </summary>
        public override bool IsInterface
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsInterface;
                if (reference is Type)
                    return ((Type)reference).IsInterface;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type has internal access.
        /// </summary>
        public override bool IsInternal
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsInternal;
                object reference = GetReferencedType();
                if (reference is IModifiers)
                    return ((IModifiers)reference).IsInternal;
                if (reference is Type)
                    return TypeUtil.IsInternal((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a nested type.
        /// </summary>
        public bool IsNested
        {
            get
            {
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsNested;
                if (reference is Type)
                    return ((Type)reference).IsNested;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a nullable type.
        /// </summary>
        public override bool IsNullableType
        {
            get { return (Name == "Nullable" && NamespaceName == "System" && _typeArguments != null && _typeArguments.Count == 1 && _typeArguments[0] != null); }
        }

        /// <summary>
        /// True if the referenced type is a partial type.
        /// </summary>
        public bool IsPartial
        {
            get
            {
                object reference = GetReferencedType();
                return (reference is ITypeDecl && ((ITypeDecl)reference).IsPartial);
            }
        }

        /// <summary>
        /// True if the type is Primitive. The Primitive Types are:
        /// sbyte, byte, short, ushort, int, uint, long, ulong, IntPtr, UIntPtr, char, bool, float, double.
        /// The following are NOT primitive types: object, void, string, decimal
        ///  </summary>
        public bool IsPrimitive
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return (PrimitiveTypeNames.Contains(((ITypeDecl)reference).Name) && ((ITypeDecl)reference).GetNamespace().Name == "System");
                if (reference is Type)
                    return ((Type)reference).IsPrimitive;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type has private access.
        /// </summary>
        public override bool IsPrivate
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsPrivate;
                object reference = GetReferencedType();
                if (reference is IModifiers)
                    return ((IModifiers)reference).IsPrivate;
                if (reference is Type)
                    return TypeUtil.IsPrivate((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type has protected access.
        /// </summary>
        public override bool IsProtected
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsProtected;
                object reference = GetReferencedType();
                if (reference is IModifiers)
                    return ((IModifiers)reference).IsProtected;
                if (reference is Type)
                    return TypeUtil.IsProtected((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type has public access.
        /// </summary>
        public override bool IsPublic
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsPublic;
                object reference = GetReferencedType();
                if (reference is IModifiers)
                    return ((IModifiers)reference).IsPublic;
                if (reference is Type)
                    return ((Type)reference).IsPublic;
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is static.
        /// </summary>
        public override bool IsStatic
        {
            get
            {
                if (HasArrayRanks) return GetElementType().IsStatic;
                object reference = GetReferencedType();
                if (reference is IModifiers)
                    return ((IModifiers)reference).IsStatic;
                if (reference is Type)
                    return TypeUtil.IsStatic((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a user-defined class (excludes 'object' and 'string').
        /// </summary>
        public bool IsUserClass
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsClass;
                if (reference is Type)
                    return TypeUtil.IsUserClass((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a user-defined struct (excludes primitive types including 'void' and 'decimal', and enums).
        /// </summary>
        public bool IsUserStruct
        {
            get
            {
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsStruct;
                if (reference is Type)
                    return TypeUtil.IsUserStruct((Type)reference);
                return false;
            }
        }

        /// <summary>
        /// True if the referenced type is a value type.
        /// </summary>
        public virtual bool IsValueType
        {
            get
            {
                // Note that we also treat ValueType as a value type, even though IsValueType is false for this
                // system type, since it's actually a class (even though it's the base class of all value types).
                if (HasArrayRanks) return false;
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    return ((ITypeDecl)reference).IsValueType;
                if (reference is Type)
                {
                    Type type = (Type)reference;
                    return (type.IsValueType || (type.Name == "ValueType" && type.Namespace == "System"));
                }
                return false;
            }
        }

        /// <summary>
        /// The name of the <see cref="TypeRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is ITypeDecl)
                    return ((ITypeDecl)_reference).Name;
                if (_reference is Type)
                {
                    Type type = (Type)_reference;
                    return (type.IsGenericType ? TypeUtil.NonGenericName(type) : type.Name);
                }
                if (_reference is EnumConstant)  // Enum constant
                    return ((EnumConstant)_reference).EnumTypeRef.Name;
                return (_reference != null ? _reference.GetType().Name : null);  // Constant
            }
        }

        /// <summary>
        /// The associated <see cref="Namespace"/> name.
        /// </summary>
        public virtual string NamespaceName
        {
            get
            {
                if (_reference is ITypeDecl)
                {
                    Namespace @namespace = ((ITypeDecl)_reference).GetNamespace();
                    return (@namespace != null ? @namespace.FullName : null);
                }
                if (_reference is Type)
                    return ((Type)_reference).Namespace;
                if (_reference is EnumConstant)
                    return ((EnumConstant)_reference).EnumTypeRef.NamespaceName;
                return _reference.GetType().Namespace;
            }
        }

        /// <summary>
        /// Change the type of the specified constant to the specified type if possible.
        /// </summary>
        public static object ChangeTypeOfConstant(object constantValue, TypeRefBase typeRefBase)
        {
            if (constantValue != null && typeRefBase != null)
            {
                bool isNullableType = false;
                if (typeRefBase.IsNullableType)
                {
                    isNullableType = true;
                    typeRefBase = typeRefBase.TypeArguments[0].SkipPrefixes() as TypeRefBase;
                }
                object reference = (typeRefBase != null ? typeRefBase.Reference : null);

                Type constantType = reference as Type;
                if (isNullableType)
                    constantType = typeof(Nullable<>).MakeGenericType(constantType);
                if (constantType != null && constantValue.GetType() != constantType)
                    constantValue = TypeUtil.ChangeType(constantValue, constantType);
            }
            return constantValue;
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for efficiency.
        /// </summary>
        public static TypeRef Create(Type type, bool isFirstOnLine, ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            if (type == null)
                return null;

            // If the type is an array, and no array ranks were specified, use the existing array ranks (otherwise override them)
            if (type.IsArray)
            {
                bool useExistingRanks = (arrayRanks == null);
                if (useExistingRanks)
                    arrayRanks = new List<int>();
                do
                {
                    if (useExistingRanks)
                        arrayRanks.Add(type.GetArrayRank());
                    type = type.GetElementType();
                }
                while (type.IsArray);
            }

            // Dereference (remove the trailing '&') if it's a reference type
            if (type.IsByRef)
                type = type.GetElementType();

            // Create a reference of the appropriate type
            if (type.IsGenericParameter)
                return new TypeParameterRef(type, isFirstOnLine, arrayRanks);  // Ignore any type arguments in this case
            if (type.IsGenericType)
            {
                // Use any type arguments in the type if none were specified
                if (typeArguments == null)
                    typeArguments = DefaultTypeArguments(type, null);
            }
            return new TypeRef(type, isFirstOnLine, typeArguments, arrayRanks);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for efficiency.
        /// </summary>
        public static TypeRef Create(Type type, bool isFirstOnLine, ChildList<Expression> typeArguments)
        {
            return Create(type, isFirstOnLine, typeArguments, null);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for efficiency.
        /// </summary>
        public static TypeRef Create(Type type, bool isFirstOnLine)
        {
            return Create(type, isFirstOnLine, (ChildList<Expression>)null, null);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for efficiency.
        /// </summary>
        public static TypeRef Create(Type type)
        {
            return Create(type, false, (ChildList<Expression>)null, null);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, ChildList<Expression> typeArguments, List<int> arrayRanks)
        {
            return Create(type, false, typeArguments, arrayRanks);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, ChildList<Expression> typeArguments)
        {
            return Create(type, false, typeArguments, null);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, bool isFirstOnLine, params Expression[] typeArguments)
        {
            // No need to check 'typeArguments' for null, because a different constructor would have been called.
            // Convert the array of expressions to a ChildList so we can use the factory method above.
            return Create(type, isFirstOnLine, new ChildList<Expression>(typeArguments));
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, params Expression[] typeArguments)
        {
            // No need to check 'typeArguments' for null, because a different constructor would have been called
            return Create(type, false, typeArguments);
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, bool isFirstOnLine, params int[] arrayRanks)
        {
            // Convert the array of ranks to a List so we can use the factory method above.
            return Create(type, isFirstOnLine, null, new List<int>(arrayRanks));
        }

        /// <summary>
        /// Construct a <see cref="TypeRef"/> from a <see cref="Type"/>.
        /// Use this factory method if the Type might be a generic parameter (requiring an <see cref="OpenTypeParameterRef"/>/<see cref="TypeParameterRef"/>) or
        /// an array of one of those, otherwise you may use a 'new TypeRef()' constructor for
        /// efficiency.
        /// </summary>
        public static TypeRef Create(Type type, params int[] arrayRanks)
        {
            return Create(type, false, arrayRanks);
        }

        /// <summary>
        /// Create a type expression for the specified <see cref="Type"/>, handling nested types.
        /// </summary>
        public static Expression CreateNested(Type type)
        {
            if (type.IsNested)
                return new Dot(CreateNested(type.DeclaringType), Create(type));
            return Create(type);
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> to a nullable version of the specified type expression.
        /// </summary>
        public static TypeRef CreateNullable(Expression typeExpression, bool isFirstOnLine, List<int> arrayRanks)
        {
            object nullableRef = Nullable1Ref.Reference;
            return new TypeRef((Type)nullableRef, isFirstOnLine, typeExpression) { ArrayRanks = arrayRanks };
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> to a nullable version of the specified type expression.
        /// </summary>
        public static TypeRef CreateNullable(Expression typeExpression, List<int> arrayRanks)
        {
            return CreateNullable(typeExpression, false, arrayRanks);
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> to a nullable version of the specified type expression.
        /// </summary>
        public static TypeRef CreateNullable(Expression typeExpression, bool isFirstOnLine)
        {
            return CreateNullable(typeExpression, isFirstOnLine, null);
        }

        /// <summary>
        /// Create a <see cref="TypeRef"/> to a nullable version of the specified type expression.
        /// </summary>
        public static TypeRef CreateNullable(Expression typeExpression)
        {
            return CreateNullable(typeExpression, false, null);
        }

        /// <summary>
        /// Find a type in the specified <see cref="Namespace"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Namespace @namespace, string typeName, bool isFirstOnLine)
        {
            return CreateTypeRef(@namespace.Find(typeName), typeName, isFirstOnLine);
        }

        /// <summary>
        /// Find a type in the specified <see cref="Namespace"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Namespace @namespace, string typeName)
        {
            return CreateTypeRef(@namespace.Find(typeName), typeName, false);
        }

        /// <summary>
        /// Find a type in the specified type or namespace <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/>, <see cref="TypeRef"/>, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias alias, string typeName, bool isFirstOnLine)
        {
            // We need to handle Namespace and Alias separately instead of using INamespace in order
            // to avoid ambiguity between INamespace and ITypeDecl when an Alias is passed in.
            return CreateTypeRef(alias.Find(typeName), typeName, isFirstOnLine);
        }

        /// <summary>
        /// Find a type in the specified type or namespace <see cref="Alias"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="NamespaceRef"/>, <see cref="TypeRef"/>, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Alias alias, string typeName)
        {
            // We need to handle Namespace and Alias separately instead of using INamespace in order
            // to avoid ambiguity between INamespace and ITypeDecl when an Alias is passed in.
            return CreateTypeRef(alias.Find(typeName), typeName, false);
        }

        /// <summary>
        /// Find a type in the specified <see cref="NamespaceRef"/> or <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(SymbolicRef symbolicRef, string typeName, bool isFirstOnLine)
        {
            if (symbolicRef is NamespaceRef)
                return CreateTypeRef(((NamespaceRef)symbolicRef).Namespace.Find(typeName), typeName, isFirstOnLine);
            if (symbolicRef is TypeRef)
            {
                TypeRef typeRef = ((TypeRef)symbolicRef).GetNestedType(typeName);
                if (typeRef != null)
                {
                    typeRef.IsFirstOnLine = isFirstOnLine;
                    return typeRef;
                }
            }
            return new UnresolvedRef(typeName, isFirstOnLine);
        }

        /// <summary>
        /// Find a type in the specified <see cref="NamespaceRef"/> or <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(SymbolicRef symbolicRef, string typeName)
        {
            return Find(symbolicRef, typeName, false);
        }

        /// <summary>
        /// Find a nested type in the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, string name, bool isFirstOnLine)
        {
            if (typeDecl != null)
            {
                TypeRef typeRef = typeDecl.GetNestedType(name);
                if (typeRef != null)
                {
                    typeRef.IsFirstOnLine = isFirstOnLine;
                    return typeRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a nested type in the specified <see cref="TypeDecl"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(TypeDecl typeDecl, string name)
        {
            return Find(typeDecl, name, false);
        }

        /// <summary>
        /// Find a nested type in the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, string name, bool isFirstOnLine)
        {
            if (type != null)
            {
                type = type.GetNestedType(name);
                if (type != null)
                    return new TypeRef(type, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find a nested type in the specified <see cref="Type"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="TypeRef"/> to the type, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static TypeRefBase Find(Type type, string name)
        {
            return Find(type, name, false);
        }

        /// <summary>
        /// Find or create a <see cref="TypeRef"/> that represents the appropriate <see cref="Type"/>
        /// that has the same FullName as the specified <see cref="Type"/>.
        /// </summary>
        public static TypeRef FindTypeRef(Type type)
        {
            TypeRef typeRef;

            // When using reflection, the 'loaded' mscorlib will always be the same as that used by the currently
            // running app, so we can just initialize using the 'typeof' type.
            // Once a TypeRef is created for a particular Type, always re-use it.
            if (TypeToTypeRefMap.TryGetValue(type, out typeRef))
                return typeRef;
            typeRef = new TypeRef(type);
            TypeToTypeRefMap.Add(type, typeRef);
            return typeRef;
        }

        /// <summary>
        /// Find a common type (using implicit conversions) that can represent both specified types.
        /// </summary>
        public static TypeRefBase GetCommonType(TypeRefBase typeRefBase1, TypeRefBase typeRefBase2)
        {
            // Handle either side being null
            if (typeRefBase1 == null)
                return ObjectRef;
            if (typeRefBase2 == null)
                return ObjectRef;

            // Check for equality (the types are the same)
            if (typeRefBase1.IsSameRef(typeRefBase2))
                return (!typeRefBase1.IsConst ? typeRefBase1 : (!typeRefBase2.IsConst ? typeRefBase2 : typeRefBase1.GetTypeWithoutConstant()));

            // Handle either side being a MethodRef or UnresolvedRef
            TypeRef typeRef1 = typeRefBase1 as TypeRef;
            TypeRef typeRef2 = typeRefBase2 as TypeRef;
            if (typeRef1 == null)
                return ObjectRef;
            if (typeRef2 == null)
                return ObjectRef;

            // Check for array ranks, making sure they're the same
            bool hasArrayRanks = typeRef1.HasArrayRanks;
            if (hasArrayRanks)
            {
                // If the ranks don't match, then the common type is 'object'
                if (!typeRef1.HasSameArrayRanks(typeRef2))
                    return ObjectRef;
            }

            // Handle either side being a nullable type
            if (typeRef1.IsNullableType || typeRef2.IsNullableType)
            {
                // Get the common type of the underlying value types, and make a nullable type of it (if it's not 'object')
                TypeRefBase valueType1 = (typeRef1.IsNullableType ? typeRef1.TypeArguments[0].SkipPrefixes() as TypeRefBase : typeRef1);
                TypeRefBase valueType2 = (typeRef2.IsNullableType ? typeRef2.TypeArguments[0].SkipPrefixes() as TypeRefBase : typeRef2);
                TypeRefBase commonValueType = GetCommonType(valueType1, valueType2);
                return (commonValueType.IsSameRef(ObjectRef) ? commonValueType : CreateNullable(commonValueType));
            }

            TypeRefBase commonType;
            TypeCode typeCode1 = typeRef1.GetTypeCode();
            TypeCode typeCode2 = typeRef2.GetTypeCode();

            // Handle reference types
            if (typeCode1 == TypeCode.Object || typeCode2 == TypeCode.Object)
            {
                // Handle subclasses
                if (typeRef2.IsSubclassOf(typeRef1))
                    commonType = (!typeRef1.IsConst ? typeRef1 : typeRef1.GetTypeWithoutConstant());
                else if (typeRef1.IsSubclassOf(typeRef2))
                    commonType = (!typeRef2.IsConst ? typeRef2 : typeRef2.GetTypeWithoutConstant());
                else
                    commonType = ObjectRef;
            }
            else
            {
                // Handle primitive types
                if (typeCode1 >= TypeCode.Char && typeCode1 <= TypeCode.Decimal
                    && typeCode2 >= TypeCode.Char && typeCode2 <= TypeCode.Decimal)
                {
                    // Special handling for constants:

                    // If one side is type 'ulong' and the other is a constant of type 'sbyte', 'short', 'int', or 'long', then
                    // the common type is 'ulong' instead of none (ObjectRef) if the constant is positive.
                    if ((typeCode1 == TypeCode.UInt64 && typeRef2.IsConst
                        && ((typeCode2 == TypeCode.SByte && (sbyte)typeRef2.GetConstantValue() >= 0) || (typeCode2 == TypeCode.Int16 && (short)typeRef2.GetConstantValue() >= 0)
                            || (typeCode2 == TypeCode.Int32 && (int)typeRef2.GetConstantValue() >= 0) || (typeCode2 == TypeCode.Int64 && (long)typeRef2.GetConstantValue() >= 0)))
                        || (typeCode2 == TypeCode.UInt64 && typeRef1.IsConst
                            && ((typeCode1 == TypeCode.SByte && (sbyte)typeRef1.GetConstantValue() >= 0) || (typeCode1 == TypeCode.Int16 && (short)typeRef1.GetConstantValue() >= 0)
                                || (typeCode1 == TypeCode.Int32 && (int)typeRef1.GetConstantValue() >= 0) || (typeCode1 == TypeCode.Int64 && (long)typeRef1.GetConstantValue() >= 0))))
                        commonType = ULongRef;
                    // If one side is type 'uint' and the other is a constant of type 'sbyte', 'short', or 'int', then
                    // the common type is 'uint' instead of 'long' if the constant is positive.
                    else if ((typeCode1 == TypeCode.UInt32 && typeRef2.IsConst
                        && ((typeCode2 == TypeCode.SByte && (sbyte)typeRef2.GetConstantValue() >= 0) || (typeCode2 == TypeCode.Int16 && (short)typeRef2.GetConstantValue() >= 0)
                            || (typeCode2 == TypeCode.Int32 && (int)typeRef2.GetConstantValue() >= 0)))
                        || (typeCode2 == TypeCode.UInt32 && typeRef1.IsConst
                            && ((typeCode1 == TypeCode.SByte && (sbyte)typeRef1.GetConstantValue() >= 0) || (typeCode1 == TypeCode.Int16 && (short)typeRef1.GetConstantValue() >= 0)
                                || (typeCode1 == TypeCode.Int32 && (int)typeRef1.GetConstantValue() >= 0))))
                        commonType = UIntRef;
                    // Handle implicit numeric type conversions
                    else
                        commonType = CommonTypeRefMap[typeCode1 - TypeCode.Char, typeCode2 - TypeCode.Char];
                }
                else
                    commonType = ObjectRef;
            }

            // If the type had array ranks, clone the result and add them on
            if (hasArrayRanks)
            {
                commonType = (TypeRef)commonType.Clone();
                commonType.ArrayRanks = new List<int>(typeRef1.ArrayRanks);
            }

            return commonType;
        }

        /// <summary>
        /// Get all (non-static) constructors for the specified code object.
        /// </summary>
        public static NamedCodeObjectGroup GetConstructors(object obj, bool currentPartOnly)
        {
            if (obj is ITypeDecl)
                return ((ITypeDecl)obj).GetConstructors(currentPartOnly);
            if (obj is Type)
            {
                Type type = (Type)obj;
                if (TypeUtil.IsDelegateType(type))
                {
                    // Delegates have a constructor that takes an object and an IntPtr that is used internally
                    // by the compiler during code generation.  We have to create a dummy constructor that will
                    // allow a MethodRef to be passed to it, in order to make the C# syntax work.  The Parent
                    // can't be set to the Type, but the Type can be acquired from the parameter's type.
                    ConstructorDecl constructorDecl =
                        new ConstructorDecl(new[] { new ParameterDecl(DelegateDecl.DelegateConstructorParameterName, new TypeRef(type)) }) { IsGenerated = true, Name = TypeUtil.NonGenericName(type) };
                    return new NamedCodeObjectGroup(constructorDecl);
                }
                // Find both public and protected instance constructors
                return new NamedCodeObjectGroup(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            }
            return null;
        }

        /// <summary>
        /// Get all (non-static) constructors for the specified code object.
        /// </summary>
        public static NamedCodeObjectGroup GetConstructors(object obj)
        {
            return GetConstructors(obj, false);
        }

        /// <summary>
        /// Initialize static <see cref="TypeRef"/>s to the standard mscorlib types.
        /// </summary>
        public static void InitializeTypeRefs()
        {
            TypeToTypeRefMap.Clear();

            ObjectRef = FindTypeRef(typeof(object));
            VoidRef = FindTypeRef(typeof(void));
            SByteRef = FindTypeRef(typeof(sbyte));
            ByteRef = FindTypeRef(typeof(byte));
            ShortRef = FindTypeRef(typeof(short));
            UShortRef = FindTypeRef(typeof(ushort));
            IntRef = FindTypeRef(typeof(int));
            UIntRef = FindTypeRef(typeof(uint));
            LongRef = FindTypeRef(typeof(long));
            ULongRef = FindTypeRef(typeof(ulong));
            CharRef = FindTypeRef(typeof(char));
            BoolRef = FindTypeRef(typeof(bool));
            StringRef = FindTypeRef(typeof(string));
            FloatRef = FindTypeRef(typeof(float));
            DoubleRef = FindTypeRef(typeof(double));
            DecimalRef = FindTypeRef(typeof(decimal));

            TypeTypeRef = FindTypeRef(typeof(Type));
            ArrayRef = FindTypeRef(typeof(Array));
            EnumRef = FindTypeRef(typeof(Enum));
            ValueTypeRef = FindTypeRef(typeof(ValueType));
            Nullable1Ref = FindTypeRef(typeof(Nullable<>));

            Dictionary2Ref = FindTypeRef(typeof(Dictionary<,>));
            IEnumerableRef = FindTypeRef(typeof(IEnumerable));
            IEnumerable1Ref = FindTypeRef(typeof(IEnumerable<>));
            ICollectionRef = FindTypeRef(typeof(ICollection));
            ICollection1Ref = FindTypeRef(typeof(ICollection<>));
            IListRef = FindTypeRef(typeof(IList));
            IList1Ref = FindTypeRef(typeof(IList<>));
            ICloneableRef = FindTypeRef(typeof(ICloneable));
            ISerializableRef = FindTypeRef(typeof(ISerializable));

            FlagsAttributeRef = FindTypeRef(typeof(FlagsAttribute));
            DelegateRef = FindTypeRef(typeof(Delegate));
            MulticastDelegateRef = FindTypeRef(typeof(MulticastDelegate));

            AsyncCallbackRef = FindTypeRef(typeof(AsyncCallback));
            IAsyncResultRef = FindTypeRef(typeof(IAsyncResult));
            TypeUtilTRef = FindTypeRef(typeof(T));

            // Initialize the keyword-to-type map.
            KeywordToTypeRefMap = new Dictionary<string, TypeRef>(16)
            {
                { "object",  ObjectRef  },
                { "void",    VoidRef    },
                { "sbyte",   SByteRef   },
                { "byte",    ByteRef    },
                { "short",   ShortRef   },
                { "ushort",  UShortRef  },
                { "int",     IntRef     },
                { "uint",    UIntRef    },
                { "long",    LongRef    },
                { "ulong",   ULongRef   },
                { "char",    CharRef    },
                { "bool",    BoolRef    },
                { "string",  StringRef  },
                { "float",   FloatRef   },
                { "double",  DoubleRef  },
                { "decimal", DecimalRef }
            };

            // Initialize the common-type map.  The TypeCode enum looks like this:
            // 0-Empty, 1-Object, 2-DBNull, 3-Boolean, 4-Char, 5-SByte, 6-Byte, 7-Int16, 8-UInt16, 9-Int32,
            // 10-UInt32, 11-Int64, 12-UInt64, 13-Single, 14-Double, 15-Decimal, 16-DateTime, 17-String
            // We will 'cheat' a bit here and use knowledge of this ordering to create a mapping table
            // for just 4-Char through 14-Double for common type calculations.
            CommonTypeRefMap = new[,]
                {  // char        sbyte       byte        short       ushort      int         uint        long        ulong       float      double     decimal
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     UIntRef,    LongRef,    ULongRef,   FloatRef,  DoubleRef, DecimalRef },  // char
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     LongRef,    LongRef,    ObjectRef,  FloatRef,  DoubleRef, DecimalRef },  // sbyte
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     UIntRef,    LongRef,    ULongRef,   FloatRef,  DoubleRef, DecimalRef },  // byte
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     LongRef,    LongRef,    ObjectRef,  FloatRef,  DoubleRef, DecimalRef },  // short
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     UIntRef,    LongRef,    ULongRef,   FloatRef,  DoubleRef, DecimalRef },  // ushort
                    { IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     IntRef,     LongRef,    LongRef,    ObjectRef,  FloatRef,  DoubleRef, DecimalRef },  // int
                    { UIntRef,    LongRef,    UIntRef,    LongRef,    UIntRef,    LongRef,    UIntRef,    LongRef,    ULongRef,   FloatRef,  DoubleRef, DecimalRef },  // uint
                    { LongRef,    LongRef,    LongRef,    LongRef,    LongRef,    LongRef,    LongRef,    LongRef,    ObjectRef,  FloatRef,  DoubleRef, DecimalRef },  // long
                    { ULongRef,   ObjectRef,  ULongRef,   ObjectRef,  ULongRef,   ObjectRef,  ULongRef,   ObjectRef,  ULongRef,   FloatRef,  DoubleRef, DecimalRef },  // ulong
                    { FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,   FloatRef,  DoubleRef, ObjectRef  },  // float
                    { DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef,  DoubleRef, DoubleRef, ObjectRef  },  // double
                    { DecimalRef, DecimalRef, DecimalRef, DecimalRef, DecimalRef, DecimalRef, DecimalRef, DecimalRef, DecimalRef, ObjectRef, ObjectRef, DecimalRef }   // decimal
                };
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            AsTextType(writer, _typeArguments, flags);
        }

        public void AsTextType(CodeWriter writer, List<Expression> typeArguments, RenderFlags flags)
        {
            // If it's a nested type, and we have no dot prefix, and the ShowParentTypes flag is set, then
            // render all parent types with appropriate type arguments (this shouldn't occur in display of
            // code, but only when displaying an evaluated type reference, such as in a tooltip).
            // Nova will include all type arguments for any parent types in a reference to a nested type
            // (as .NET Reflection also does).  Such parent type arguments are ignored for display purposes
            // if the TypeRef is displayed in code that includes parent type prefixes or is within the scope
            // of the parent generic type.
            // Also do this for GenericParameters (which are treated as 'nested' by reflection).
            if (IsNested || IsGenericParameter)
            {
                TypeRefBase typeRefBase = GetDeclaringType();
                if (typeRefBase != null)
                {
                    // Recursively render the parent type minus the type arguments that belong to the current type
                    List<Expression> parentTypeArguments = null;
                    if (typeArguments != null)
                    {
                        int localCount = GetLocalTypeArgumentCount();
                        int parentCount = typeArguments.Count - localCount;
                        if (parentCount > 0)
                        {
                            if (localCount == 0)
                            {
                                parentTypeArguments = typeArguments;
                                typeArguments = null;
                            }
                            else
                            {
                                parentTypeArguments = typeArguments.GetRange(0, parentCount);
                                typeArguments = typeArguments.GetRange(parentCount, localCount);
                            }
                        }
                    }

                    // We must always extract the parent type arguments, but only render the parent types if appropriate
                    if (!flags.HasFlag(RenderFlags.HasDotPrefix) && flags.HasFlag(RenderFlags.ShowParentTypes) && typeRefBase is TypeRef)
                    {
                        // If we're about to render an enclosing type without type parameters, then use the declared
                        // ones by default.  This is necessary for nested Enums, since they are never considered to
                        // be generic types, and won't have any type arguments even if nested inside generic types.
                        // Since an Enum can't be instantiated, the declared type arguments are always appropriate.
                        if (parentTypeArguments == null || parentTypeArguments.Count == 0)
                            parentTypeArguments = ((TypeRef)typeRefBase).GetTypeParametersAsArguments();

                        ((TypeRef)typeRefBase).AsTextType(writer, parentTypeArguments, flags);
                        writer.Write(Dot.ParseToken);
                        flags |= RenderFlags.HasDotPrefix;
                    }
                }
            }

            object reference = GetReferencedType();
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            if (reference is Type)
            {
                // Handle references to Types
                Type type = (Type)reference;

                // If we have array ranks, or were requested to suppress them (by NewArray for jagged
                // arrays), then suppress them in the Type rendering by getting the innermost type.
                if (HasArrayRanks || flags.HasFlag(RenderFlags.SuppressBrackets))
                {
                    while (type.IsArray)
                        type = type.GetElementType();
                }

                // If we have type arguments, override any in the Type itself
                if (typeArguments != null)
                {
                    // Render "Nullable<Type>" as "Type?" if optimizations is on *and* there's no "System." prefix
                    if (IsNullableType && !flags.HasFlag(RenderFlags.HasDotPrefix))
                    {
                        // Render the Nullable type argument followed by '?'
                        typeArguments[0].AsText(writer, passFlags & ~RenderFlags.ShowParentTypes);
                        writer.Write(ParseTokenNullable);
                    }
                    else
                    {
                        AsTextType(writer, type, passFlags | RenderFlags.SuppressTypeArgs);
                        AsTextTypeArguments(writer, typeArguments, flags);
                    }
                }
                else
                {
                    // If the TypeRef has no type arguments, we still want to suppress any on the
                    // type itself - this is valid for bad (incomplete) code.
                    AsTextType(writer, type, passFlags | RenderFlags.SuppressTypeArgs);
                }
            }
            else
            {
                // Handle references to TypeDecls
                writer.WriteName(Name, flags, true);
                AsTextTypeArguments(writer, typeArguments, flags);
            }

            // Render any array ranks last
            AsTextArrayRanks(writer, passFlags);
        }

        /// <summary>
        /// Get the base type of the referenced type.
        /// </summary>
        public TypeRefBase GetBaseType()
        {
            if (HasArrayRanks) return ArrayRef;
            TypeRefBase baseTypeRef = null;
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                baseTypeRef = ((ITypeDecl)reference).GetBaseType();
            else if (reference is Type)
                baseTypeRef = Create(((Type)reference).BaseType);
            return baseTypeRef;
        }

        /// <summary>
        /// Get the value of any represented constant.  For enums, an <see cref="EnumConstant"/> object will be
        /// returned, which has both the Enum type and a constant value of its underlying type.
        /// </summary>
        public override object GetConstantValue()
        {
            if (IsConst)
            {
                if (_reference is ITypeDecl || _reference is Type)
                    return null;
                return _reference;
            }
            return null;
        }

        /// <summary>
        /// Get the non-static constructor for the referenced type.
        /// </summary>
        public ConstructorRef GetConstructor(params TypeRefBase[] parameterTypes)
        {
            if (HasArrayRanks)
                return ArrayRef.GetConstructor(parameterTypes);
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetConstructor(parameterTypes);
            if (reference is Type)
            {
                Type[] types = GetTypeRefsAsTypes(parameterTypes);
                if (types != null)
                {
                    ConstructorInfo constructorInfo = ((Type)reference).GetConstructor(types);
                    if (constructorInfo != null)
                        return new ConstructorRef(constructorInfo);
                }
            }
            return null;
        }

        /// <summary>
        /// Get all (non-static) constructors for the referenced type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors(bool currentPartOnly)
        {
            return GetConstructors(Reference, currentPartOnly);
        }

        /// <summary>
        /// Get all (non-static) constructors for the referenced type.
        /// </summary>
        public NamedCodeObjectGroup GetConstructors()
        {
            return GetConstructors(Reference, false);
        }

        /// <summary>
        /// Get the declaring type.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
            {
                CodeObject parent = ((ITypeDecl)reference).Parent;
                return (parent is TypeDecl ? ((TypeDecl)parent).CreateRef() : null);
            }
            if (reference is Type)
            {
                Type declaringType = ((Type)reference).DeclaringType;
                return (declaringType != null ? new TypeRef(declaringType) : null);
            }
            return null;
        }

        /// <summary>
        /// Get the delegate parameters if the expression evaluates to a delegate type.
        /// </summary>
        public override ICollection GetDelegateParameters()
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetDelegateParameters();
            if (reference is Type)
                return TypeUtil.GetDelegateParameters((Type)reference);
            // It would be nice to replace any type parameters in the types of the parameters with type
            // arguments if possible as in GetDelegateReturnType() below, but this is not feasible since
            // we can't modify the types of the parameter objects, and we need the parameter objects in
            // order to check ref/out flags.  The only way to do this would be to allocate new collections
            // of fake ParameterDecl objects with evaluated types, but that gets really messy.  Instead,
            // callers of this method have to worry about evaluating any type parameters.
            return null;
        }

        /// <summary>
        /// Get the delegate return type if the expression evaluates to a delegate type.
        /// </summary>
        public override TypeRefBase GetDelegateReturnType()
        {
            TypeRefBase returnTypeRef = null;
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                returnTypeRef = ((ITypeDecl)reference).GetDelegateReturnType();
            else if (reference is Type)
            {
                Type type = (Type)reference;
                if (TypeUtil.IsDelegateType(type))
                {
                    MethodInfo delegateInvokeMethodInfo = TypeUtil.GetInvokeMethod(type);
                    if (delegateInvokeMethodInfo != null)
                    {
                        Type returnType = delegateInvokeMethodInfo.ReturnType;
                        returnTypeRef = Create(returnType);
                    }
                }
            }
            return returnTypeRef;
        }

        /// <summary>
        /// Get the field with the specified name.
        /// </summary>
        public FieldRef GetField(string name)
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetField(name);
            if (reference is Type)
            {
                FieldInfo fieldInfo = ((Type)reference).GetField(name);
                if (fieldInfo != null)
                    return new FieldRef(fieldInfo);
            }
            return null;
        }

        /// <summary>
        /// Get the full name of the object, including the namespace name.
        /// </summary>
        public override string GetFullName()
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetFullName();
            if (reference is Type)
                return ((Type)reference).Namespace + "." + ((Type)reference).Name;  // FullName might return null!
            return null;
        }

        /// <summary>
        /// Get all interfaces directly implemented by the type.
        /// </summary>
        public List<TypeRef> GetInterfaces(bool includeBaseInterfaces)
        {
            List<TypeRef> interfaces = new List<TypeRef>();
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
            {
                // Only look at type declarations that can implement interfaces.
                if (reference is BaseListTypeDecl && !(reference is EnumDecl))
                {
                    BaseListTypeDecl baseListTypeDecl = (BaseListTypeDecl)reference;
                    List<Expression> baseTypes = baseListTypeDecl.GetAllBaseTypes();
                    if (baseTypes != null)
                    {
                        // Check all interfaces implemented directly by the type declaration
                        foreach (Expression baseTypeExpression in baseTypes)
                        {
                            TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                            if (baseTypeRef != null)
                            {
                                if (baseTypeRef.IsInterface)
                                    interfaces.Add(baseTypeRef);
                                if (includeBaseInterfaces)
                                    interfaces.AddRange(baseTypeRef.GetInterfaces(true));
                            }
                        }
                    }
                }
            }
            else if (reference is Type)
            {
                Type[] allInterfaces = ((Type)reference).GetInterfaces();
                if (!includeBaseInterfaces)
                {
                    // Stupid reflection doesn't provide a way to get just those interfaces implemented directly
                    // by the current type, so the best we can do is get all interfaces, then subtract those
                    // implemented by base types (even though this can produce incorrect results if the same
                    // interface is implemented at more than one level.
                    Type baseType = ((Type)reference).BaseType;
                    if (baseType != null && baseType != typeof(object))
                    {
                        Type[] baseInterfaces = baseType.GetInterfaces();
                        if (baseInterfaces.Length > 0)
                            allInterfaces = Array.FindAll(allInterfaces, delegate (Type i) { return !ArrayUtil.Contains(baseInterfaces, i); });
                    }
                }
                foreach (Type @interface in allInterfaces)
                {
                    TypeRef interfaceRef = Create(@interface);
                    interfaces.Add(interfaceRef);
                }
            }
            return interfaces;
        }

        /// <summary>
        /// Get all interfaces directly implemented by the type.
        /// </summary>
        public List<TypeRef> GetInterfaces()
        {
            return GetInterfaces(false);
        }

        /// <summary>
        /// Calculate a hash code for the referenced object which is the same for all references where IsSameRef() is true.
        /// </summary>
        /// <remarks>
        /// We don't want to override GetHashCode(), because we want all TypeRefs to have unique hashes so they can be
        /// used as dictionary keys.  However, we also sometimes want hashes to be the same if IsSameRef() is true - this
        /// method allows for that.
        /// </remarks>
        public override int GetIsSameRefHashCode()
        {
            // Make the hash codes as unique as possible while still ensuring that they are identical
            // for any objects for which IsSameRef() returns true.
            int hashCode = Name.GetHashCode();
            string namespaceName = NamespaceName;
            if (namespaceName != null) hashCode ^= namespaceName.GetHashCode();
            List<int> arrayRanks = ArrayRanks;
            if (arrayRanks != null)
            {
                foreach (int rank in arrayRanks)
                    hashCode = (hashCode << 1) ^ rank;
            }
            ChildList<Expression> typeArguments = TypeArguments;
            if (typeArguments != null)
            {
                foreach (Expression typeArgument in typeArguments)
                {
                    if (typeArgument != null)
                    {
                        TypeRefBase typeRefBase = typeArgument.SkipPrefixes() as TypeRefBase;
                        if (typeRefBase != null)
                            hashCode ^= typeRefBase.GetIsSameRefHashCode();
                    }
                }
            }
            return hashCode;
        }

        /// <summary>
        /// Get the number of specified type arguments.
        /// </summary>
        public int GetLocalTypeArgumentCount()
        {
            int count = 0;
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                count = ((ITypeDecl)reference).TypeParameterCount;
            else if (reference is Type)
                count = TypeUtil.GetLocalGenericArgumentCount((Type)reference);
            return count;
        }

        /// <summary>
        /// Get the method with the specified name, binding flags, and parameter types.
        /// </summary>
        public MethodRef GetMethod(string name, BindingFlags bindingFlags, params TypeRefBase[] parameterTypes)
        {
            if (HasArrayRanks)
                return ArrayRef.GetMethod(name, bindingFlags, parameterTypes);
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetMethod(name, parameterTypes);
            if (reference is Type)
            {
                Type[] types = GetTypeRefsAsTypes(parameterTypes);
                if (types != null)
                {
                    MethodInfo methodInfo = TypeUtil.GetMethod((Type)reference, name, bindingFlags, types);
                    if (methodInfo != null)
                        return new MethodRef(methodInfo);
                }
            }
            return null;
        }

        /// <summary>
        /// Get the method with the specified name and parameter types.
        /// </summary>
        public MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
        {
            return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        public void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        {
            if (HasArrayRanks)
                ArrayRef.GetMethods(name, searchBaseClasses, results);
            else
            {
                object reference = GetReferencedType();
                if (reference is ITypeDecl)
                    ((ITypeDecl)reference).GetMethods(name, searchBaseClasses, results);
                else if (reference is Type)
                    GetMethods((Type)reference, name, searchBaseClasses, results);
            }
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="searchBaseClasses">Pass <c>false</c> to NOT search base classes.</param>
        public List<MethodRef> GetMethods(string name, bool searchBaseClasses)
        {
            NamedCodeObjectGroup results = new NamedCodeObjectGroup();
            GetMethods(name, searchBaseClasses, results);
            return MethodRef.MethodRefsFromGroup(results);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        /// <param name="name">The method name.</param>
        public List<MethodRef> GetMethods(string name)
        {
            return GetMethods(name, true);
        }

        /// <summary>
        /// Get the nested type with the specified name.
        /// </summary>
        public TypeRef GetNestedType(string name)
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetNestedType(name);
            if (reference is Type)
            {
                Type type = ((Type)reference).GetNestedType(name);
                if (type != null)
                    return new TypeRef(type);
            }
            return null;
        }

        /// <summary>
        /// Get the property with the specified name.
        /// </summary>
        public PropertyRef GetProperty(string name)
        {
            if (HasArrayRanks)
                return ArrayRef.GetProperty(name);
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
                return ((ITypeDecl)reference).GetProperty(name);
            if (reference is Type)
            {
                PropertyInfo propertyInfo = TypeUtil.GetProperty((Type)reference, name);
                if (propertyInfo != null)
                    return new PropertyRef(propertyInfo);
            }
            return null;
        }

        /// <summary>
        /// Get the actual type reference, retrieving from any constant value if necessary.
        /// </summary>
        /// <returns>The <see cref="ITypeDecl"/> (<see cref="TypeDecl"/> or <see cref="TypeParameter"/>, but NOT <see cref="Alias"/>)
        /// or <see cref="Type"/> (or null if the type is unresolved).</returns>
        public override object GetReferencedType()
        {
            object reference = Reference;
            if (IsConst)
            {
                if (reference is ITypeDecl || reference is Type)
                    return reference;
                if (reference is EnumConstant)
                    return ((EnumConstant)reference).EnumTypeRef.Reference;
                return reference.GetType();
            }
            return reference;
        }

        /// <summary>
        /// Get the <see cref="TypeCode"/> of the referenced type.
        /// </summary>
        public TypeCode GetTypeCode()
        {
            object reference = GetReferencedType();
            if (reference is ITypeDecl)
            {
                TypeCode typeCode;
                if (TypeNameToTypeCodeMap.TryGetValue(((ITypeDecl)reference).Name, out typeCode) && ((ITypeDecl)reference).GetNamespace().Name == "System")
                    return typeCode;
            }
            else if (reference is Type)
                return Type.GetTypeCode((Type)reference);
            return TypeCode.Object;
        }

        /// <summary>
        /// Get the declared type parameters (if any) of the referenced type as type arguments.
        /// </summary>
        public ChildList<Expression> GetTypeParametersAsArguments()
        {
            ChildList<Expression> typeArguments = null;
            object reference = GetReferencedType();
            if (reference is TypeDecl)
            {
                ChildList<TypeParameter> typeParameters = ((TypeDecl)reference).TypeParameters;
                if (typeParameters != null)
                {
                    typeArguments = new ChildList<Expression>(typeParameters.Count);
                    foreach (TypeParameter typeParameter in typeParameters)
                        typeArguments.Add(typeParameter.CreateRef());
                }
            }
            else if (reference is Type)
            {
                Type[] genericArguments = TypeUtil.GetLocalGenericArguments((Type)reference);
                if (genericArguments.Length > 0)
                {
                    typeArguments = new ChildList<Expression>(genericArguments.Length);
                    foreach (Type genericArgument in genericArguments)
                        typeArguments.Add(Create(genericArgument));
                }
            }
            else if (reference is Alias)
            {
                TypeRef aliasedTypeRef = ((Alias)reference).Type;
                if (aliasedTypeRef != null)
                    typeArguments = aliasedTypeRef.GetTypeParametersAsArguments();
            }
            return typeArguments;
        }

        /// <summary>
        /// Get a <see cref="TypeRef"/> for the actual type, excluding any constant values.
        /// </summary>
        public override TypeRefBase GetTypeWithoutConstant()
        {
            if (IsConst)
            {
                // For an ITypeDecl or Type, return a new TypeRef without the Const flag set
                if (_reference is ITypeDecl || _reference is Type)
                {
                    TypeRef newTypeRef = (TypeRef)Clone();
                    newTypeRef.SetFormatFlag(FormatFlags.Const, false);
                    return newTypeRef;
                }
                // For an enum or primitive-type constant, return a TypeRef to the type
                if (_reference is EnumConstant)
                    return ((EnumConstant)_reference).EnumTypeRef;
                return new TypeRef(_reference.GetType(), IsFirstOnLine);
            }
            return this;
        }

        /// <summary>
        /// Get the the underlying type if this is an enum type (otherwise returns null).
        /// </summary>
        public TypeRefBase GetUnderlyingTypeOfEnum()
        {
            if (IsEnum)
            {
                object reference = GetReferencedType();
                if (reference is EnumDecl)
                    return ((EnumDecl)reference).UnderlyingType.SkipPrefixes() as TypeRefBase;
                if (reference is Type)
                    return Create(Enum.GetUnderlyingType((Type)reference));
                if (reference is EnumConstant)
                    return ((EnumConstant)reference).EnumTypeRef.GetUnderlyingTypeOfEnum();
            }
            return null;
        }

        /// <summary>
        /// True if the current type is assignable from the specified type.
        /// </summary>
        public bool IsAssignableFrom(TypeRef typeRef)
        {
            return (typeRef != null && (IsSameRef(typeRef) || typeRef.IsSubclassOf(this) || typeRef.IsImplementationOf(this)));
        }

        /// <summary>
        /// Determines if the current <see cref="TypeRef"/> implements the specified interface <see cref="TypeRef"/>.
        /// </summary>
        public bool IsImplementationOf(TypeRef interfaceTypeRef)
        {
            if (interfaceTypeRef == null)
                return false;

            object reference = GetReferencedType();
            if (reference is ITypeDecl)
            {
                // We need to (recursively) resolve any open type arguments here, because the
                // declarations won't have access to the type parameters.
                // Only look at type declarations that can implement interfaces.
                if (reference is BaseListTypeDecl && !(reference is EnumDecl))
                {
                    BaseListTypeDecl baseListTypeDecl = (BaseListTypeDecl)reference;
                    List<Expression> baseTypes = baseListTypeDecl.GetAllBaseTypes();
                    if (baseTypes != null)
                    {
                        // Check all interfaces implemented directly by the type declaration
                        foreach (Expression baseTypeExpression in baseTypes)
                        {
                            TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                            if (baseTypeRef != null && baseTypeRef.IsInterface)
                            {
                                // Resolve any open type arguments, and compare the interface to the target
                                if (baseTypeRef.IsSameRef(interfaceTypeRef))
                                    return true;
                            }
                        }
                        // If we didn't find a match yet, search any base type and/or all interfaces for implemented interfaces
                        foreach (Expression baseTypeExpression in baseTypes)
                        {
                            TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                            if (baseTypeRef != null)
                            {
                                if (baseTypeRef.IsImplementationOf(interfaceTypeRef))
                                    return true;
                            }
                        }
                    }
                }
                return false;
            }
            if (reference is Type)
            {
                bool interfaceTypeRefIsGeneric = interfaceTypeRef.IsGenericType;

                // Search all interfaces implemented by the type or any base types
                foreach (Type @interface in ((Type)reference).GetInterfaces())
                {
                    // Make sure the generic status matches
                    if (@interface.IsGenericType == interfaceTypeRefIsGeneric)
                    {
                        // Because we might need to resolve any open type arguments, we have to convert the
                        // interface to a TypeRef before we can compare it to the target interface.  We also
                        // have to do this because 'interfaceTypeRef' might refer to an InterfaceDecl even
                        // though this reference refers to a Type.
                        TypeRef interfaceRef = Create(@interface);
                        if (interfaceRef.IsSameRef(interfaceTypeRef))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determine if the specified <see cref="TypeRefBase"/> refers to the same generic type, regardless of actual type arguments.
        /// </summary>
        public override bool IsSameGenericType(TypeRefBase typeRefBase)
        {
            TypeRef typeRef = (typeRefBase is AliasRef ? ((AliasRef)typeRefBase).Type : typeRefBase as TypeRef);
            return (typeRef != null && Name == typeRef.Name && NamespaceName == typeRef.NamespaceName
                && (_typeArguments != null ? _typeArguments.Count : 0) == typeRef.TypeArgumentCount && HasSameArrayRanks(typeRef));
        }

        /// <summary>
        /// Determine if the current reference refers to the same code object as the specified reference.
        /// </summary>
        public override bool IsSameRef(SymbolicRef symbolicRef)
        {
            // Comparing the type name and namespace is the most efficient method.
            // Just comparing the type objects wouldn't always work - such as when types are present both in an assembly
            // reference, and also as CodeDOM objects (either in the current project, or a referenced project), which can
            // occur if one or both types are in a project with assembly references instead of project references to a type
            // that is defined in the current solution, which isn't uncommon (it will occur in "master" solutions that include
            // projects that are also used in other solutions, and also if a project in the solution uses a non-supported
            // language and so is referenced by its assembly.  It would also fail in the case that both types are partial types
            // of the same type declaration (meaning two parts of an ITypeDecl in the same project).  Finally, it would fail if
            // the types are the same type from assemblies with different versions, such as from different versions of the .NET
            // framework, which can easily occur if different projects in the same solution target different framework versions.
            TypeRef typeRef = (symbolicRef is AliasRef ? ((AliasRef)symbolicRef).Type : symbolicRef as TypeRef);
            return (typeRef != null && Name == typeRef.Name && NamespaceName == typeRef.NamespaceName
                && HasSameTypeArguments(typeRef) && HasSameArrayRanks(typeRef));
        }

        /// <summary>
        /// Determines if the current <see cref="TypeRef"/> is a subclass of the specified <see cref="TypeRef"/>.
        /// </summary>
        public bool IsSubclassOf(TypeRef classTypeRef)
        {
            TypeRefBase baseTypeRef = GetBaseType();
            return (baseTypeRef is TypeRef && (baseTypeRef.IsSameRef(classTypeRef) || ((TypeRef)baseTypeRef).IsSubclassOf(classTypeRef)));
        }

        internal static new void AddParsePoints()
        {
            // Parse built-in types and '?' nullable types here in TypeRef, because they will
            // parse as resolved TypeRefs.  Generic types and arrays are parsed in UnresolvedRef,
            // because they may or may not parse as resolved.

            // Install parse-points for all built-in type names (without any scope restrictions) - this
            // will also parse built-in nullable types.
            foreach (KeyValuePair<string, TypeRef> keyValue in KeywordToTypeRefMap)
                Parser.AddParsePoint(keyValue.Key, ParseType);

            // Parse nullable types - use a parse-priority of 100 (Conditional uses 0)
            Parser.AddParsePoint(ParseTokenNullable, 100, ParseNullableType);
        }

        protected internal static TypeRefBase CreateTypeRef(object obj, string name, bool isFirstOnLine)
        {
            if (obj is TypeDecl)
                return new TypeRef((TypeDecl)obj, isFirstOnLine);
            if (obj is Type)
                return new TypeRef((Type)obj, isFirstOnLine);
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Determine if this <see cref="TypeRef"/> has the same array ranks as the specified <see cref="TypeRef"/>.
        /// </summary>
        protected bool HasSameArrayRanks(TypeRef typeRef)
        {
            int arrayRanksCount = (ArrayRanks != null ? ArrayRanks.Count : 0);
            if (arrayRanksCount != (typeRef.ArrayRanks != null ? typeRef.ArrayRanks.Count : 0))
                return false;
            if (arrayRanksCount > 0 && !CollectionUtil.CompareList(ArrayRanks, typeRef.ArrayRanks))
                return false;
            return true;
        }

        /// <summary>
        /// Determine if this <see cref="TypeRef"/> has the same type arguments as the specified <see cref="TypeRef"/>.
        /// </summary>
        protected bool HasSameTypeArguments(TypeRef typeRef)
        {
            ChildList<Expression> typeArguments1 = TypeArguments;
            ChildList<Expression> typeArguments2 = typeRef.TypeArguments;
            int typeArgumentCount = (typeArguments1 != null ? typeArguments1.Count : 0);
            if (typeArgumentCount != (typeArguments2 != null ? typeArguments2.Count : 0))
                return false;
            if (typeArgumentCount > 0)
            {
                for (int i = 0; i < typeArgumentCount; ++i)
                {
                    Expression typeArg1 = typeArguments1[i];
                    Expression typeArg2 = typeArguments2[i];
                    if (typeArg1 == null && typeArg2 == null)
                        continue;
                    if (typeArg1 == null || typeArg2 == null)
                        return false;
                    TypeRefBase typeArgRef1 = typeArg1.SkipPrefixes() as TypeRefBase;
                    TypeRefBase typeArgRef2 = typeArg2.SkipPrefixes() as TypeRefBase;
                    if (typeArgRef1 == null || typeArgRef2 == null)
                        return false;
                    if (typeArgRef1.IsSameRef(typeArgRef2))
                        continue;
                    return false;
                }
            }
            return true;
        }

        private static void GetMethods(Type type, string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        {
            try
            {
                // Get all methods with the specified name
                MemberInfo[] members = type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
                    | (searchBaseClasses ? BindingFlags.FlattenHierarchy : BindingFlags.DeclaredOnly));
                foreach (MemberInfo memberInfo in members)
                {
                    if (memberInfo is MethodBase)
                        results.Add(memberInfo);
                }

                // If we're searching an interface, we have to manually search base interfaces
                if (type.IsInterface)
                {
                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        members = interfaceType.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        foreach (MemberInfo memberInfo in members)
                        {
                            if (memberInfo is MethodBase)
                                results.Add(memberInfo);
                        }
                    }
                }
            }
            catch
            {
                // Ignore any exceptions - can be caused by missing dependent assemblies
            }
        }

        /// <summary>
        /// Convert a TypeRefBase[] to a Type[].  Returns null if any conversion fails.
        /// </summary>
        private static Type[] GetTypeRefsAsTypes(TypeRefBase[] typeRefs)
        {
            int count = (typeRefs != null ? typeRefs.Length : 0);
            Type[] types = new Type[count];
            for (int i = 0; i < count; ++i)
            {
                object reference = typeRefs[i].GetReferencedType();
                if (reference is Type)
                    types[i] = (Type)reference;
                else
                    return null;
            }
            return types;
        }

        /// <summary>
        /// Parse a '?' nullable type.
        /// </summary>
        private static SymbolicRef ParseNullableType(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If we get here, Conditional has already failed to parse the '?' (because it failed to find a ':'),
            // so this *should* truly be a nullable type, but still make sure it has an unused expression.
            if (parser.HasUnusedExpression)
                return new TypeRef(parser, parent, false, flags);
            return null;
        }

        /// <summary>
        /// Parse a built-in type, or a nullable built-in type.
        /// </summary>
        private static TypeRef ParseType(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new TypeRef(parser, parent, true, flags);
        }
    }
}
