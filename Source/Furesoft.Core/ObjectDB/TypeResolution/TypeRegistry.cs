namespace Furesoft.Core.ObjectDB.TypeResolution;

/// <summary>
///     Provides access to a central registry of aliased <see cref="System.Type" />s.
/// </summary>
/// <remarks>
///     <p>
///         Simplifies configuration by allowing aliases to be used instead of
///         fully qualified type names.
///     </p>
///     <p>
///         Comes 'pre-loaded' with a number of convenience alias' for the more
///         common types; an example would be the '<c>int</c>' (or '<c>Integer</c>'
///         for Visual Basic.NET developers) alias for the <see cref="int" />
///         type.
///     </p>
/// </remarks>
internal static class TypeRegistry
{
	/// <summary>
	///     The alias around the 'int' type.
	/// </summary>
	private const string Int32Alias = "int";

	/// <summary>
	///     The alias around the 'int[]' array type.
	/// </summary>
	private const string Int32ArrayAlias = "int[]";

	/// <summary>
	///     The alias around the 'decimal' type.
	/// </summary>
	private const string DecimalAlias = "decimal";

	/// <summary>
	///     The alias around the 'decimal[]' array type.
	/// </summary>
	private const string DecimalArrayAlias = "decimal[]";

	/// <summary>
	///     The alias around the 'char' type.
	/// </summary>
	private const string CharAlias = "char";

	/// <summary>
	///     The alias around the 'char[]' array type.
	/// </summary>
	private const string CharArrayAlias = "char[]";

	/// <summary>
	///     The alias around the 'long' type.
	/// </summary>
	private const string Int64Alias = "long";

	/// <summary>
	///     The alias around the 'long[]' array type.
	/// </summary>
	private const string Int64ArrayAlias = "long[]";

	/// <summary>
	///     The alias around the 'short' type.
	/// </summary>
	private const string Int16Alias = "short";

	/// <summary>
	///     The alias around the 'short[]' array type.
	/// </summary>
	private const string Int16ArrayAlias = "short[]";

	/// <summary>
	///     The alias around the 'unsigned int' type.
	/// </summary>
	private const string UInt32Alias = "uint";

	/// <summary>
	///     The alias around the 'unsigned long' type.
	/// </summary>
	private const string UInt64Alias = "ulong";

	/// <summary>
	///     The alias around the 'ulong[]' array type.
	/// </summary>
	private const string UInt64ArrayAlias = "ulong[]";

	/// <summary>
	///     The alias around the 'uint[]' array type.
	/// </summary>
	private const string UInt32ArrayAlias = "uint[]";

	/// <summary>
	///     The alias around the 'unsigned short' type.
	/// </summary>
	private const string UInt16Alias = "ushort";

	/// <summary>
	///     The alias around the 'ushort[]' array type.
	/// </summary>
	private const string UInt16ArrayAlias = "ushort[]";

	/// <summary>
	///     The alias around the 'double' type.
	/// </summary>
	private const string DoubleAlias = "double";

	/// <summary>
	///     The alias around the 'double[]' array type.
	/// </summary>
	private const string DoubleArrayAlias = "double[]";

	/// <summary>
	///     The alias around the 'float' type.
	/// </summary>
	private const string FloatAlias = "float";

	/// <summary>
	///     The alias around the 'Single' type (Visual Basic.NET style).
	/// </summary>
	private const string SingleAlias = "Single";

	/// <summary>
	///     The alias around the 'float[]' array type.
	/// </summary>
	private const string FloatArrayAlias = "float[]";

	/// <summary>
	///     The alias around the 'DateTime' type.
	/// </summary>
	private const string DateTimeAlias = "DateTime";

	/// <summary>
	///     The alias around the 'DateTime' type (C# style).
	/// </summary>
	private const string DateAlias = "date";

	/// <summary>
	///     The alias around the 'DateTime[]' array type.
	/// </summary>
	private const string DateTimeArrayAlias = "DateTime[]";

	/// <summary>
	///     The alias around the 'DateTime[]' array type.
	/// </summary>
	private const string DateTimeArrayAliasCSharp = "date[]";

	/// <summary>
	///     The alias around the 'bool' type.
	/// </summary>
	private const string BoolAlias = "bool";

	/// <summary>
	///     The alias around the 'bool[]' array type.
	/// </summary>
	private const string BoolArrayAlias = "bool[]";

	/// <summary>
	///     The alias around the 'string' type.
	/// </summary>
	private const string StringAlias = "string";

	/// <summary>
	///     The alias around the 'string[]' array type.
	/// </summary>
	private const string StringArrayAlias = "string[]";

	/// <summary>
	///     The alias around the 'object' type.
	/// </summary>
	private const string ObjectAlias = "object";

	/// <summary>
	///     The alias around the 'object[]' array type.
	/// </summary>
	private const string ObjectArrayAlias = "object[]";

	/// <summary>
	///     The alias around the 'int?' type.
	/// </summary>
	private const string NullableInt32Alias = "int?";

	/// <summary>
	///     The alias around the 'int?[]' array type.
	/// </summary>
	private const string NullableInt32ArrayAlias = "int?[]";

	/// <summary>
	///     The alias around the 'decimal?' type.
	/// </summary>
	private const string NullableDecimalAlias = "decimal?";

	/// <summary>
	///     The alias around the 'decimal?[]' array type.
	/// </summary>
	private const string NullableDecimalArrayAlias = "decimal?[]";

	/// <summary>
	///     The alias around the 'char?' type.
	/// </summary>
	private const string NullableCharAlias = "char?";

	/// <summary>
	///     The alias around the 'char?[]' array type.
	/// </summary>
	private const string NullableCharArrayAlias = "char?[]";

	/// <summary>
	///     The alias around the 'long?' type.
	/// </summary>
	private const string NullableInt64Alias = "long?";

	/// <summary>
	///     The alias around the 'long?[]' array type.
	/// </summary>
	private const string NullableInt64ArrayAlias = "long?[]";

	/// <summary>
	///     The alias around the 'short?' type.
	/// </summary>
	private const string NullableInt16Alias = "short?";

	/// <summary>
	///     The alias around the 'short?[]' array type.
	/// </summary>
	private const string NullableInt16ArrayAlias = "short?[]";

	/// <summary>
	///     The alias around the 'unsigned int?' type.
	/// </summary>
	private const string NullableUInt32Alias = "uint?";

	/// <summary>
	///     The alias around the 'unsigned long?' type.
	/// </summary>
	private const string NullableUInt64Alias = "ulong?";

	/// <summary>
	///     The alias around the 'ulong?[]' array type.
	/// </summary>
	private const string NullableUInt64ArrayAlias = "ulong?[]";

	/// <summary>
	///     The alias around the 'uint?[]' array type.
	/// </summary>
	private const string NullableUInt32ArrayAlias = "uint?[]";

	/// <summary>
	///     The alias around the 'unsigned short?' type.
	/// </summary>
	private const string NullableUInt16Alias = "ushort?";

	/// <summary>
	///     The alias around the 'ushort?[]' array type.
	/// </summary>
	private const string NullableUInt16ArrayAlias = "ushort?[]";

	/// <summary>
	///     The alias around the 'double?' type.
	/// </summary>
	private const string NullableDoubleAlias = "double?";

	/// <summary>
	///     The alias around the 'double?[]' array type.
	/// </summary>
	private const string NullableDoubleArrayAlias = "double?[]";

	/// <summary>
	///     The alias around the 'float?' type.
	/// </summary>
	private const string NullableFloatAlias = "float?";

	/// <summary>
	///     The alias around the 'float?[]' array type.
	/// </summary>
	private const string NullableFloatArrayAlias = "float?[]";

	/// <summary>
	///     The alias around the 'bool?' type.
	/// </summary>
	private const string NullableBoolAlias = "bool?";

	/// <summary>
	///     The alias around the 'bool?[]' array type.
	/// </summary>
	private const string NullableBoolArrayAlias = "bool?[]";

    private static readonly IDictionary<string, Type> types = new Dictionary<string, Type>();

    /// <summary>
    ///     Registers standard and user-configured type aliases.
    /// </summary>
    static TypeRegistry()
    {
        types["Int32"] = typeof(int);
        types[Int32Alias] = typeof(int);
        types[Int32ArrayAlias] = typeof(int[]);

        types["UInt32"] = typeof(uint);
        types[UInt32Alias] = typeof(uint);
        types[UInt32ArrayAlias] = typeof(uint[]);

        types["Int16"] = typeof(short);
        types[Int16Alias] = typeof(short);
        types[Int16ArrayAlias] = typeof(short[]);

        types["UInt16"] = typeof(ushort);
        types[UInt16Alias] = typeof(ushort);
        types[UInt16ArrayAlias] = typeof(ushort[]);

        types["Int64"] = typeof(long);
        types[Int64Alias] = typeof(long);
        types[Int64ArrayAlias] = typeof(long[]);

        types["UInt64"] = typeof(ulong);
        types[UInt64Alias] = typeof(ulong);
        types[UInt64ArrayAlias] = typeof(ulong[]);

        types[DoubleAlias] = typeof(double);
        types[DoubleArrayAlias] = typeof(double[]);

        types[FloatAlias] = typeof(float);
        types[SingleAlias] = typeof(float);
        types[FloatArrayAlias] = typeof(float[]);

        types[DateTimeAlias] = typeof(DateTime);
        types[DateAlias] = typeof(DateTime);
        types[DateTimeArrayAlias] = typeof(DateTime[]);
        types[DateTimeArrayAliasCSharp] = typeof(DateTime[]);

        types[BoolAlias] = typeof(bool);
        types[BoolArrayAlias] = typeof(bool[]);

        types[DecimalAlias] = typeof(decimal);
        types[DecimalArrayAlias] = typeof(decimal[]);

        types[CharAlias] = typeof(char);
        types[CharArrayAlias] = typeof(char[]);

        types[StringAlias] = typeof(string);
        types[StringArrayAlias] = typeof(string[]);

        types[ObjectAlias] = typeof(object);
        types[ObjectArrayAlias] = typeof(object[]);

        types[NullableInt32Alias] = typeof(int?);
        types[NullableInt32ArrayAlias] = typeof(int?[]);

        types[NullableDecimalAlias] = typeof(decimal?);
        types[NullableDecimalArrayAlias] = typeof(decimal?[]);

        types[NullableCharAlias] = typeof(char?);
        types[NullableCharArrayAlias] = typeof(char?[]);

        types[NullableInt64Alias] = typeof(long?);
        types[NullableInt64ArrayAlias] = typeof(long?[]);

        types[NullableInt16Alias] = typeof(short?);
        types[NullableInt16ArrayAlias] = typeof(short?[]);

        types[NullableUInt32Alias] = typeof(uint?);
        types[NullableUInt32ArrayAlias] = typeof(uint?[]);

        types[NullableUInt64Alias] = typeof(ulong?);
        types[NullableUInt64ArrayAlias] = typeof(ulong?[]);

        types[NullableUInt16Alias] = typeof(ushort?);
        types[NullableUInt16ArrayAlias] = typeof(ushort?[]);

        types[NullableDoubleAlias] = typeof(double?);
        types[NullableDoubleArrayAlias] = typeof(double?[]);

        types[NullableFloatAlias] = typeof(float?);
        types[NullableFloatArrayAlias] = typeof(float?[]);

        types[NullableBoolAlias] = typeof(bool?);
        types[NullableBoolArrayAlias] = typeof(bool?[]);
    }

    /// <summary>
    ///     Resolves the supplied <paramref name="alias" /> to a <see cref="System.Type" />.
    /// </summary>
    /// <param name="alias">
    ///     The alias to resolve.
    /// </param>
    /// <returns>
    ///     The <see cref="System.Type" /> the supplied <paramref name="alias" /> was
    ///     associated with, or <see lang="null" /> if no <see cref="System.Type" />
    ///     was previously registered for the supplied <paramref name="alias" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="alias" /> is <see langword="null" /> or
    ///     contains only whitespace character(s).
    /// </exception>
    public static Type ResolveType(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentNullException("alias");

        types.TryGetValue(alias, out var type);
        return type;
    }
}