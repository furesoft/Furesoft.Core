using Furesoft.Core.CodeDom.Compiler.Core.Collections;

namespace Furesoft.Core.CodeDom.Compiler.Instructions
{
    internal sealed class StructuralStorePrototypeComparer
        : IEqualityComparer<StorePrototype>
    {
        public bool Equals(StorePrototype x, StorePrototype y)
        {
            return object.Equals(x.ResultType, y.ResultType)
                && x.IsVolatile == y.IsVolatile
                && x.Alignment == y.Alignment;
        }

        public int GetHashCode(StorePrototype obj)
        {
            var hash = EnumerableComparer.EmptyHash;
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.ResultType);
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.IsVolatile);
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.Alignment);
            return hash;
        }
    }

    internal sealed class StructuralStoreIndirectPrototypeComparer
        : IEqualityComparer<StoreIndirectPrototype>
    {
        public bool Equals(StoreIndirectPrototype x, StoreIndirectPrototype y)
        {
            return object.Equals(x.ResultType, y.ResultType)
                && x.IsVolatile == y.IsVolatile
                && x.Alignment == y.Alignment;
        }

        public int GetHashCode(StoreIndirectPrototype obj)
        {
            var hash = EnumerableComparer.EmptyHash;
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.ResultType);
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.IsVolatile);
            hash = EnumerableComparer.FoldIntoHashCode(hash, obj.Alignment);
            return hash;
        }
    }
}