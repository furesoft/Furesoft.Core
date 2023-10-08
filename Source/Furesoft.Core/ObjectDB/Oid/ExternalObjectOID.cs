using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Oid;

internal sealed class ExternalObjectOID : ObjectOID, IExternalOID
{
    private readonly IDatabaseId _databaseId;

    public ExternalObjectOID(OID oid, IDatabaseId databaseId) : base(oid.ObjectId)
    {
        _databaseId = databaseId;
    }

    #region IExternalOID Members

    public IDatabaseId GetDatabaseId()
    {
        return _databaseId;
    }

    #endregion IExternalOID Members
}