using System.Runtime.CompilerServices;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem
{
    /// <summary>
    /// A data structure that can be queried for method implementations.
    /// </summary>
    internal sealed class VTable
    {
        // This cache interns all VTables.
        private static ConditionalWeakTable<IType, VTable> instanceCache
            = new();

        private Dictionary<IMethod, IMethod> implementations;

        private VTable(IType type)
        {
            var impls = new Dictionary<IMethod, IMethod>();
            foreach (var baseType in type.BaseTypes)
            {
                foreach (var pair in Get(baseType).implementations)
                {
                    impls[pair.Key] = pair.Value;
                }
            }
            foreach (var method in type.Methods.Concat(
                type.Properties.SelectMany(prop => prop.Accessors)))
            {
                foreach (var baseMethod in method.BaseMethods)
                {
                    impls[baseMethod] = method;
                }
            }
            implementations = impls;
        }

        public static VTable Get(IType type)
        {
            return instanceCache.GetValue(type, t => new VTable(t));
        }

        public IMethod GetImplementation(IMethod method)
        {
            if (implementations.TryGetValue(method, out IMethod impl))
            {
                return GetImplementation(impl);
            }
            else
            {
                return method;
            }
        }
    }
}