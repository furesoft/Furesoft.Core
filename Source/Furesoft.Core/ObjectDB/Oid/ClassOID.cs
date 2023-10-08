using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Oid;

internal sealed class ClassOID : BaseOID
{
    public ClassOID(long oid) : base(oid)
    {
    }

    public override int CompareTo(OID oid)
    {
        if (oid == null || !(oid is ClassOID))
            return -1000;

        var otherOid = oid;
        return (int) (ObjectId - otherOid.ObjectId);
    }

    public override bool Equals(object @object)
    {
        var oid = @object as OID;

        return this == @object || CompareTo(oid) == 0;
    }

    public override int GetHashCode()
    {
        // Copy of the Long hashcode algorithm
        return (int) (ObjectId ^ UrShift(ObjectId, 32));
    }
}