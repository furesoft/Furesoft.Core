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

        public IPropertyMethod Getter { get; set; }
        public bool HasGetter { get { return Getter != null; } }
        public IPropertyMethod Setter { get; set; }
        public bool HasSetter { get { return Setter != null; } }

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

        public void AddIndexer(Parameter parameter)
        {
            _indexers.Add(parameter);
        }
    }

    public class DescribedPropertyMethod : DescribedMember, IPropertyMethod
    {
        private readonly IType _parentType;
        public MethodBody Body { get; set; }

        public DescribedPropertyMethod(UnqualifiedName name, IType parentType) : base(name.Qualify(parentType.FullName.Qualifier))
        {
            _parentType = parentType;
        }

        public IType ParentType => _parentType;
    }
}