using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions
{
    public sealed class StoreFieldPointerPrototype : InstructionPrototype
    {
        private IType _resultType;

        public StoreFieldPointerPrototype(IType resultType, IField field)
        {
            _resultType = resultType;
            Field = field;
        }

        public override IType ResultType => _resultType;

        public override int ParameterCount => 0;

        public IField Field { get; }

        public override IReadOnlyList<string> CheckConformance(Instruction instance, MethodBody body)
        {
            return EmptyArray<string>.Value;
        }

        public override InstructionPrototype Map(MemberMapping mapping)
        {
            return this;
        }
    }
}