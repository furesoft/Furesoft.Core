using System.Collections;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Api.Query;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq;

internal sealed class PlaceHolderQuery<T> : ILinqQuery<T>
{
    public PlaceHolderQuery(IOdb odb)
    {
        QueryFactory = odb;
    }

    public IOdb QueryFactory { get; }

    #region ILinqQuery<T> Members

    public IEnumerator<T> GetEnumerator()
    {
        var query = QueryFactory.Query<T>();
        return query.Execute<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion ILinqQuery<T> Members
}