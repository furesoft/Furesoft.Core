using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler.TypeSystem
{
    public class DescribedProperty : DescribedMember, IProperty
    {
        private readonly IType _parentType;
        private readonly IType _propertyType;
        private IList<IAccessor> _accessors;
        private IList<Parameter> _indexers;

        public DescribedProperty(UnqualifiedName name, IType propertyType, IType parentType)
            : base(name.Qualify(parentType.FullName))
        {
            _propertyType = propertyType;
            _parentType = parentType;
            _indexers = new List<Parameter>();
            _accessors = new List<IAccessor>();
        }

        public IReadOnlyList<IAccessor> Accessors => (IReadOnlyList<IAccessor>)_accessors;
        public IReadOnlyList<Parameter> IndexerParameters => (IReadOnlyList<Parameter>)_indexers;
        public IType ParentType => _parentType;
        public IType PropertyType => _propertyType;

        public void AddAccessor(IAccessor accessor)
        {
            _accessors.Add(accessor);
        }

        public void AddGetter(UnqualifiedName name, bool isStatic, IType returnType, MethodBody body)
        {
            var accessor = new DescribedBodyAccessor(this, AccessorKind.Get,
                name, isStatic, returnType);
            accessor.Body = body;

            _accessors.Add(accessor);
        }

        public void AddGetter(UnqualifiedName name, bool isStatic, IType returnType)
        {
            var accessor = new DescribedAccessor(this, AccessorKind.Get,
                name, isStatic, returnType);

            _accessors.Add(accessor);
        }

        public void AddIndexer(Parameter parameter)
        {
            _indexers.Add(parameter);
        }

        public void AddSetter(UnqualifiedName name, bool isStatic, IType returnType)
        {
            var accessor = new DescribedAccessor(this, AccessorKind.Set,
                name, isStatic, returnType);

            _accessors.Add(accessor);
        }

        public void AddSetter(UnqualifiedName name, bool isStatic, IType returnType, MethodBody body)
        {
            var accessor = new DescribedBodyAccessor(this, AccessorKind.Set,
                name, isStatic, returnType);
            accessor.Body = body;

            _accessors.Add(accessor);
        }
    }
}