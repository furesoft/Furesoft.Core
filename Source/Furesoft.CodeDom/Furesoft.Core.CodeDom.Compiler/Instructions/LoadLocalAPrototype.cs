using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions
{
    public sealed class LoadLocalAPrototype : InstructionPrototype
    {
        private IType _resultType;

        public LoadLocalAPrototype(IType resultType, Parameter parameter)
        {
            _resultType = resultType;
            Parameter = parameter;
        }

        public override IType ResultType => _resultType;

        public override int ParameterCount => 0;

        public Parameter Parameter { get; }

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