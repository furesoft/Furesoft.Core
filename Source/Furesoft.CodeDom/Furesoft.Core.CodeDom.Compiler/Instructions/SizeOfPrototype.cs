using System.Collections.Generic;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Instructions;

namespace Furesoft.Core.CodeDom.Compiler.Instructions
{
    /// <summary>
    /// A prototype for sizeof instructions, which compute the size of a type.
    /// </summary>
    public sealed class SizeOfPrototype : InstructionPrototype
    {
        private SizeOfPrototype(IType measuredType, IType resultType)
        {
            MeasuredType = measuredType;
            resultTy = resultType;
        }

        /// <summary>
        /// Gets the type to measure.
        /// </summary>
        /// <returns>The type to measure.</returns>
        public IType MeasuredType { get; private set; }

        private IType resultTy;

        /// <inheritdoc/>
        public override IType ResultType => resultTy;

        /// <inheritdoc/>
        public override int ParameterCount => 0;

        /// <inheritdoc/>
        public override IReadOnlyList<string> CheckConformance(Instruction instance, MethodBody body)
        {
            return EmptyArray<string>.Value;
        }

        /// <inheritdoc/>
        public override InstructionPrototype Map(MemberMapping mapping)
        {
            return Create(mapping.MapType(MeasuredType), mapping.MapType(ResultType));
        }

        /// <summary>
        /// Instantiates this prototype.
        /// </summary>
        /// <returns>A sizeof instruction.</returns>
        public Instruction Instantiate()
        {
            return Instantiate(EmptyArray<ValueTag>.Value);
        }

        private static readonly InterningCache<SizeOfPrototype> instanceCache
            = new(
                new StructuralSizeOfPrototypeComparer());

        /// <summary>
        /// Gets or creates a sizeof instruction prototype.
        /// </summary>
        /// <param name="measuredType">The type to measure.</param>
        /// <param name="resultType">The instruction's result type.</param>
        /// <returns>A sizeof instruction prototype.</returns>
        public static SizeOfPrototype Create(IType measuredType, IType resultType)
        {
            return instanceCache.Intern(new SizeOfPrototype(measuredType, resultType));
        }
    }

    internal sealed class StructuralSizeOfPrototypeComparer
        : IEqualityComparer<SizeOfPrototype>
    {
        public bool Equals(SizeOfPrototype x, SizeOfPrototype y)
        {
            return object.Equals(x.MeasuredType, y.MeasuredType)
                && object.Equals(x.ResultType, y.ResultType);
        }

        public int GetHashCode(SizeOfPrototype obj)
        {
            var hash = EnumerableComparer.EmptyHash;
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.MeasuredType.GetHashCode());
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.ResultType.GetHashCode());
            return hash;
        }
    }
}
