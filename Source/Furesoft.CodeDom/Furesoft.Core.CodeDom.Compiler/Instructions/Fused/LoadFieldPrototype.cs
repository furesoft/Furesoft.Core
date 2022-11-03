using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions.Fused;

/// <summary>
/// An instruction prototype that loads a field. It is a
/// fused instruction prototype that is equivalent to a
/// get-field-pointer followed by a load.
/// </summary>
public sealed class LoadFieldPrototype : FusedInstructionPrototype
{
    private static readonly InterningCache<LoadFieldPrototype> instanceCache
        = new(
            new MappedComparer<LoadFieldPrototype, IField>(proto => proto.Field));

    private LoadFieldPrototype(IField field)
    {
        Field = field;
    }

    /// <summary>
    /// Gets the field that is loaded.
    /// </summary>
    /// <value>The field that is loaded.</value>
    public IField Field { get; private set; }

    /// <inheritdoc/>
    public override int ParameterCount => 1;

    /// <summary>
    /// Gets or creates an instruction prototype for instructions
    /// that load a particular field.
    /// </summary>
    /// <param name="field">
    /// The field that is to be loaded.
    /// </param>
    /// <returns>
    /// A load-field instruction prototype.
    /// </returns>
    public static LoadFieldPrototype Create(IField field)
    {
        return instanceCache.Intern(new LoadFieldPrototype(field));
    }

    /// <inheritdoc/>
    public override InstructionPrototype Map(MemberMapping mapping)
    {
        return Create(mapping.MapField(Field));
    }

    /// <inheritdoc/>
    public override void Expand(NamedInstructionBuilder instance)
    {
        var insn = instance.Instruction;
        AssertIsPrototypeOf(insn);

        var gfp = instance.InsertBefore(
            Instruction.CreateGetFieldPointer(
                Field, insn.Arguments[0]));
        instance.Instruction = Instruction.CreateLoad(Field.FieldType, gfp);
    }

    /// <summary>
    /// Creates an instance of this load-field prototype.
    /// </summary>
    /// <param name="basePointer">
    /// A pointer to a value that includes the field referred
    /// to by the load-field prototype.
    /// </param>
    /// <returns>A load-field instruction.</returns>
    public Instruction Instantiate(ValueTag basePointer)
    {
        return Instantiate(new ValueTag[] { basePointer });
    }
}