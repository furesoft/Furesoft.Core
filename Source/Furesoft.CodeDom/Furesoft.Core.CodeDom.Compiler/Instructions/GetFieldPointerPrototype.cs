using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions;

/// <summary>
/// A prototype for instructions that compute the address of
/// a field from a base address.
/// </summary>
public sealed class GetFieldPointerPrototype : InstructionPrototype
{
    private static readonly InterningCache<GetFieldPointerPrototype> instanceCache
        = new(
            new StructuralGetFieldPointerPrototypeComparer());

    private IType fieldPointerType;

    private GetFieldPointerPrototype(IField field)
    {
        Field = field;
        fieldPointerType = field.FieldType.MakePointerType(PointerKind.Reference);
    }

    /// <summary>
    /// Gets the field whose address is taken.
    /// </summary>
    /// <value>The field whose address is taken.</value>
    public IField Field { get; private set; }

    /// <inheritdoc/>
    public override IType ResultType => fieldPointerType;

    /// <inheritdoc/>
    public override int ParameterCount => 1;

    /// <summary>
    /// Gets or creates a get-field-pointer instruction prototype
    /// that computes the address of a particular field.
    /// </summary>
    /// <param name="field">
    /// The field whose address is to be computed.
    /// </param>
    /// <returns>
    /// A get-field-pointer instruction prototype.
    /// </returns>
    public static GetFieldPointerPrototype Create(IField field)
    {
        return instanceCache.Intern(new GetFieldPointerPrototype(field));
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> CheckConformance(Instruction instance, MethodBody body)
    {
        var basePtr = GetBasePointer(instance);
        var basePtrType = body.Implementation.GetValueType(basePtr) as PointerType;
        if (basePtrType == null || basePtrType.ElementType != Field.ParentType)
        {
            return new[]
            {
                $"get-field-prototype instruction for field '{Field.FullName}' expected " +
                $"a single argument that is a pointer to type '{Field.ParentType}'; " +
                $"the argument of type '{body.Implementation.GetValueType(basePtr)}' is not one."
            };
        }
        return EmptyArray<string>.Value;
    }

    /// <inheritdoc/>
    public override InstructionPrototype Map(MemberMapping mapping)
    {
        return Create(mapping.MapField(Field));
    }

    /// <summary>
    /// Gets a pointer to the value of which a field's address is
    /// computed by an instance of this prototype.
    /// </summary>
    /// <param name="instance">
    /// An instance of this prototype.
    /// </param>
    /// <returns>
    /// A pointer to the value that includes the field.
    /// </returns>
    public ValueTag GetBasePointer(Instruction instance)
    {
        AssertIsPrototypeOf(instance);
        return instance.Arguments[0];
    }

    /// <summary>
    /// Creates an instance of this get-field-pointer prototype.
    /// </summary>
    /// <param name="basePointer">
    /// A pointer to a value that includes the field referred
    /// to by the get-field-pointer prototype.
    /// </param>
    /// <returns>A get-field-pointer instruction.</returns>
    public Instruction Instantiate(ValueTag basePointer)
    {
        return Instantiate(new ValueTag[] { basePointer });
    }
}

internal sealed class StructuralGetFieldPointerPrototypeComparer
    : IEqualityComparer<GetFieldPointerPrototype>
{
    public bool Equals(GetFieldPointerPrototype x, GetFieldPointerPrototype y)
    {
        return x.Field == y.Field;
    }

    public int GetHashCode(GetFieldPointerPrototype obj)
    {
        return obj.Field.GetHashCode();
    }
}