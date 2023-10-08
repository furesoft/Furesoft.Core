using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Meta;
using Furesoft.Core.ObjectDB.Meta.Introspector;

namespace Furesoft.Core.ObjectDB.Triggers;

internal sealed class ObjectRepresentation : IObjectRepresentation
{
    private readonly IObjectIntrospectionDataProvider _classInfoProvider;
    private readonly NonNativeObjectInfo _nnoi;

    public ObjectRepresentation(NonNativeObjectInfo nnoi, IObjectIntrospectionDataProvider classInfoProvider)
    {
        _nnoi = nnoi;
        _classInfoProvider = classInfoProvider;
    }

    #region IObjectRepresentation Members

    public object GetValueOf(string attributeName)
    {
        if (_nnoi.IsNull())
            throw new OdbRuntimeException(
                NDatabaseError.TriggerCalledOnNullObject.AddParameter(_nnoi.GetClassInfo().FullClassName)
                    .AddParameter(attributeName));
        return _nnoi.GetValueOf(attributeName);
    }

    public void SetValueOf(string attributeName, object value)
    {
        var introspector = (IObjectIntrospector) new ObjectIntrospector(_classInfoProvider);
        var aoi = introspector.GetMetaRepresentation(value, true, null, new DefaultInstrumentationCallback());
        _nnoi.SetValueOf(attributeName, aoi);
    }

    public OID GetOid()
    {
        return _nnoi.GetOid();
    }

    public NonNativeObjectInfo GetNnoi()
    {
        return _nnoi;
    }

    public string GetObjectClassName()
    {
        return _nnoi.GetClassInfo().FullClassName;
    }

    #endregion IObjectRepresentation Members
}