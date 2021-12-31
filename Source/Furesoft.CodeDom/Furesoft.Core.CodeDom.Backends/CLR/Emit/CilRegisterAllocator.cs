using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler;

namespace Furesoft.Core.CodeDom.Backends.CLR.Emit
{
    /// <summary>
    /// A register as produced by the CIL register allocator.
    /// </summary>
    internal struct CilCodegenRegister : IEquatable<CilCodegenRegister>
    {
        /// <summary>
        /// Creates a register from a parameter definition.
        /// </summary>
        /// <param name="parameter">
        /// A parameter definition.
        /// </param>
        /// <param name="type">
        /// The register's type.
        /// </param>
        public CilCodegenRegister(ParameterDefinition parameter, IType type)
        {
            this.ParameterOrNull = parameter;
            this.VariableOrNull = null;
            this.Type = type;
        }

        /// <summary>
        /// Creates a register from a variable definition.
        /// </summary>
        /// <param name="variable">
        /// A variable definition.
        /// </param>
        /// <param name="type">
        /// The register's type.
        /// </param>
        public CilCodegenRegister(VariableDefinition variable, IType type)
        {
            this.ParameterOrNull = null;
            this.VariableOrNull = variable;
            this.Type = type;
        }

        public bool IsParameter => ParameterOrNull != null;
        public bool IsVariable => VariableOrNull != null;
        public ParameterDefinition ParameterOrNull { get; private set; }
        public IType Type { get; private set; }
        public VariableDefinition VariableOrNull { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is CilCodegenRegister
                && Equals((CilCodegenRegister)obj);
        }

        public bool Equals(CilCodegenRegister other)
        {
            return (IsVariable && VariableOrNull == other.VariableOrNull)
                || (IsParameter && ParameterOrNull == other.ParameterOrNull);
        }

        public override int GetHashCode()
        {
            if (IsParameter)
            {
                return ParameterOrNull.GetHashCode();
            }
            else
            {
                return VariableOrNull.GetHashCode();
            }
        }
    }

    /// <summary>
    /// A register allocator for the CIL backend.
    /// </summary>
    internal sealed class CilRegisterAllocator : GreedyRegisterAllocator<CilCodegenRegister>
    {
        private ModuleDefinition module;

        private Dictionary<ValueTag, ParameterDefinition> paramRegisters;

        private HashSet<ValueTag> usedValues;

        /// <summary>
        /// Creates a CIL register allocator that will only allocate
        /// registers to a particular set of values.
        /// </summary>
        /// <param name="usedValues">
        /// The values to allocate registers to.
        /// </param>
        /// <param name="paramRegisters">
        /// The registers assigned to the method's parameters.
        /// </param>
        /// <param name="module">
        /// The module to use for importing CLR types.
        /// </param>
        public CilRegisterAllocator(
            HashSet<ValueTag> usedValues,
            Dictionary<ValueTag, ParameterDefinition> paramRegisters,
            ModuleDefinition module)
        {
            this.usedValues = usedValues;
            this.paramRegisters = paramRegisters;
            this.module = module;
        }

        /// <inheritdoc/>
        protected override CilCodegenRegister CreateRegister(IType type)
        {
            return new CilCodegenRegister(
                new VariableDefinition(module.ImportReference(type)), type);
        }

        /// <inheritdoc/>
        protected override bool RequiresRegister(
            ValueTag value,
            FlowGraph graph)
        {
            return usedValues.Contains(value);
        }

        /// <inheritdoc/>
        protected override bool TryGetPreallocatedRegister(
            ValueTag value,
            FlowGraph graph,
            out CilCodegenRegister register)
        {
            ParameterDefinition parameter;
            if (paramRegisters.TryGetValue(value, out parameter))
            {
                register = new CilCodegenRegister(parameter, graph.GetValueType(value));
                return true;
            }
            else
            {
                register = default(CilCodegenRegister);
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool TryRecycleRegister(
            IType type,
            IEnumerable<CilCodegenRegister> registers,
            out CilCodegenRegister result)
        {
            result = registers.FirstOrDefault(reg => reg.Type == type);
            return result.IsParameter || result.IsVariable;
        }
    }
}