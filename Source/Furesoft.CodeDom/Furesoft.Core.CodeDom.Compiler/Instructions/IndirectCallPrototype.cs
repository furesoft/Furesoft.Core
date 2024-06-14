using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions;

/// <summary>
/// An instruction prototype for indirect call instructions:
/// instructions that call a delegate or function pointer.
/// </summary>
public sealed class IndirectCallPrototype : InstructionPrototype
{
    private static readonly InterningCache<IndirectCallPrototype> instanceCache
        = new(
            new StructuralIndirectCallPrototypeComparer());

    private IType returnType;

    private IndirectCallPrototype(
                        IType returnType,
        IReadOnlyList<IType> parameterTypes)
    {
        this.returnType = returnType;
        ParameterTypes = parameterTypes;
    }

    /// <inheritdoc/>
    public override int ParameterCount => 1 + ParameterTypes.Count;

    /// <summary>
    /// Gets the list of parameter types.
    /// </summary>
    /// <returns>The parameter types.</returns>
    public IReadOnlyList<IType> ParameterTypes { get; private set; }

    /// <inheritdoc/>
    public override IType ResultType => returnType;

    /// <summary>
    /// Gets the indirect call instruction prototype for a particular return
    /// type and parameter type list.
    /// </summary>
    /// <param name="returnType">The type of value returned by the callee.</param>
    /// <param name="parameterTypes">A list of the callee's parameter types.</param>
    /// <returns>An indirect call instruction prototype.</returns>
    public static IndirectCallPrototype Create(IType returnType, IReadOnlyList<IType> parameterTypes)
    {
        return instanceCache.Intern(new IndirectCallPrototype(returnType, parameterTypes));
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> CheckConformance(
        Instruction instance,
        MethodBody body)
    {
        var errors = new List<string>();

        var argList = GetArgumentList(instance);
        int paramCount = ParameterTypes.Count;
        for (int i = 0; i < paramCount; i++)
        {
            var paramType = ParameterTypes[i];
            var argType = body.Implementation.GetValueType(argList[i]);

            if (!paramType.Equals(argType))
            {
                errors.Add(
                    string.Format(
                        "Argument of type '{0}' was provided where an " +
                        "argument of type '{1}' was expected.",
                        paramType.FullName,
                        argType.FullName));
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets the argument list in an instruction that conforms to
    /// this prototype.
    /// </summary>
    /// <param name="instruction">
    /// An instruction that conforms to this prototype.
    /// </param>
    /// <returns>The formal argument list.</returns>
    public ReadOnlySlice<ValueTag> GetArgumentList(Instruction instruction)
    {
        AssertIsPrototypeOf(instruction);
        return new ReadOnlySlice<ValueTag>(
            instruction.Arguments,
            1,
            instruction.Arguments.Count - 1);
    }

    /// <summary>
    /// Gets the callee delegate or function pointer argument in an
    /// instruction that conforms to this prototype.
    /// </summary>
    /// <param name="instruction">
    /// An instruction that conforms to this prototype.
    /// </param>
    /// <returns>The callee argument.</returns>
    public ValueTag GetCallee(Instruction instruction)
    {
        AssertIsPrototypeOf(instruction);
        return instruction.Arguments[0];
    }

    /// <summary>
    /// Instantiates this indirect call prototype.
    /// </summary>
    /// <param name="callee">
    /// The delegate or function pointer to call.
    /// </param>
    /// <param name="arguments">
    /// The argument list for the call.
    /// </param>
    /// <returns>
    /// An indirect call instruction.
    /// </returns>
    public Instruction Instantiate(ValueTag callee, IReadOnlyList<ValueTag> arguments)
    {
        var extendedArgs = new List<ValueTag>
        {
            callee
        };
        extendedArgs.AddRange(arguments);
        return Instantiate(extendedArgs);
    }

    /// <inheritdoc/>
    public override InstructionPrototype Map(MemberMapping mapping)
    {
        var newReturnType = mapping.MapType(returnType);
        var newParamTypes = ParameterTypes.EagerSelect<IType, IType>(mapping.MapType);
        return Create(newReturnType, newParamTypes);
    }
}

internal sealed class StructuralIndirectCallPrototypeComparer
    : IEqualityComparer<IndirectCallPrototype>
{
    public bool Equals(IndirectCallPrototype x, IndirectCallPrototype y)
    {
        return object.Equals(x.ResultType, y.ResultType)
            && x.ParameterTypes.SequenceEqual<IType>(y.ParameterTypes);
    }

    public int GetHashCode(IndirectCallPrototype obj)
    {
        int hashCode = EnumerableComparer.EmptyHash;
        hashCode = EnumerableComparer.FoldIntoHashCode(hashCode, obj.ResultType);
        var paramTypes = obj.ParameterTypes;
        var paramTypeCount = paramTypes.Count;
        for (int i = 0; i < paramTypeCount; i++)
        {
            hashCode = EnumerableComparer.FoldIntoHashCode(hashCode, paramTypes[i]);
        }
        return hashCode;
    }
}