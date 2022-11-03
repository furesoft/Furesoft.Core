using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Backends.CLR;

/// <summary>
/// A type environment that extracts relevant BCL types from a single
/// core library based on their names.
/// </summary>
public sealed class CorlibTypeEnvironment : TypeEnvironment
{
    private InterningCache<ClrArrayType> arrayTypeCache;

    private Lazy<IType> capturedExceptionType;

    private Lazy<IType> charType;

    private Lazy<Func<int, IGenericParameter, IReadOnlyList<IType>>> createArrayBaseTypes;

    private Lazy<IType> fieldTokenType;

    private Lazy<IType> float32Type;

    private Lazy<IType> float64Type;

    private Lazy<IType> intPtrType;

    private Lazy<IType> methodTokenType;

    private Lazy<IType> objectType;

    private Lazy<Dictionary<int, IType>> signedIntegerTypes;

    private Lazy<IType> stringType;

    private Lazy<IType> typeTokenType;

    private Lazy<IType> uintPtrType;

    private Lazy<Dictionary<int, IType>> unsignedIntegerTypes;

    private Lazy<IType> voidType;

    /// <summary>
    /// Creates a type system based on a core library assembly.
    /// </summary>
    /// <param name="corlib">
    /// The core library, which supplies the types in the type
    /// system.
    /// </param>
    public CorlibTypeEnvironment(IAssembly corlib)
        : this(new ReadOnlyTypeResolver(corlib))
    { }

    /// <summary>
    /// Creates a type system based on a core library type resolver.
    /// </summary>
    /// <param name="corlibTypeResolver">
    /// A type resolver for the core library, which supplies the
    /// types in the type system.
    /// </param>
    public CorlibTypeEnvironment(ReadOnlyTypeResolver corlibTypeResolver)
    {
        this.CorlibTypeResolver = corlibTypeResolver;
        this.arrayTypeCache = new InterningCache<ClrArrayType>(
            new RankClrArrayTypeComparer());
        this.createArrayBaseTypes = new Lazy<Func<int, IGenericParameter, IReadOnlyList<IType>>>(
            ResolveArrayBaseTypes);
        this.signedIntegerTypes = new Lazy<Dictionary<int, IType>>(ResolveSignedIntegerTypes);
        this.unsignedIntegerTypes = new Lazy<Dictionary<int, IType>>(ResolveUnsignedIntegerTypes);
        this.voidType = new Lazy<IType>(() => ResolveSystemType(nameof(Void)));
        this.float32Type = new Lazy<IType>(() => ResolveSystemType(nameof(Single)));
        this.float64Type = new Lazy<IType>(() => ResolveSystemType(nameof(Double)));
        this.charType = new Lazy<IType>(() => ResolveSystemType(nameof(Char)));
        this.stringType = new Lazy<IType>(() => ResolveSystemType(nameof(String)));
        this.intPtrType = new Lazy<IType>(() => ResolveSystemType(nameof(IntPtr)));
        this.uintPtrType = new Lazy<IType>(() => ResolveSystemType(nameof(UIntPtr)));
        this.objectType = new Lazy<IType>(() => ResolveSystemType(nameof(Object)));
        this.typeTokenType = new Lazy<IType>(() => ResolveSystemType(nameof(RuntimeTypeHandle)));
        this.fieldTokenType = new Lazy<IType>(() => ResolveSystemType(nameof(RuntimeFieldHandle)));
        this.methodTokenType = new Lazy<IType>(() => ResolveSystemType(nameof(RuntimeMethodHandle)));
        this.capturedExceptionType = new Lazy<IType>(
            () => CorlibTypeResolver.ResolveTypes(
                new SimpleName("ExceptionDispatchInfo")
                .Qualify("ExceptionServices")
                .Qualify("Runtime")
                .Qualify("System"))
                .FirstOrDefault());
    }

    /// <inheritdoc/>
    public override IType CapturedException => capturedExceptionType.Value;

    /// <inheritdoc/>
    public override IType Char => charType.Value;

    /// <summary>
    /// Gets a type resolver for the core library from
    /// which types are resolved.
    /// </summary>
    /// <returns>The core library type resolver.</returns>
    public ReadOnlyTypeResolver CorlibTypeResolver { get; private set; }

    /// <inheritdoc/>
    public override IType FieldToken => fieldTokenType.Value;

    /// <inheritdoc/>
    public override IType Float32 => float32Type.Value;

    /// <inheritdoc/>
    public override IType Float64 => float64Type.Value;

    /// <inheritdoc/>
    public override IType MethodToken => methodTokenType.Value;

    /// <inheritdoc/>
    public override IType NaturalInt => intPtrType.Value;

    /// <inheritdoc/>
    public override IType NaturalUInt => uintPtrType.Value;

    /// <inheritdoc/>
    public override IType Object => objectType.Value;

    /// <inheritdoc/>
    public override IType String => stringType.Value;

    /// <inheritdoc/>
    public override SubtypingRules Subtyping => ClrSubtypingRules.Instance;

    /// <inheritdoc/>
    public override IType TypeToken => typeTokenType.Value;

    /// <inheritdoc/>
    public override IType Void => voidType.Value;

    /// <inheritdoc/>
    public override bool TryMakeArrayType(
        IType elementType,
        int rank,
        out IType arrayType)
    {
        arrayType = GetGenericArrayType(rank)
            .MakeGenericType(elementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool TryMakeSignedIntegerType(int sizeInBits, out IType integerType)
    {
        return signedIntegerTypes.Value.TryGetValue(sizeInBits, out integerType);
    }

    /// <inheritdoc/>
    public override bool TryMakeUnsignedIntegerType(int sizeInBits, out IType integerType)
    {
        return unsignedIntegerTypes.Value.TryGetValue(sizeInBits, out integerType);
    }

    private ClrArrayType GetGenericArrayType(int rank)
    {
        var createBaseTypes = createArrayBaseTypes.Value;
        return arrayTypeCache.Intern(
            new ClrArrayType(
                rank,
                param => createBaseTypes(rank, param)));
    }

    private Func<int, IGenericParameter, IReadOnlyList<IType>> ResolveArrayBaseTypes()
    {
        var arrayClass = ResolveSystemType("Array");

        var listType = CorlibTypeResolver.ResolveTypes(
            new SimpleName("IList", 1)
            .Qualify("Generic")
            .Qualify("Collections")
            .Qualify("System"))
            .FirstOrDefault();

        return (rank, param) =>
        {
            if (listType == null || rank != 1)
            {
                return new[] { arrayClass };
            }
            else
            {
                return new[] { arrayClass, listType.MakeGenericType(param) };
            }
        };
    }

    private Dictionary<int, IType> ResolveSignedIntegerTypes()
    {
        return new Dictionary<int, IType>
        {
            { 8, ResolveSystemType(nameof(SByte)) },
            { 16, ResolveSystemType(nameof(Int16)) },
            { 32, ResolveSystemType(nameof(Int32)) },
            { 64, ResolveSystemType(nameof(Int64)) }
        };
    }

    private IType ResolveSystemType(string name)
    {
        return CorlibTypeResolver.ResolveTypes(
            new SimpleName(name)
                .Qualify("System"))
                .FirstOrDefault();
    }

    private Dictionary<int, IType> ResolveUnsignedIntegerTypes()
    {
        return new Dictionary<int, IType>
        {
            { 1, ResolveSystemType(nameof(Boolean)) },
            { 8, ResolveSystemType(nameof(Byte)) },
            { 16, ResolveSystemType(nameof(UInt16)) },
            { 32, ResolveSystemType(nameof(UInt32)) },
            { 64, ResolveSystemType(nameof(UInt64)) }
        };
    }
}