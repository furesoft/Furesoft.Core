using System.Reflection;
using Furesoft.Core.ObjectDB.Container;
using Furesoft.Core.ObjectDB.Services;
using Furesoft.Core.ObjectDB.Tool;

namespace Furesoft.Core.ObjectDB.Api.Triggers;

internal sealed class EnrichWithOidTrigger : SelectTrigger
{
    private static readonly Dictionary<Type, FieldInfo> oidFields = new();

    private readonly IReflectionService _reflectionService;

    public EnrichWithOidTrigger()
    {
        _reflectionService = DependencyContainer.Resolve<IReflectionService>();
    }

    public override void AfterSelect(object @object, OID oid)
    {
        var type = @object.GetType();
        var oidField = oidFields.GetOrAdd(type, SearchOidSupportableField);

        if (oidField == null)
            return;

        if (oidField.FieldType == typeof(OID))
            oidField.SetValue(@object, oid);
        else
            oidField.SetValue(@object, oid.ObjectId);
    }

    private FieldInfo SearchOidSupportableField(Type type)
    {
        var fields = _reflectionService.GetFields(type);

        return (from fieldInfo in fields
            let attributes = fieldInfo.GetCustomAttributes(true)
            let hasAttribute = attributes.OfType<OIDAttribute>().Any()
            let isOidSupportedType = fieldInfo.FieldType == typeof(OID) || fieldInfo.FieldType == typeof(long)
            where hasAttribute && isOidSupportedType
            select fieldInfo).FirstOrDefault();
    }
}