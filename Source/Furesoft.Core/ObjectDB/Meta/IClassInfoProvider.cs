using Furesoft.Core.ObjectDB.Meta.Introspector;

namespace Furesoft.Core.ObjectDB.Meta;

public interface IClassInfoProvider
{
    IObjectIntrospectionDataProvider GetClassInfoProvider();
}