namespace Furesoft.Core.GraphDb.IO.Blocks;

public abstract class PropertyBlock(
    int propertyId,
    bool used,
    PropertyType propertyType,
    int propertyNameId,
    byte[] value,
    int nextPropertyId,
    int nodeId)
{
    public int NextPropertyId = nextPropertyId;
    public int NodeId = nodeId;
    public int PropertyId = propertyId;
    public int PropertyNameId = propertyNameId;
    public PropertyType PropertyType = propertyType;
    public bool Used = used;
    public byte[] Value = value;

    protected PropertyBlock(PropertyBlock other) : this(other.PropertyId, other.Used, other.PropertyType,
        other.PropertyNameId, other.Value, other.NextPropertyId, other.NodeId)
    {
    }
}