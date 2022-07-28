using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.Instructions
{
    public sealed class LoadFieldPrototype : InstructionPrototype
    {
        private readonly IField _field;

        public LoadFieldPrototype(IField field)
        {
            this._field = field;
        }

        public override IType ResultType => _field.FieldType;

        public override int ParameterCount => 0;

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