using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Pipeline
{
    /// <summary>
    /// A description of a target assembly's contents.
    /// </summary>
    public sealed class AssemblyContentDescription
    {
        public AssemblyContentDescription(
            QualifiedName fullName,
            AttributeMap attributes,
            IAssembly assembly,
            IMethod entryPoint)
        {
            this.FullName = fullName;
            this.Attributes = attributes;
            this.Assembly = assembly;
            this.EntryPoint = entryPoint;
        }

        public IAssembly Assembly { get; }

        /// <summary>
        /// Gets the attribute map for the assembly to build.
        /// </summary>
        /// <returns>An attribute map.</returns>
        public AttributeMap Attributes { get; private set; }

        /// <summary>
        /// Gets the assembly's entry point. Returns <c>null</c> if the
        /// assembly has no entry point.
        /// </summary>
        /// <returns>The assembly's entry point.</returns>
        public IMethod EntryPoint { get; private set; }

        public QualifiedName FullName { get; private set; }
    }
}