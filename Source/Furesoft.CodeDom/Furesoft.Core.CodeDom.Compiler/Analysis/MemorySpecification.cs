using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using static Furesoft.Core.CodeDom.Compiler.Analysis.MemorySpecification;

namespace Furesoft.Core.CodeDom.Compiler.Analysis;

/// <summary>
/// Extension methods that make working with memory specifications easier.
/// </summary>
public static class MemorySpecificationExtensions
{
    /// <summary>
    /// Gets a method's memory specification.
    /// </summary>
    /// <param name="method">The method to examine.</param>
    /// <returns>
    /// The explicit memory specification encoded in <paramref name="method"/>'s memory specification
    /// attribute, if it has one; otherwise, an unknown read/write specification.
    /// </returns>
    public static MemorySpecification GetMemorySpecification(this IMethod method)
    {
        if (method.Attributes.TryGet(MemorySpecificationAttribute.AttributeType, out IAttribute attr))
        {
            return ((MemorySpecificationAttribute)attr).Specification;
        }
        else
        {
            return Unknown;
        }
    }
}

/// <summary>
/// A base class for descriptions of how an instruction interacts with memory.
/// </summary>
public abstract class MemorySpecification
{
    /// <summary>
    /// A memory access spec that indicates that an instruction neither
    /// reads from or writes to memory.
    /// </summary>
    /// <value>A memory access spec that represents a memory-unrelated operation.</value>
    public static readonly MemorySpecification Nothing =
        new UnknownSpec(false, false);

    /// <summary>
    /// A memory access spec that indicates that the memory access behavior of
    /// an instruction is unknown.
    /// </summary>
    /// <value>A memory access spec that represents an unknown operation.</value>
    public static readonly MemorySpecification Unknown =
        new UnknownSpec(true, true);

    /// <summary>
    /// A memory access spec that identifies a read from an unknown location.
    /// </summary>
    /// <value>A memory access spec that represents an unknown read.</value>
    public static readonly MemorySpecification UnknownRead =
        new UnknownSpec(true, false);

    /// <summary>
    /// A memory access spec that identifies a write to an unknown location.
    /// </summary>
    /// <value>A memory access spec that represents an unknown write.</value>
    public static readonly MemorySpecification UnknownWrite =
        new UnknownSpec(false, true);

    /// <summary>
    /// Tells if this memory access spec implies that the instruction it is attached
    /// to might read from some address.
    /// </summary>
    /// <returns><c>true</c> if the instruction might read; otherwise, <c>false</c>.</returns>
    public abstract bool MayRead { get; }

    /// <summary>
    /// Tells if this memory access spec implies that the instruction it is attached
    /// to might write to some address.
    /// </summary>
    /// <returns><c>true</c> if the instruction might write; otherwise, <c>false</c>.</returns>
    public abstract bool MayWrite { get; }

    /// <summary>
    /// A read from an address encoded by an argument.
    /// </summary>
    public sealed class ArgumentRead : MemorySpecification
    {
        private ArgumentRead(int parameterIndex)
        {
            ParameterIndex = parameterIndex;
        }

        /// <inheritdoc/>
        public override bool MayRead => true;

        /// <inheritdoc/>
        public override bool MayWrite => false;

        /// <summary>
        /// Gets the index of the parameter that corresponds to
        /// the argument that is read.
        /// </summary>
        /// <value>A parameter index.</value>
        public int ParameterIndex { get; private set; }

        /// <summary>
        /// Creates a memory access spec that corresponds to a read
        /// from a particular argument.
        /// </summary>
        /// <param name="parameterIndex">
        /// The index of the parameter that corresponds to
        /// the argument that is read.
        /// </param>
        /// <returns>A memory access spec that represents an argument read.</returns>
        public static ArgumentRead Create(int parameterIndex)
        {
            return new ArgumentRead(parameterIndex);
        }
    }

    /// <summary>
    /// A write to an address encoded by an argument.
    /// </summary>
    public sealed class ArgumentWrite : MemorySpecification
    {
        private ArgumentWrite(int parameterIndex)
        {
            ParameterIndex = parameterIndex;
        }

        /// <inheritdoc/>
        public override bool MayRead => false;

        /// <inheritdoc/>
        public override bool MayWrite => true;

        /// <summary>
        /// Gets the index of the parameter that corresponds to
        /// the argument that is written to.
        /// </summary>
        /// <value>A parameter index.</value>
        public int ParameterIndex { get; private set; }

        /// <summary>
        /// Creates a memory access spec that corresponds to a write
        /// to a particular argument.
        /// </summary>
        /// <param name="parameterIndex">
        /// The index of the parameter that corresponds to
        /// the argument that is written to.
        /// </param>
        /// <returns>A memory access spec that represents an argument write.</returns>
        public static ArgumentWrite Create(int parameterIndex)
        {
            return new ArgumentWrite(parameterIndex);
        }
    }

    /// <summary>
    /// A union of memory access specs.
    /// </summary>
    public sealed class Union : MemorySpecification
    {
        private Union(IReadOnlyList<MemorySpecification> elements)
        {
            Elements = elements;
        }

        /// <summary>
        /// Gets the memory access specs whose behavior is unified.
        /// </summary>
        /// <value>A list of memory access specs.</value>
        public IReadOnlyList<MemorySpecification> Elements { get; private set; }

        /// <inheritdoc/>
        public override bool MayRead => Elements.Any(elem => elem.MayRead);

        /// <inheritdoc/>
        public override bool MayWrite => Elements.Any(elem => elem.MayWrite);

        /// <summary>
        /// Creates a memory access spec that represents the union of other
        /// memory access specs.
        /// </summary>
        /// <param name="elements">A sequence of memory access specs.</param>
        /// <returns>A union memory access spec.</returns>
        public static Union Create(IReadOnlyList<MemorySpecification> elements)
        {
            return new Union(elements);
        }
    }

    private sealed class UnknownSpec : MemorySpecification
    {
        private bool mayReadValue;

        private bool mayWriteValue;

        public UnknownSpec(bool mayRead, bool mayWrite)
        {
            mayReadValue = mayRead;
            mayWriteValue = mayWrite;
        }

        /// <inheritdoc/>
        public override bool MayRead => mayReadValue;

        /// <inheritdoc/>
        public override bool MayWrite => mayWriteValue;
    }
}

/// <summary>
/// Maps instruction prototypes to memory access specs.
/// </summary>
public abstract class PrototypeMemorySpecs
{
    /// <summary>
    /// Gets the memory access specification for a particular instruction
    /// prototype.
    /// </summary>
    /// <param name="prototype">The prototype to examine.</param>
    /// <returns>A memory specification for <paramref name="prototype"/>.</returns>
    public abstract MemorySpecification GetMemorySpecification(InstructionPrototype prototype);
}

/// <summary>
/// Assigns memory specifications to prototypes based
/// on a set of user-configurable rules.
/// </summary>
public sealed class RuleBasedPrototypeMemorySpecs : PrototypeMemorySpecs
{
    /// <summary>
    /// Gets the default prototype memory spec rules.
    /// </summary>
    /// <value>The default prototype memory spec rules.</value>
    public static readonly RuleBasedPrototypeMemorySpecs Default;

    private RuleBasedSpecStore<MemorySpecification> store;

    static RuleBasedPrototypeMemorySpecs()
    {
        Default = new RuleBasedPrototypeMemorySpecs();

        Default.Register<AllocaArrayPrototype>(Nothing);
        Default.Register<AllocaPrototype>(Nothing);
        Default.Register<BoxPrototype>(Nothing);
        Default.Register<ConstantPrototype>(Nothing);
        Default.Register<CopyPrototype>(Nothing);
        Default.Register<DynamicCastPrototype>(Nothing);
        Default.Register<GetStaticFieldPointerPrototype>(Nothing);
        Default.Register<ReinterpretCastPrototype>(Nothing);
        Default.Register<GetFieldPointerPrototype>(Nothing);
        Default.Register<NewDelegatePrototype>(Nothing);
        Default.Register<UnboxPrototype>(Nothing);

        // Mark volatile loads and stores as unknown to ensure that they are never reordered
        // with regard to other memory operations.
        // TODO: is this really how we should represent volatility?
        Default.Register<LoadPrototype>(proto =>
            proto.IsVolatile ? Unknown : ArgumentRead.Create(0));
        Default.Register<StorePrototype>(proto =>
            proto.IsVolatile ? Unknown : ArgumentWrite.Create(0));

        // Call-like instruction prototypes.
        Default.Register<CallPrototype>(
            proto => proto.Lookup == MethodLookup.Static
                ? proto.Callee.GetMemorySpecification()
                : Unknown);
        Default.Register<NewObjectPrototype>(
            proto => proto.Constructor.GetMemorySpecification());
        Default.Register<IndirectCallPrototype>(Unknown);

        // Arithmetic intrinsics never read or write.
        foreach (var name in ArithmeticIntrinsics.Operators.All)
        {
            Default.Register(
                ArithmeticIntrinsics.GetArithmeticIntrinsicName(name, false),
                Nothing);
            Default.Register(
                ArithmeticIntrinsics.GetArithmeticIntrinsicName(name, true),
                Nothing);
        }

        // Array intrinsics.
        Default.Register(
            ArrayIntrinsics.Namespace.GetIntrinsicName(ArrayIntrinsics.Operators.GetElementPointer),
            Nothing);
        Default.Register(
            ArrayIntrinsics.Namespace.GetIntrinsicName(ArrayIntrinsics.Operators.LoadElement),
            UnknownRead);
        Default.Register(
            ArrayIntrinsics.Namespace.GetIntrinsicName(ArrayIntrinsics.Operators.StoreElement),
            UnknownWrite);
        Default.Register(
            ArrayIntrinsics.Namespace.GetIntrinsicName(ArrayIntrinsics.Operators.GetLength),
            Nothing);
        Default.Register(
            ArrayIntrinsics.Namespace.GetIntrinsicName(ArrayIntrinsics.Operators.NewArray),
            Nothing);

        // Exception intrinsics.
        Default.Register(
            ExceptionIntrinsics.Namespace.GetIntrinsicName(ExceptionIntrinsics.Operators.GetCapturedException),
            UnknownRead);

        // Object intrinsics.
        Default.Register(
            ObjectIntrinsics.Namespace.GetIntrinsicName(ObjectIntrinsics.Operators.UnboxAny),
            UnknownRead);

        // Memory intrinsics.
        Default.Register(
            MemoryIntrinsics.Namespace.GetIntrinsicName(MemoryIntrinsics.Operators.AllocaPinned),
            Nothing);
    }

    /// <summary>
    /// Creates an empty set of prototype exception spec rules.
    /// </summary>
    public RuleBasedPrototypeMemorySpecs()
    {
        store = new RuleBasedSpecStore<MemorySpecification>(Unknown);
    }

    /// <summary>
    /// Creates a copy of another set of prototype exception spec rules.
    /// </summary>
    public RuleBasedPrototypeMemorySpecs(RuleBasedPrototypeMemorySpecs other)
    {
        store = new RuleBasedSpecStore<MemorySpecification>(other.store);
    }

    /// <inheritdoc/>
    public override MemorySpecification GetMemorySpecification(InstructionPrototype prototype)
    {
        return store.GetSpecification(prototype);
    }

    /// <summary>
    /// Registers a function that computes memory specifications
    /// for a particular type of instruction prototype.
    /// </summary>
    /// <param name="getMemorySpec">
    /// A function that computes memory specifications for all
    /// instruction prototypes of type T.
    /// </param>
    /// <typeparam name="T">
    /// The type of instruction prototypes to which
    /// <paramref name="getMemorySpec"/> is applicable.
    /// </typeparam>
    public void Register<T>(Func<T, MemorySpecification> getMemorySpec)
        where T : InstructionPrototype
    {
        store.Register<T>(getMemorySpec);
    }

    /// <summary>
    /// Maps a particular type of instruction prototype
    /// to a memory specification.
    /// </summary>
    /// <param name="memorySpec">
    /// The memory specification to register.
    /// </param>
    /// <typeparam name="T">
    /// The type of instruction prototypes to which
    /// <paramref name="memorySpec"/> is applicable.
    /// </typeparam>
    public void Register<T>(MemorySpecification memorySpec)
        where T : InstructionPrototype
    {
        store.Register<T>(memorySpec);
    }

    /// <summary>
    /// Registers a function that computes memory specifications
    /// for a particular type of intrinsic.
    /// </summary>
    /// <param name="intrinsicName">
    /// The name of the intrinsic to compute memory
    /// specifications for.
    /// </param>
    /// <param name="getMemorySpec">
    /// A function that takes an intrinsic prototype with
    /// name <paramref name="intrinsicName"/> and computes
    /// the memory specification for that prototype.
    /// </param>
    public void Register(string intrinsicName, Func<IntrinsicPrototype, MemorySpecification> getMemorySpec)
    {
        store.Register(intrinsicName, getMemorySpec);
    }

    /// <summary>
    /// Registers a function that assigns a fixed memory specification
    /// to a particular type of intrinsic.
    /// </summary>
    /// <param name="intrinsicName">
    /// The name of the intrinsic to assign the memory
    /// specifications to.
    /// </param>
    /// <param name="memorySpec">
    /// The memory specification for all intrinsic prototypes with
    /// name <paramref name="intrinsicName"/>.
    /// </param>
    public void Register(string intrinsicName, MemorySpecification memorySpec)
    {
        store.Register(intrinsicName, memorySpec);
    }
}